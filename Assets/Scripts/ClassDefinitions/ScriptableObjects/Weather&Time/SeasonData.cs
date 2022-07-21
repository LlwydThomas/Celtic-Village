using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeasonData : ScriptableObject {
    public int id;
    public int minTemp, maxTemp;

    public string uniqueName;
    public float chanceOfPrecipiation;

    public int morningEnd, eveningStart;
}