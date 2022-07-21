using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
[System.Serializable]
public class FunctionHandler : MonoBehaviour {
    private PurposeData currentPurpose;
    public int id;
    private float assignedRadius = -1f;
    private LinkedList<TaskGroup> taskQueue = new LinkedList<TaskGroup>();
    public int workerCount = 0;
    private FunctionHandlerStatus functionHandlerStatus = FunctionHandlerStatus.Unset;
    private int totalRelevantGameObjs = 0;
    public List<GameObject> relevantGameObjects = new List<GameObject>();
    public List<CraftingQueueItem> craftingQueueItems;
    private List<TaskData> allowedTasks = new List<TaskData>();
    [SerializeField]
    List<Node> restrictedUseNodes = new List<Node>();
    public Build relatedBuild;
    private float timerMax = 1f;
    private float timer;
    public ManagerReferences managerReferences;
    private ControllerManager controllerManager;
    public List<TaskData.TaskType> emptyTaskTypes;
    private List<FloraData> availableFloras = null;
    [SerializeField]
    private List<Farm> farmList = new List<Farm>();
    private int relevantObjectCount = 0;
    public int RelevantObjectCount {
        get {
            return relevantObjectCount;
        }
        set {
            relevantObjectCount = value;
            EventController.TriggerEvent("funch" + this.id + "RelevantObjectChange");
        }
    }
    public bool debugBool = true;
    // Start is called before the first frame update
    private void FixedUpdate() {
        timer -= Time.deltaTime;
        if (timer <= 0) {
            timer = timerMax;
            if (currentPurpose != null)
                Debug.Log("Function handler " + id + " of type: " + currentPurpose.purposeName + " contains: " + taskQueue.Count + " tasks waiting.");
            // Debug.Log("RequestingTask");
        }
    }

    public void AmendObjectCounter(bool reset, int amount = 0) {
        if (reset) {
            totalRelevantGameObjs = 0;
        } else {
            if (totalRelevantGameObjs + amount >= 0) {
                if (totalRelevantGameObjs > relevantGameObjects.Count) totalRelevantGameObjs -= 1;
                totalRelevantGameObjs += amount;
            }
        }
    }

    public LinkedList<TaskGroup> TaskQueueReturn() {
        return taskQueue;
    }

    public void RemoveTasks(List<TaskGroup> tasks) {
        foreach (TaskGroup task in tasks) {
            taskQueue.Remove(task);
        }
    }

    private void Awake() {
        managerReferences = GameObject.Find("MainWorld").GetComponent<ManagerReferences>();
        controllerManager = managerReferences.controllerManager;
    }

    private void Start() {
        EventController.StartListening("recalculateBuildingObjects", PopulateGameObjectList);
        timer = timerMax;
        //Debug.Log(controllerManager); 
    }

    public List<TaskGroup> FunctionTaskRequest(int id, int count = 1) {
        List<TaskGroup> returnList = new List<TaskGroup>();
        //Debug.Log(taskQueue.Count);
        if (taskQueue.Count >= count) {
            for (int i = 0; i < count; i++) {
                returnList.Add(taskQueue.First.Value);
                taskQueue.RemoveFirst();
            }
        } else {
            PopulateTaskQueue();
        }
        Debug.Log("Requested " + returnList.Count + " tasks.");
        return returnList;
    }

    public void RecallTasks(List<TaskGroup> removeTasks) {
        foreach (TaskGroup task in removeTasks) {
            taskQueue.Remove(task);
        }
    }

    public void SetBuild(Build build) {
        taskQueue.Clear();
        relevantGameObjects.Clear();
        AmendObjectCounter(true);
        this.relatedBuild = build;
        DetermineFunctionhandlerStatus();
        currentPurpose = build.purposeData;
        if (currentPurpose.ID == 2) craftingQueueItems = new List<CraftingQueueItem>();
        RelevantObjectCount = 0;
        availableFloras = build.purposeData.possibleFloraCreations;
        availableFloras = BuildingFunctions.FlorasOfQuality(availableFloras, build.structureData.qualityTier);
        assignedRadius = Mathf.FloorToInt(build.structureData.radiusMultiplier * build.purposeData.radius);
        Debug.Log("Purpose ID: " + this.relatedBuild.purposeDataID);
        SetAllowedTasks(build.purposeData.permittedTasks);
        if (controllerManager != null) {
            PopulateGameObjectList();
        }
    }

