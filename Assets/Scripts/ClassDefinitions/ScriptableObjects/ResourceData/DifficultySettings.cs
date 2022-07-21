using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DifficultySettings : ScriptableObject {
    public Difficulty difficulty;
    public List<RequiredResources> startingInventoryItems;
    public int healthChangeApplied;
    public int backLogLimit;
}