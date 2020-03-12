using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundManager : MonoBehaviour
{
    public static RoundManager instance;
    public int currentRound;

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
        
    }

    private void GenerateNewWave()
    {

    }
}
