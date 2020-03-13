using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundManager : MonoBehaviour
{
    public static RoundManager instance;
    public int currentRound;
    public bool roundActive = false;
    int epochs = 5;
    int roundSet;

    public float difficulty;
    public float deathTimer = 3.0f;
    public float waveTimer = 0.1f;
    private float elapsedTime;
    private float targetTime;
    // Start is called before the first frame update

    private void Awake()
    {
        if (instance == null) instance = this;
    }
    private void LateUpdate()
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
        if (!roundActive)
        {
            elapsedTime += Time.deltaTime;

            if (elapsedTime > targetTime)
            {
                roundActive = true;
                elapsedTime = 0;
                targetTime = 0;
            }
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
        if (roundSet == epochs)
        {
            //Save the current ga as a JSON file.
            roundSet = 0;
        }
        else
            roundSet += 1;
        for (int i = 0; i < GAinst.GetAlienList().Count; i++)
        {
            alienlist[i].GetComponent<GeneticAlien.Alien>().movesUsed = 0;
            alienlist[i].GetComponent<GeneticAlien.Alien>().dna = GAinst.ga.population[i];
        }
        GAinst.ResetPlayerAI();
        currentRound++;
        if (playerHit)
            targetTime = deathTimer;
        else
            targetTime = waveTimer;
    }
}
