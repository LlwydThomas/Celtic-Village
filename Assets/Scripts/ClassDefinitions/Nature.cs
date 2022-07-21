using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class FloraItem {
    public int ID;
    public Vector3 location;
    public string type;
    public FloraData floraData;
    public bool infected = false;
    public int relFuncHandID = -1;
    public int relatedTaskID = -1;
    public int floraDataID;
    public string uniqueName;
    public GameObject gameObject;
    public GameObject prefabLocation;
    public Stage floraStage;
    public float amendedTimeToYield;

    [System.NonSerialized]
    public Farm farm;
    public enum Stage {
        Empty,
        Seedling,
        Growing,
        Mature,
    }
    public float growthPercentage, floraHealth;
    public FloraItem(Vector3 _location, string _uniqueType, FloraData floraData, float growthStage, float plantHealth = 100) {
        this.location = _location;
        this.type = _uniqueType;
        this.floraData = floraData;
        this.floraHealth = plantHealth;
        this.floraDataID = floraData.ID;
        this.growthPercentage = growthStage;
        amendedTimeToYield = floraData.daysToMature;
    }

}

public class PlantedFloraItem {
    public FloraItem floraItem;
    public LoadingBarInfo loadingBarInfo;
    public int loadingBarID;
    public PlantedFloraItem(FloraItem _floraItem, LoadingBarInfo _loadingBarInfo) {
        floraItem = _floraItem;
        loadingBarInfo = _loadingBarInfo;
        loadingBarID = loadingBarInfo.ID;
    }
}