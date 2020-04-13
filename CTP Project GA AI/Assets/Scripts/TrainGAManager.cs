using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Unity;
using UnityEditor;

[System.Serializable]
public class SaveData
{
    public int generation;
    public GeneticAlgorithm gaData;
    public List<char> rawGAData = new List<char>();
    public List<char> rawGABackupData = new List<char>();
    
    public GeneticAlgorithm gaBData;
    public DNA gaBDNA;
}

public class TrainGAManager : MonoBehaviour
{
    public bool Load = false;
    public static TrainGAManager instance;
    [SerializeField] public GameObject newAlgorithm;
    [SerializeField] private bool saveTrigger;
    public List<char>[] container = new List<char>[2];

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }
    // Update is called once per frame
    void Update()
    {
        if (saveTrigger)
        {
            SaveGAData();
        }
    }

    private void SaveGAData()
    {
        var _dataSnapshot = GeneticAlien._instance;

        SaveData save = new SaveData();
        save.generation = _dataSnapshot.ga.generation;

        save.gaData = _dataSnapshot.ga;
        for (int i = 0; i < _dataSnapshot.ga.population.Count; i++)
        {
            foreach (var gene in _dataSnapshot.ga.population[i].genes)
            {
                save.rawGAData.Add(gene[0]);
                save.rawGAData.Add(gene[1]);
            }
        }
        for (int i = 0; i < _dataSnapshot.gaB.population.Count; i++)
        {
            foreach (var gene in _dataSnapshot.gaB.population[i].genes)
            {
                save.rawGABackupData.Add(gene[0]);
                save.rawGABackupData.Add(gene[1]);
            }
        }
        string filepath = Application.dataPath + "GAData.json";
        string data = JsonUtility.ToJson(save, true);
        File.WriteAllText(filepath, data);

        print(data);
        saveTrigger = false;
    }
}


