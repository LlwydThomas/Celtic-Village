using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public static class BuildingFunctions {
    // Start is called before the first frame update
    public static List<RequiredResources> DetermineStartingInventory(Dictionary<int, ResourceData> resDic, Difficulty difficulty) {
        // Based on the set difficulty, determine which resources are provided at the start of a new game.
        List<RequiredResources> returnList = new List<RequiredResources>();
        switch (difficulty) {
            case Difficulty.Easy:
                returnList.Add(new RequiredResources(resDic[1], 80));
                returnList.Add(new RequiredResources(resDic[1], 80));
                returnList.Add(new RequiredResources(resDic[1], 80));
                returnList.Add(new RequiredResources(resDic[1], 80));
                break;
            case Difficulty.Medium:
                returnList.Add(new RequiredResources(resDic[1], 80));
                returnList.Add(new RequiredResources(resDic[1], 80));
                returnList.Add(new RequiredResources(resDic[1], 80));
                break;
            case Difficulty.Hard:
                returnList.Add(new RequiredResources(resDic[1], 80));
                returnList.Add(new RequiredResources(resDic[1], 80));
                returnList.Add(new RequiredResources(resDic[1], 80));
                break;
        }
        return returnList;
    }

    public static void AppendHeaderToolTip(string header, string text, GameObject game) {
        // Add a tooltip component and set its header and text.
        ToolTipHeaderView toolTip = game.AddComponent<ToolTipHeaderView>();
        toolTip.SetTooltipData(header, text, 2, null);
    }

    public static List<FloraData> FlorasOfQuality(List<FloraData> floras, QualityTier qualityTier) {
        // Find all floras of a certain quality tier from a given list of floras.
        List<QualityTier> possibleQualities = LessOrEqualToQualityTier(qualityTier);
        return floras.FindAll(x => possibleQualities.Contains(x.qualityTier));
    }

    public static List<CraftingData> CraftingDatasOfQuality(List<CraftingData> craftingDatas, QualityTier qualityTier) {
        // Find all crafting recipes of a certain quality tier from a given list of crafting recipes.
        List<QualityTier> possibleQualities = LessOrEqualToQualityTier(qualityTier);
        Debug.Log("BF - Total available crafts: " + craftingDatas.Count + ", QualityTier: " + qualityTier.ToString() + ", possibleQualities: " + possibleQualities.Count);
        return craftingDatas.FindAll(x => possibleQualities.Contains(x.qualityTier));
    }

    public static List<FishData> FishOfQuality(List<FishData> fish, QualityTier qualityTier) {
        // Find all fish of a certain quality tier from a given list of fish.
        List<QualityTier> possibleQualities = LessOrEqualToQualityTier(qualityTier);
        return fish.FindAll(x => possibleQualities.Contains(x.qualityTier));
    }

    public static List<QualityTier> LessOrEqualToQualityTier(QualityTier qualityTier) {
        // Return all quality tiers that are less than or equal to a given tier.
        int index = System.Array.IndexOf(System.Enum.GetValues(qualityTier.GetType()), qualityTier);
        List<QualityTier> qualityTiers = new List<QualityTier>();
        for (int i = 0; i <= index; i++) {
            QualityTier quality = (QualityTier) System.Enum.GetValues(qualityTier.GetType()).GetValue(i);
            qualityTiers.Add(quality);
        }
        return qualityTiers;
    }

    public static void BuildingNodeCalculator(Build build, Dictionary<Vector2, Node> nodeBank, float cellSize) {
        // Given a build, determine which nodes are contained within its available, internal and worker node lists.
        PurposeData purposeData = build.purposeData;
        BuildingReferences buildRefs = build.buildingReferences;
        // Determine which nodes within the buildings radius are available to the build.
        int radiusSize = Mathf.FloorToInt(build.structureData.radiusMultiplier * purposeData.radius);
        build.availableNodes = GridFunctions.NodesWithinRadius(nodeBank, cellSize, build.worldPosition, radiusSize);

        if (buildRefs.internalNodeRectTransform != null) build.internalNodes = GridFunctions.FindNodesFromRect(buildRefs.internalNodeRectTransform, cellSize, nodeBank);

        if (build.buildingReferences.workerNodeTransform != null) {
            build.workerNodes = GridFunctions.FindNodesFromRect(buildRefs.workerNodeTransform, cellSize, nodeBank);
        }

        Debug.Log("BF - Total internal nodes: " + build.internalNodes.Count + ", Total available Nodes: " + build.availableNodes.Count + ", Total Worker nodes: " + build.workerNodes.Count);
    }
}