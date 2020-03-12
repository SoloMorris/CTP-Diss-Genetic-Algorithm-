using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity;


public class TrainGAManager : MonoBehaviour
{
    [SerializeField] public GameObject newAlgorithm;

    [SerializeField] private bool save;
    // Start is called before the first frame update
    void Start()
    {
        SetUpNewAlgorithm();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SetUpNewAlgorithm()
    {
        if (newAlgorithm == null)
        {
            newAlgorithm = Instantiate(gameObject);
        }
        if (!PrefabUtility.SaveAsPrefabAsset(newAlgorithm, "Assets"))
        {
            Debug.LogWarning("Could not save new Algorithm!");
        }
        else
            Debug.Log("Saved new algorithm successfully!");
    }
}
