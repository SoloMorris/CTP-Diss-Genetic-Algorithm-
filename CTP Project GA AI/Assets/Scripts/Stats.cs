using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stats : MonoBehaviour
{
    public static Stats instance;
    public int timesFired;
    public int hits;
    public int misses;
    public float shotAccuracy;
    public float levelTime;

    public int round;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }
    
    void Update()
    {
        round = GeneticAlien._instance.round;
    }

    public void MissedShot()
    {
        misses++;
    }
    public void HitShot()
    {
        hits++;
    }
}
