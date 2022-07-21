using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Pawn {
    public int id;
    public string name;
    public int age = 21;
    public PawnStatus pawnStatus;
    //public Dictionary<SkillData, float> skillLevels;
    //public Dictionary<SkillData, int> priorities;
    //public List<SkillLevelled> skillsList;
    //public List<TraitData> traitList;
    public StorageContainer storageContainer;
    //link a pawn and its gameObject, and to check if the pawn is initialised.
    public GameObject pawnGameObject;
    public Color[] pawnColours;
    public Vector3 saveLocation;

    public int functionHandlerID = -1;
    public int sleepingNodeID = -1;
    [System.NonSerialized]
    public Node sleepingNode;
    public PawnController pawnController;
    public PawnTaskSystem pawnTaskSystem;

    public Pawn(string _name, int _age, Color[] _colours, PawnStatus _pawnStatus, int _id) {
        id = _id;
        age = _age;
        pawnColours = _colours;
        pawnStatus = _pawnStatus;
        this.name = _name;
        storageContainer = new StorageContainer(150f, false);
    }

}

[System.Serializable]
public class SkillLevelled {
    public int skillDataID;
    public int level;
    public bool refusal;

    public int xpRemaining = 0;
    public int xp = 0;
    public SkillData skillData;
    public SkillLevelled(SkillData _skill, int _level, bool _refusal) {
        this.skillDataID = _skill.ID;
        this.level = _level;
        this.refusal = _refusal;
        this.skillData = _skill;
    }
}

[System.Serializable]
public class PawnStatus {
    public CurrentStatus currentStatus;
    public float tiredModifier;
    public float hungerModifier;

    public int totalHealth = 100;
    public int hungerLevel = 100;
    public int tirednessLevel = 100;
    public int mortalityStage = 0;
    public PawnStatus(float tiredMod, float hungerMod) {
        tiredModifier = tiredMod;
        hungerModifier = hungerMod;
        currentStatus = CurrentStatus.Idle;
    }

    public enum CurrentStatus {
        Working,
        Eating,
        Sleeping,
        Idle,
    }
}