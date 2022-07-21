using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishData : ScriptableObject {
    public int id;
    public string fishName;
    public RequiredResources outputResource;
    public bool[] seasons = new bool[4];
    public float rarity;
    public QualityTier qualityTier;
}