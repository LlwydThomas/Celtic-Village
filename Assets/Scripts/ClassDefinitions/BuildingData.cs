using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Build {

    //Object to store individual building information, to be stored by the building model
    private bool debug = true;
    public string type;
    public int id;
    [System.NonSerialized]
    [SerializeField]
    public List<Node> availableNodes = new List<Node>();
    [System.NonSerialized]
    [SerializeField]
    public List<Node> internalNodes = new List<Node>();
    [System.NonSerialized]
    [SerializeField]
    public List<Node> workerNodes = new List<Node>();
    [System.NonSerialized]
    public List<Node> totalNodes = new List<Node>();
    public Vector3 worldPosition, centre;
    public List<CraftingQueueItem> craftingQueueItems;
    public string uniqueName;
    public StructureData structureData;
    public int structureDataID, purposeDataID, currentResourceExtraction = -1;
    public PurposeData purposeData;
    // public List<Farm> farmList = new List<Farm>();
    // public List<GameObject> permanentAssociatedObjs = new List<GameObject>();
    public int functionHandlerID = -1;
    public StorageContainer storageContainer = null;
    // public LinkedList<GameObject> gameObjectsInRadius = new LinkedList<GameObject>();
    public GameObject buildingGameObject, functionHandlerObject;
    public BuildingReferences buildingReferences;
    public FunctionHandler functionHandler;
    public Build(string _uniqueName, Vector3 _worldPos, GameObject _buildObject, StructureData _structureData, PurposeData _purposeData, int _id) {
        worldPosition = _worldPos;
        buildingGameObject = _buildObject;
        uniqueName = _uniqueName;
        purposeData = _purposeData;
        structureData = _structureData;
        purposeDataID = purposeData.ID;
        structureDataID = structureData.ID;
        type = structureData.structureName;
        if (structureData.maxResourceExtraction > 0) currentResourceExtraction = 0;
        id = _id;
    }

    public void SetPurpose(PurposeData purpose) {
        this.purposeData = purpose;
    }
}