using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public class StructureData : ScriptableObject {
    public int ID;
    public string structureName, structureDescription;
    public GameObject ghostPrefab, loadingPrefab, completedStructure;
    public List<RequiredResources> requiredRes;
    //public List<ResourceData> RequiredResources;
    //public List<int> associatedCounts;
    public List<PurposeData> possiblePurposes;
    public QualityTier qualityTier;
    public RequiredTiles[] requiredTiles;
    public Vector2 offset = Vector2.zero;
    public int maxWorkers;
    public int maxCreations;
    public int maxCropCount;
    public float radiusMultiplier;
    public Sprite buildingSprite;
    public Sprite buildingIcon;
    public float inventoryStorageMax = -1;
    public int buildingWeight;
    public int maxResourceExtraction;
}

[System.Serializable]
public class RequiredResources {

    public ResourceData resource;
    public int count;

    public RequiredResources(ResourceData _resourceData, int _count) {
        resource = _resourceData;
        count = _count;
    }
}

[System.Serializable]
public class ProbableRequiredResource {

    public RequiredResources requiredResource;
    public float probability;

    public ProbableRequiredResource(RequiredResources _resourceData, float _probability) {
        requiredResource = _resourceData;
        probability = _probability;
    }
}

[System.Serializable]
public class RequiredTiles {
    public int count;
    public List<TileBase> allowedTiles;
}

public enum QualityTier {
    I,
    II,
    III,
}