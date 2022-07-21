using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingData : ScriptableObject {
    public int id;
    public string craftingRecipeName;
    public List<ResourceByCategory> categoryInputs;
    public List<RequiredResources> inputs;
    public List<RequiredResources> outputs;
    public QualityOutput[] qualityOutputs = new QualityOutput[3];
    public CraftingCategory craftingCategory;
    public int craftingDuration;
    public QualityTier qualityTier;
    [System.Serializable]
    public class ListClass {
        public List<RequiredResources> requiredResources;
    }

    [System.Serializable]
    public class ResourceByCategory {
        public ResourceData.SubCategory subCategory;
        public int count;
    }

    [System.Serializable]
    public class QualityOutput {
        public int qualityLevel;
        public int count;
    }

    public enum CraftingCategory {
        Cooking,
        Forging,
        Art
    }
}