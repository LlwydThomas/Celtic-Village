using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
public class FarmingController : MonoBehaviour {
    public ResourceController resourceTracker;
    public GameObject selectionPanel, cropPatchPrefab, topMapObject, farmMainParent;

    public GameObject farmSquarePrefab;
    public LayerMask floraLayerMask;
    public FunctionHandler generalFunctionHandler;
    public ManagerReferences managerReferences;
    private ControllerManager controllerManager;
    private ModelManager modelManager;
    private FarmingModel farmingModel;
    private Tilemap topMap;
    public FishDataList fishDataList;
    public List<Farm> farmList;
    public GameObject worldSpacePrefab;

    public Sprite axeIcon, plagueIcon, scytheIcon;
    private void Start() {
        // Create reference to a singleton farming model, and begin event listeners.
        modelManager = managerReferences.modelManager;
        controllerManager = managerReferences.controllerManager;
        farmingModel = modelManager.farmingModel;
        farmList = farmingModel.farmList;
        topMap = topMapObject.GetComponent<Tilemap>();
        farmingModel.fishDatas = fishDataList.fishDatas;
        foreach (FishData fish in farmingModel.fishDatas) {
            farmingModel.fishDataLookup.Add(fish.id, fish);
        }
        // Initialise all farming data (crops) from the scriptable object list, to be stored in the model.
    }

    public void ResetFarmList(List<Farm> _newFarmList = null) {
        // Clear all references and objects regarding previous farms.
        foreach (Farm farm in farmingModel.farmList) DestroyImmediate(farm.farmObject);
        farmingModel.farmList.Clear();
        farmingModel.farmGameObjectLookUp.Clear();
        farmingModel.farmLookupByID.Clear();

        if (_newFarmList != null) {
            // If a list of farms is provided, populate the farming model with these farms.
            foreach (Farm farm in _newFarmList) {
                GameObject farmObject = Instantiate(farmSquarePrefab, new Vector3(farm.farmingRect.x, farm.farmingRect.y), Quaternion.identity, farmMainParent.transform);
                farmObject.transform.localScale = new Vector3(farm.farmingRect.width, farm.farmingRect.height);
                RegisterFarm(farmObject, new Rect(farm.farmingRect.x, farm.farmingRect.y, farm.farmingRect.width, farm.farmingRect.height), controllerManager.natureController.FindFloraDataByID(farm.floraDataID), farm.cropList, functionHandlerID : farm.relFuncHandID);
            }
        }

    }

