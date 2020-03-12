using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundManager : MonoBehaviour
{
    //public int currentRound;
    //private bool initRound = false;
    //private bool roundActive = false;
    //// Start is called before the first frame update
    //void Start()
    //{
    //    currentRound = 0;
    //}

    //// Update is called once per frame
    //void Update()
    //{
    //    if (!AlienManager._instance.CheckAliensAlive())
    //    {
    //        StartNextRound();
    //        print("Round over! New round is " + (currentRound + 1));
    //    }
            
    //    switch (currentRound)
    //    {
    //        case 0:
    //            AddRowsToAliens(1);
    //            AddAliensToRows(1);
    //            initRound = false;

    //            break;

    //        case 1:
    //            AddAliensToRows(1);
    //            initRound = false;

    //            break;

    //        case 2:
    //            AddRowsToAliens(1);
    //            AddAliensToRows(1);
    //            initRound = false;

    //            break;

    //        case 3:
    //            AddRowsToAliens(1);
    //            initRound = false;

    //            break;
    //    }
    //}

    //private void InitialiseRound()
    //{
    //}

    //private void StartNextRound()
    //{
    //    initRound = true;
    //    currentRound++;
    //    AlienManager._instance.InitAliens();
    //    InitialiseRound();

    //}

    //private void AddRowsToAliens(int newRows)
    //{
    //    if (!initRound)
    //        return;
    //    AlienManager._instance.rowCount += newRows;
    //}

    //private void AddAliensToRows(int newAliens)
    //{
    //    if (!initRound)
    //        return;
    //    AlienManager._instance.rowLength += newAliens;
    //}

}
