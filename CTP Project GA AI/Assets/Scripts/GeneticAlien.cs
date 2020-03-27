﻿using System.Collections;
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
    public int allowedMoves = 50;
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

    public class GeneLogic
    {
        int geneLength = GeneticAlien._instance.allowedMoves;
        public List<char[]> genes = new List<char[]>();
        //public char[] instructions = new char[2];
        string al = "abcdefghijklmnopqrstuvwxyz";
        string directions = "01234567";
        public GeneLogic()
        {
            for (int i = 0; i < geneLength; i++)
            {
                //Create new instructions, randomly initialise from the alphabet and each available direction
                char[] instructions = new char[2];
                while (instructions[0] != 'm' && instructions[0] != 's' && instructions[0] != 't')
                {
                    char inst = al[UnityEngine.Random.Range(0, al.Length)];
                    
                    instructions[0] = inst;
                }

                instructions[1] = directions[UnityEngine.Random.Range(0, directions.Length)];
                genes.Add(instructions);
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
        public GeneLogic genes;
        public Grid.Tile occupiedTile;
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
        else
        {
            //difficultyText.text = ("That wave's effectiveness was " + FindDifficulty().ToString() + ".");
            //difficultyText.enabled = true;
            
        }
    }

    public void CheckIfAliensKilled()
    {
        killCount = 0;
        for (int i = 0; i < aliens.Count; i++)
        {
            //Disable aliens if killed
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

    private void AlienLogic()
    {
        verticalMovementTimer += Time.deltaTime;

        AlienBehaviour();
        
        if (verticalMovementTimer > (tickRate / 2))
        {
            for (int i = 0; i < aliens.Count; i++)
            {
                if (aliens[i].GetComponent<Alien>().alive)
                {
                    //aliens[i].GetComponent<Alien>().instance.transform.position = new Vector3(
                    //aliens[i].GetComponent<Alien>().instance.transform.position.x,
                    //(aliens[i].GetComponent<Alien>().instance.transform.position.y - verticalMovementRate),
                    //aliens[i].GetComponent<Alien>().instance.transform.position.z);
                    var myTile = aliens[i].GetComponent<Alien>().occupiedTile;
                    //saliens[i].GetComponent<Alien>().occupiedTile = myTile.surroundingTiles[]; 
                    aliens[i].transform.position = myTile.position;
             
                }
            }
            verticalMovementTimer = 0;
        }
    }

    private void AlienBehaviour()
    {
        if (elapsedTime > (tickRate * 2))
        {

            for (int i = 0; i < aliens.Count; i++)
            {

                if (aliens[i].GetComponent<Alien>().alive)
                {
                    //Alien Logic
                    var pos = aliens[i].GetComponent<Alien>().instance.gameObject.transform.position;
                    //switch (aliens[i].GetComponent<Alien>().dna.genes[aliens[i].GetComponent<Alien>().movesUsed])
                    switch (aliens[i].GetComponent<Alien>().dna.genes[aliens[i].GetComponent<Alien>().movesUsed][0])
                    {
                        case 'm':
                            aliens[i].GetComponent<Alien>().instance.transform.position = new Vector3(
                                (pos.x - horizontalMovementRate), pos.y, pos.z);
                            print("YAAT");
                            break;
                        case 's':
                            print("YEET");
                            break;
                        case 't':
                            aliens[i].GetComponent<Alien>().instance.transform.position = new Vector3(
                                (pos.x + horizontalMovementRate), pos.y, pos.z);
                            print("YOTE");
                            break;
                        //case 3:
                        //    if (UnityEngine.Random.Range(1, 10) > 5)
                        //        aliens[i].GetComponent<Alien>().FireLaser();
                        //    break;
                    }
                    aliens[i].GetComponent<Alien>().movesUsed++;
                }
            }
            elapsedTime = 0;
            var alienID = FindAlienInList();
            if (alienID >= 0)
            {
                var alien = aliens[alienID].GetComponent<Alien>();

                //Spawn the aliens at a position based on their genes
                //alien.instance.transform.position = spawnPoints[ga.population[alienID].genes[0]].transform.position;
                var startTile = Grid.instance.gridTiles[Grid.instance.gridTiles.Count - 1][
                    (int)UnityEngine.Random.Range(0, Grid.instance.gridTiles[0].Count)];
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
        b[0] = a.genes[0][0];
        b[1] = a.genes[0][1];
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
            if (player.GetComponent<PlayerController>().GetTarget() == aliens[i].GetComponent<Alien>() && aliens[i].GetComponent<Alien>().instance.activeSelf)
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