    public void RegisterFarm(GameObject gameObject, Rect rect, FloraData _floraData, List<FloraItem> _cropList = null, int functionHandlerID = -1) {
        gameObject.transform.position = rect.position + new Vector2(rect.width / 2, rect.height / 2);
        // gameObject.transform.localScale = new Vector3(rect.width, rect.height);
        // Create & name empty gameobjects to hold the newly created farm
        Debug.Log("Farm at " + rect.x + ", " + rect.y + " registered.");
        GameObject farmParent = new GameObject();
        GameObject cropTileParent = new GameObject();
        cropTileParent.name = "CropTileParent";
        farmParent.name = "Farm" + farmingModel.farmList.Count;
        farmParent.tag = "Farm";
        BoxCollider2D farmCollider = gameObject.AddComponent<BoxCollider2D>();
        farmCollider.isTrigger = true;
        farmCollider.tag = "Farm";
        // Amend the structure to organise the child objects.
        farmParent.transform.SetParent(farmMainParent.transform);
        cropTileParent.transform.SetParent(farmParent.transform);
        gameObject.transform.SetParent(farmParent.transform);
        FunctionHandler farmHandler = controllerManager.buildingController.FindFuncHandlerByID(functionHandlerID);

        // Create a new farm object based on the inputted variables.
        //Debug.Log(farmingModel.farmingDatas.Count);
        Farm newFarm = new Farm(gameObject, cropTileParent, FindAvailableID(), _floraData, rect);
        newFarm.floraDataID = _floraData.ID;
        // For each tile in the farm rect, create a new crop patch class object and game object to store and display growth info.
        List<Node> cropNodes = controllerManager.gridController.FindNodesFromRect(newFarm.farmingRect);
        foreach (Node node in cropNodes) {
            CropTile tile = new CropTile(node, false, null);
            newFarm.cropTiles.Add(tile);
        }

        if (_cropList != null) {
            foreach (FloraItem crop in _cropList) {
                if (crop.floraStage != FloraItem.Stage.Empty) {
                    Vector3 offset = new Vector3(0f, 0.5f);
                    CropTile cropTile = newFarm.SearchCropTileListWithLocation(crop.location + offset);
                    if (cropTile.occupied && cropTile.relatedFlora != null) continue;
                    FloraItem newCrop = controllerManager.natureController.InstantiateFloraData(crop.floraDataID, crop.location, crop.growthPercentage, parent : cropTileParent.transform, offset : false);
                    newFarm.cropList.Add(newCrop);
                    if (cropTile != null) {
                        cropTile.SetCropTileValues(true, false, newCrop);
                    }
                    newCrop.farm = newFarm;
                    newCrop.relFuncHandID = functionHandlerID;
                }
            }
        }
        // If a build is associated with the farm, assign the id and the function handler.
        if (farmHandler != null) {
            newFarm.relFuncHandID = functionHandlerID;
            SetFunctionHandler(newFarm, farmHandler);
        }
        AmendFarmOutput(newFarm, _floraData);
        farmingModel.farmList.Add(newFarm);
        farmingModel.farmGameObjectLookUp.Add(gameObject, newFarm);
        farmingModel.farmLookupByID.Add(newFarm.ID, newFarm);
        float scale = rect.width > rect.height ? rect.width : rect.height;
        controllerManager.gridController.RegenerateSection(rect.center, Mathf.CeilToInt(scale * 2));
        Debug.Log("FARMC - Farm " + newFarm.ID + " has " + newFarm.cropList.Count + " crops, and " + newFarm.cropTiles.Count + " total crop tiles.");

        //farmingModel.farmList[0].farmObject.GetComponent<SpriteRenderer>().color = Color.red;
    }

    public List<Farm> FindFarmsFromFuncHandler(int funcHandlerID) {
        FunctionHandler farmHandler = controllerManager.buildingController.FindFuncHandlerByID(funcHandlerID);
        List<Farm> farmList = farmingModel.farmList.FindAll(x => x.farmingHandler == farmHandler);
        return farmList;

    }

    private int FindAvailableID() {
        int id = Random.Range(1, 99999);
        int whileCount = 0;
        while (farmingModel.farmList.Find(x => x.ID == id) != null) {
            id = Random.Range(1, 99999);
            whileCount++;
            if (whileCount > 1000) break;
        }
        return id;
    }

    public void ReleaseFarmFromHandler(int functionHandlerID) {
        if (functionHandlerID == -1) return;
        List<Farm> farmList = FindFarmsFromFuncHandler(functionHandlerID);
        foreach (Farm farm in farmList) {
            farm.relFuncHandID = -1;
            farm.farmingHandler = null;
            farm.floraData = null;
            farm.floraDataID = -1;
            foreach (CropTile crop in farm.cropTiles.FindAll(x => x.occupied && x.relatedFlora != null)) {
                controllerManager.natureController.DestroyFloraItem(crop.relatedFlora);
                crop.SetCropTileValues(false, false, null);
            }
        }
    }

    public void ProcessCropFailure() {
        float randomRoll = Random.Range(0f, 1f);
        if (randomRoll > 0.5f) {
            // Based on the number of farms present, determine how many farms are effected.
            int farmsEffected = 1;
            float farmRoll = Random.Range(0f, 1f);
            List<Farm> farmList = farmingModel.farmList;
            int farmMax = farmList.Count > 4 ? Mathf.RoundToInt((float) farmList.Count / 4f) : 1;
            if (randomRoll > 0.85f) farmsEffected = Random.Range(1, farmMax);
            List<Farm> shuffledList = GeneralFunctions.FisherYatesShuffle<Farm>(farmingModel.farmList);
            for (int i = 0; i < farmsEffected; i++) {
                // Iterate through the shuffled list and determine how many of the farm's crops are effected.
                if (farmList.Count < i + 1) break;
                Farm farm = shuffledList[i];
                int cropCount = farm.cropList.Count;
                if (cropCount == 0) continue;
                int infectedCount = Random.Range(1, cropCount);
                List<FloraItem> shuffledCrops = GeneralFunctions.FisherYatesShuffle<FloraItem>(farm.cropList);
                Debug.Log("FC - Farms Effected: " + farmsEffected + ", Crops Infected: " + infectedCount + ", Total Crops: " + cropCount);
                for (int j = 0; j < infectedCount; j++) {
                    // Iterate through the shuffled crop list and mark a specified number as infected.
                    FloraItem flora = shuffledCrops[j];
                    Debug.Log("FC - Settings flora of id " + flora.ID + " to infected.");
                    flora.infected = true;
                    Vector3 position = flora.location += new Vector3(0, 0.5f, 0);
                    GeneralFunctions.FormatWorldIcon(worldSpacePrefab, position, flora.gameObject.transform, plagueIcon);
                }
            }
        }
    }