    public void SetAllowedTasks(List<TaskData> taskDatas) {
        allowedTasks = taskDatas;
    }

    public void AppendOrRemoveFarm(Farm farm) {
        if (farmList.Contains(farm)) farmList.Remove(farm);
        else farmList.Add(farm);
    }

    public List<GameObject> RemoveGameObjectFromList(GameObject gameObject = null) {
        List<GameObject> returnList = new List<GameObject>(relevantGameObjects);
        Debug.Log("FH - " + id + " removing GameObject from list");
        if (gameObject != null) {
            if (relevantGameObjects.Contains(gameObject)) {
                relevantGameObjects.Remove(gameObject);
                AmendObjectCounter(false, -1);
            }
        } else {
            relevantGameObjects.Clear();
            AmendObjectCounter(true);
        }
        return returnList;
    }
    private void PopulateGameObjectList() {
        Debug.Log("GameObjectList Refreshed");
        if (relatedBuild != null) {
            int maxObjects = currentPurpose.maxCreationsApplies ? relatedBuild.structureData.maxCreations : relatedBuild.structureData.maxCropCount;
            List<GameObject> objectsToAdd = new List<GameObject>();
            foreach (Collider2D x in Physics2D.OverlapCircleAll(relatedBuild.worldPosition, assignedRadius)) {
                //if (objectsToAdd.Count >= maxObjects) break;
                if (relatedBuild.purposeData.radiusReqs.FindAll(y => y.tagName == x.tag).Count > 0) {
                    objectsToAdd.Add(x.gameObject);
                    SpriteRenderer spriteRenderer;
                    if (!x.gameObject.TryGetComponent<SpriteRenderer>(out spriteRenderer)) {
                        spriteRenderer = x.gameObject.GetComponentInParent<SpriteRenderer>();
                    }

                    if (spriteRenderer != null) spriteRenderer.color = Color.red;
                }
            }
            AppendGameObjectsToQueue(objectsToAdd);
            RelevantObjectCount += objectsToAdd.Count;
        }
    }

    public void InformOfCancellation(TaskGroup task, GameObject relevantObject = null) {
        TaskData taskData = controllerManager.taskController.FindTaskDataByID(task.taskTypeID);
        if (taskData.taskType == TaskData.TaskType.SpawningObject) RelevantObjectCount -= 1;
        if (relevantObject != null) {
            if (!relevantGameObjects.Contains(relevantObject)) {
                relevantGameObjects.Add(relevantObject);
            }
        }
    }

    public void AppendGameObjectToQueue(GameObject gameObject) {
        if (!controllerManager.buildingController.CheckGameObjectOwnership(gameObject)) {
            SpriteRenderer spriteRenderer;
            if (gameObject.TryGetComponent<SpriteRenderer>(out spriteRenderer)) {
                spriteRenderer.color = Color.red;
            }
            AmendObjectCounter(false, 1);
            //gameObject.GetComponent<SpriteRenderer>().color = Color.red;
            //Debug.Log(gameObject.name);
            controllerManager.buildingController.AssignGameObject(gameObject, this);
        }
        Debug.Log("FH - " + id + " relevant queue is now: " + relevantGameObjects.Count);
    }
    public void AppendGameObjectsToQueue(List<GameObject> objectsToAdd) {
        if (objectsToAdd.Count > 0) {
            if (objectsToAdd.Count < totalRelevantGameObjs) totalRelevantGameObjs -= objectsToAdd.Count;
            foreach (GameObject gameObject in objectsToAdd) {
                AppendGameObjectToQueue(gameObject);
            }
            //Debug.Log("FH - Appending " + objectsToAdd.Count + " objects to the queue of function handler " + id);
        }
    }

    private void PopulateTaskQueue() {
        if (relatedBuild != null) {
            Debug.Log("FH - " + id + " Relevant GameObjects count: " + relevantGameObjects.Count);
            foreach (GameObject gameObject in relevantGameObjects) {
                if (ConvertObjectToTask(gameObject, TaskData.TaskType.HarvestingObject)) return;
            }
            CreateTasksFromEmpty(emptyTaskTypes);
        } else {
            if (relevantGameObjects.Count > 0) {
                foreach (GameObject gameObject in relevantGameObjects) {
                    if (ConvertObjectToTask(gameObject, TaskData.TaskType.HarvestingObject)) return;
                }
            }
        }
    }

