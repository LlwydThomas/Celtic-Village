using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BuildingModel {
    public List<Build> buildingList = new List<Build>();
    public Dictionary<StructureData, int> structureCount = new Dictionary<StructureData, int>();
    public Dictionary<GameObject, Build> gameObjectToBuild = new Dictionary<GameObject, Build>();
    public List<Node> sleepingNodes = new List<Node>();
    public Dictionary<Node, Pawn> assignedSleepingNodes = new Dictionary<Node, Pawn>();

    // Scriptable Object storage/lookups
    public List<PurposeData> purposeDatas = new List<PurposeData>();
    public Dictionary<int, PurposeData> purposeDataLookUp = new Dictionary<int, PurposeData>();
    public List<StructureData> structureDatas = new List<StructureData>();
    public Dictionary<int, StructureData> structureDataLookUp = new Dictionary<int, StructureData>();

    public Dictionary<GameObject, FunctionHandler> gameObjectOwnership = new Dictionary<GameObject, FunctionHandler>();
    public List<int> functionHandlerIDs = new List<int>();
    public Dictionary<int, FunctionHandler> functionHandlerLookup = new Dictionary<int, FunctionHandler>();

    public List<int> buildingIDs = new List<int>();
    public Dictionary<int, Build> buildingLookupByID = new Dictionary<int, Build>();

    public bool purposeIconsShown = false;

}