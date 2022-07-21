using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScheduledEvent : ScriptableObject {
    public int ID;
    public string uniqueName;

    public bool confirmationRequired;
    public bool locRequired = true;

    public bool independentEvent;

    [Range(0, 1)]
    public float percentageChance;
    public List<EventCondition> eventConditions;

}

[System.Serializable]
public class EventCondition {

    public int value;
    public ComparisonValue comparisonValue;
    public ConditionIdentifier conditionIdentifier;

    public enum ConditionIdentifier {
        TotalItemWealth,
        TotalPopulation,
        TotalBuildings,
        TotalBuildingWeight
    }

    public EventCondition(ConditionIdentifier _id, int _value) {
        conditionIdentifier = _id;
        value = _value;
    }
}

public enum ComparisonValue {
    Equal,
    Greater,
    Less
}