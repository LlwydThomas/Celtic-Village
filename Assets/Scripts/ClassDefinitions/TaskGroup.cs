using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public abstract class TaskGroup {

    public int taskTypeID;
    public int uniqueID;
    public FunctionHandler associatedHandler;
    public float weightRequiredToComplete = 0;
    public int priority;
    public List<ElementaryTask> associatedTasks = new List<ElementaryTask>();
    public List<RequiredResources> inputResources = new List<RequiredResources>();
    public List<RequiredResources> outputResources = new List<RequiredResources>();
    public string listeningIdentifier = null, cancelListener = null;
    public LoadingBarInfo loadingBarInfo;
    public GameObject associatedGameObject;
    private UnityAction cancelTask = null;

    public float SetWeightRequired(List<RequiredResources> res) {
        float weight = 0;
        foreach (RequiredResources resources in res) {
            weight += resources.count * resources.resource.weightPerItem;
        }
        return weight;
    }
    public class FloraDestructionTask : TaskGroup {
        public FloraData flora;
        public FloraDestructionTask(FloraItem floraItem, string _listenerIdentifier, ControllerManager controller, FunctionHandler _associatedHandler, int _uniqueID, int _priority = 9, Farm farm = null) {
            cancelListener = floraItem.floraData.uniqueType + floraItem.ID + "HasDied";
            UnityAction cancelTaskAction = null;
            cancelTaskAction += cancelTask;
            EventController.StartListening(cancelListener, cancelTaskAction);
            foreach (ProbableRequiredResource resource in floraItem.floraData.outputResources) {
                float roll = UnityEngine.Random.Range(0f, 1f);
                if (roll <= resource.probability) outputResources.Add(resource.requiredResource);
            }
            weightRequiredToComplete = SetWeightRequired(outputResources);
            listeningIdentifier = _listenerIdentifier;
            taskTypeID = 1;
            priority = _priority;
            associatedHandler = _associatedHandler;
            uniqueID = _uniqueID;
            UnityAction onArrival = null;
            flora = floraItem.floraData;
            if (associatedHandler != null) {
                cancelTask += delegate {
                    associatedHandler.InformOfCancellation(this, floraItem.gameObject);
                    EventController.StopListening(cancelListener, cancelTaskAction);
                };
            }
            onArrival += () => loadingBarInfo = controller.natureController.BeginFloraDestruction(floraItem, 3, farm, cancelTask);
            associatedTasks.Add(new ElementaryTask.MovementTask(floraItem.location, onArrival, 3));
            associatedTasks.Add(new ElementaryTask.WaitingTask(listeningIdentifier));
            foreach (RequiredResources req in outputResources) {
                associatedTasks.Add(new ElementaryTask.InventoryTask(req.count, req.resource));
            }
            associatedGameObject = floraItem.gameObject;
            SetElementaryParents();
        }
    }

    public class FloraCreationTask : TaskGroup {
        public Farm relFarm;
        public FloraData flora;
        public Vector3 targetLocation;
        public FloraCreationTask(Vector3 _targetLocation, FloraData floraData, ControllerManager controller, List<RequiredResources> inputs, List<ResourceTransaction> resourceTransactions, FunctionHandler _associatedHandler, int _uniqueID, int _priority = 9, Farm farm = null, Build build = null, float growthMultiplier = 1) {
            targetLocation = _targetLocation;
            listeningIdentifier = targetLocation.x + "," + targetLocation.y + floraData.uniqueType + "Planted";
            flora = floraData;
            taskTypeID = 2;
            priority = _priority;
            associatedHandler = _associatedHandler;
            uniqueID = _uniqueID;
            relFarm = farm;
            if (floraData.requiredToGrow.Count > 0) {
                //foreach (RequiredResources req in floraData.requiredToGrow) inputResources.Add(new RequiredResources(req.resource, req.count));
                inputResources.AddRange(floraData.requiredToGrow);
            }
            UnityAction onArrival = null;
            Vector3 movementTarget = targetLocation + new Vector3(-1, 0, 0);
            inputResources = inputs;
            if (resourceTransactions != null) {
                // Convert all resource transactions into movement inventory tasks.
                foreach (ResourceTransaction resourceTransaction in resourceTransactions) {
                    StorageContainer storage = controller.storageController.FindStorageByID(resourceTransaction.referenceResource.storageContainerID);
                    ElementaryTask.MovementTask movementTask = new ElementaryTask.MovementTask(storage.location, null, 0);
                    associatedTasks.Add(movementTask);
                    ElementaryTask.InventoryTask inventoryTask = new ElementaryTask.InventoryTask(resourceTransaction.resourceChange, resourceTransaction.referenceResource.resourceData, storage);
                    inventoryTask.walkingTask = movementTask;
                    associatedTasks.Add(inventoryTask);
                }
            }
            if (farm == null) {
                if (build != null) {
                    // If the flora is not in a farm, and has an associated build then plant the flora item, and add it to the build's queue.
                    onArrival += (delegate {
                        PlantedFloraItem plantedFloraItem = controller.natureController.PlantFloraItem(floraData.ID, targetLocation, build : build);
                        build.functionHandler.AppendGameObjectToQueue(plantedFloraItem.floraItem.gameObject);
                        loadingBarInfo = plantedFloraItem.loadingBarInfo;
                        cancelTask += delegate {
                            build.functionHandler.RemoveGameObjectFromList(plantedFloraItem.floraItem.gameObject);
                            controller.natureController.DestroyFloraItem(plantedFloraItem.floraItem);
                        };
                    });
                    build.functionHandler.RemoveNodeFromRestriction(targetLocation);
                } else {
                    onArrival += (() => controller.natureController.PlantFloraItem(floraData.ID, targetLocation));
                }
                associatedTasks.Add(new ElementaryTask.MovementTask(movementTarget, onArrival, 3));
            } else {
                //Debug.Log("CropTileParent" + farm.cropTileParent.name);
                onArrival += delegate {
                    PlantedFloraItem plantedFloraItem = controller.natureController.PlantFloraItem(floraData.ID, targetLocation, parent : farm.cropParent.transform, farm : farm, growthMultiplier : growthMultiplier);
                    farm.cropList.Add(plantedFloraItem.floraItem);
                    loadingBarInfo = plantedFloraItem.loadingBarInfo;
                    cancelTask += delegate {
                        farm.farmingHandler.RemoveGameObjectFromList(plantedFloraItem.floraItem.gameObject);
                        controller.natureController.DestroyFloraItem(plantedFloraItem.floraItem);
                    };
                    Debug.Log("FC - Farm " + farm.ID + " crop list count: " + farm.cropList.Count);
                };
                associatedTasks.Add(new ElementaryTask.MovementTask(movementTarget, onArrival, 3));
            }
            associatedTasks.Add(new ElementaryTask.WaitingTask(listeningIdentifier));
            foreach (RequiredResources required in inputResources) {
                associatedTasks.Add(new ElementaryTask.InventoryTask(-required.count, required.resource));
            }
            SetElementaryParents();
        }
    }

    public class MiningTask : TaskGroup {
        public ResourceData miningOutput;
        public MiningTask(ControllerManager controllerManager, Vector3 _targetMiningLoc, ResourceData _outputResource, int count, FunctionHandler _funcHandler, int _uniqueID, string _listeningIdentifier) {
            outputResources.Add(new RequiredResources(_outputResource, count));
            listeningIdentifier = _listeningIdentifier;
            taskTypeID = 5;
            priority = 9;
            uniqueID = _uniqueID;
            miningOutput = _outputResource;
            associatedHandler = _funcHandler;
            UnityAction onArrival = null;
            UnityAction onLoadingComplete = null;
            onLoadingComplete += delegate { EventController.TriggerEvent(listeningIdentifier); };
            onArrival += (delegate {
                loadingBarInfo = controllerManager.loadingBarController.GenerateLoadingBar(_targetMiningLoc, onLoadingComplete, 1f);
            });
            associatedHandler = _funcHandler;

            associatedTasks.Add(new ElementaryTask.MovementTask(_targetMiningLoc, onArrival, 0));
            associatedTasks.Add(new ElementaryTask.WaitingTask(listeningIdentifier));
            associatedTasks.Add(new ElementaryTask.InventoryTask(count, _outputResource));
            SetElementaryParents();
        }
    }

    public class FishingTask : TaskGroup {
        public FishData currentFish;
        public FishingTask(ControllerManager controllerManager, Vector3 _targetFishingLocation, FishData fish, FunctionHandler _functionHandler, int _uniqueID, string _listeningIdentifier) {
            RequiredResources fishOutput = fish.outputResource;
            outputResources.Add(new RequiredResources(fishOutput.resource, fishOutput.count));
            listeningIdentifier = _listeningIdentifier;
            associatedHandler = _functionHandler;
            taskTypeID = 8;
            priority = 9;
            uniqueID = _uniqueID;
            currentFish = fish;
            UnityAction onArrival = null;
            UnityAction onLoadingComplete = null;
            onLoadingComplete += delegate { EventController.TriggerEvent(listeningIdentifier); };
            onArrival += (delegate {
                loadingBarInfo = controllerManager.loadingBarController.GenerateLoadingBar(_targetFishingLocation, onLoadingComplete, 1f);
            });
            associatedHandler = _functionHandler;

            associatedTasks.Add(new ElementaryTask.MovementTask(_targetFishingLocation, onArrival, 0));
            associatedTasks.Add(new ElementaryTask.WaitingTask(listeningIdentifier));
            associatedTasks.Add(new ElementaryTask.InventoryTask(fishOutput.count, fishOutput.resource));
            SetElementaryParents();
        }
    }

    public class TransferInventoryTask : TaskGroup {
        public ResourceData transferredResource;
        public TransferInventoryTask(StorageContainer _targetStorage, string _listenerIdentifier, Vector3 _storageLoc, ResourceData resource, int count, int _uniqueID, int _priority = 4) {
            listeningIdentifier = _listenerIdentifier;
            weightRequiredToComplete = 0;
            taskTypeID = 6;
            priority = _priority;
            uniqueID = _uniqueID;
            transferredResource = resource;
            associatedTasks.Add(new ElementaryTask.MovementTask(_storageLoc, null, 2));
            associatedTasks.Add(new ElementaryTask.InventoryTask(count, resource, _targetStorage));
            SetElementaryParents();
        }
    }

    public class CraftingTask : TaskGroup {
        public CraftingData recipeData;
        public CraftingTask(ControllerManager controllerManager, Node node, CraftingData craftingData, List<RequiredResources> inputs, FunctionHandler handler, List<ResourceTransaction> transactions, int _uniqueID) {
            taskTypeID = 7;
            priority = 4;
            uniqueID = _uniqueID;
            inputResources = inputs;
            outputResources = craftingData.outputs;
            recipeData = craftingData;
            associatedHandler = handler;
            string _listeningIdentifier = "CraftingTask" + _uniqueID + "CompletedTransfer";
            listeningIdentifier = _listeningIdentifier;
            string debug = ("TG - Trying to add a crafting task of id " + uniqueID);
            Vector3 loc = node.worldPosition;
            foreach (ResourceTransaction resourceTransaction in transactions) {
                StorageContainer storage = controllerManager.storageController.FindStorageByID(resourceTransaction.referenceResource.storageContainerID);
                associatedTasks.Add(new ElementaryTask.MovementTask(storage.location, null, 0));
                associatedTasks.Add(new ElementaryTask.InventoryTask(resourceTransaction.resourceChange, resourceTransaction.referenceResource.resourceData, storage));
            }
            int craftingQuality = ResourceFunctions.DetermineCraftingQuality(inputs);
            UnityAction onArrival = null;
            UnityAction onCompletion = null;
            onArrival += (delegate {
                loadingBarInfo = controllerManager.loadingBarController.GenerateLoadingBar(loc, onCompletion, craftingData.craftingDuration);
            });
            onCompletion += (delegate { EventController.TriggerEvent(listeningIdentifier); });
            associatedTasks.Add(new ElementaryTask.MovementTask(loc, onArrival, 0));
            foreach (RequiredResources required in inputResources) {
                associatedTasks.Add(new ElementaryTask.InventoryTask(-required.count, required.resource));
            }
            associatedTasks.Add(new ElementaryTask.WaitingTask(listeningIdentifier));
            foreach (RequiredResources output in outputResources) {
                int count;
                if (craftingData.qualityOutputs.Length > 0) {
                    count = craftingData.qualityOutputs[craftingQuality - 1].count;
                } else count = output.count;
                associatedTasks.Add(new ElementaryTask.InventoryTask(count, output.resource));
            }
            SetElementaryParents();
        }
    }
    public class SleepingTask : TaskGroup {
        public SleepingTask(Node bedLocation, int _uniqueID, Pawn pawn) {
            priority = 2;
            taskTypeID = 4;
            uniqueID = _uniqueID;
            listeningIdentifier = "pawn" + pawn.id + "FullRest";
            if (bedLocation != null) associatedTasks.Add(new ElementaryTask.MovementTask(bedLocation.worldPosition, null, 0));
            associatedTasks.Add(new ElementaryTask.WaitingTask(listeningIdentifier, PawnStatus.CurrentStatus.Sleeping));
            SetElementaryParents();
        }
    }
    public class ConsumeFoodTask : TaskGroup {
        public ResourceData foodItem;
        public ConsumeFoodTask(ControllerManager controllerManager, Vector3 _storageLoc, StorageContainer _storageContainer, int requiredFood, ResourceData _foodItem, Pawn pawn, string _listenerIdentifier, int _uniqueID) {
            weightRequiredToComplete = SetWeightRequired(outputResources);
            priority = 1;
            taskTypeID = 3;
            uniqueID = _uniqueID;
            foodItem = _foodItem;
            UnityAction onArrival = null;
            UnityAction onLoadingComplete = null;
            listeningIdentifier = _listenerIdentifier;
            onLoadingComplete += (delegate {
                controllerManager.skillsController.ManuallyAmendStatusLevels(pawn, _foodItem.hungerRegeneration);
                EventController.TriggerEvent(listeningIdentifier);
            });
            onArrival += (delegate { loadingBarInfo = controllerManager.loadingBarController.GenerateLoadingBar(_storageLoc, onLoadingComplete, 0.5f); });
            inputResources.Add(new RequiredResources(_foodItem, requiredFood));
            associatedTasks.Add(new ElementaryTask.MovementTask(_storageLoc, onArrival, 2));
            associatedTasks.Add(new ElementaryTask.InventoryTask(requiredFood, _foodItem, _storageContainer));

            associatedTasks.Add(new ElementaryTask.WaitingTask(listeningIdentifier, PawnStatus.CurrentStatus.Eating));
            associatedTasks.Add(new ElementaryTask.InventoryTask(-requiredFood, _foodItem));
            SetElementaryParents();
        }
    }

    public class SlayAnimalTask : TaskGroup {
        public NPC.AnimalNPC targetAnimal;
        public SlayAnimalTask(ControllerManager controllerManager, NPC.AnimalNPC _targetAnimal, List<RequiredResources> outputs, string _listenerIdentifier, int _uniqueID) {
            priority = 9;
            taskTypeID = 10;
            targetAnimal = _targetAnimal;
            outputResources.AddRange(outputs);
            uniqueID = _uniqueID;
            Vector3 targetPosition = targetAnimal.npcObject.transform.position;
            UnityAction onArrival = null;
            UnityAction onLoadingComplete = null;
            listeningIdentifier = _listenerIdentifier;
            onLoadingComplete += (delegate {
                EventController.TriggerEvent(listeningIdentifier);
                controllerManager.nPCController.SlayAnimalNPC(targetAnimal, listeningIdentifier);
            });
            onArrival += (delegate {
                loadingBarInfo = controllerManager.loadingBarController.GenerateLoadingBar(targetPosition, onLoadingComplete, targetAnimal.animalData.timeToKill, targetAnimal.npcObject);
                targetAnimal.nPCLogicController.CancelNPCMovement(true);
            });
            associatedTasks.Add(new ElementaryTask.MovementTask(targetPosition, onArrival, 1, targetAnimal.npcObject));
            associatedTasks.Add(new ElementaryTask.WaitingTask(listeningIdentifier));
            foreach (RequiredResources output in outputResources) {
                associatedTasks.Add(new ElementaryTask.InventoryTask(output.count, output.resource));
            }
            SetElementaryParents();
        }
    }

    protected void SetElementaryParents() {
        foreach (ElementaryTask elementaryTask in associatedTasks) {
            elementaryTask.taskGroup = this;
            elementaryTask.taskGroupID = uniqueID;
        }
    }

    public void CancelTask() {
        if (cancelTask != null) cancelTask.Invoke();
        if (loadingBarInfo != null) loadingBarInfo.tagForDeletion = true;
        EventController.TriggerEvent(listeningIdentifier);
    }
}