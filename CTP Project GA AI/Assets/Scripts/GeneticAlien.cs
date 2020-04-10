using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GeneticAlien : MonoBehaviour
{

    public int round = 0;
    [SerializeField] GameObject player;
    public static GeneticAlien _instance;
    [SerializeField] GameObject[] spawnPoints = new GameObject[4];
    [SerializeField] GameObject laserPrefab;


    [SerializeField] public List<GameObject> primaryAliens = new List<GameObject>(); // The main evolving set of aliens
    [SerializeField] private List<GameObject> activeAliens = new List<GameObject>(); //  Aliens that are on the screen and moving.
    [SerializeField] public List<GameObject> backupAliens = new List<GameObject>(); //  Aliens that i may need for swapping between waves?
    [SerializeField] int alienCap;  //Retrieves the wave size from RoundManager
    public GameObject alienPrefab;  //The base alien object from which each alien is built

    public bool roundActive;

    //Size scaling
    [SerializeField] Vector3 alienSizeMin; //   Min size for each alien, x vals are not valid
    private Vector3 alienSizeDefault;
    [SerializeField] Vector3 alienSizeMax; //   Max size for each alien, x vals are not valid

    public GameObject wall1;    //  Left boundary of the level
    public GameObject wall2;    //  Right boundary of the level

    //Variables for algorithm
    [Header("Algorithm")]
    /*              ----Algorithm----
     *
     *   The aim is to have a dynamic algorithm.
     *  
     *  Start: The backup (GA-B) algorithm is built 
     *      and the active (GA-A) algorithm
     *      adopts its values, then GA-B is wiped. 
     *  
     *  Update: When aliens are killed during gameplay
     *      their genes are added to GA-B's List. 
     *      When the waveSize is reached, all
     *      unspawned aliens in GA-A are given 
     *      GA-B's new improved genes instead.
     */
    public GeneticAlgorithm ga; //  The primary genetic algorithm
    public GeneticAlgorithm gaB; // The backup genetic algorithm, for alternating between waves.
    private float difficulty = 0;   //  The score of each of the aliens averaged
    private System.Random random; //    Used for initialisation of ga-genes.
    [SerializeField] private Text difficultyText;
        
    public int killCount = 0;
    [SerializeField] [Range(0.01f, 0.1f)] float mutationRate = 0.02f;
    public int allowedMoves = 200;

    [Header("Misc")]
    [SerializeField] int targetTimeAlive = 8;
    [SerializeField] int[] bestFitnessMoves;
    [SerializeField] Vector3 targetY = new Vector3(0, -4);
    [SerializeField] private int aliensKeptPerGeneration = 1;

    //Alien spawning and tickrate
    [Header("Movement and tickrate")]
    [SerializeField] private float elapsedTime = 0.0f;
    [SerializeField] private float alienTickRate = 0.0f;
    [SerializeField] private float alienMovementTimer = 0.0f;

    public class GeneLogic
    {
        int geneLength = GeneticAlien._instance.allowedMoves;
        public List<char[]> geneList = new List<char[]>();
        string al = "abcdefghijklmnopqrstuvwxyz";
        string directions = "01234567";
        public GeneLogic()
        {
            for (int i = 0; i < geneLength; i++)
            {
                //Create new instructions, randomly initialise from the alphabet and each available direction
                char[] instructions = new char[2];
                while (instructions[0] != 'm' && instructions[0] != 's') //&& instructions[0] != 't')
                {
                    char inst = al[UnityEngine.Random.Range(0, al.Length)];
                    
                    instructions[0] = inst;
                }

                instructions[1] = directions[UnityEngine.Random.Range(0, directions.Length)];
                geneList.Add(instructions);
            }
        }
    }

    public class Alien : MonoBehaviour
    {
        public GameObject instance;
        public GameObject laserPrefab;
        public bool alive;
        public float distanceTraveled;
        public int speed;
        public int id;
        public int movesUsed;
        public bool hitPlayer;
        public int wave;
        public DNA dna;
        public Grid.Tile occupiedTile;
        public int occupedTileVal;
        public Vector2 occupiedTilePos;
        public Grid.Tile targetTile;
        public void FireLaser(Grid.Tile _target)
        {
            var firedLaser = Instantiate(laserPrefab);
            firedLaser.transform.position = _target.boundingBox.position;
            firedLaser.GetComponent<Laser>().inUse = true;
        }
    }

    //  Create a singleton instance of this script
    private void Awake()
    {
        //Will be accessed by other scripts, so singleton
        if (_instance == null)
        {
            _instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        var waveSize = RoundManager.instance.roundLength;

        difficultyText.enabled = false;
        random = new System.Random();

        //  Create the genetic algorithms and make an alien set for each.
        ga = new GeneticAlgorithm(waveSize, allowedMoves, random, GetRandomMovementDirection, FitnessFunction, aliensKeptPerGeneration, mutationRate);
        gaB = ga;
        
        CreateNewAlienSet(ga, primaryAliens, waveSize);
        alienSizeDefault = primaryAliens[0].GetComponent<Alien>().instance.gameObject.transform.localScale;
        roundActive = true;
    }

    private void CreateNewAlienSet(GeneticAlgorithm _ga, List<GameObject> _aliens, int _count)
    {
        // Create new aliens, give them genes
        for (int i = 0; i < (_count); i++)
        {
            var newAlien = Instantiate(alienPrefab);
            var alienBrain = newAlien.GetComponent<Alien>();

            alienBrain.instance = newAlien.gameObject;
            AttachDNAToGA(_ga, i, newAlien);
            alienBrain.instance.gameObject.SetActive(false);
            alienBrain.instance.GetComponent<AlienController>().uid = i;
            alienBrain.id = i;
            alienBrain.laserPrefab = laserPrefab;

            _aliens.Add(newAlien);

            //print("Successfully attached to alien " + ga.population[i].Owner.instance.gameObject.GetComponent<AlienController>().uid);
        }
    }

    //  Sets the alien as the owner of an index in the GA's genes,
    //  Then the alien retrieves its genes from the GA.
    private void AttachDNAToGA(GeneticAlgorithm _ga, int i, GameObject _alien)
    {
        _ga.population[i] = (new DNA(allowedMoves, GetRandomMovementDirection, FitnessFunction, _alien.GetComponent<Alien>()));
        _alien.GetComponent<Alien>().dna = _ga.population[i];
    }

    void Update()
    {
        roundActive = RoundManager.instance.roundActive;
        elapsedTime += Time.deltaTime;
        if (roundActive)
        {
            if (player.GetComponent<PlayerController>().automatePlayerMovement)
            {
                if (!CheckPlayerTargetValid())
                {
                    print("Could not find player's target!");
                }
            }
            difficultyText.enabled = false;
            var deathCounter = CheckIfAliensKilled(true);
            if (deathCounter != null && deathCounter.Count >= 5) 
            {
                SwapGABacktoFront(deathCounter);
            }
            AlienLogic();
        }
    }

     /*
      * By default, handles setting the ActiveAliens and
      * checks if aliens were killed every frame.
      * List is returned to update GA-B once per epoch
      */
    public List<GameObject> CheckIfAliensKilled(bool _returnDead = false)
    {
        activeAliens.Clear();
        killCount = 0;
        List<GameObject> deadAliens = new List<GameObject>();
        for (int i = 0; i < primaryAliens.Count; i++)
        {
            //Disable aliens if killed
            if (primaryAliens[i].GetComponent<Alien>().instance.GetComponent<AlienController>().killed)
            {
                var _alien = primaryAliens[i].GetComponent<Alien>();
                if (_alien.occupiedTile != null)
                {
                    _alien.occupiedTile.currentTileState = Grid.Tile.TileState.Empty;
                    _alien.occupiedTile = null;
                }
                _alien.targetTile = null;

                var alien = primaryAliens[i].GetComponent<Alien>().instance.gameObject;
                alien.transform.position = new Vector3(-99, -99);
                alien.transform.localScale = alienSizeDefault;
                alien.SetActive(false);
                primaryAliens[i].GetComponent<Alien>().alive = false;
                killCount++;

                deadAliens.Add(primaryAliens[i]);
            }
            else if (primaryAliens[i].GetComponent<Alien>().instance.GetComponent<AlienController>().alive)
            {
                activeAliens.Add(primaryAliens[i]);
            }
        }
        if (_returnDead)
        {
            return deadAliens;
        }
        return null;
    }

    //  Add x dead aliens to the backup GA, then create a new generation.
    //  If it exceeds 50, remove the excess then give all dead aliens the new DNA.
    private void SwapGABacktoFront(List<GameObject> _deadAliens)
    {
        print("Swapping GA");
        for (int i = 0; i < _deadAliens.Count; i++)
        {
            gaB.population.Add(_deadAliens[i].GetComponent<Alien>().dna);
        }
        gaB.CreateNewGeneration();

        if (gaB.population.Count > 50)
        {
            gaB.population.RemoveRange(10, (gaB.population.Count - 1));
        }
        for (int i = 0; i < gaB.population.Count; i++)
        {
            for (int j = 0; j < primaryAliens.Count; j++)
            {
                if (primaryAliens[j].GetComponent<AlienController>().killed)
                {
                    primaryAliens[j].GetComponent<Alien>().alive = false;
                    primaryAliens[j].GetComponent<Alien>().dna = gaB.population[i];
                    primaryAliens[j].GetComponent<Alien>().movesUsed = 0;
                    primaryAliens[j].GetComponent<AlienController>().killed = false;
                }
            }
        }
    }

    private void AlienLogic()
    {
        alienMovementTimer += Time.deltaTime;
        if (alienMovementTimer > (alienTickRate))
        {
            for (int i = 0; i < activeAliens.Count; i++)
            {
                AlienBehaviour(i);
                var alien = activeAliens[i].GetComponent<Alien>();

                if (alien.dna.genes[alien.movesUsed][0] == 'm')
                    UpdateAlienPosition(i);
                else
                    if (alien.dna.genes[alien.movesUsed][0] == 's')
                        FireAtTile(i, activeAliens[i].GetComponent<Alien>().targetTile);
            }
            alienMovementTimer = 0;
            elapsedTime = 0;

        }
        else if (alienMovementTimer > (alienTickRate/3))
        SpawnAliensInRound();
    }

    //  Check if aliens have a tile to move to, if they do then 
    //  clear my tile and move to the new one.
    private void MoveToNewTile(int _index, Grid.Tile _newTile)
    {
        var alien = activeAliens[_index].GetComponent<Alien>();
        if (alien.targetTile != null && alien.dna.genes[alien.movesUsed][0] == 'm')
        {
            alien.occupiedTile.currentTileState = Grid.Tile.TileState.Empty;
            alien.occupiedTile = _newTile;
            alien.targetTile = null;
        }
    }
    /*  Check the alien's current tile against their 
    *   target tile. Clear their current tile and 
        Move to the new one
    */
    private void UpdateAlienPosition(int _index)
    {
        CheckTilesAgainstAliens();

            var myTile = activeAliens[_index].GetComponent<Alien>().occupiedTile;
            if (myTile.boundingBox.position != 
                (Vector2)activeAliens[_index].GetComponent<Alien>().transform.position)
            {
                activeAliens[_index].GetComponent<Alien>().transform.position =
                    myTile.boundingBox.position;
            }

            var alien = activeAliens[_index].GetComponent<Alien>();
            MoveToNewTile(_index, alien.targetTile);
            myTile = alien.occupiedTile;
            activeAliens[_index].transform.position = myTile.boundingBox.position;
            alien.occupiedTile.currentTileState = Grid.Tile.TileState.OccupiedByAlien;
            alien.occupedTileVal = alien.occupiedTile.id;
            alien.occupiedTilePos = alien.occupiedTile.boundingBox.position;
    }

    private void FireAtTile(int _index, Grid.Tile _target)
    {
        var alien = activeAliens[_index].GetComponent<Alien>();
        if (alien.targetTile != null && alien.dna.genes[alien.movesUsed][0] == 's')
        {
            activeAliens[_index].GetComponent<Alien>().FireLaser(_target);
        }
    }
    private void AlienBehaviour(int _index)
    {
        if (elapsedTime > (alienTickRate))
        {
                //Alien Logic
                var alien = activeAliens[_index].GetComponent<Alien>();
                switch (alien.dna.genes[alien.movesUsed][0])
                {
                    case 'm':
                        SetBehaviour('m', _index);
                        print("YAAT");
                        break;
                    case 's':
                        SetBehaviour('s', _index);
                        print("YEET");
                        break;
                    case 't':
                        print("YOTE");
                        break;
                        //case 3:
                        //    if (UnityEngine.Random.Range(1, 10) > 5)
                        //        aliens[i].GetComponent<Alien>().FireLaser();
                        //    break;
                }
                activeAliens[_index].GetComponent<Alien>().movesUsed++;
        }
    }

    private void SpawnAliensInRound()
    {
        var alienID = FindAlienInList();
        if (alienID >= 0)
        {
            var alien = primaryAliens[alienID].GetComponent<Alien>();

            //Spawn the aliens at a position based on their genes
            //alien.instance.transform.position = spawnPoints[ga.population[alienID].genes[0]].transform.position;
            var startTile = new Grid.Tile();
            startTile.currentTileState = Grid.Tile.TileState.OccupiedByAlien;
            while (startTile.currentTileState != Grid.Tile.TileState.Empty)
            {
                startTile = Grid.instance.gridTiles[Grid.instance.gridTiles.Count - 1][
                    (int)UnityEngine.Random.Range(0, Grid.instance.gridTiles[0].Count)];
            }
            alien.instance.transform.position = startTile.boundingBox.position;
            alien.occupiedTile = startTile;

            alien.alive = true;
            alien.instance.gameObject.SetActive(true);
            alien.instance.gameObject.transform.localScale = new Vector3(
                alien.instance.gameObject.transform.localScale.x,
                UnityEngine.Random.Range(alienSizeMin.y, alienSizeMax.y),
                UnityEngine.Random.Range(alienSizeMin.z, alienSizeMax.z));
        }
    }

    //  Read alien genes and execute behaviour based on it.
    private void SetBehaviour(char _gene, int _index)
    {
        if (_gene == 'm')
        {
            var alien = activeAliens[_index].GetComponent<Alien>();

            char targetTile = alien.dna.genes[alien.movesUsed][1];
            var tIndex = (int)char.GetNumericValue(targetTile); //index of the target tile converted to int from char

            //  Check if the target tile is valid before moving there
            //  If it isn't, loop the function

            if ( alien.occupiedTile != null && (alien.occupiedTile.surroundingTiles[tIndex] != null &&
                alien.occupiedTile.surroundingTiles[tIndex].currentTileState == Grid.Tile.TileState.Empty))
            {
                alien.targetTile = alien.occupiedTile.surroundingTiles[tIndex];
                print("Alien " + _index + " behaviour set");
            }
            else
            {
                alien.movesUsed++;
                SetBehaviour(_gene, _index);
                return;
            }
        }
        else if (_gene == 's')
        {
            var alien = activeAliens[_index].GetComponent<Alien>();

            char targetTile = alien.dna.genes[alien.movesUsed][1];
            var tIndex = (int)char.GetNumericValue(targetTile); //index of the target tile converted to int from char

            //  Check if the target tile is valid before firing there
            //  If it isn't, loop the function

            if (alien.occupiedTile != null && (alien.occupiedTile.surroundingTiles[tIndex] != null))
            {
                alien.targetTile = alien.occupiedTile.surroundingTiles[tIndex];
                print("Alien " + _index + " behaviour set");

            }
            else
            {
                alien.movesUsed++;
                SetBehaviour(_gene, _index);
                return;
            }
        }
    }

    //  Checks that two aliens do not have the same target tile, 
    //  if they do then the first alien gets the tile.
    private void CheckTilesAgainstAliens()
    {
        List<Grid.Tile> targetedTiles = new List<Grid.Tile>();
        for (int i = 0; i < activeAliens.Count; i++)
        {
            if (activeAliens[i].GetComponent<Alien>().targetTile != null)
            {
                targetedTiles.Add(activeAliens[i].GetComponent<Alien>().targetTile);
            }
        }

        
        for (int i = 0; i < targetedTiles.Count; i++)
        {
            for (int j = 0; j < activeAliens.Count; j++)
            {
                if (activeAliens[j].GetComponent<Alien>().targetTile == targetedTiles[i]
                    && activeAliens[j].GetComponent<Alien>().dna.genes[activeAliens[j].GetComponent<Alien>().movesUsed][0] == 'm')
                {
                    for (int k = (j+1); k < activeAliens.Count; k++)
                    {
                        if (activeAliens[k].GetComponent<Alien>().targetTile == targetedTiles[i])
                        {
                            activeAliens[k].GetComponent<Alien>().targetTile = null;
                            break;
                        }
                    }
                }
            }
        }
    }
    public int FindAlienInList()
    {
        for (int i = 0; i < primaryAliens.Count; i++)
        {
            if (!primaryAliens[i].GetComponent<Alien>().instance.GetComponent<AlienController>().killed
                && !primaryAliens[i].GetComponent<Alien>().alive)
            {
                return i;
            }
        }
        return -1;
    }

    public char[] GetRandomMovementDirection()
    {
        GeneLogic a = new GeneLogic();
        char[] b = new char[2];
        b[0] = a.geneList[0][0];
        b[1] = a.geneList[0][1];
        return b;
    }

    private float FitnessFunction(int index)
    {
        float score = 0;
        if (ga.population[index].Owner.hitPlayer)
        {
            score += 5;
        }
        else
        {
            score += Vector3.Distance(ga.population[index].Owner.
                instance.GetComponent<AlienController>().deathPosition, 
                new Vector3(0,-0.2f));

            score /= Vector3.Distance(ga.population[index].Owner.
                instance.GetComponent<AlienController>().deathPosition, 
                new Vector3(0,-4));
        }
        print("Score" + score +" for alien " +ga.population[index].Owner.instance.GetComponent<AlienController>().uid);
        return score;

    }
    public void ResetAliens()
    {
        for (int i = 0; i < primaryAliens.Count; i++)
        {
            primaryAliens[i].GetComponent<Alien>().instance.GetComponent<AlienController>().killed = true;
        }
        CheckIfAliensKilled();
    }
    //public float FindDifficulty()
    //{
    //    difficulty = 0;
    //    for (int i = 0; i < activeAliens.Count; i++)
    //    {
    //        difficulty += FitnessFunction(i);
    //    }
    //    return (difficulty);
    //}

    public GeneticAlgorithm GetGA()
    {
        return ga;
    }
    public List<GameObject> GetAlienList()
    {
        return primaryAliens;
    }

    public void ResetPlayerAI()
    {
        player.GetComponent<PlayerController>().ResetPlayerAI();
    }

    bool CheckPlayerTargetValid()
    {
        for (int i = 0; i < primaryAliens.Count; i++)
        {
            if (player.GetComponent<PlayerController>().GetTarget() == activeAliens[i].GetComponent<Alien>() 
                && activeAliens[i].GetComponent<Alien>().instance.activeSelf)
            {
                return true;
            }
        }
        player.GetComponent<PlayerController>().ResetPlayerAI();
        return false;
    }

    public Vector3 GetAlienSizeMax()
    {
        return alienSizeMax;
    }
  
}
//aliens.Add(new Alien());
//            aliens[i].instance = Instantiate(alienPrefab);
//ga.population[i] = (new DNA(allowedMoves, GetRandomMovementDirection, FitnessFunction, aliens[i]));
//            aliens[i].dna = ga.population[i];
//            aliens[i].instance.gameObject.SetActive(false);
//            aliens[i].instance.GetComponent<AlienController>().uid = i;
//            aliens[i].id = i;