    public void CreateFarmTasks(Farm farm) {
        /* if (farm.farmingHandler != null) {
            List<TaskGroup> taskList = new List<TaskGroup>();
            int cropLimit = farm.farmingHandler.relatedBuild.structureData.maxCropCount;
            int currentCropCount = farm.farmingHandler.CurrentCropCount;
            Debug.Log("FC - Current Crops: " + currentCropCount + ", Crop Limit: " + cropLimit);

            // Check the function handlers task list to ensure duplicate tasks aren't created.
            List<TaskGroup> floraTasks = new List<TaskGroup>(farm.farmingHandler.TaskQueueReturn());
            floraTasks = floraTasks.FindAll(x => x.taskTypeID == 2);
            List<TaskGroup.FloraCreationTask> floraCreations = new List<TaskGroup.FloraCreationTask>();
            foreach (TaskGroup task in floraTasks) {
                floraCreations.Add(task as TaskGroup.FloraCreationTask);
            }
            if (currentCropCount >= cropLimit) return;
            foreach (Vector3 loc in farm.cropLocations) {
                // Loop through each crop location in the farm, and check whether a crop exists at each location.
                if (farm.cropList.Find(x => x.location == loc) == null) {
                    if (floraCreations.Find(x => x.targetLocation == loc) == null) {
                        // If no task exists for this location, construct a new task.
                        int id = controllerManager.taskController.ReturnAvailableID();
                        TaskData taskData = controllerManager.taskController.FindTaskDataByID(7);
                        //Debug.Log("Rel skill: " + taskData.relatedSkill.uniqueName + ", xp reward: " + farm.floraData.xpReward);
                        Debug.Log("FRMC - " + loc);
                        Debug.Log("FRMC - " + farm.floraData);
                        Debug.Log("FRMC - " + farm.farmingHandler.id);
                        List<ResourceTransaction> transactions = controllerManager.storageController.ConvertToResourceTransactions(farm.floraData.requiredToGrow, true);
                        TaskGroup.FloraCreationTask floraTask = new TaskGroup.FloraCreationTask(loc, farm.floraData, controllerManager, farm.floraData.requiredToGrow, transactions, farm.farmingHandler, id, 9, farm);
                        controllerManager.taskController.AppendOrRemoveID(id, floraTask);
                        taskList.Add(floraTask);
                    }
                }
            }
            Debug.Log("Farm" + farm.ID + " assigning " + taskList.Count + " tasks to funcHand" + farm.farmingHandler.id);
            farm.farmingHandler.AppendTaskGroup(taskList);
        } */
    }

    public void PopulateCropLocation(Farm farm, Vector2 location) {
        FloraItem floraItem = controllerManager.natureController.InstantiateFloraData(farm.floraData.ID, location, 0, false);
        farm.cropList.Add(floraItem);
    }

