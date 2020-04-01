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

    [SerializeField] private List<GameObject> aliens = new List<GameObject>();
    [SerializeField] private List<GameObject> activeAliens = new List<GameObject>();
    [SerializeField] int alienCap;
    public GameObject alienPrefab;

    public bool roundActive;

    //Size scaling
    [SerializeField] Vector3 alienSizeMin;
    private Vector3 alienSizeDefault;
    [SerializeField] Vector3 alienSizeMax;

    public GameObject wall1;
    public GameObject wall2;

    //Variables for algorithm
    [Header("Algorithm")]
    //Algorithm
    public GeneticAlgorithm ga;
    private System.Random random;
    private float difficulty = 0;
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
                while (instructions[0] != 'm') //&& instructions[0] != 's' && instructions[0] != 't')
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
        public DNA dna;
        public Grid.Tile occupiedTile;
        public int occupedTileVal;
        public Vector2 occupiedTilePos;
        public Grid.Tile targetTile;
        public void FireLaser()
        {
            var firedLaser = Instantiate(laserPrefab);
            firedLaser.transform.position = instance.transform.position;
            firedLaser.GetComponent<Laser>().inUse = true;
        }
    }

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
        ga = new GeneticAlgorithm(waveSize, allowedMoves, random, GetRandomMovementDirection, FitnessFunction, aliensKeptPerGeneration, mutationRate);
        
        // Create new aliens, give them genes
        for (int i = 0; i < (waveSize); i++)
        {
            var newAlien = Instantiate(alienPrefab);
            var alienBrain = newAlien.GetComponent<Alien>();
            alienBrain.instance = newAlien.gameObject;
            ga.population[i] = (new DNA(allowedMoves, GetRandomMovementDirection, FitnessFunction, newAlien.GetComponent<Alien>()));
            alienBrain.dna = ga.population[i];
            alienBrain.instance.gameObject.SetActive(false);
            alienBrain.instance.GetComponent<AlienController>().uid = i;
            alienBrain.id = i;
            alienBrain.laserPrefab = laserPrefab;

            aliens.Add(newAlien);
            
            //print("Successfully attached to alien " + ga.population[i].Owner.instance.gameObject.GetComponent<AlienController>().uid);
        }
        alienSizeDefault = aliens[0].GetComponent<Alien>().instance.gameObject.transform.localScale;
        roundActive = true;
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
            CheckIfAliensKilled();
            AlienLogic();
        }
    }
     
    public void CheckIfAliensKilled()
    {
        activeAliens.Clear();
        killCount = 0;
        for (int i = 0; i < aliens.Count; i++)
        {
            //Disable aliens if killed
            if (aliens[i].GetComponent<Alien>().instance.GetComponent<AlienController>().killed)
            {
                var _alien = aliens[i].GetComponent<Alien>();
                if (_alien.occupiedTile != null)
                {
                    _alien.occupiedTile.currentTileState = Grid.Tile.TileState.Empty;
                    _alien.occupiedTile = null;
                }
                _alien.targetTile = null;

                var alien = aliens[i].GetComponent<Alien>().instance.gameObject;
                alien.transform.position = new Vector3(-99, -99);
                alien.transform.localScale = alienSizeDefault;
                alien.SetActive(false);
                aliens[i].GetComponent<Alien>().alive = false;
                killCount++;

            }

            else if (aliens[i].GetComponent<Alien>().instance.GetComponent<AlienController>().alive)
            {
                activeAliens.Add(aliens[i]);
            }
        }
    }

    private void AlienLogic()
    {
        alienMovementTimer += Time.deltaTime;
        if (alienMovementTimer > (alienTickRate))
        {
            AlienBehaviour();
            UpdateAlienPosition();
            alienMovementTimer = 0;
        }

    }

    //  Check if aliens have a tile to move to, if they do then 
    //  clear my tile and move to the new one.
    private void MoveToNewTile(int _index, Grid.Tile _newTile)
    {
        var alien = activeAliens[_index].GetComponent<Alien>();
        if (alien.targetTile != null)
        {
            alien.occupiedTile.currentTileState = Grid.Tile.TileState.Empty;
            alien.occupiedTile = _newTile;
            alien.targetTile = null;
        }
    }
    private void UpdateAlienPosition()
    {
        CheckTilesAgainstAliens();
        for (int i = 0; i < activeAliens.Count; i++)
        {
            var myTile = activeAliens[i].GetComponent<Alien>().occupiedTile;
            if (myTile.position != 
                (Vector2)activeAliens[i].GetComponent<Alien>().transform.position)
            {
                activeAliens[i].GetComponent<Alien>().transform.position =
                    myTile.position;
            }

            MoveToNewTile(i, activeAliens[i].GetComponent<Alien>().targetTile);
            myTile = activeAliens[i].GetComponent<Alien>().occupiedTile;
            activeAliens[i].transform.position = myTile.position;
            activeAliens[i].GetComponent<Alien>().occupiedTile.currentTileState = Grid.Tile.TileState.OccupiedByAlien;
            activeAliens[i].GetComponent<Alien>().occupedTileVal = activeAliens[i].GetComponent<Alien>().occupiedTile.id;
            activeAliens[i].GetComponent<Alien>().occupiedTilePos = activeAliens[i].GetComponent<Alien>().occupiedTile.position;
        }
    }

    private void AlienBehaviour()
    {
        if (elapsedTime > (alienTickRate))
        {

            for (int i = 0; i < activeAliens.Count; i++)
            {
                //Alien Logic
                var pos = activeAliens[i].GetComponent<Alien>().instance.gameObject.transform.position;
                //switch (aliens[i].GetComponent<Alien>().dna.genes[aliens[i].GetComponent<Alien>().movesUsed])
                switch (activeAliens[i].GetComponent<Alien>().dna.genes[activeAliens[i].GetComponent<Alien>().movesUsed][0])
                {
                    case 'm':
                        ExecuteBehaviour('m', i);
                        print("YAAT");
                        break;
                    case 's':
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
                activeAliens[i].GetComponent<Alien>().movesUsed++;
            }
            elapsedTime = 0;
            SpawnAliensInRound();
        }
    }

    private void SpawnAliensInRound()
    {
        var alienID = FindAlienInList();
        if (alienID >= 0)
        {
            var alien = aliens[alienID].GetComponent<Alien>();

            //Spawn the aliens at a position based on their genes
            //alien.instance.transform.position = spawnPoints[ga.population[alienID].genes[0]].transform.position;
            var startTile = new Grid.Tile();
            startTile.currentTileState = Grid.Tile.TileState.OccupiedByAlien;
            while (startTile.currentTileState != Grid.Tile.TileState.Empty)
            {
                startTile = Grid.instance.gridTiles[Grid.instance.gridTiles.Count - 1][
                    (int)UnityEngine.Random.Range(0, Grid.instance.gridTiles[0].Count)];
            }
            alien.instance.transform.position = startTile.position;
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
    private void ExecuteBehaviour(char _gene, int _index)
    {
        if (_gene == 'm')
        {
            var alien = activeAliens[_index].GetComponent<Alien>();

            char targetTile = alien.dna.genes[alien.movesUsed][1];
            var tIndex = (int)char.GetNumericValue(targetTile); //index of the target tile converted to int from char

            //  Check if the target tile doesn't exist or isn't empty before moving there
            if (alien.occupiedTile.surroundingTiles[tIndex] == null ||
                alien.occupiedTile.surroundingTiles[tIndex].currentTileState != Grid.Tile.TileState.Empty)
            {
                alien.movesUsed++;
                ExecuteBehaviour(_gene, _index);
                return;
            }
            alien.targetTile = alien.occupiedTile.surroundingTiles[tIndex];
        }
        else if (_gene == 's')
        {

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
                if (aliens[j].GetComponent<Alien>().targetTile == targetedTiles[i])
                {
                    for (int k = (i+1); k < aliens.Count; k++)
                    {
                        if (aliens[k].GetComponent<Alien>().targetTile == targetedTiles[i])
                        {
                            aliens[k].GetComponent<Alien>().targetTile = null;
                            break;
                        }
                    }
                }
            }
        }
    }
    public int FindAlienInList()
    {
        for (int i = 0; i < aliens.Count; i++)
        {
            if (!aliens[i].GetComponent<Alien>().instance.GetComponent<AlienController>().killed
                && !aliens[i].GetComponent<Alien>().alive)
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
        for (int i = 0; i < aliens.Count; i++)
        {
            aliens[i].GetComponent<Alien>().instance.GetComponent<AlienController>().killed = true;
        }
        CheckIfAliensKilled();
    }

    

    public float FindDifficulty()
    {
        difficulty = 0;
        for (int i = 0; i < aliens.Count; i++)
        {
            difficulty += FitnessFunction(i);
        }
        return (difficulty);
    }

    public GeneticAlgorithm GetGA()
    {
        return ga;
    }
    public List<GameObject> GetAlienList()
    {
        return aliens;
    }

    public void ResetPlayerAI()
    {
        player.GetComponent<PlayerController>().ResetPlayerAI();
    }

    bool CheckPlayerTargetValid()
    {
        for (int i = 0; i < aliens.Count; i++)
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