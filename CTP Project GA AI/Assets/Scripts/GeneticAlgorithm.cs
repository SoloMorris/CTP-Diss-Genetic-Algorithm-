using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Tutorial followed : https://www.youtube.com/watch?v=G8KJWONEeGo

[System.Serializable]
public class GeneticAlgorithm
{
    public List<DNA> population = new List<DNA>();
    List<DNA> newPopulation = new List<DNA>();

    public int generation;
    public float mutationRate;
    private System.Random random;
    private float totalFitness = 0;
    public float bestFitness;
    public char[][] bestGenes;
    public int Elitism;


    //  Constructor
    public GeneticAlgorithm(int _populationSize, int _dnaSize, System.Random _random, Func<char[]> _getRandomGene, Func<int, float> _fitnessFunction,
        int elitism, float _mutationRate = 0.01f)
    {
        generation = 1;
        Elitism = elitism;
        mutationRate = _mutationRate;

        random = _random;

        bestGenes = new char[_dnaSize][];
        for (int i = 0; i < _populationSize; i++)
        {
            population.Add(new DNA(_dnaSize, _getRandomGene, _fitnessFunction, null));
        }
    }

    //Populate a new generation with offspring of two parents
    public void CreateNewGeneration()
    {
        if (population.Count <= 0)
        {
            return;
        }

        CalculateFitness();
        population.Sort(CompareDNA);
        newPopulation.Clear();

        for(int i = 0; i < population.Count; i++)
        {
            if (i < Elitism)
            {
                newPopulation.Add(population[i]);
            }
            else
            {
                DNA parent1 = ChooseParent();
                DNA parent2 = ChooseParent();
                DNA child = parent1.Crossover(parent2);

                child.Mutate(mutationRate);
                newPopulation.Add(child);
            }
        }

        List<DNA> tempList = population;
        population = newPopulation;
        newPopulation = tempList;
        generation++;
    }

    public int CompareDNA(DNA a, DNA b)
    {
        if (a.Fitness > b.Fitness)
        {
            return -1;
        }
        else if (a.Fitness < b.Fitness)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }

    //Calculate the fitness of a child.
    public void CalculateFitness()
    {
        totalFitness = 0;
        DNA best = population[0];

        for (int i = 0; i < population.Count; i++)
        {
            totalFitness += population[i].CalculateFitness(i);

            if (population[i].Fitness > best.Fitness)
            {
                best = population[i];
            }
        }

        bestFitness = best.Fitness;
        best.genes.CopyTo(bestGenes, 0);
    }

    private DNA ChooseParent()
    {
        double randomNumber = random.NextDouble() * totalFitness;

        for (int i = 0; i < population.Count; i++)
        {
            if (randomNumber < population[i].Fitness)
            {
                return population[i];
            }
            randomNumber -= population[i].Fitness;

        }
        return null;
    }
}
