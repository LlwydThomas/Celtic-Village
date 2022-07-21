using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmingData : ScriptableObject {
    public string uniqueType;

    public int ID;
    public Sprite seedling, growing, mature;
    public ResourceData outputResource;
    public int growthRate;
    public float destructionSpeedFactor;

    public bool[] growthSeasons = new bool[4];

}