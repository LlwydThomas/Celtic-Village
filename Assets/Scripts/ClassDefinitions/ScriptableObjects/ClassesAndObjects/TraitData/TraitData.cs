using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TraitData : ScriptableObject {
    public int id;
    public string uniqueName;
    public List<ExperienceModifier> ExperienceModifiers;
    public List<MoodModifier> moodModifiers;

    public List<TraitData> incompatibleTraits;

}

[System.Serializable]
public class ExperienceModifier {
    public bool allSkills;
    public SkillData relatedSkill;
    public float modifierValue;
}

[System.Serializable]
public class MoodModifier {
    public string requiredEvent;
    public float modifierValue;
}

[System.Serializable]
public class EventMoodModifier {
    public float modifierValue;
}