    public void ReceiveReturnedTaskGroup(TaskGroup returnedTask) {
        if (!taskQueue.Contains(returnedTask)) taskQueue.AddLast(returnedTask);
    }

    public TaskGroup FindTaskByID(int id, bool remove) {
        foreach (TaskGroup task in taskQueue) {
            if (task.uniqueID == id) {
                if (remove) taskQueue.Remove(task);
                return task;
            }
        }
        return null;
    }

    private void DetermineFunctionhandlerStatus(FunctionHandlerStatus status = FunctionHandlerStatus.Unset) {
        if (status != FunctionHandlerStatus.Unset) {
            functionHandlerStatus = status;
            return;
        } else {
            if (relatedBuild == null) {
                functionHandlerStatus = FunctionHandlerStatus.TasksAllowed;
                return;
            } else {
                int maxResourceExtraction = relatedBuild.structureData.maxResourceExtraction;
                if (maxResourceExtraction != -1) {
                    if (relatedBuild.currentResourceExtraction >= maxResourceExtraction) {
                        status = FunctionHandlerStatus.TasksDisallowed;
                    } else status = FunctionHandlerStatus.TasksAllowed;
                }
                return;
            }
        }

    }

    public void AppendTaskGroup(List<TaskGroup> taskList) {
        foreach (TaskGroup task in taskList) {
            taskQueue.AddLast(task);
        }
    }

