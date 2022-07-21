using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PawnController : MonoBehaviour {
    // Start is called before the first frame update
    private float gameSpeed = 0f;
    private Rigidbody2D pawnBody;
    public float waitingTimer, currentGameSpeed, waitingTimeElapsed;
    private float timer, timerMax;
    private DateController TimeModel;
    public PawnModel pawnModel;
    public PawnTaskSystem pawnTaskSystem;
    private SkillsController skillsTracker;
    private PawnModel.TaskState state;
    private TaskGroup currentTaskGroup;
    public TaskGroup CurrentTaskGroup {
        get {
            return currentTaskGroup;
        }
        set {
            currentTaskGroup = value;
            if (value != null) Debug.Log("PWNC - current task group set for pawn" + pawnModel.pawn.id + " to " + value.taskTypeID);
            EventController.TriggerEvent(pawnModel.pawn.id + "HasNewTaskGroup");
        }
    }
    private FunctionHandler specificFunctionHandler;
    private bool waitingRequirementSet = false, taskInterruption = false, loadingBarPresent = false;
    private ManagerReferences managerReferences;
    private ControllerManager controllerManager;
    public float timeSinceLastTask = 0, timeSinceStorageDump = 0;
    public SpriteRenderer[] clothingSprites;
    private bool pauseTask = false;
    private void Awake() {
        // Instantiate classes.
        pawnModel = new PawnModel();
        pawnTaskSystem = new PawnTaskSystem();

        // Initialise independent variables.
        timerMax = 1f;
        waitingTimer = 0;
        waitingTimeElapsed = -1;
        state = PawnModel.TaskState.AwaitingTask;
    }
    void Start() {
        // Define variables at runtime.
        managerReferences = managerReferences = GameObject.Find("MainWorld").GetComponent<ManagerReferences>();
        controllerManager = managerReferences.controllerManager;
        skillsTracker = controllerManager.skillsController;
        TimeModel = controllerManager.dateController;
        pawnBody = this.GetComponent<Rigidbody2D>();
        // Call initial functions, and define event listeners.
        AmendGameSpeed();
        EventController.StartListening("gameSpeedChange", AmendGameSpeed);
        CurrentTaskGroup = null;
    }

    private void DequeueTasksMain() {
        //this.pawnTaskSystem.DequeueTasks();
    }

    public void SetPawnInModel(Pawn pawn) {
        this.pawnModel.pawn = pawn;
        pawnTaskSystem.SetPawn(pawn);
        pawn.pawnTaskSystem = pawnTaskSystem;
    }

    // Fixed update to process pawn actions, to seperate the speed from the framerate.
    void FixedUpdate() {
        gameSpeed = controllerManager.dateController.GameSpeedReturn();
        //Debug.Log(currentTask);
        // Determine the pawn's current state, to deduce whether a task should be requested.
        float timerIncrease = Time.deltaTime * gameSpeed;
        timeSinceStorageDump += timerIncrease;
        switch (state) {
            case PawnModel.TaskState.AwaitingTask:
                timeSinceLastTask += timerIncrease;
                timer -= timerIncrease;
                // Intermitently request a new task from the function handler;
                if (timer <= 0) {
                    timer = timerMax;
                    PawnRequestNextTaskGroup();
                    // Debug.Log("RequestingTask");
                }
                break;
            case PawnModel.TaskState.PerformingTask:
                if (!pauseTask && currentTaskGroup != null) {
                    if (currentTaskGroup.associatedTasks.Count > 0) {
                        //Debug.Log("PWNC - Pawn " + pawnModel.pawn.id + " currently performing: " + currentTaskGroup.associatedTasks[0].identifier + " from taskgroup " + currentTaskGroup.taskTypeID + " with total remaining tasks of: " + currentTaskGroup.associatedTasks.Count + " and id of " + currentTaskGroup.uniqueID);
                        PerformTask(currentTaskGroup.associatedTasks[0]);
                    } else {
                        CompleteTaskGroup(currentTaskGroup);
                        Debug.Log("PWNC - Pawn " + pawnModel.pawn.id + " was trying to perform a task with 0 associated tasks.");
                    }
                }
                break;
        }
    }

    private void PawnRequestNextTaskGroup() {
        // Get the next 'task' from the pawn's task sysem, and remove it from the queue.
        if (currentTaskGroup == null) {
            // If there is no current task group, find the first task group available, and check if the conditions are met to take it.
            TaskGroup taskGroup = pawnTaskSystem.RequestNextTaskGroup(true);
            if (taskGroup == null) {
                if (!pawnTaskSystem.AppendTaskGroup(specificFunctionHandler.FunctionTaskRequest(1, 1), false) && timeSinceLastTask >= 20f) {
                    // If a task hasn't been provided for x seconds, request a general task.
                    FunctionHandler generalHandler = controllerManager.buildingController.FindFuncHandlerByID(1);
                    if (!pawnTaskSystem.AppendTaskGroup(generalHandler.FunctionTaskRequest(1)) && timeSinceStorageDump > 10f) {
                        pawnTaskSystem.AppendStorageDumpTransferTask(controllerManager);
                    }

                }
                return;
            }

            StorageContainer pawnInv = pawnModel.pawn.storageContainer;
            if (StorageFunctions.CheckStorageAvailable(pawnInv, taskGroup.weightRequiredToComplete)) {
                if (controllerManager.storageController.CheckIfResourcesInStorage(taskGroup.inputResources, new int[] { pawnInv.id })) {
                    Debug.Log("PWNC - Pawn" + pawnModel.pawn.id + " requested new task and has required storage and weight to receive.");
                    CurrentTaskGroup = pawnTaskSystem.RequestNextTaskGroup();
                    List<ElementaryTask> elementaryTasks = CurrentTaskGroup.associatedTasks.FindAll(x => x.identifier == "amendInventory");
                    foreach (ElementaryTask invTask in elementaryTasks) {
                        ElementaryTask.InventoryTask inventoryTask = invTask as ElementaryTask.InventoryTask;
                        Debug.Log("PWNC - Checking inventory tasks for existing resources of type " + inventoryTask.resourceData);
                        // Positive count = pawn gaining items.
                        // If the pawn is gaining items, and already has the required items, skip the movement and inv tasks.
                        if (inventoryTask.count > 0 && inventoryTask.targetStorage != null) {
                            if (StorageFunctions.CheckIfResourceAvailable(new RequiredResources(inventoryTask.resourceData, inventoryTask.count), pawnInv.inventory)) {
                                Debug.Log("PWNC - Check successful");
                                pawnInv.inventory.Find(x => x.resourceData == inventoryTask.resourceData).reserved += inventoryTask.count;
                                if (inventoryTask.walkingTask != null) CurrentTaskGroup.associatedTasks.Remove(inventoryTask.walkingTask);
                                CurrentTaskGroup.associatedTasks.Remove(inventoryTask);
                            } else Debug.Log("PWNC - Check failed");
                        }
                    }
                } else Debug.Log("PWNC - Not enough resources in storage for " + taskGroup.uniqueID + " of type " + taskGroup.taskTypeID);
            } else {
                StorageContainer pawnStorage = pawnModel.pawn.storageContainer;
                pawnTaskSystem.AppendStorageDumpTransferTask(controllerManager);
                return;
            }

            pawnModel.pawn.pawnStatus.currentStatus = PawnStatus.CurrentStatus.Working;

        } else {
            state = PawnModel.TaskState.PerformingTask;
        }
    }

    public FunctionHandler PeekPawnHandler() {
        return specificFunctionHandler;
    }
    private void PerformTask(ElementaryTask task) {
        string debugText = "PWNC - " + pawnModel.pawn.name + " performing task " + task.identifier + " part of task" + task.taskGroup.uniqueID + "(" + task.taskGroup.taskTypeID + ")";

        // Determine how long since the pawn has received a task from a specific handler.
        if (currentTaskGroup.associatedHandler != null) {
            timeSinceLastTask = currentTaskGroup.associatedHandler.id == 1 ? timeSinceLastTask : 0;
        }
        // Using the tasks identifier (category) determine which derived class to reference, and perform the relevant functions.
        switch (task.identifier) {
            case "movement":
                // Define the reference to the derived class of movement.
                ElementaryTask.MovementTask movementTask = task as ElementaryTask.MovementTask;
                // If the movement task has no defined path of nodes, populate this list based on the pawn's world location.
                debugText += " towards " + movementTask.targetPos;
                if (movementTask.nodePath == null) {
                    movementTask.InitialiseStartPosition(this.gameObject.transform.position, controllerManager);
                }
                if (movementTask.nodePath.Count > 0) {
                    // If nodes in the path remain, set the first node in the path to be the target node.
                    Node nextTile = movementTask.nodePath.First.Value;
                    // Begin the translation of the pawn gameobject to its destination.
                    AiFunctions.MoveTowards(pawnBody, nextTile.worldPosition, currentGameSpeed, 0.5f);

                    //Debug.Log(movementTask.nodePath.Count);
                    if (Vector2.Distance(this.transform.position, nextTile.worldPosition) < 0.1f) {
                        // Check whether the destination has been reached and if so, move onto the next node.
                        Node tempNode = movementTask.nodePath.First.Value;
                        movementTask.nodePath.RemoveFirst();
                        if (!nextTile.walkable) {
                            movementTask.UpdateEndPosition(controllerManager.pathfindingController, tempNode.worldPosition, movementTask.targetPos);
                        } else {
                            if (movementTask.targetObject != null) {
                                // Update the target position if the target has moved.
                                if (movementTask.targetObject.transform.position != movementTask.targetPos) {
                                    movementTask.UpdateEndPosition(controllerManager.pathfindingController, tempNode.worldPosition, movementTask.targetObject.transform.position);
                                    //Debug.Log("PCMOV - New start pos: " +);
                                }
                            }
                        }

                    }
                } else {
                    // If no nodes remain in the path, complete the task and complete all pre-defined actions.

                    CompleteElementaryTask("MovementNodesEmpty", movementTask);
                    if (movementTask.onArrivalAction != null) {
                        movementTask.onArrivalAction.Invoke();
                    }
                }
                break;
            case "waiting":
                ElementaryTask.WaitingTask waitingTask = task as ElementaryTask.WaitingTask;
                if (!waitingRequirementSet) {
                    loadingBarPresent = waitingTask.taskGroup.loadingBarInfo != null ? true : false;
                    if (waitingTask.listenerIdentifier != null) {
                        EventController.StartListening(waitingTask.listenerIdentifier, () => CompleteElementaryTask("WaitingEvent", waitingTask));
                    }
                    pawnModel.pawn.pawnStatus.currentStatus = waitingTask.targetState;
                    if (currentTaskGroup.loadingBarInfo != null) controllerManager.loadingBarController.PauseOrHideLoadingBar(currentTaskGroup.loadingBarInfo.ID, false);
                    waitingTask.location = pawnModel.pawn.pawnGameObject.transform.position;
                    waitingRequirementSet = true;
                }
                debugText += " with identifier of " + waitingTask.listenerIdentifier;
                // Check if a loading bar has expired and complete the task (prevents tasks stuck in waiting due to listener issues)
                if (waitingTask.taskGroup.loadingBarInfo == null && loadingBarPresent) CompleteElementaryTask("AutomaticOverride", waitingTask);
                break;

            case "amendInventory":
                ElementaryTask.InventoryTask inventoryTask = task as ElementaryTask.InventoryTask;
                StorageContainer pawnStorage = pawnModel.pawn.storageContainer;
                // If the task has a recipient storage
                int count = inventoryTask.count;
                Debug.Log("PWNC - Task id " + currentTaskGroup.uniqueID + " inventory transaction completed " + inventoryTask.transactionCompleted);
                if (!inventoryTask.transactionCompleted) {
                    inventoryTask.transactionCompleted = true;
                    if (inventoryTask.targetStorage != null) {
                        // If the pawn is removing the item from a storage container, amend the target storage, and then the pawn's.
                        int totalExtraction = inventoryTask.count;
                        // Positive count = pawn gaining items.
                        if (count > 0) {
                            InstantiatedResource instantiated = pawnStorage.inventory.Find(x => x.resourceData == inventoryTask.resourceData);
                            // If the pawn already has the item in their inventory, amend the required amount;
                            if (instantiated != null) {
                                if (instantiated.count > inventoryTask.count) {
                                    CompleteElementaryTask("InventoryFoundInPawn", inventoryTask);
                                    totalExtraction = 0;
                                } else totalExtraction -= instantiated.count;
                            }
                        }

                        int change = StorageFunctions.TryAmendStorage(inventoryTask.targetStorage, inventoryTask.resourceData, -totalExtraction);
                        if (change != 0) {
                            StorageFunctions.TryAmendStorage(pawnStorage, inventoryTask.resourceData, totalExtraction);
                            if (totalExtraction > 0) pawnStorage.inventory.Find(x => x.resourceData == inventoryTask.resourceData).reserved += totalExtraction;
                        }

                    } else {
                        StorageFunctions.TryAmendStorage(pawnStorage, inventoryTask.resourceData, count);
                    }

                    //controllerManager.storageController.TryIncreaseStorage(this.pawnModel.pawn.storageContainer, inventoryTask.resourceData, inventoryTask.count);
                    CompleteElementaryTask("InventoryAmended", inventoryTask);
                    if (pawnTaskSystem.AmendTaskGroupCount(6, 0) == 0) {
                        if ((float) pawnStorage.weightFill / (float) pawnStorage.weightCapacity >= 0.2f) {
                            Debug.Log("Trying to Dump storage");
                            pawnTaskSystem.AppendStorageDumpTransferTask(controllerManager);
                        }
                    }
                }
                break;
        }

        if (GeneralEnumStorage.debugActive) Debug.Log(debugText);
    }

    public void AmendGameSpeed() {
        currentGameSpeed = controllerManager.dateController.GameSpeedReturn();
        if (currentGameSpeed == 0) pauseTask = true;
        else pauseTask = false;

    }

    public bool RemoveTasksWithSameHandler(List<TaskGroup> tasks) {
        foreach (TaskGroup task in tasks) {
            if (task == pawnTaskSystem.mainTaskList.First.Value) {
                Debug.Log("PC - Task found at start of pawn list, being cancelled and completed.");
                task.CancelTask();
                if (task.loadingBarInfo != null) {
                    controllerManager.loadingBarController.AddOrRemoveBar(task.loadingBarInfo);
                }
                CompleteTaskGroup(task);
            }

            pawnTaskSystem.mainTaskList.Remove(task);
        }

        return true;
    }

    public bool ReassignFunctionHandler(FunctionHandler newFunctionHandler) {
        FunctionHandler currentFunctionHandler = this.specificFunctionHandler;
        if (newFunctionHandler.relatedBuild != null) {
            Debug.Log("Worker count: " + newFunctionHandler.workerCount);
            Debug.Log("Max count: " + newFunctionHandler.relatedBuild.structureData.maxWorkers);
            if (newFunctionHandler.workerCount + 1 <= newFunctionHandler.relatedBuild.structureData.maxWorkers) {

                if (pawnTaskSystem != null) {
                    foreach (TaskGroup task in pawnTaskSystem.mainTaskList) {
                        if (task.associatedHandler == currentFunctionHandler) {
                            if (task == pawnTaskSystem.mainTaskList.First.Value) {
                                task.CancelTask();
                            } else
                                pawnTaskSystem.ReturnTaskGroup(task, currentFunctionHandler);
                        }
                    }
                }

                newFunctionHandler.workerCount += 1;
                if (currentFunctionHandler != null) currentFunctionHandler.workerCount -= 1;
                this.specificFunctionHandler = newFunctionHandler;
                this.pawnModel.pawn.functionHandlerID = newFunctionHandler.id;
                //Debug.Log("Current handler Count: " + currentFunctionHandler.workerCount);
                //Debug.Log("New Handler count: " + newFunctionHandler.workerCount);
                return true;
            } else return false;
        } else {
            // If the inputted handler is null, assign the general function handler to the pawn.
            if (currentFunctionHandler != null) currentFunctionHandler.workerCount -= 1;
            newFunctionHandler.workerCount += 1;
            this.specificFunctionHandler = newFunctionHandler;
            this.pawnModel.pawn.functionHandlerID = newFunctionHandler.id;
            return true;
        }
    }
    public void InterruptOrResetCurrentTaskGroup() {
        if (currentTaskGroup == null) return;
        if (pawnTaskSystem.mainTaskList.First.Value != currentTaskGroup) {
            ElementaryTask currentTask = currentTaskGroup.associatedTasks[0];
            switch (currentTask.identifier) {
                case "movement":
                    ElementaryTask.MovementTask movementTask = currentTask as ElementaryTask.MovementTask;
                    movementTask.nodePath = null;
                    break;
                case "waiting":
                    ElementaryTask.WaitingTask waitingTask = currentTask as ElementaryTask.WaitingTask;
                    if (currentTaskGroup.loadingBarInfo != null) {
                        controllerManager.loadingBarController.PauseOrHideLoadingBar(currentTaskGroup.loadingBarInfo.ID, true);
                    }
                    waitingRequirementSet = false;
                    if (waitingTask.location != Vector3.zero) {
                        ElementaryTask.MovementTask moveBackTask = new ElementaryTask.MovementTask(waitingTask.location, null, 0);
                        moveBackTask.taskGroup = currentTaskGroup;
                        moveBackTask.taskGroupID = currentTaskGroup.uniqueID;
                        currentTaskGroup.associatedTasks.Insert(0, moveBackTask);
                    }
                    break;
                case "inventory":
                    ElementaryTask.InventoryTask inventoryTask = currentTask as ElementaryTask.InventoryTask;
                    if (inventoryTask.transactionCompleted) CompleteElementaryTask("InterruptionWithInvCompleted", inventoryTask);
                    break;
            }
        }

        CurrentTaskGroup = null;
        state = PawnModel.TaskState.AwaitingTask;
    }

    private void CompleteTaskGroup(TaskGroup task) {
        Debug.Log("PWNC - Pawn " + pawnModel.pawn.id + " completing task " + task.taskTypeID + " with a total of: " + task.associatedTasks.Count + " tasks remaining with id of " + task.uniqueID);
        pawnTaskSystem.CompleteTaskGroup(task);
        CurrentTaskGroup = null;
        state = PawnModel.TaskState.AwaitingTask;
        Debug.Log("PWNC - Pawn " + pawnModel.pawn.id + " state: " + state.ToString() + " current task group: " + currentTaskGroup);
    }

    public void CompleteElementaryTask(string completionSource, ElementaryTask completedTask) {
        // Find the elementary task within its task group, and remove the task.
        if (completedTask == null || currentTaskGroup == null) return;
        if (currentTaskGroup.associatedTasks.Contains(completedTask)) {
            Debug.Log("PWNC - Pawn " + pawnModel.pawn.id + " attempting to complete elementary task of " + completedTask.identifier + " as part of tasktype " + currentTaskGroup.taskTypeID + " with id " + currentTaskGroup.uniqueID);
            if (completedTask.identifier == "waiting") {
                ElementaryTask.WaitingTask waitingTask = completedTask as ElementaryTask.WaitingTask;
                EventController.StopListening(waitingTask.listenerIdentifier, () => CompleteElementaryTask(completionSource, waitingTask));
                waitingRequirementSet = false;
            }
            if (currentTaskGroup.associatedTasks.Count - 1 == 0 || currentTaskGroup.associatedTasks.Count == 0) {
                controllerManager.taskController.AppendOrRemoveID(currentTaskGroup.uniqueID);
                CompleteTaskGroup(currentTaskGroup);
            } else {
                currentTaskGroup.associatedTasks.Remove(completedTask);
            }
        }
    }
}

public class PawnModel {
    public Pawn pawn;
    public CurrentState currentState;

    public TaskState taskState;
    public enum TaskState {
        AwaitingTask,
        PerformingTask,
    }

    public enum CurrentState {
        Idle,
        Gathering,
        Creating,
        Sleeping,
        Eating,
    }
}