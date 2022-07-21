using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GeneralEnumStorage {
    public static Difficulty currentDifficulty;
    public static Difficulty CurrentDifficulty {
        get {
            return currentDifficulty;
        }
        set {
            Debug.Log("GES - new difficulty: " + value.ToString());
            currentDifficulty = value;
        }
    }

    public static DifficultySettings currentDifficultySettings;
    public static LayerMask entityLayers;

    public static Color greenGhost = new Color(0, 1, 0, 0.5f);

    public static Color redGhost = new Color(1, 0, 0, 0.5f);

    public static bool debugActive = false;

    public static bool developerOptions = false;
}