using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class StorageController : MonoBehaviour {
    // Start is called before the first frame update
    public ManagerReferences references;
    private UiManagement uiManagement;
    private ControllerManager controllerManager;
    public List<StorageContainer> storageContainers;
    public Dictionary<int, StorageContainer> storageLookup = new Dictionary<int, StorageContainer>();
    public List<InstantiatedResource> currentTotal = new List<InstantiatedResource>();
    private void Awake() {
        EventController.StartListening("mapCompleted", ResetStorageList);
        uiManagement = references.uiManagement;
        controllerManager = references.controllerManager;
    }
    /* public int TryAmendStorage(StorageContainer storageContainer, ResourceData resourceData, int change, bool transferMax = true, bool amendReserved = true) {
        string debugText = "STC - Old Inventory:";
        foreach (InstantiatedResource res in storageContainer.inventory) {
            debugText += " " + res.resourceData.resourceName + ": " + res.count;
        }

        debugText += "; New Inventory";

        // Determine all values required to calculate whether a storage container can accomodate new resources.
        float priorFill = storageContainer.weightFill;
        int priorInventoryCount = storageContainer.inventory.Count;
        float weightChange = change * resourceData.weightPerItem;
        float remainingWeight = storageContainer.weightCapacity - storageContainer.weightFill;
        int maxTransfer = Mathf.FloorToInt(remainingWeight / resourceData.weightPerItem);
        Debug.Log("Division: " + remainingWeight + "/" + resourceData.weightPerItem + ", Max Transfer: " + maxTransfer);
        Debug.Log("STC - trying to add " + change + " " + resourceData.resourceName + " to storage " + storageContainer.id);
        // If the maximum transfer of an item is zero and the requested change is more than 0, return.
        if (change >= 0 && maxTransfer == 0) return 0;

        InstantiatedResource existingResource = storageContainer.inventory.Find(x => x.resourceData == resourceData);
        // Determine whether the target storage already has an entry for the specified resource.
        if (existingResource != null) {
            Debug.Log("STC - existing count = " + existingResource.count + ", attempted addition = " + change);
            if (existingResource.count + change >= 0) {
                // 
                if (CheckStorageAvailable(storageContainer, weightChange)) {
                    existingResource.count += change;
                    if (amendReserved) {
                        if (existingResource.reserved > 0 && change < 0) {
                            existingResource.reserved = Mathf.Clamp(existingResource.reserved + change, 0, existingResource.count);
                        }
                    }
                } else {
                    if (transferMax) {
                        change = maxTransfer;
                        TryAmendStorage(storageContainer, resourceData, change);
                        return change;
                    } else return 0;
                }
            } else return 0;
        } else {
            if (change >= 0) {
                Debug.Log("STC - attempting to add ");
                if (CheckStorageAvailable(storageContainer, weightChange)) {
                    Debug.Log("Passed Storage Check For New Res");
                    existingResource = new InstantiatedResource(resourceData, change);
                    storageContainer.inventory.Add(existingResource);
                } else {
                    change = maxTransfer;
                    TryAmendStorage(storageContainer, resourceData, change);
                    return change;
                }
            } else return 0;
        }
        if (existingResource != null) {
            if (existingResource.count == 0) storageContainer.inventory.Remove(existingResource);
            existingResource.storageContainerID = storageContainer.id;
        }
        storageContainer.weightFill += weightChange;
        float postFill = storageContainer.weightFill;
        int postInventoryCount = storageContainer.inventory.Count;
        foreach (InstantiatedResource res in storageContainer.inventory) {
            debugText += " " + res.resourceData.resourceName + ": " + res.count;
        }
        Debug.Log(debugText);
        Debug.Log("STC - Original count for " + storageContainer.id + ": " + priorInventoryCount + ", Post count: " + postInventoryCount);
        Debug.Log("ID: " + storageContainer.id + ", Prior Fill: " + priorFill + ", Post Fill: " + postFill);
        Debug.Log("ID: " + storageContainer.id + ", New Count: " + storageContainer.inventory.Count);
        EventController.TriggerEvent(storageContainer.id + "storageAmended");
        EventController.TriggerEvent("resourceChange");
        return change;
    } */

    public StorageContainer FindStorageByID(int id) {
        if (storageLookup.ContainsKey(id)) return storageLookup[id];
        else return null;
    }

    public bool CheckIfResourcesInStorage(List<RequiredResources> reqs, int[] ids = null) {
        if (StorageFunctions.CheckIfResourcesAvailable(reqs, CompileTotalResourceList(ids))) return true;
        else return false;
    }

    public InstantiatedResource FindResourcesOfSubType(ResourceData.SubCategory subCategory, int count) {
        List<InstantiatedResource> total = CompileTotalResourceList(stationary: 1);
        total = total.FindAll(x => x.resourceData.subCategory == subCategory && x.count > count);
        return total[0];
    }

    public bool CheckIfSubCategoryInStorage(List<CraftingData.ResourceByCategory> categories) {
        List<InstantiatedResource> total = CompileTotalResourceList(stationary: 1);
        foreach (CraftingData.ResourceByCategory category in categories) {
            if (!CheckIfSubCategoryInStorage(category, total)) return false;
        }
        return true;
    }

    public bool CheckIfSubCategoryInStorage(CraftingData.ResourceByCategory category, List<InstantiatedResource> total = null) {
        if (total == null) total = CompileTotalResourceList(stationary: 1);
        if (total.Find(x => x.resourceData.subCategory == category.subCategory && x.count - x.reserved >= category.count) != null) return true;
        else return false;
    }

    public bool TryDeleteItemFromStorage(StorageContainer storageContainer, RequiredResources res) {
        InstantiatedResource resource = storageContainer.inventory.Find(x => x.resourceData == res.resource);
        if (resource != null) {
            if (resource.reserved == 0) {
                StorageFunctions.TryAmendStorage(storageContainer, res.resource, -res.count);
                return true;
            } else return false;
        } else return false;

    }

    public List<InstantiatedResource> CompileTotalResourceList(int[] storageIDs = null, int stationary = 1, bool reservedTotal = false) {
        List<StorageContainer> checkableList = new List<StorageContainer>();
        checkableList = ReturnStorages(stationary);

        if (storageIDs != null) {
            foreach (int id in storageIDs) {
                StorageContainer tempStorage = FindStorageByID(id);
                if (tempStorage != null) {
                    if (!checkableList.Contains(tempStorage)) checkableList.Add(tempStorage);
                }
            }
        }
        List<InstantiatedResource> compiledList = StorageFunctions.CompileResourceList(checkableList, reservedTotal);
        currentTotal = compiledList;
        return compiledList;
    }

    public float ReturnTotalStorageCapacity(int stationary) {
        float cap = 0;
        List<StorageContainer> storagesToCheck = ReturnStorages(stationary);
        foreach (StorageContainer storage in storagesToCheck) {
            cap += storage.weightCapacity;
        }
        return cap;
    }

    public float ReturnValueOfItems(bool stationary) {
        float totalValue = 0;
        List<StorageContainer> possibleStorages;
        if (stationary) possibleStorages = ReturnStorages(1);
        else possibleStorages = storageContainers;

        foreach (StorageContainer storageContainer in possibleStorages) {
            foreach (InstantiatedResource res in storageContainer.inventory) totalValue += (res.count * res.resourceData.resourceValue);
        }
        return totalValue;
    }

    public float ReturnStorageUsed() {
        float used = 0;;
        foreach (StorageContainer storageContainer in ReturnStorages(1)) {
            used += storageContainer.weightFill;
        }
        return used;
    }

    public bool FindAndExtractResourcesFromStorage(List<RequiredResources> _requiredResources, bool overwriteRequired = false) {
        List<InstantiatedResource> currentTotal = CompileTotalResourceList(reservedTotal: true, stationary: -1);
        return StorageFunctions.FindAndExtractResourcesFromStorage(_requiredResources, ReturnStorages(-1), currentTotal, overwriteRequired);
        /* List<InstantiatedResource> extractionContainers = new List<InstantiatedResource>();
        List<RequiredResources> foundItems = new List<RequiredResources>();
        List<RequiredResources> requiredResources = _requiredResources;
        if (overwriteRequired) {
            requiredResources = GeneralFunctions.CopyResourceList(_requiredResources);
        }
        Debug.Log("STC - Finding and extracting " + requiredResources.Count + " resources from a total of " + currentTotal.Count + " resources found in storage.");
        if (requiredResources.Count == 0) {
            Debug.Log("STC - No resources to process");
            return true;
        }
        string debugString = "Extracted:";
        if (StorageFunctions.CheckIfResourcesAvailable(requiredResources, currentTotal)) {
            foreach (RequiredResources res in requiredResources) {
                List<InstantiatedResource> storages = StoragesWithResource(res.resource, res.count, 1);
                foreach (InstantiatedResource instantiated in storages) {
                    Debug.Log("STC - Storage " + instantiated.storageContainerID + " of " + instantiated.resourceData.resourceName + " has a count of " + instantiated.count);
                }
                if (storages != null) {
                    extractionContainers.AddRange(storages);
                    foundItems.Add(new RequiredResources(res.resource, res.count));
                }
            }
        } else {
            Debug.Log("STC - Resource not available in storages.");
            return false;
        }

        Debug.Log("STC - Required Resources: " + requiredResources.Count + ", Found Items: " + foundItems.Count + "; Storages Found: " + extractionContainers.Count);
        if (requiredResources.Count == foundItems.Count) {
            for (int i = 0; i < extractionContainers.Count; i++) {
                InstantiatedResource extraction = extractionContainers[i];
                StorageContainer storage = FindStorageByID(extraction.storageContainerID);
                RequiredResources currentResource = requiredResources.Find(x => x.resource == extraction.resourceData);
                if (currentResource != null) {
                    int change = StorageFunctions.TryAmendStorage(storage, currentResource.resource, -currentResource.count, true, false);
                    if (change == 0) Debug.Log("STC - Unable to amend inventory");
                    else debugString += " " + change + " " + currentResource.resource.resourceName + " from " + storage.id + " ";
                    currentResource.count += change;
                    Debug.Log("STC - storage " + storage.id + " amended by " + change + " for resource " + currentResource.resource.resourceName);
                } else {
                    Debug.Log("STC - No resource found in current storage");
                    return false;
                }
            }

            foreach (RequiredResources res in requiredResources) {
                if (res.count <= 0) {
                    Debug.Log("STC - All resources have been found.");
                    Debug.Log("STC - " + debugString);
                } else Debug.Log("STC - Could not find all resources");
            }
            return true;
        } else {
            Debug.Log("STC - Required resource count does not equal found item count: " + requiredResources.Count + " vs " + foundItems.Count);
            return false;
        } */
    }

    public List<InstantiatedResource> StoragesWithResource(ResourceData resource, int count = 0, int stationary = 1) {
        return StorageFunctions.StoragesWithResource(ReturnStorages(stationary), resource, count, stationary);
    }

    public List<InstantiatedResource> StoragesWithResources(List<RequiredResources> required, int stationary = 1) {
        List<InstantiatedResource> instantiatedResources = new List<InstantiatedResource>();
        if (StorageFunctions.CheckIfResourcesAvailable(required, CompileTotalResourceList(reservedTotal: true))) {
            foreach (RequiredResources res in required) {
                instantiatedResources.AddRange(StoragesWithResource(res.resource, res.count, stationary));
            }
        } else return null;
        return instantiatedResources;
    }

    public InstantiatedResource TotalOfResourceData(ResourceData resourceData) {
        List<InstantiatedResource> totalList = CompileTotalResourceList(stationary: -1);
        return totalList.Find(x => x.resourceData == resourceData);
    }

    public void ReserveItem(List<RequiredResources> requireds, List<InstantiatedResource> sources) {
        foreach (RequiredResources required in requireds) {
            int count = required.count;
            List<InstantiatedResource> relevant = sources.FindAll(x => x.resourceData == required.resource);
            foreach (InstantiatedResource instantiated in relevant) {
                int remaining;
                if (instantiated.count < count) remaining = instantiated.count;
                else remaining = count;
                instantiated.reserved += remaining;
                count -= remaining;
                if (count <= 0) break;
            }
        }
    }

    public void ReserveItem(List<ResourceTransaction> transactions) {
        foreach (ResourceTransaction transaction in transactions) {
            int current = transaction.referenceResource.count;
            transaction.referenceResource.reserved = Mathf.Clamp(transaction.referenceResource.reserved + transaction.resourceChange, 0, current);
            Debug.Log("STC - Transaction " + transaction.referenceResource.resourceID + " of count " + transaction.resourceChange + " has a reserved value of " + transaction.referenceResource.reserved);
        }
    }
    /*  public StorageContainer FindResourceInStorage(ResourceData resource, Vector3 currentLocation, bool stationary = true) {
         // Return the nearest storage that contains a specific resource.
         List<StorageContainer> possibleStorages = StoragesWithResource(resource, stationary : true);
         StorageContainer returnStorage = null;
         float minDistance = Mathf.Infinity;
         foreach (StorageContainer storageContainer in possibleStorages) {
             float distance = Vector3.Distance(currentLocation, storageContainer.location);
             if (distance < minDistance) {
                 returnStorage = storageContainer;
                 minDistance = distance;
             }
         }
         return returnStorage;
     } */

    public InstantiatedResource FindNearestWithCategory(ResourceData.category category, Vector3 currentLocation, int minHunger = -1, int count = 1) {
        // Return the nearest storage that contains a resource of a specific category.
        List<StorageContainer> possibleStorages = FindStoragesWithResourceCategory(category, minHunger);
        StorageContainer returnStorage = null;
        float minDistance = Mathf.Infinity;
        foreach (StorageContainer storageContainer in possibleStorages) {
            float distance = Vector3.Distance(currentLocation, storageContainer.location);
            if (distance < minDistance) {
                returnStorage = storageContainer;
                minDistance = distance;
            }
        }
        if (returnStorage != null) return StorageFunctions.FindResourceOfType(category, returnStorage);
        else return null;
    }

    private List<StorageContainer> FindStoragesWithResourceCategory(ResourceData.category category, int minHunger = -1, int[] additionalStorages = null) {
        List<StorageContainer> possibleStorages = new List<StorageContainer>();
        List<StorageContainer> totalStorages = ReturnStorages(1);
        if (additionalStorages != null) {
            foreach (int id in additionalStorages) {
                StorageContainer storageContainer = FindStorageByID(id);
                if (storageContainer != null)
                    if (!possibleStorages.Contains(storageContainer)) possibleStorages.Add(storageContainer);
            }
        }
        foreach (StorageContainer storage in totalStorages) {
            if (storage.inventory.Count > 0) {
                InstantiatedResource res = storage.inventory.Find(x => x.resourceData.categoryType == category && x.resourceData.hungerRegeneration >= minHunger);
                if (res != null) {
                    if (res.count - res.reserved > 0)
                        possibleStorages.Add(storage);
                }
            }
        }
        return possibleStorages;
    }

    public InstantiatedResource FindBestFood(int[] additionalIDs = null) {
        List<StorageContainer> foodContainers = FindStoragesWithResourceCategory(ResourceData.category.Food, 1, additionalIDs);
        List<InstantiatedResource> totalOptions = new List<InstantiatedResource>();
        foreach (StorageContainer storage in foodContainers) {
            totalOptions.AddRange(StorageFunctions.FindResourcesOfType(ResourceData.category.Food, storage, 1));
        }
        return ResourceFunctions.CompareHungerValues(totalOptions);
    }

    public StorageContainer FindNearestStorageContainer(float requiredWeight, Vector3 currentLocation) {
        StorageContainer returnStorage = null;
        List<StorageContainer> possibleStorages = new List<StorageContainer>();
        foreach (StorageContainer storage in storageContainers) {
            if (storage.weightFill + requiredWeight <= storage.weightCapacity && storage.stationary && !storage.externalInventory) {
                possibleStorages.Add(storage);
            }
        }
        float minDistance = Mathf.Infinity;
        foreach (StorageContainer storageContainer in possibleStorages) {
            float distance = Vector3.Distance(currentLocation, storageContainer.location);
            if (distance < minDistance) {
                returnStorage = storageContainer;
                minDistance = distance;
            }
        }
        if (returnStorage == null) {
            // Warning message?
        }

        return returnStorage;
    }

    public List<StorageContainer> FindEmptiestStorages() {
        // List<StorageContainer> returnStorages = new List<StorageContainer>();
        List<StorageContainer> totalStorages = ReturnStorages(1);
        // foreach (StorageContainer storageContainer in totalStorages) {
        //     Debug.Log("STC - Checking STC " + storageContainer.id + " with a capacity of " + storageContainer.weightCapacity + ", and a fill of " + storageContainer.weightFill);
        //     if (storageContainer.weightCapacity - storageContainer.weightFill > 0) {
        //         returnStorages.Add(storageContainer);
        //     } else Debug.Log("STC - Storage not passed check");
        // }
        // returnStorages = returnStorages.OrderByDescending(s => (s.weightCapacity - s.weightFill)).ToList<StorageContainer>();
        return StorageFunctions.FindEmptiestStorages(totalStorages);
    }

    public List<ResourceTransaction> ConvertToResourceTransactions(List<RequiredResources> reqs, bool reserve = true, float bulkTakeMultiplier = 1) {
        // Firstly check whether the required items are available in storage.
        List<InstantiatedResource> totalStorage = StoragesWithResources(reqs, 1);
        if (totalStorage == null) return null;
        List<ResourceTransaction> resourceTransactions = StorageFunctions.ConvertToResourceTransactions(totalStorage, reqs);
        // Reserve each item recovered, and return the list of formatted transactions.
        if (reserve) ReserveItem(resourceTransactions);
        return resourceTransactions;
    }

    public bool OffloadMultipleResources(List<RequiredResources> requiredResources) {
        if (requiredResources.Count == 0) return true;
        List<StorageContainer> storagesWithCapacity = FindEmptiestStorages();
        if (storagesWithCapacity.Count == 0) {
            Debug.Log("STC - No resources to process");
            return false;
        }
        string debugString = "Offloaded:";
        foreach (RequiredResources res in requiredResources) {
            List<InstantiatedResource> results = StoragesWithResource(res.resource, 1, 1);
            StorageContainer storage;
            if (results != null) {
                if (results.Count > 0) {
                    for (int i = 0; i < results.Count; i++) {
                        storage = FindStorageByID(results[i].storageContainerID);
                        int change = StorageFunctions.TryAmendStorage(storage, res.resource, res.count, true);
                        res.count -= change;
                        debugString += " " + change + " " + res.resource.resourceName + " to " + storage.id + ", ";

                    }
                }
            }

            if (res.count > 0) {
                foreach (StorageContainer storage1 in storagesWithCapacity) {
                    int change = StorageFunctions.TryAmendStorage(storage1, res.resource, res.count, true);
                    debugString += " " + change + " " + res.resource.resourceName + " to " + storage1.id + ", ";
                }
            }
        }
        Debug.Log("STC - " + debugString);
        return true;
    }

    public List<StorageContainer> ReturnStorages(int stationaryInt) {
        if (stationaryInt >= 0) {
            bool stationary = stationaryInt == 1 ? true : false;
            return storageContainers.FindAll(x => x.stationary == stationary && x.externalInventory == false);
        } else
            return storageContainers.FindAll(x => x.externalInventory == false);
    }

    public List<StorageContainer> ReturnStoragesByInternal(bool externalStorage) {
        return storageContainers.FindAll(x => x.externalInventory == externalStorage);
    }

    public void RegisterStorage(StorageContainer storage, List<InstantiatedResource> savedList = null, GameObject locationObject = null) {
        storage.inventory = new List<InstantiatedResource>();
        if (savedList != null) {
            Debug.Log("STC - Save list count: " + savedList.Count);
            foreach (InstantiatedResource res in savedList) {
                ResourceData resData = controllerManager.resourceController.LookupResourceData(res.resourceID);
                StorageFunctions.TryAmendStorage(storage, resData, res.count, false);
            }
        }
        if (!storageContainers.Contains(storage)) {
            if (storage.stationary) {
                storage.location = locationObject.transform.position;
            }
            //storage.inventory.Add(new InstantiatedResource(controllerManager.resourceController.LookupResourceData(1), 20));
            int _id = Random.Range(1, 10000);
            while (storageContainers.Find(x => x.id == _id) != null) {
                _id = Random.Range(1, 10000);
            }

            storageLookup.Add(_id, storage);
            storage.id = _id;
            Debug.Log("New Storage ID: " + storage.id + ", Inv Count: " + storage.inventory.Count + ", Weight Cap: " + storage.weightCapacity + " stationary: " + storage.stationary);
            storageContainers.Add(storage);
        }
    }

    public void DeregisterStorage(StorageContainer storage) {
        if (storageContainers.Contains(storage)) {
            storageContainers.Remove(storage);
            storageLookup.Remove(storage.id);
        }
    }

    public void ResetStorageList() {
        storageContainers.Clear();
    }
}

[System.Serializable]
public class StorageContainer {
    public bool externalInventory;
    public List<InstantiatedResource> inventory;
    public float weightFill;
    public float weightCapacity;
    public Vector3 location;
    public bool stationary;
    public int id;
    public StorageContainer(float invLimit, bool _stationary, bool _externalInvetory = false) {
        weightCapacity = invLimit;
        inventory = new List<InstantiatedResource>();
        stationary = _stationary;
        externalInventory = _externalInvetory;
    }
}

public class ResourceTransaction {
    public InstantiatedResource referenceResource;
    public int resourceChange;

    public ResourceTransaction(InstantiatedResource instantiated, int _resourceChange) {
        referenceResource = instantiated;
        resourceChange = _resourceChange;
    }
}