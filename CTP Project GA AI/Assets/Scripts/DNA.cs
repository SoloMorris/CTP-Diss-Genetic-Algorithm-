using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DNA : MonoBehaviour
{
    public List<char[]> genes = new List<char[]>();
    public float Fitness;
    [SerializeField] private int genomeLength;
    private System.Random random;
    private Func<char[]> GetRandomGene;
    private Func<int, float> FitnessFunction;
    public GeneticAlien.Alien Owner;
    
    public DNA(int _genomeLength, Func<char[]> getRandomGene, Func<int, float> fitnessFunction, GeneticAlien.Alien owner, bool initGenes = true)
    {
        GetRandomGene = getRandomGene;
        FitnessFunction = fitnessFunction;
        random = new System.Random();
        Owner = owner;
        if (initGenes)
        {
            genes.Clear();
            for (int i = 0; i < _genomeLength; i++)
            {
                genes.Add(getRandomGene());
            }
        }
    }

    public float CalculateFitness(int index)
    {
        Fitness = FitnessFunction(index);
        return Fitness; // This??
    }

    public DNA Crossover(DNA partner)
    {
        DNA newchild = new DNA(genes.Count, GetRandomGene, FitnessFunction, Owner, false);

        for (int i = 0; i < genes.Count; i++)
        {
            newchild.genes.Add(random.NextDouble() < 0.5f ? genes[i] : partner.genes[i]);
        }
        return newchild;
    }

    public void Mutate(float mutationRate)
    {
        for (int i = 0; i < genes.Count; i++)
        {
            if (random.NextDouble() < mutationRate)
            {
                genes[i] = GetRandomGene();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