    public void CreateTasksFromEmpty(List<TaskData.TaskType> types) {

        TaskData taskData = allowedTasks.Find(x => types.Contains(x.taskType));
        Debug.Log("FH - FuncHand " + this.id + " of type " + this.relatedBuild.purposeData.purposeName + ": " + allowedTasks.Count + " tasks allowed and " + taskData);
        // Determine whether there are "Spwaning" tasks available to the handler, and whether it should automatically create its own tasks.
        if (taskData == null || !currentPurpose.automaticEmptyTaskCreation) return;
        // if (functionHandlerStatus != FunctionHandlerStatus.TasksAllowed) return;
        string debugText = "FH - Task of type " + taskData.taskName + " cannot be created as ";

        Debug.Log("FH - FuncHand " + this.id + " of type " + this.relatedBuild.purposeData.purposeName + " gameObCount: " + relevantGameObjects.Count + " ; totalFuture: " + totalRelevantGameObjs);

        if (DetermineIfCreationLimitReached(relatedBuild.structureData, relatedBuild.purposeData)) {
            if (debugBool) Debug.Log(debugText + "due to max creation limit.");
            return;
        }
        Node node = null;
        /* Vector3 targetLocation = node.worldPosition; */
        TaskGroup retrievedTask = null;
        int id = controllerManager.taskController.ReturnAvailableID();
        switch (taskData.ID) {
            case 2:
                // Flora Creation Task:
                if (availableFloras == null) return;
                Debug.Log("FH - Flora creation has a total of " + availableFloras.Count + " floras avaiable.");
                FloraData creation = StorageFunctions.CheckFloraDatasForItems(availableFloras, controllerManager.storageController.CompileTotalResourceList(stationary: -1));
                if (creation != null) {
                    node = FindAvailableNode(creation.size.x, creation.size.y, false, true, false);
                    if (node == null) return;
                    List<ResourceTransaction> transactions = controllerManager.storageController.ConvertToResourceTransactions(creation.requiredToGrow);
                    TaskGroup.FloraCreationTask floraCreation = new TaskGroup.FloraCreationTask(node.worldPosition, creation, controllerManager, creation.requiredToGrow, transactions, this, id, build : relatedBuild, growthMultiplier : 0.5f);
                    retrievedTask = floraCreation;
                    AmendObjectCounter(false, 1);
                }
                break;
            case 3:
                break;
            case 5:
                // Mining Task:
                node = FindAvailableNode(1, 1, true, true, true);
                if (node == null) return;
                Vector3 targetLocation = node.worldPosition;
                List<TieredProbableResource> possibleResources = relatedBuild.purposeData.possibleMinerals.FindAll(x => x.resourceTier == relatedBuild.structureData.qualityTier);
                float[] chanceArray = new float[possibleResources.Count];
                for (int i = 0; i < chanceArray.Length; i++) {
                    chanceArray[i] = possibleResources[i].probableRequiredResource.probability;
                }
                int resIndex = GeneralFunctions.PickRandomValueFromChanceArray(chanceArray, Random.Range(0f, 1f));
                RequiredResources chosen = possibleResources[resIndex].probableRequiredResource.requiredResource;
                ResourceData outputResource = chosen.resource;
                //Debug.Log("Creating mining task of " + outputResource.resourceName + ", at " + targetLocation);
                string listeningIdentifier = outputResource.resourceName + "MinedAt" + targetLocation.x + "," + targetLocation.y;
                restrictedUseNodes.Add(node);
                TaskGroup.MiningTask miningTask = new TaskGroup.MiningTask(controllerManager, targetLocation, outputResource, Random.Range(1, chosen.count), this, id, listeningIdentifier);
                retrievedTask = miningTask;
                break;
            case 7:
                CraftingData craftingData = null;
                List<RequiredResources> chosenInput = null;
                if (craftingQueueItems != null) {
                    if (craftingQueueItems.Count > 0) {
                        foreach (CraftingQueueItem craftingQueueItem in craftingQueueItems.ToArray()) {
                            bool found = false;
                            CraftingData current = craftingQueueItem.craftingData;
                            /* if (craftingQueueItem.count <= 0 && craftingQueueItem.count != -1) {
                                Debug.Log("FUNCH - Attempting to remove crafting queue of " + craftingQueueItem.craftingData.craftingRecipeName);
                                craftingQueueItems.Remove(craftingQueueItem);
                                continue;
                            } */
                            List<RequiredResources> chosenSource = new List<RequiredResources>();
                            if (current.categoryInputs.Count > 0) {
                                if (controllerManager.storageController.CheckIfSubCategoryInStorage(current.categoryInputs)) {
                                    chosenInput = new List<RequiredResources>();
                                    string debug = "FH - Crafting category items found including";
                                    foreach (CraftingData.ResourceByCategory resource in craftingQueueItem.craftingData.categoryInputs) {
                                        InstantiatedResource res = controllerManager.storageController.FindResourcesOfSubType(resource.subCategory, resource.count);
                                        chosenInput.Add(new RequiredResources(res.resourceData, resource.count));
                                        debug += " " + res.resourceData.resourceName + " x" + resource.count + " ";
                                    }
                                    Debug.Log(debug);
                                    found = true;
                                }
                            } else {
                                if (controllerManager.storageController.CheckIfResourcesInStorage(current.inputs)) {
                                    chosenInput = current.inputs;
                                    found = true;
                                }
                            }
                            if (found) {
                                if (craftingQueueItem.count != -1) ReduceCraftingOrder(craftingQueueItem, -1);
                                craftingData = craftingQueueItem.craftingData;
                                break;
                            }
                        }
                    } else return;
                } else return;
                if (craftingData == null || chosenInput == null) return;
                string debug1 = "FH - Chosen Inputs:";
                foreach (RequiredResources req in chosenInput) {
                    debug1 += " " + req.resource.resourceName + " x" + req.count + " ";
                }
                Debug.Log(debug1);

                List<ResourceTransaction> resourceTransactions = new List<ResourceTransaction>();
                resourceTransactions = controllerManager.storageController.ConvertToResourceTransactions(chosenInput, true);
                if (resourceTransactions != null) {
                    node = FindAvailableNode(1, 1, true, true, true);
                    TaskGroup.CraftingTask craftingTask = new TaskGroup.CraftingTask(controllerManager, node, craftingData, chosenInput, this, resourceTransactions, id);
                    retrievedTask = craftingTask;
                }
                /* foreach (RequiredResources req in copyList) {
                    List<InstantiatedResource> relevantContainers = totalStorage.FindAll(x => x.resourceData == req.resource);
                    foreach (InstantiatedResource res in relevantContainers) {
                        if (req.count > 0) {
                            int extractionCount;
                            if (req.count > res.count) extractionCount = res.count;
                            else extractionCount = req.count;
                            StorageContainer storage = controllerManager.storageController.FindStorageByID(res.storageContainerID);
                            resourceTransactions.Add(new ResourceTransaction(res, extractionCount));
                            req.count -= extractionCount;
                        }
                    }
                    if (req.count > 0) {
                        return;
                    }
                } */

                break;

            case 8:
                // Fishing Task:
                node = FindAvailableNode(1, 1, true, true, true);
                Vector3 fishLocation = node.worldPosition;
                FishData fish = controllerManager.farmingController.PickRandomFish();
                if (fish == null) return;
                //Debug.Log("Creating mining task of " + outputResource.resourceName + ", at " + targetLocation);
                string listeningIdentifier1 = fish.fishName + "CaughtAt" + fishLocation.x + "," + fishLocation.y;
                TaskGroup.FishingTask fishingTask = new TaskGroup.FishingTask(controllerManager, fishLocation, fish, this, id, listeningIdentifier1);
                retrievedTask = fishingTask;
                break;
            case 9:
                Debug.Log("FH - Flora creation has a total of " + availableFloras.Count + " floras avaiable.");
                // Flora creation crops.
                SeasonData season = controllerManager.weatherController.SeasonNumReturn();
                FloraData floraData = null;
                CropTile cropTile = null;
                Farm selectedFarm = null;
                foreach (Farm farm in farmList) {
                    // Check each farm for free spaces to plant crops.
                    if (farm.cropList.Count < farm.cropTiles.Count) {
                        // Check the farm's chosen crop is within season.
                        if (farm.floraData.growthSeasons[season.id - 1]) {
                            // Set the chosen crop tile and the selected farm in order to create the planting task.
                            cropTile = farm.cropTiles.Find(x => !x.occupied && !x.reserved);
                            floraData = farm.floraData;
                            selectedFarm = farm;
                            break;
                        }
                    }
                }
                if (floraData != null && cropTile != null) {
                    if (StorageFunctions.CheckFloraDataForItems(floraData, controllerManager.storageController.CompileTotalResourceList(stationary: -1))) {
                        List<ResourceTransaction> transactions = controllerManager.storageController.ConvertToResourceTransactions(floraData.requiredToGrow);
                        TaskGroup.FloraCreationTask floraCreation = new TaskGroup.FloraCreationTask(cropTile.location.worldPosition - new Vector3(0, 0.5f), floraData, controllerManager, floraData.requiredToGrow, transactions, this, id, build : relatedBuild, farm : selectedFarm);
                        retrievedTask = floraCreation;
                        cropTile.reserved = true;
                        AmendObjectCounter(false, 1);
                    } else Debug.Log("FH - Lacking required items for farming creation.");
                }
                break;
            default:
                Debug.Log("FUNCH - No available tasks from empty for FH" + this.id);
                break;
        }

        if (retrievedTask != null) {
            taskQueue.AddLast(retrievedTask);
            controllerManager.taskController.AppendOrRemoveID(id);
            TaskData task = controllerManager.taskController.FindTaskDataByID(retrievedTask.taskTypeID);
            if (task.taskType == TaskData.TaskType.SpawningObject) RelevantObjectCount += 1;
            if (node != null) {
                restrictedUseNodes.Add(node);
                if (retrievedTask.listeningIdentifier != null) {
                    EventController.StartListening(retrievedTask.listeningIdentifier, delegate { RemoveNodeFromRestriction(node, retrievedTask.listeningIdentifier); });
                }
            }
        }
    }

