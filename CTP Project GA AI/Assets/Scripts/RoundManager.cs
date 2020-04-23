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

    //  Ensure aliens cannot collide with each-other.
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

    //If the right number of aliens are killed, end the round.
    private void MonitorRound()
    {
        var GAinst = GeneticAlien._instance;
        if (GAinst.killCount > (roundLength / 2))
        {
            //GAinst.killCount = 0;
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

    //Kill all aliens and prepare to create a new generation. 
    //If the player was hit, reset them as well.
    public void EndRound( bool playerHit = false)
    {
        print("Round over");
        roundActive = false;
        var GAinst = GeneticAlien._instance;
        var alienlist = GAinst.GetAlienList();
        
        //difficulty = GAinst.FindDifficulty();

        if (roundSet == epochs)
        {
            //Save the current ga as a JSON file.
            roundSet = 0;
        }
        else
            roundSet += 1;

        foreach (var alien  in alienlist)
        {
            if (alien.GetComponent<GeneticAlien.Alien>().occupiedTile != null)
                alien.GetComponent<GeneticAlien.Alien>().occupiedTile.currentTileState = Grid.Tile.TileState.Empty;
            alien.GetComponent<GeneticAlien.Alien>().occupiedTile = null;
        }
        GAinst.SetupRound();
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