    public FishData PickRandomFish(int[] fishIds = null) {
        // If ids are specified find fish with these ids, or use all fish to create a list of probabilites for each fish.
        float[] chanceArray;
        List<FishData> possibleFish = new List<FishData>();
        if (fishIds != null) {
            foreach (int id in fishIds) possibleFish.Add(farmingModel.fishDataLookup[id]);
        } else possibleFish = farmingModel.fishDatas;

        // Filter the possible list based on which season it is.
        SeasonData current = controllerManager.weatherController.SeasonNumReturn();
        possibleFish = possibleFish.FindAll(x => x.seasons[current.id - 1]);
        chanceArray = new float[possibleFish.Count];
        for (int i = 0; i < possibleFish.Count; i++) {
            chanceArray[i] = possibleFish[i].rarity;
        }

        // Normalise the probabilities and randomly select a fish.
        float random = Random.Range(0f, 1f);
        //chanceArray = GeneralFunctions.NormaliseProbabilities(chanceArray);
        int fishIndex = GeneralFunctions.PickRandomValueFromChanceArray(chanceArray, random);
        Debug.Log("FC - Possible Fish: " + possibleFish.Count + ", Chance Array Count: " + chanceArray.Length + ", fish index: " + fishIndex);
        if (fishIndex < 0) return null;
        else return possibleFish[fishIndex];
    }

    public void AmendFarmOutput(Farm farm, FloraData floraData, bool destroyOld = false) {
        farm.floraData = floraData;
        farm.floraDataID = floraData.ID;
        if (destroyOld) {
            foreach (FloraItem item in farm.cropList.ToArray()) {
                farm.farmingHandler.RemoveGameObjectFromList(item.gameObject);
                controllerManager.natureController.DestroyFloraItem(item);
                farm.cropList.Remove(item);
            }
        }

        RemoveAllAssociatedTasks(farm);
        CreateFarmTasks(farm);
    }

    public void BeginFarmSelection() {
        // Enable the rectangle selection game object, and define the action upon completion of selection.
        FloraData flora = controllerManager.natureController.FindFloraDataByID(4);
        selectionPanel.SetActive(true);
        selectionPanel.GetComponent<RectSelectAlt>().SetAction((GameObject game, Rect rect) => RegisterFarm(game, rect, flora), true, true, 100);
    }

    public void SetFunctionHandler(Farm farm, FunctionHandler functionHandler) {
        FunctionHandler oldHandler;
        Build build = functionHandler.relatedBuild;
        if (farm.farmingHandler != null) {
            oldHandler = farm.farmingHandler;
        } else oldHandler = null;

        if (oldHandler != null) {
            oldHandler.RelevantObjectCount -= farm.cropList.Count;
            oldHandler.AppendOrRemoveFarm(farm);
        }

        foreach (FloraItem crop in farm.cropList) {
            if (crop.relatedTaskID != -1) {
                if (oldHandler != null) oldHandler.FindTaskByID(crop.relatedTaskID, true);
            }
        }

        farm.relFuncHandID = functionHandler.id;
        farm.farmingHandler = functionHandler;
        farm.farmingHandler.RelevantObjectCount += farm.cropList.Count;
        farm.farmingHandler.AppendOrRemoveFarm(farm);
        //CreateFarmTasks(farm);
    }

    public Farm GameObjectToFarm(GameObject gameObject) {
        if (farmingModel.farmGameObjectLookUp.ContainsKey(gameObject)) {
            return farmingModel.farmGameObjectLookUp[gameObject];
        } else return null;
    }

    public List<Farm> FarmListReturn() {
        return farmingModel.farmList;
    }

    private void RemoveAllAssociatedTasks(Farm farm) {
        FunctionHandler farmHandler = farm.farmingHandler;
        if (farmHandler == null) return;
        List<Pawn> pawns = controllerManager.skillsController.FindPawnFromFunctionHandler(farmHandler.id);
        List<TaskGroup> handlerTasks = new List<TaskGroup>(farmHandler.TaskQueueReturn());
        List<TaskGroup> tasksToRemove1 = new List<TaskGroup>();
        handlerTasks = handlerTasks.FindAll(x => x.taskTypeID == 2);
        foreach (TaskGroup task in handlerTasks) {
            TaskGroup.FloraCreationTask farmTask = task as TaskGroup.FloraCreationTask;
            Farm currentFarm = farmTask.relFarm;
            if (currentFarm != null) {
                if (currentFarm == farm) tasksToRemove1.Add(task);
            }
        }
        Debug.Log("FC - Attempting to remove a total of " + tasksToRemove1.Count + " from function handler " + farmHandler.id);
        farmHandler.RemoveTasks(tasksToRemove1);

        foreach (Pawn pawn in pawns) {
            List<TaskGroup> tasksToRemove2 = new List<TaskGroup>();
            List<TaskGroup> tasks = new List<TaskGroup>(pawn.pawnTaskSystem.mainTaskList);
            tasks = tasks.FindAll(x => x.taskTypeID == 2);
            foreach (TaskGroup task in tasks) {
                TaskGroup.FloraCreationTask farmTask = task as TaskGroup.FloraCreationTask;
                Farm currentFarm = farmTask.relFarm;
                if (currentFarm != null) {
                    if (currentFarm == farm) tasksToRemove2.Add(task);
                }
            }
            Debug.Log("FC - Total of " + tasksToRemove2.Count + " tasks to remove from pawn " + pawn.name);
            pawn.pawnController.RemoveTasksWithSameHandler(tasksToRemove2);
        }
    }

