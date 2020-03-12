using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlienController : GeneticAlien.Alien
{
    public bool killed; 
    public int uid;
    public Vector3 deathPosition = Vector3.zero;
    // Start is called before the first frame update
    void Start()
    {
        //print("My ID is " + uid);
        instance = GeneticAlien._instance.GetAlienList()[uid].GetComponent<GeneticAlien.Alien>().instance;
    }

    // Update is called once per frame
    void Update()
    {
        alive = GeneticAlien._instance.GetAlienList()[uid].GetComponent<GeneticAlien.Alien>().alive;
        killed = !alive;
    }

}
