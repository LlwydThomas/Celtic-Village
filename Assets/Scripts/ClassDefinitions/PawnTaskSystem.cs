using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PawnTaskSystem {

    public LinkedList<ElementaryTask> taskList = new LinkedList<ElementaryTask>();
    public LinkedList<TaskGroup> mainTaskList = new LinkedList<TaskGroup>();
    public Dictionary<int, int> taskQueueTypes = new Dictionary<int, int>();
    private Pawn pawn;
    public ElementaryTask RequestNextTask() {
        if (taskList.Count > 0) {
            // Assign pawn first ElementaryTask.
            ElementaryTask ElementaryTask = taskList.First.Value;
            taskList.RemoveFirst();
            return ElementaryTask;
        } else {
            return null;
        }
    }

    public void SetPawn(Pawn pawn) {
        this.pawn = pawn;
    }

    public void ReturnTaskGroup(TaskGroup taskGroup, FunctionHandler functionHandler) {
        foreach (ElementaryTask elementary in taskGroup.associatedTasks) {
            taskList.Remove(elementary);
        }
        mainTaskList.Remove(taskGroup);
        functionHandler.ReceiveReturnedTaskGroup(taskGroup);
    }

    // This is used to request tasks externally from the function handler, such as sleeping and eating.
    public bool QueuePersonalTask(ManagerReferences managerReferences, int id = -1) {
        ControllerManager controller = managerReferences.controllerManager;
        UiManagement uiManagement = managerReferences.uiManagement;

        List<TaskGroup> returnTaskGroup = new List<TaskGroup>();
        TaskData taskType = controller.taskController.FindTaskDataByID(id);
        Debug.Log("QueuePersonalTasks has been run. Adding task of " + taskType.taskName);
        if (id != -1) {
            int taskID = controller.taskController.ReturnAvailableID();
            switch (id) {
                case 4:
                    Vector3 position = pawn.pawnController.gameObject.transform.position;
                    if (pawn.sleepingNode == null) {
                        pawn.sleepingNode = controller.buildingController.LocateFreeSleepingNode(pawn);
                        if (pawn.sleepingNode == null) uiManagement.warningLogView.AppendMessageToLog("PawnUnableToFindSleepingSpot", pawn.pawnController.gameObject.transform.position, -1, targetObj : pawn.pawnGameObject);
                        else {
                            pawn.sleepingNodeID = pawn.sleepingNode.id;
                            position = pawn.sleepingNode.worldPosition;
                        }
                    }
                    TaskGroup.SleepingTask sleepingTask = new TaskGroup.SleepingTask(pawn.sleepingNode, taskID, pawn);
                    sleepingTask.priority = 2;
                    returnTaskGroup.Add(sleepingTask);
                    // sleeping
                    break;

                case 3:
                    // Locate the best source of food within storage and the pawn's inventory.
                    InstantiatedResource res = controller.storageController.FindBestFood(new int[] { pawn.storageContainer.id });
                    if (res != null) {
                        StorageContainer target = controller.storageController.FindStorageByID(res.storageContainerID);
                        Vector3 targetLoc;
                        if (!target.stationary) targetLoc = pawn.pawnGameObject.transform.position;
                        else targetLoc = target.location;
                        TaskGroup.ConsumeFoodTask consumeFoodTask = new TaskGroup.ConsumeFoodTask(managerReferences.controllerManager, targetLoc, target, 1, res.resourceData, pawn, pawn.id + "Eaten" + res.resourceData.resourceName + taskID, taskID);
                        res.reserved += 1;
                        consumeFoodTask.priority = 1;
                        returnTaskGroup.Add(consumeFoodTask);
                    } else {
                        uiManagement.warningLogView.AppendMessageToLog("PawnUnableToFindFood", pawn.pawnController.gameObject.transform.position, -1, targetObj : pawn.pawnGameObject);
                    }

                    break;

                    // case 6:
                    //     // InventoryWithMovement.

                    //     break;
                default:
                    break;
            }

        }

        if (returnTaskGroup.Count > 0) {
            AppendTaskGroup(returnTaskGroup);
            return true;
        } else return false;
    }

    public bool AppendStorageDumpTransferTask(ControllerManager controller) {
        float weight = pawn.storageContainer.weightFill;
        if (pawn.storageContainer.inventory.Count < 1) return true;
        List<TaskGroup> taskList = new List<TaskGroup>();
        List<ResourceTransaction> transactions = new List<ResourceTransaction>();

        foreach (InstantiatedResource resource in pawn.storageContainer.inventory) {
            int countRemaining = resource.count - resource.reserved;
            List<InstantiatedResource> existingStorage = controller.storageController.StoragesWithResource(resource.resourceData);
            Debug.Log("PTS - Attempting to dump " + resource.resourceData.resourceName);
            // If an existing storage already contains this resource, attempt to merge the pawn's storage with the existing storage. 
            if (existingStorage != null) {
                foreach (InstantiatedResource existing in existingStorage) {
                    Debug.Log("PTS - Existing resource of type " + existing.resourceData.resourceName + " found in storage container " + existing.storageContainerID);
                    StorageContainer storageContainer = controller.storageController.FindStorageByID(existing.storageContainerID);
                    int maxCount = Mathf.FloorToInt((storageContainer.weightCapacity - storageContainer.weightFill) / existing.resourceData.weightPerItem);
                    int change = maxCount <= countRemaining ? maxCount : countRemaining;
                    ResourceTransaction resourceTransaction = new ResourceTransaction(existing, change);
                    transactions.Add(resourceTransaction);
                    countRemaining -= change;
                    if (countRemaining <= 0) break;
                }
            }

            // If the pawn still has outstanding resources, find the emptiest available storages and try to offload all the resources into these storages.
            List<StorageContainer> emptiestStorages = controller.storageController.FindEmptiestStorages();
            foreach (StorageContainer storageContainer1 in emptiestStorages) {
                int maxCount = Mathf.FloorToInt((storageContainer1.weightCapacity - storageContainer1.weightFill) / resource.resourceData.weightPerItem);
                if (maxCount < 1) continue;
                int change = maxCount <= countRemaining ? maxCount : countRemaining;
                InstantiatedResource relevantResource = storageContainer1.inventory.Find(x => x.resourceData == resource.resourceData);
                if (relevantResource == null) {
                    relevantResource = new InstantiatedResource(resource.resourceData, 0);
                    relevantResource.SetStorageContainer(storageContainer1);
                    storageContainer1.inventory.Add(relevantResource);
                }

                ResourceTransaction resourceTransaction = new ResourceTransaction(relevantResource, change);
                transactions.Add(resourceTransaction);
                countRemaining -= change;
                if (countRemaining <= 0) break;
            }
        }

        // Convert all of the resource transactions specified above into inventory transfer tasks and append these to the pawn's queue.
        Debug.Log("PTS - Appending a total of " + transactions.Count + " transactions to Pawn " + pawn.id);
        foreach (ResourceTransaction resourceTransaction1 in transactions) {
            if (resourceTransaction1.resourceChange == 0) continue;
            int taskID = UnityEngine.Random.Range(0, 10000);
            while (controller.taskController.CheckTaskID(taskID)) {
                taskID = UnityEngine.Random.Range(0, 10000);
            }
            InstantiatedResource ownedResource = pawn.storageContainer.inventory.Find(x => x.resourceData == resourceTransaction1.referenceResource.resourceData);
            ownedResource.reserved += resourceTransaction1.resourceChange;
            StorageContainer storageContainer = controller.storageController.FindStorageByID(resourceTransaction1.referenceResource.storageContainerID);
            taskList.Add(new TaskGroup.TransferInventoryTask(storageContainer, "", storageContainer.location, resourceTransaction1.referenceResource.resourceData, -resourceTransaction1.resourceChange, taskID));
        }

        AppendTaskGroup(taskList);
        pawn.pawnController.timeSinceStorageDump = 0f;
        return taskList.Count > 0 ? true : false;
    }

    public TaskGroup RequestNextTaskGroup(bool peek = false) {
        if (mainTaskList.Count > 0) {
            TaskGroup newTaskGroup = mainTaskList.First.Value;
            //if (!peek) mainTaskList.RemoveFirst();
            return newTaskGroup;
        } else {
            return null;
        }
    }

    public List<TaskGroup> RequestTaskGroups(int count) {
        List<TaskGroup> taskReturn = new List<TaskGroup>();
        if (mainTaskList.Count >= count) {
            foreach (TaskGroup task in mainTaskList) {
                mainTaskList.Remove(task);
                taskReturn.Add(task);
            }
        } else return null;
        return taskReturn;
    }

    public bool AppendTaskGroup(List<TaskGroup> tasks, bool first = false) {
        Debug.Log("Attempted to add " + tasks.Count + " tasks. Total taskGroup count: " + mainTaskList.Count);
        foreach (TaskGroup taskGroup in tasks) {
            if (first) mainTaskList.AddFirst(taskGroup);
            else mainTaskList.AddLast(taskGroup);
            AmendTaskGroupCount(taskGroup.taskTypeID, 1);
        }
        if (mainTaskList.Count > 0) {
            if (mainTaskList.Count > 1) {
                TaskGroup taskGroup = mainTaskList.First.Value;
                mainTaskList = new LinkedList<TaskGroup>(QuickSort(new List<TaskGroup>(mainTaskList)));
                if (mainTaskList.First.Value != taskGroup) pawn.pawnController.InterruptOrResetCurrentTaskGroup();
            }
            return true;
        } else return false;
    }

    // Start Citation
    public static List<TaskGroup> QuickSort(List<TaskGroup> list) {
        // Sort a task list based on the priority of the tasks.
        string preString = "Pre-List: ";
        foreach (TaskGroup task in list) {
            preString += task.priority + " (" + task.taskTypeID + ") " + ", ";
        }

        if (list.Count <= 1) return list;
        int pivotPosition = list.Count / 2;
        TaskGroup pivotClass = list[pivotPosition];
        int pivotValue = pivotClass.priority;
        list.RemoveAt(pivotPosition);
        List<TaskGroup> smaller = new List<TaskGroup>();
        List<TaskGroup> larger = new List<TaskGroup>();
        foreach (TaskGroup item in list) {
            if (item.priority < pivotValue) {
                smaller.Add(item);
            } else {
                larger.Add(item);
            }
        }
        List<TaskGroup> sorted = QuickSort(smaller);
        sorted.Add(pivotClass);
        sorted.AddRange(QuickSort(larger));
        string postString = "Post-List: ";
        foreach (TaskGroup task in sorted) {
            postString += task.priority + ", ";
        }
        Debug.Log("(" + preString + ") => (" + postString + ")");
        return sorted;
    }

    // End Citation
    // A Mitov (2013) Implementation of Quicksort algorithm with List<int> data structure in C# (version 1) [Source Code] https://gist.github.com/aleksmitov/4614041

    public void AppendTask(ElementaryTask ElementaryTask) {
        taskList.AddLast(ElementaryTask);
    }

    public void CompleteTaskGroup(TaskGroup taskGroup) {
        AmendTaskGroupCount(taskGroup.taskTypeID, -1);
        mainTaskList.Remove(taskGroup);
        Debug.Log("PTS - Task group id of " + taskGroup.uniqueID + " has been removed. Remaining queue: " + mainTaskList.Count);
    }

    public int AmendTaskGroupCount(int id, int change) {
        if (!taskQueueTypes.ContainsKey(id)) {
            taskQueueTypes.Add(id, 0);
        }
        if (taskQueueTypes[id] + change >= 0) {
            taskQueueTypes[id] += change;
        } else taskQueueTypes[id] = 0;
        Debug.Log("PTS - Task group id of " + id + " has " + taskQueueTypes[id] + " tasks for pawn " + pawn.id);
        return taskQueueTypes[id];
    }

    /*
        private List<ElementaryTask> taskList; //Tasks ready to be executed.
        private List<QueuedTask> taskQueue; // Any queued ElementaryTask must be validated prior to dequeue.
        public abstract class ElementaryTask {
            public class MoveToPos : ElementaryTask {
                public Vector3 targetPosition;
            }
        }
        

        public void EnqueueTask(Func<ElementaryTask> tryGetTaskFunc){
            QueuedTask queuedTask = new QueuedTask(tryGetTaskFunc);
            taskQueue.Add(queuedTask);
        }
        public void DequeueTasks(){
            for (int i = 0; i < taskQueue.Count; i++){
                QueuedTask queuedTask = taskQueue[i];
                ElementaryTask ElementaryTask = queuedTask.TryDequeueTask();
                if(ElementaryTask != null){
                    AppendTask(ElementaryTask);
                    taskQueue.RemoveAt(i);
                    i--;
                    Debug.Log("Dequeued");
                } else{
                    // Null ElementaryTask.
                }
            }
        }

        
        public class QueuedTask {
            private Func<ElementaryTask> tryParseTask;
            public QueuedTask (Func<ElementaryTask> tryParseTask) {
                this.tryParseTask = tryParseTask;
            }
            public ElementaryTask TryDequeueTask () {
                return tryParseTask ();
            }
        } */
}