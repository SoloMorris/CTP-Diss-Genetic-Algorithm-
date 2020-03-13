using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundManager : MonoBehaviour
{
    public static RoundManager instance;
    public int currentRound;
    public bool roundActive = false;
    public float difficulty;
    // Start is called before the first frame update

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        MonitorRound();
    }

    private void GenerateNewWave()
    {

    }

    private void MonitorRound()
    {
        var GAinst = GeneticAlien._instance;
        if (GAinst.killCount > GAinst.waveSize)
        {
            GAinst.killCount = 0;
            EndRound();
        }
    }
    public void EndRound( bool playerHit = false)
    {
        print("Round over");
        roundActive = false;

        var GAinst = GeneticAlien._instance;
        var alienlist = GAinst.GetAlienList();

        difficulty = GAinst.FindDifficulty();

        for (int i = 0; i < alienlist.Count; i++)
        {
            alienlist[i].GetComponent<GeneticAlien.Alien>().instance.GetComponent<AlienController>().killed = false;
        }
        GAinst.ga.CreateNewGeneration();
        for (int i = 0; i < GAinst.GetAlienList().Count; i++)
        {
            alienlist[i].GetComponent<GeneticAlien.Alien>().movesUsed = 0;
            alienlist[i].GetComponent<GeneticAlien.Alien>().dna = GAinst.ga.population[i];
        }
        GAinst.ResetPlayerAI();
        currentRound++;
    }
}