    public void DeleteFarm(Farm farm) {
        RemoveAllAssociatedTasks(farm);
        DestroyImmediate(farm.farmObject);
        foreach (FloraItem flora in farm.cropList) controllerManager.natureController.DestroyFloraItem(flora, false);
        if (farmingModel.farmList.Contains(farm)) {
            farmingModel.farmList.Remove(farm);
            farmingModel.farmLookupByID.Remove(farm.ID);
            farmingModel.farmGameObjectLookUp.Remove(farm.farmObject);
        }
    }

    public Farm IsFloraItemOwned(FloraItem flora) {
        Farm farm = farmingModel.farmList.Find(x => x.cropList.Contains(flora));
        if (farm != null) return farm;
        else return null;
    }

    public void ExportFloraList(GameObject gameObject, Rect rect, List<string> tags, bool fullyGrown = true) {
        Destroy(gameObject);

        // Using the define rect, detect all gameobjects that collide with the rect with the layer mask applied.
        Collider2D[] floras = Physics2D.OverlapBoxAll(rect.center, new Vector2(rect.width, rect.height), 0, floraLayerMask);
        List<GameObject> exportList = new List<GameObject>();
        Debug.Log("FC - EFL: Collider count from overlap: " + floras.Length);

        foreach (Collider2D collider in floras) {
            // Cycle through all relevant tags, to determine whether the gameobject is required.
            foreach (string tag in tags) {
                if (collider.gameObject.CompareTag(tag)) {

                    Debug.Log("FC - " + collider.gameObject.name + " is a " + tag);
                    FloraItem flora = controllerManager.natureController.GameObjectToFloraItem(collider.gameObject);

                    if (flora != null) {
                        // Deduce the GameObject's flora item, and check if the flora is ready for harvesting.
                        if (flora.growthPercentage > 99) {
                            // Append the item to the return list, and instantiate an icon to show the object has been retrieved.
                            exportList.Add(collider.gameObject);
                            Vector3 position = flora.location;
                            Vector3 centreOffset = new Vector3(0, (float) flora.floraData.size.y / 2f, 0);
                            position += centreOffset;

                            // Determine which icon to display using the flora category and instantiate it on at the centre of the flora item.
                            Sprite chosenIcon = flora.floraData.floraCategory == FloraData.category.Tree ? axeIcon : scytheIcon;
                            GeneralFunctions.FormatWorldIcon(worldSpacePrefab, position, flora.gameObject.transform, chosenIcon);
                        }
                    }
                } else Debug.Log("FC - " + collider.gameObject.tag + " does not equal " + tag);
            }
        }
        generalFunctionHandler.AppendGameObjectsToQueue(exportList);
    }
    public void BeginFloraSelection(FloraData.category[] categories) {
        RectSelectAlt rectSelectAlt = managerReferences.viewManager.rectSelectAlt;
        // Convert the flora categories to a list of strings, and set the action of the selection rectangle to the exporting of game objects with these tags.
        string debug = "Looking for flora of types: ";
        List<string> tagList = new List<string>();
        foreach (FloraData.category category in categories) {
            tagList.Add(category.ToString());
            //Debug.Log(category.ToString());
            debug += category.ToString() + ", ";
        }
        Debug.Log(debug);
        rectSelectAlt.gameObject.SetActive(true);
        rectSelectAlt.SetAction((GameObject game, Rect rect) => ExportFloraList(game, rect, tagList), false, false);
    }
}