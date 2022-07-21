using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceData : ScriptableObject {
    public int ID;
    public string resourceName;
    public int perishableTimer;
    public bool tradeable;
    public float resourceValue;
    public string description;
    public float weightPerItem;
    public int hungerRegeneration;
    public Sprite icon;
    public Color iconColour = Color.white;
    public int craftingCategoryQuality = 1;
    public category categoryType;
    public SubCategory subCategory;

    public int maxTraderCapacity = 50;
    [System.Serializable]
    public enum category {
        Null,
        Food,
        Material,
        Trading
    }

    [System.Serializable]
    public enum SubCategory {
        Fish,
        Berry,
        Seed,
        Vegetable,
        Grain,
        Meat,
        Building,
        Rock,
        Metal,
        Animal,
        CookedFood,
        Tool,
        Art
    }
}