    private Node FindAvailableNode(int sizeX, int sizeY, bool workerNodes = false, bool walkableRequired = true, bool occupiedRequired = false) {
        Node returnNode = null;
        Debug.Log("Available Nodes: " + relatedBuild.availableNodes.Count + ", Worker Nodes:" + relatedBuild.workerNodes.Count);
        if (relatedBuild.availableNodes.Count > 0 && !workerNodes) {
            returnNode = controllerManager.gridController.CheckNodeListForFreeSpaces(relatedBuild.availableNodes, sizeX + 2, sizeY + 2, Vector3.zero, walkableRequired, occupiedRequired, true);
        }
        // Get the size of the output product, check for available space using grid cont function, define the node as the target location for the crop. If no space is available, return.
        if (relatedBuild.workerNodes.Count > 0 && workerNodes) {
            returnNode = controllerManager.gridController.CheckNodeListForFreeSpaces(relatedBuild.workerNodes, sizeY, sizeX, Vector3.zero, walkableRequired, occupiedRequired, true);
        }
        if (returnNode != null) Debug.Log("FUNCH - a node has been found for " + id);
        return returnNode;
    }

    public void RemoveNodeFromRestriction(Node node, string listeningIdentifier = null) {
        if (listeningIdentifier != null) EventController.StopListening(listeningIdentifier, delegate { RemoveNodeFromRestriction(node, listeningIdentifier); });
        if (restrictedUseNodes.Contains(node)) restrictedUseNodes.Remove(node);
    }

