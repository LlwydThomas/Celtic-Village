using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public class FloraData : ScriptableObject {
    public string uniqueType;
    public int ID;
    public float destructionSpeedFactor;
    public List<ProbableRequiredResource> outputResources;
    public List<RequiredResources> requiredToGrow = new List<RequiredResources>();
    public bool growable;
    public Vector2 prefabOffset = Vector2.zero;
    public Sprite icon;
    public GameObject seedlingPrefab, growingPrefab, maturePrefab;
    public TileBase[] allowedTiles;
    public float spreadChance = 0;
    public int[] spreadCount = new int[2] { 1, 3 };
    public bool[] growthSeasons = new bool[4];
    public int baseYield;
    public Vector2Int size;
    public float daysToMature;
    public category floraCategory;
    public QualityTier qualityTier;

    [System.Serializable]
    public enum category {
        Tree,
        Herb,
        Bush,
        Crop,
        All
    }

}