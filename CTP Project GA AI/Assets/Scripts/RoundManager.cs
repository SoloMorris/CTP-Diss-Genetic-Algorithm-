using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundManager : MonoBehaviour
{
    public static RoundManager instance;

    //Alien/Wave management
    public int currentRound;
    public bool roundActive = false;
    int epochs = 5;
    [Range(1,100)] public int roundLength;
    int roundSet;

    //Round timers
    public float difficulty;
    public float deathTimer = 3.0f;
    public float waveTimer = 0.1f;
    private float elapsedTime;
    private float targetTime;
    // Start is called before the first frame update

    private void Awake()
    {
        if (instance == null) instance = this;
        Physics2D.IgnoreLayerCollision(8, 8);
        Physics2D.IgnoreLayerCollision(0, 8);
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
        if (GAinst.killCount > (roundLength / 2))
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

        var GAinst = GeneticAlien._instance;
        var alienlist = GAinst.GetAlienList();
        
        difficulty = GAinst.FindDifficulty();

        for (int i = 0; i < alienlist.Count; i++)
        {
            alienlist[i].GetComponent<GeneticAlien.Alien>().instance.GetComponent<AlienController>().killed = false;
        }
        if (roundSet == epochs)
        {
            GAinst.ga.CreateNewGeneration();
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
        {
            roundActive = false;
            targetTime = deathTimer;
        }
        else
        {
            targetTime = waveTimer;
        }
    }
}