    private bool DetermineIfCreationLimitReached(StructureData structureData, PurposeData purposeData) {
        if (purposeData.maxCreationsApplies || purposeData.maxCropsApplies) {
            int maxObjects = purposeData.maxCropsApplies ? structureData.maxCropCount : structureData.maxCreations;
            return RelevantObjectCount < maxObjects ? false : true;
        } else return false;

    }

    public List<CraftingQueueItem> AppendCraftOrder(CraftingQueueItem craftingQueueItem) {
        CraftingQueueItem existingItem = craftingQueueItems.Find(x => x.craftingData == craftingQueueItem.craftingData);
        if (existingItem != null) {
            if (craftingQueueItem.count == -1 || existingItem.count == -1) existingItem.count = -1;
            else existingItem.count += craftingQueueItem.count;
        } else craftingQueueItems.Add(craftingQueueItem);
        Debug.Log("FUNCH - Crafting queue is now: " + craftingQueueItems.Count);
        return craftingQueueItems;
    }

    public void ReduceCraftingOrder(CraftingQueueItem craftingQueue, int change = -100) {
        if (change > 0 || !craftingQueueItems.Contains(craftingQueue)) return;
        if (change != -100) {
            craftingQueue.count += change;
            if (craftingQueue.count <= 0 && craftingQueue.count != -1) {
                craftingQueueItems.Remove(craftingQueue);
            };
        } else {
            craftingQueueItems.Remove(craftingQueue);
        }
        EventController.TriggerEvent(relatedBuild.id + "CraftingQueueChange");
    }

    public void RemoveNodeFromRestriction(Vector3 location) {
        Node node = controllerManager.gridController.NodeFromWorld(location);
        RemoveNodeFromRestriction(node);
    }

    public bool ConvertObjectToTask(GameObject item, TaskData.TaskType taskType) {
        // Find all available tasks of a certain type which involves the game object's tag.
        List<TaskData> taskDatas = controllerManager.taskController.FindTasksByTag(item.tag, taskType);
        Debug.Log(item.name + " can be converted to " + taskDatas.Count + " tasks within function handler " + id);
        foreach (TaskData taskData in taskDatas) {
            // Iterate through the relevant tasks, attempting to convert the object into a task.
            int taskID = controllerManager.taskController.ReturnAvailableID();
            TaskGroup taskGroup = null;
            switch (taskData.ID) {
                case 1:
                    FloraItem currentFloraItem = controllerManager.natureController.GameObjectToFloraItem(item);
                    if (currentFloraItem == null) return false;
                    //Debug.Log("Growth Percentage of flora item: " + currentFloraItem.growthPercentage);
                    if (currentFloraItem.growthPercentage >= 95f) {
                        string listeningIdentifier = currentFloraItem.uniqueName + "Destroyed";
                        TaskGroup.FloraDestructionTask floraTask = new TaskGroup.FloraDestructionTask(currentFloraItem, listeningIdentifier, controllerManager, this, taskID, farm : currentFloraItem.farm);
                        taskGroup = floraTask;
                        /* Node floraNode = controllerManager.gridController.NodeFromWorld(currentFloraItem.location);
                        EventController.StartListening(listeningIdentifier, delegate { RemoveNodeFromRestriction(floraNode, listeningIdentifier); }); */
                    }
                    break;
                case 10:
                    NPC.AnimalNPC animalNPC = controllerManager.nPCController.GameObjectToNPC(item) as NPC.AnimalNPC;
                    AnimalData animal = animalNPC.animalData;
                    string listener = animal.uniqueName + animal.ID + "HasBeenSlayed";
                    List<RequiredResources> outputs = GeneralFunctions.CalculatePotentialResources(animal.outputResources);
                    TaskGroup.SlayAnimalTask slayAnimalTask = new TaskGroup.SlayAnimalTask(controllerManager, animalNPC, outputs, listener, taskID);
                    taskGroup = slayAnimalTask;
                    break;
            }

            // Determine whether a task has been created, and if so, append the task to the queue, remove the object from the list and exit the function.

            if (taskGroup != null) {
                RemoveGameObjectFromList(item);
                controllerManager.taskController.AppendOrRemoveID(taskID, taskGroup);
                taskQueue.AddLast(taskGroup);
                AmendObjectCounter(false, -1);
                return true;
            }
        }

        // If no task has been found for any of the available tasks, return false.

        return false;
    }

    public LinkedList<TaskGroup> TaskGroupRetun() {
        return taskQueue;
    }

    public enum FunctionHandlerStatus {
        Unset,
        TasksAllowed,
        TasksDisallowed
    }
}