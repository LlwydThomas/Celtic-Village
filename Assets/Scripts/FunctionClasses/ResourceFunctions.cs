using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ResourceFunctions {
    // Start is called before the first frame update
    public static int DetermineCraftingQuality(List<RequiredResources> inputs) {
        int totalCraftingQuality = 0;
        foreach (RequiredResources required in inputs) {
            totalCraftingQuality += required.resource.craftingCategoryQuality;
        }

        return Mathf.FloorToInt((totalCraftingQuality / inputs.Count));
    }

    public static List<ResourceData> RandomResourceDatas(int count, List<ResourceData> possibleResources, bool repeatAllowed = false) {
        if ((!repeatAllowed && count > possibleResources.Count) || possibleResources.Count < 1) return null;
        possibleResources = GeneralFunctions.FisherYatesShuffle<ResourceData>(possibleResources);
        List<ResourceData> returnList = new List<ResourceData>();
        while (returnList.Count < count) {
            ResourceData resource = possibleResources[Random.Range(0, possibleResources.Count)];
            if (!repeatAllowed && returnList.Contains(resource)) { } else {
                returnList.Add(resource);
            }
        }
        return returnList;
    }

    public static InstantiatedResource CompareHungerValues(List<InstantiatedResource> inputs) {
        inputs = inputs.OrderByDescending(e => e.resourceData.hungerRegeneration).ToList<InstantiatedResource>();
        if (inputs.Count > 0) return inputs[0];
        else return null;
    }

    public static int DetermineTaxValue(int totalBuildingValue, int totalWealth, Difficulty difficulty) {
        int value = 20 * totalBuildingValue;
        switch (difficulty) {
            case Difficulty.Easy:
                value = Mathf.RoundToInt((float) value / 2f);
                break;
            case Difficulty.Hard:
                value = Mathf.RoundToInt((float) value * 2f);
                break;
            default:
                break;
        }
        value = Mathf.Clamp(value, 0, 10000);
        return value;
    }
}