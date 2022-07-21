using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PurposeData : ScriptableObject {
    public int ID;
    public string purposeName, purposeDescription;
    public SkillData relevantSkill;
    public int radius;
    public List<RadiusRequirements> radiusReqs;
    public List<RequiredResources> requiredResources;
    public List<FloraData> possibleFloraCreations;
    public List<FishData> possibleFish;
    public List<CraftingData> possibleCraftingRecipes;
    public List<TieredProbableResource> possibleMinerals;
    public List<TaskData> permittedTasks;
    public bool associatedStorage;
    public bool workersRequired;
    public bool automaticEmptyTaskCreation;
    public Sprite icon;

    public bool maxCropsApplies, maxCreationsApplies;

    [System.Serializable]
    public struct RadiusRequirements {
        public string tagName;
        public int tagCount;
    }
}

[System.Serializable]
public class TieredProbableResource {

    public ProbableRequiredResource probableRequiredResource;
    public QualityTier resourceTier;

}