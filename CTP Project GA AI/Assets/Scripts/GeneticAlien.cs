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
    [SerializeField] GameObject spawnPoint;
    [SerializeField] GameObject laserPrefab;

    [SerializeField] private List<GameObject> aliens = new List<GameObject>();
    [SerializeField] int alienCap;
    [SerializeField] GameObject alienPrefab;

    public bool roundActive = true;

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
    
    [Range(0, 30)] public int waveSize = 10;
    
    public int killCount = 0;
    [SerializeField] [Range(0.01f, 0.1f)] float mutationRate = 0.02f;
    [SerializeField] int allowedMoves = 50;
    int[] validMoves = new int[4];
    [Header("Misc")]
    [SerializeField] int targetTimeAlive = 8;
    [SerializeField] int[] bestFitnessMoves;
    [SerializeField] Vector3 targetY = new Vector3(0, -4);
    [SerializeField] private int aliensKeptPerGeneration = 1;

    //Alien spawning and tickrate
    [Header("Movement and tickrate")]
    [SerializeField] private float elapsedTime = 0.0f;
    [SerializeField] private float tickRate = 0.0f;
    [SerializeField] private float horizontalMovementRate = 0.0f;
    [SerializeField] private float verticalMovementTimer = 0.0f;
    [SerializeField] private float verticalMovementRate = 0.0f;


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
        public void FireLaser()
        {
            var firedLaser = Instantiate(laserPrefab);
            firedLaser.transform.position = instance.transform.position;
            firedLaser.GetComponent<Laser>().inUse = true;
        }
        //To track when the alien is moving left or right
        public enum MoveDir
        {
            Wait = 0,
            Left = 1,
            Right = 2,
            Shoot = 3
        }
        public MoveDir movingTo;
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
        difficultyText.enabled = false;
        random = new System.Random();
        ga = new GeneticAlgorithm(waveSize, allowedMoves, random, GetRandomMovementDirection, FitnessFunction, aliensKeptPerGeneration, mutationRate);
        
        for (int i = 0; i < waveSize; i++)
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
        elapsedTime += Time.deltaTime;
        if (roundActive)
        {
           if (!CheckPlayerTargetValid())
            {
                print("Could not find player's target!");
            }
            difficultyText.enabled = false;
            CheckIfAliensKilled();
            SpawnAliens();
        }
        else
        {
            //difficultyText.text = ("That wave's effectiveness was " + FindDifficulty().ToString() + ".");
            //difficultyText.enabled = true;
            if (elapsedTime > 0.5f)
            {

                roundActive = true;
                elapsedTime = 0;
            }
        }
    }

    public void CheckIfAliensKilled()
    {
        killCount = 0;
        for (int i = 0; i < aliens.Count; i++)
        {
            if (aliens[i].GetComponent<Alien>().instance.GetComponent<AlienController>().killed)
            {
                var alien = aliens[i].GetComponent<Alien>().instance.gameObject;

                alien.transform.position = new Vector3(-99, -99);
                alien.transform.localScale = alienSizeDefault;
                alien.SetActive(false);
                killCount++;

            }
        }
    }

    private void SpawnAliens()
    {
        verticalMovementTimer += Time.deltaTime;

        if (elapsedTime > (tickRate * 2))
        {

            for (int i = 0; i < aliens.Count; i++)
            {
                if (aliens[i].GetComponent<Alien>().alive)
                {
                    var pos = aliens[i].GetComponent<Alien>().instance.gameObject.transform.position;
                    switch (aliens[i].GetComponent<Alien>().dna.genes[aliens[i].GetComponent<Alien>().movesUsed])
                    {
                        case 0:
                            aliens[i].GetComponent<Alien>().instance.transform.position = new Vector3(
                                (pos.x - horizontalMovementRate), pos.y, pos.z);
                            break;
                        case 1:
                            break;
                        case 2:
                            aliens[i].GetComponent<Alien>().instance.transform.position = new Vector3(
                                (pos.x + horizontalMovementRate), pos.y, pos.z);
                            break;
                        case 3:
                            aliens[i].GetComponent<Alien>().FireLaser();
                            break;
                    }
                    aliens[i].GetComponent<Alien>().movesUsed++;
                }
            }
            elapsedTime = 0;
            var alienID = FindAlienInList();
            if (alienID >= 0)
            {
                aliens[alienID].GetComponent<Alien>().instance.transform.position = spawnPoint.transform.position;
                aliens[alienID].GetComponent<Alien>().alive = true;
                aliens[alienID].GetComponent<Alien>().instance.gameObject.SetActive(true);
                aliens[alienID].GetComponent<Alien>().instance.gameObject.transform.localScale = new Vector3(
                    UnityEngine.Random.Range(alienSizeMin.x, alienSizeMax.x),
                    UnityEngine.Random.Range(alienSizeMin.y, alienSizeMax.y),
                    UnityEngine.Random.Range(alienSizeMin.z, alienSizeMax.z));
            }

        }

        if (verticalMovementTimer > (tickRate / 4))
        {
            for (int i = 0; i < aliens.Count; i++)
            {
                if (aliens[i].GetComponent<Alien>().alive)
                {
                    aliens[i].GetComponent<Alien>().instance.transform.position = new Vector3(
                                aliens[i].GetComponent<Alien>().instance.transform.position.x,
                                (aliens[i].GetComponent<Alien>().instance.transform.position.y - verticalMovementRate),
                                aliens[i].GetComponent<Alien>().instance.transform.position.z);
                }
            }
            verticalMovementTimer = 0;
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

    public int GetRandomMovementDirection()
    {
        var i = random.Next(validMoves.Length);
        return i;
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
                spawnPoint.transform.position);

            score /= Vector3.Distance(ga.population[index].Owner.
                instance.GetComponent<AlienController>().deathPosition, 
                new Vector3(0,-4,0));
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
            if (player.GetComponent<PlayerController>().GetTarget() == aliens[i].GetComponent<Alien>() && aliens[i].GetComponent<Alien>().instance.activeSelf)
            {
                return true;
            }
        }
        player.GetComponent<PlayerController>().ResetPlayerAI();
        return false;
    }
}
//aliens.Add(new Alien());
//            aliens[i].instance = Instantiate(alienPrefab);
//ga.population[i] = (new DNA(allowedMoves, GetRandomMovementDirection, FitnessFunction, aliens[i]));
//            aliens[i].dna = ga.population[i];
//            aliens[i].instance.gameObject.SetActive(false);
//            aliens[i].instance.GetComponent<AlienController>().uid = i;
//            aliens[i].id = i;