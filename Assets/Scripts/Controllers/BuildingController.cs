using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingController : MonoBehaviour {
    /* Variable Definitions*/

    // Start is called before the first frame update

    // Scriptable object list references.
    public StructureDataList structureDataListObject;
    public PurposeDataList purposeDataListObject;

    public FunctionHandler generalFunctionHandler;

    public GameObject functionHandlerPrefab;
    public ManagerReferences managerReferences;
    private ControllerManager controllerManager;
    public BuildingModel buildingModel;
    public GameObject ghostParent;
    public ModelManager modelManager;
    public GameObject buildingParent;
    void Start() {
        buildingModel = modelManager.buildingModel;
        controllerManager = managerReferences.controllerManager;
        InitialiseScriptableObjects();
        IntitialiseCount();
        ResetBuildList();
    }

    private void InitialiseScriptableObjects() {
        // Populate the building model's storage of strucute, purpose and task data.
        buildingModel.structureDatas = structureDataListObject.structureDatas;
        buildingModel.purposeDatas = purposeDataListObject.purposeDatas;

        foreach (StructureData structure in buildingModel.structureDatas) {
            buildingModel.structureDataLookUp.Add(structure.ID, structure);
        }
        foreach (PurposeData purposeData in buildingModel.purposeDatas) {
            buildingModel.purposeDataLookUp.Add(purposeData.ID, purposeData);
        }

    }

    // This needs a structure data parameter;
    public Build InitialiseNewBuild(GameObject gameObject, int structureDataID, int purposeDataID, int functionHandlerID = -1, int id = -1) {
        // Create a new build object using the Game Object provided.
        if (id == -1) id = FindAvailableID();
        Build newBuild = new Build(gameObject.name, gameObject.transform.position, gameObject, buildingModel.structureDataLookUp[structureDataID], buildingModel.purposeDataLookUp[purposeDataID], id);
        BuildingReferences buildingReferences;
        if (newBuild.buildingGameObject.TryGetComponent<BuildingReferences>(out buildingReferences)) {
            newBuild.buildingReferences = buildingReferences;
        }
        RectTransform rect = newBuild.buildingGameObject.GetComponent<RectTransform>();
        Vector3 centre = new Vector3(gameObject.transform.position.x + rect.rect.width / 2, gameObject.transform.position.y + rect.rect.height / 2);
        newBuild.centre = centre;

        // Change the structure count and initialise the build's purpose.
        buildingModel.buildingLookupByID.Add(id, newBuild);
        buildingModel.buildingIDs.Add(id);

        IncrementStructureCount(newBuild.structureData, 1);
        //Instantiate(Resources.Load("MarkerGreen"), gameObject.transform.position, Quaternion.identity);
        string uniqueName = newBuild.structureData.structureName + id;
        newBuild.buildingGameObject.name = uniqueName;
        newBuild.uniqueName = uniqueName;
        controllerManager.gridController.RegenerateSection(gameObject.transform.position, 10);
        AmendPurpose(newBuild, newBuild.purposeData.ID, functionHandlerID);
        // Add the new build to the model's data.
        buildingModel.buildingList.Add(newBuild);
        buildingModel.gameObjectToBuild.Add(gameObject, newBuild);
        return newBuild;
    }

    public void NewGameBuildingCreation(Difficulty difficulty) {
        List<Node> placeNodes = controllerManager.gridController.FindWalkableSquare(Vector3.zero, true, 1, 5, 5);
        GameObject buildObj = GameObject.Instantiate(buildingModel.structureDataLookUp[1].completedStructure, placeNodes[0].worldPosition, Quaternion.identity, buildingParent.transform);
        Build build = InitialiseNewBuild(buildObj, 1, 6);
        Dictionary<int, ResourceData> resDic = modelManager.resourceModel.resourceDataLookup;

        List<RequiredResources> resources = controllerManager.settingsController.FindDifficultySettings(difficulty).startingInventoryItems;
        if (build.storageContainer != null) {
            foreach (RequiredResources resources1 in resources) {
                StorageFunctions.TryAmendStorage(build.storageContainer, resources1.resource, resources1.count, false);
            }
        }
    }

    public GameObject CreateBuildFromSave(Build build) {
        // Instantiate a build from a build object in a save file.
        return GameObject.Instantiate(buildingModel.structureDataLookUp[build.structureDataID].completedStructure, build.worldPosition, Quaternion.identity, buildingParent.transform);
    }

    public Build LookupBuildFromID(int id) {
        //Debug.Log("Builds Registered " + buildingModel.buildingIDs.Count);
        if (buildingModel.buildingIDs.Contains(id)) return buildingModel.buildingLookupByID[id];
        else return null;
    }

    public void RemoveGameObjectOwnership(Build build) {
        foreach (TaskGroup task in build.functionHandler.TaskGroupRetun()) {
            if (CheckGameObjectOwnership(task.associatedGameObject)) {
                //Debug.Log("This has Passed");
                buildingModel.gameObjectOwnership.Remove(task.associatedGameObject);
            }
        }
        List<GameObject> relObjList = new List<GameObject>(build.functionHandler.relevantGameObjects);
        foreach (GameObject gameObject in relObjList.ToArray()) {
            if (CheckGameObjectOwnership(gameObject)) {
                build.functionHandler.relevantGameObjects.Remove(gameObject);
                buildingModel.gameObjectOwnership.Remove(gameObject);
            }
        }

        EventController.TriggerEvent("recalculateBuildingObjects");
    }

    public void DeleteBuild(Build build) {
        //Debug.Log("Task Items: " + build.functionHandler.TaskGroupRetun().Count + ", Free object: " + build.gameObjectsInRadius.Count);

        // Remove ownership of all current task groups' game objects and make available.
        if (build.functionHandler != null) ResetAndReleaseHandler(build, true);

        if (build.storageContainer != null) controllerManager.storageController.DeregisterStorage(build.storageContainer);

        if (build.structureData.requiredTiles.Length > 0) {
            foreach (Node node in build.workerNodes) node.walkableOverride = false;
        }

        // Remove references to the build and its objects, and amend the structure count.
        buildingModel.buildingList.Remove(build);
        buildingModel.gameObjectToBuild.Remove(build.buildingGameObject);
        IncrementStructureCount(build.structureData, -1);
        // Recalculate the nodes within a radius of the build and finally destroy the game object.
        Vector3 regenerationPosition = build.buildingGameObject.transform.position;
        Destroy(build.buildingGameObject);
        controllerManager.gridController.RegenerateSection(regenerationPosition, 10);

    }

    public void ReleaseAllPawns(FunctionHandler functionHandler) {
        foreach (Pawn pawn in controllerManager.skillsController.PawnListReturn(false)) {
            if (pawn.functionHandlerID == functionHandler.id) {
                pawn.pawnController.ReassignFunctionHandler(generalFunctionHandler);
            }
        }
    }

    private void ResetAndReleaseHandler(Build build, bool delete) {
        if (build.functionHandler != null) {
            controllerManager.farmingController.ReleaseFarmFromHandler(build.functionHandlerID);
            RemoveGameObjectOwnership(build);
            // Remove all currently assigned pawns to the general handler.
            ReleaseAllPawns(build.functionHandler);
            if (delete) RemoveFunctionHandler(build.functionHandler);
        }
    }

    public int FindAvailableID() {
        int id = Random.Range(1, 10000);
        while (buildingModel.buildingIDs.Contains(id)) {
            id = Random.Range(1, 10000);
        }
        return id;
    }

    public void RemoveBuildIDs(int id = -1) {
        if (id == -1) {
            buildingModel.buildingIDs.Clear();
            buildingModel.buildingLookupByID.Clear();
        } else {
            buildingModel.buildingLookupByID.Remove(id);
            buildingModel.buildingIDs.Remove(id);
        }
    }

    public void RemoveFunctionHandlerIDs(int id = -1) {
        if (id == -1) {
            buildingModel.functionHandlerIDs.Clear();
            buildingModel.functionHandlerLookup.Clear();
        } else {
            buildingModel.functionHandlerIDs.Remove(id);
            buildingModel.functionHandlerLookup.Remove(id);
        }
    }

    public void AmendPurpose(Build build, int purposeDataID, int functionHandlerID = -1) {
        //if (build.purposeData != null && build.purposeData.ID == purposeDataID) return;
        PurposeData purposeData = PurposeDataLookUp(purposeDataID);
        if (!build.structureData.possiblePurposes.Contains(purposeData)) return;
        build.purposeData = purposeData;
        build.purposeDataID = purposeData.ID;

        Debug.Log("Build " + build.uniqueName + " is assigned:" + purposeData.purposeName);
        if (purposeData.workersRequired) {
            if (build.functionHandler != null) {
                ResetAndReleaseHandler(build, false);
                PrepareBuildingFunctionHandler(build, purposeData, functionHandlerID);
            } else {
                build.functionHandlerObject = (GameObject) Instantiate(functionHandlerPrefab, build.buildingGameObject.transform.position, Quaternion.identity, build.buildingGameObject.transform);
                build.functionHandler = build.functionHandlerObject.GetComponent<FunctionHandler>();
                PrepareBuildingFunctionHandler(build, purposeData, functionHandlerID);
            }
        } else {
            if (build.functionHandler != null) {
                Destroy(build.functionHandlerObject);
                RemoveFunctionHandler(build.functionHandler);
            }
        }
        if (build.buildingReferences.purposeIcon != null) {
            build.buildingReferences.purposeIcon.sprite = purposeData.icon;
            bool active = PlayerPrefs.GetInt("BuildingIconsEnabled", 1) == 1 ? true : false;
            build.buildingReferences.purposeIcon.gameObject.SetActive(active);
        }

        if (purposeData.associatedStorage) {
            if (build.storageContainer == null) {
                StorageContainer storage = new StorageContainer(build.structureData.inventoryStorageMax, true);
                build.storageContainer = storage;
                controllerManager.storageController.RegisterStorage(storage, locationObject : build.buildingGameObject);
                /* controllerManager.storageController.TryAmendStorage(storage, controllerManager.resourceController.LookupResourceData(3), 100, false);
                controllerManager.storageController.TryAmendStorage(storage, controllerManager.resourceController.LookupResourceData(6), 50, false); */
            }
        } else {
            if (build.storageContainer != null) controllerManager.storageController.DeregisterStorage(build.storageContainer);
            build.storageContainer = null;
        }

        // Reset the purpose of the function handler, and prepare the handler for inputting into the building model.
        NodeCalculator(build);
    }

    public void ShowOrHidePurposeIcons() {
        buildingModel.purposeIconsShown = !buildingModel.purposeIconsShown;
        foreach (Build build in buildingModel.buildingList) {
            if (build.buildingReferences != null) {
                if (build.buildingReferences.purposeIcon != null) {
                    build.buildingReferences.purposeIcon.gameObject.SetActive(buildingModel.purposeIconsShown);
                }
            }
        }
    }
    public void IntitialiseCount() {
        // Reset all building counts within the model.
        buildingModel.structureCount.Clear();
        foreach (StructureData structure in buildingModel.structureDatas) {
            buildingModel.structureCount.Add(structure, 0);
        }
    }

    public void PrepareBuildingFunctionHandler(Build build, PurposeData purposeData, int functionHandlerID) {
        if (functionHandlerID == -1) {
            // If no id is inputted, find a random id that isn't currently taken and input it into the building model.
            functionHandlerID = Random.Range(0, 10000);
            while (buildingModel.functionHandlerLookup.ContainsKey(functionHandlerID)) functionHandlerID = Random.Range(0, 10000);
        }
        // Register any new function handler.
        if (!buildingModel.functionHandlerIDs.Contains(functionHandlerID)) {
            buildingModel.functionHandlerIDs.Add(functionHandlerID);
            buildingModel.functionHandlerLookup.Add(functionHandlerID, build.functionHandler);
        }
        // Add the new ID to the specific build object, and format it.
        build.functionHandlerID = functionHandlerID;
        build.functionHandler.id = build.functionHandlerID;
        build.functionHandlerObject.name = purposeData.purposeName + build.functionHandlerID;

        // Clear the current associated game objects, and repopulate the list.
        build.functionHandler.RemoveGameObjectFromList();
        build.functionHandler.SetBuild(build);
    }

    private void NodeCalculator(Build build) {
        GridModel gridModel = modelManager.gridModel;
        BuildingFunctions.BuildingNodeCalculator(build, gridModel.nodeBank, gridModel.cellSize);
        if (build.structureData.requiredTiles.Length > 0) {
            UnityEngine.Tilemaps.TileBase waterTile = controllerManager.mapController.tileArray[3];
            foreach (Node workerNode in build.workerNodes) {
                workerNode.walkable = true;
                if (workerNode.associatedTile == waterTile) workerNode.walkableOverride = true;
            }
        }
        if (build.purposeData.ID == 7) {
            buildingModel.sleepingNodes.AddRange(build.internalNodes);
        } else {
            foreach (Node node in build.internalNodes) {
                if (buildingModel.sleepingNodes.Contains(node)) buildingModel.sleepingNodes.Remove(node);
                if (buildingModel.assignedSleepingNodes.ContainsKey(node)) {
                    buildingModel.assignedSleepingNodes[node].sleepingNodeID = -1;
                    buildingModel.assignedSleepingNodes[node].sleepingNode = null;
                    buildingModel.assignedSleepingNodes.Remove(node);
                }
            }
        }
    }

    public void DetermineIfHandlerNeeded(Build build, PurposeData purposeData) {
        bool required;
        switch (build.purposeDataID) {
            case 1:
            case 2:
                required = false;
                break;
            default:
                required = true;
                break;
        }

        if (required) PrepareBuildingFunctionHandler(build, build.purposeData, build.functionHandlerID);
    }

    public void PrepareGeneralFunctionHandler(FunctionHandler functionHandler) {
        // Setup the general function handler, and assign it the ID of 1, this is the default handler and will provide general tasks.
        functionHandler.relatedBuild = null;
        functionHandler.id = 1;
        buildingModel.functionHandlerIDs.Add(1);
        buildingModel.functionHandlerLookup.Add(1, functionHandler);
        functionHandler.SetAllowedTasks(modelManager.taskModel.taskDatas.FindAll(x => x.manualCommandAvailable));
    }

    public Node LocateFreeSleepingNode(Pawn pawn) {
        foreach (Node node in buildingModel.sleepingNodes) {
            if (!buildingModel.assignedSleepingNodes.ContainsKey(node)) {
                pawn.sleepingNodeID = node.id;
                buildingModel.assignedSleepingNodes.Add(node, pawn);
                return node;
            }
        }
        Debug.Log("Unable to find a sufficient sleeping node for pawn" + pawn.id);
        return null;
    }

    public void ToggleIconVisibility(int active) {
        bool activeBool = active == 1 ? true : false;
        foreach (Build build in buildingModel.buildingList) {
            BuildingReferences references = build.buildingReferences;
            if (references != null) {
                if (references.purposeIcon != null) {
                    references.purposeIcon.gameObject.SetActive(activeBool);
                }
            }
        }
    }

    private void RemoveFunctionHandler(FunctionHandler functionHandler) {
        if (buildingModel.functionHandlerIDs.Contains(functionHandler.id)) {
            buildingModel.functionHandlerIDs.Remove(functionHandler.id);
            buildingModel.functionHandlerLookup.Remove(functionHandler.id);
        }
    }
    public Build RequestBuildFromGameObject(GameObject gameObject) {
        Build build = null;
        if (buildingModel.gameObjectToBuild.ContainsKey(gameObject)) build = buildingModel.gameObjectToBuild[gameObject];
        //Debug.Log("Build Name: " + build.uniqueName + ", Count: " + build.gameObjectsInRadius.Count);
        return build;
    }

    public List<Build> ReturnBuildList() {
        return buildingModel.buildingList;
    }

    public void AssignGameObject(GameObject gameObject, FunctionHandler functionHandler) {
        if (functionHandler != null) {
            functionHandler.relevantGameObjects.Add(gameObject);
            buildingModel.gameObjectOwnership.Add(gameObject, functionHandler);
            Debug.Log("BC - " + gameObject.name + " assigned to funchand" + functionHandler.id + ", game object queue for func is now: " + functionHandler.relevantGameObjects.Count);
            FloraItem flora = controllerManager.natureController.GameObjectToFloraItem(gameObject);
            if (flora != null) flora.relFuncHandID = functionHandler.id;
        }

    }

    public bool CheckGameObjectOwnership(GameObject gameObject) {
        /* Debug.Log(gameObject.name + " is owned: " +
            buildingModel.gameObjectOwnership.ContainsKey(gameObject) + ", Total Objs: " + buildingModel.gameObjectOwnership.Count); */
        if (gameObject != null) return buildingModel.gameObjectOwnership.ContainsKey(gameObject);
        else return false;
    }

    public void ResetBuildList(List<Build> _newBuildList = null) {
        foreach (Transform x in ghostParent.transform) {
            Destroy(x.gameObject);
        }
        RemoveBuildIDs();
        RemoveFunctionHandlerIDs();
        PrepareGeneralFunctionHandler(generalFunctionHandler);
        IntitialiseCount();
        foreach (Build build in buildingModel.buildingList) DestroyImmediate(build.buildingGameObject);
        buildingModel.buildingList.Clear();
        buildingModel.gameObjectToBuild.Clear();
        if (_newBuildList != null) {
            // If a list of builds is provided, populate the building model with these builds.
            foreach (Build build in _newBuildList) {
                build.buildingGameObject = CreateBuildFromSave(build);
                Build newBuild = InitialiseNewBuild(build.buildingGameObject, build.structureDataID, build.purposeDataID, id : build.id, functionHandlerID : build.functionHandlerID);
                if (newBuild.storageContainer != null) {
                    newBuild.storageContainer.inventory = null;
                    Debug.Log("BUIC - Found storage container on build, with an inventory count of " + build.storageContainer.inventory.Count);
                    controllerManager.storageController.RegisterStorage(newBuild.storageContainer, build.storageContainer.inventory, build.buildingGameObject);
                } else Debug.Log("BUIC - Storage container is null for save game build.");
            }
        }
        int iconsOn = PlayerPrefs.GetInt("BuildingIconsEnabled", 1);
        ToggleIconVisibility(iconsOn);
    }

    public FunctionHandler FindFuncHandlerByID(int id) {
        if (buildingModel.functionHandlerIDs.Contains(id)) return buildingModel.functionHandlerLookup[id];
        else return null;
    }

    public PurposeData PurposeDataLookUp(int id) {
        return buildingModel.purposeDataLookUp[id];
    }

    public StructureData StructureDataLookUp(int id) {
        return buildingModel.structureDataLookUp[id];
    }
    public void IncrementStructureCount(StructureData structure, int value) {
        buildingModel.structureCount[structure] += value;
    }
    public void SetStructureCount(StructureData structure, int value) {
        buildingModel.structureCount[structure] = value;
    }

    public int GetStructureCount(StructureData structure) {
        return buildingModel.structureCount[structure];
    }
}