using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public static class StorageFunctions {
    // Start is called before the first frame update
    public static bool CheckStorageAvailable(StorageContainer storage, float additionalWeight) {
        //Debug.Log(storage.weightFill + additionalWeight + " of " + storage.weightCapacity);
        if (storage.weightFill + additionalWeight <= storage.weightCapacity) return true;
        else return false;
    }

    public static int TryAmendStorage(StorageContainer storageContainer, ResourceData resourceData, int change, bool transferMax = true, bool amendReserved = true) {
        string debugText = "STC - Old Inventory:";
        Debug.Log("SF - Amending storage container " + storageContainer.id + " regarding resource " + resourceData.resourceName + " by the amount of " + change);
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
        Debug.Log("STC - Division: " + remainingWeight + "/" + resourceData.weightPerItem + ", Max Transfer: " + maxTransfer);
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
                        TryAmendStorage(storageContainer, resourceData, change, transferMax, amendReserved);
                        return change;
                    } else return 0;
                }
            } else {
                if (transferMax) {
                    change = -existingResource.count;
                    TryAmendStorage(storageContainer, resourceData, change, transferMax, amendReserved);
                    return change;
                } else return 0;
            }
        } else {
            if (change >= 0) {
                Debug.Log("STC - attempting to add ");
                if (CheckStorageAvailable(storageContainer, weightChange)) {
                    Debug.Log("Passed Storage Check For New Res");
                    existingResource = new InstantiatedResource(resourceData, change);
                    existingResource.SetStorageContainer(storageContainer);
                    storageContainer.inventory.Add(existingResource);
                } else {
                    change = maxTransfer;
                    TryAmendStorage(storageContainer, resourceData, change, transferMax, amendReserved);
                    return change;
                }
            } else return 0;
        }
        if (existingResource != null) {
            if (existingResource.count == 0) storageContainer.inventory.Remove(existingResource);
            existingResource.storageContainerID = storageContainer.id;
        }
        storageContainer.weightFill = 0;
        int postInventoryCount = storageContainer.inventory.Count;
        foreach (InstantiatedResource res in storageContainer.inventory) {
            debugText += " " + res.resourceData.resourceName + ": " + res.count;
            storageContainer.weightFill += (res.count * res.resourceData.weightPerItem);
        }
        float postFill = storageContainer.weightFill;
        Debug.Log(debugText);
        Debug.Log("STC - Original count for " + storageContainer.id + ": " + priorInventoryCount + ", Post count: " + postInventoryCount);
        Debug.Log("ID: " + storageContainer.id + ", Prior Fill: " + priorFill + ", Post Fill: " + postFill);
        Debug.Log("ID: " + storageContainer.id + ", New Count: " + storageContainer.inventory.Count);
        if (EventController.instance) {
            EventController.TriggerEvent(storageContainer.id + "storageAmended");
            EventController.TriggerEvent("resourceChange");
        }
        return change;
    }

    public static bool FindAndExtractResourcesFromStorage(List<RequiredResources> _requiredResources, List<StorageContainer> availableStorages, List<InstantiatedResource> currentTotal, bool overwriteRequired = false) {
        List<InstantiatedResource> extractionContainers = new List<InstantiatedResource>();
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
                List<InstantiatedResource> storages = StoragesWithResource(availableStorages, res.resource, res.count, 1);
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
                StorageContainer storage = extraction.storageContainer;
                if (storage == null) Debug.Log("SF - Null storage for " + extraction.resourceData.resourceName + " with count of " + extraction.count);
                RequiredResources currentResource = requiredResources.Find(x => x.resource == extraction.resourceData);
                if (currentResource != null) {
                    int change = TryAmendStorage(storage, currentResource.resource, -currentResource.count, true, false);
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
            /* for (int i = 0; i < currentTotal.Count; i++) {
                int change = TryAmendStorage(extractionContainers[0], currentTotal[0].resourceData, currentTotal[0].count, false);
            } */
            return true;
        } else {
            Debug.Log("STC - Required resource count does not equal found item count: " + requiredResources.Count + " vs " + foundItems.Count);
            return false;
        }
    }

    public static StorageContainer AmendStorageCapacity(StorageContainer storage, float newCapacity) {
        storage.weightCapacity = newCapacity;
        return storage;
    }

    public static bool CheckIfResourceAvailable(RequiredResources requiredResource, List<InstantiatedResource> resources) {
        InstantiatedResource resource = resources.Find(x => x.resourceData == requiredResource.resource);
        if (resource != null) {
            if (resource.count - resource.reserved < requiredResource.count) return false;
        } else {
            if (requiredResource.count == 0) return true;
            else return false;
        }
        return true;
    }
    public static bool CheckIfResourcesAvailable(List<RequiredResources> requiredResources, List<InstantiatedResource> resourceList) {
        if (requiredResources.Count == 0) return true;
        foreach (RequiredResources res in requiredResources) {
            if (!CheckIfResourceAvailable(res, resourceList)) return false;
        }
        return true;
    }

    public static FloraData CheckFloraDatasForItems(List<FloraData> possible, List<InstantiatedResource> resources) {
        possible = possible.OrderByDescending(x => x.qualityTier).ToList();
        foreach (FloraData flora in possible) {
            if (CheckFloraDataForItems(flora, resources)) return flora;
            else Debug.Log("SF - " + flora.uniqueType + " doesn't have resources.");
        }
        return null;
    }

    public static bool CheckFloraDataForItems(FloraData flora, List<InstantiatedResource> resources) {
        return CheckIfResourcesAvailable(flora.requiredToGrow, resources);
    }

    public static InventoryItemReferences FormatInventoryItem(GameObject gameObject, string resName, int count, bool disableBackground, Color color, Color iconColour, float height, Sprite icon = null, float maxFont = -1) {
        string countText;
        if (count == -1) countText = "";
        else countText = count.ToString();
        return FormatInventoryItem(gameObject, resName, countText, disableBackground, color, iconColour, height, icon, maxFont);
    }

    public static List<ResourceTransaction> ConvertToResourceTransactions(List<InstantiatedResource> totalStorage, List<RequiredResources> reqs) {
        Debug.Log("FH - Passed total storage retrieval with " + totalStorage.Count + " storages.");
        List<ResourceTransaction> resourceTransactions = new List<ResourceTransaction>();
        List<RequiredResources> copyList = GeneralFunctions.CopyResourceList(reqs);
        foreach (RequiredResources req in copyList) {
            // Using each required item, find the containers with this item and add them to a list.
            List<InstantiatedResource> relevantContainers = totalStorage.FindAll(x => x.resourceData == req.resource);
            //int bulkCount = Mathf.RoundToInt((float) req.count * bulkTakeMultiplier);
            if (req.count < 0) return null;
            foreach (InstantiatedResource res in relevantContainers) {
                if (req.count > 0) {
                    // Cycle through each relevant resource, and track how many are available to be used.
                    int available = res.count - res.reserved;
                    int extractionCount;
                    if (req.count > available) extractionCount = available;
                    else extractionCount = req.count;
                    // Create a new transaction from the found resource, with the count being calculated from the items required.
                    resourceTransactions.Add(new ResourceTransaction(res, extractionCount));
                    req.count -= extractionCount;
                }
            }
            if (req.count > 0) {
                return null;
            }
        }

        return resourceTransactions;
    }

    public static void AppendResourceTooltip(ManagerReferences managerReferences, ResourceData resource, GameObject game, Vector3 offset) {

        ResourceTipHandler resourceTipHandler = game.AddComponent<ResourceTipHandler>();
        resourceTipHandler.SetTooltipData(managerReferences, resource, 1, null);
    }

    public static InventoryItemReferences FormatInventoryItem(GameObject gameObject, string resName, string countString, bool disableBackground, Color color, Color iconColour, float height, Sprite icon = null, float maxFont = -1, bool deleteActive = false) {
        InventoryItemReferences itemReferences = gameObject.GetComponent<InventoryItemReferences>();
        itemReferences.SetActiveObjects(icon != null, countString != "", deleteActive);
        Transform objTransform = gameObject.transform;
        Image image = itemReferences.icon;
        if (icon != null) {
            image.gameObject.SetActive(true);
            image.sprite = icon;
            image.color = iconColour;
        }
        RectTransform rect = gameObject.GetComponent<RectTransform>();
        GeneralFunctions.SetContentHeight(rect, height, null);
        if (maxFont != -1) {
            itemReferences.resourceNameText.fontSizeMax = maxFont;
            itemReferences.countText.fontSizeMax = maxFont - 1;
        }
        itemReferences.deleteButton.onClick.RemoveAllListeners();
        itemReferences.resourceNameText.SetText(resName);
        itemReferences.countText.SetText(countString);

        if (disableBackground) itemReferences.backgroundImage.color = Color.clear;
        else itemReferences.backgroundImage.color = GeneralFunctions.blackBackground;
        itemReferences.countText.color = color;

        return itemReferences;
    }
    public static void FormatTextWithValue(GameObject gameObject, string label, string value, float maxFont) {
        TextMeshProUGUI labelText = gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI valueText = gameObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        labelText.fontSizeMax = maxFont;
        valueText.fontSizeMax = maxFont;
        labelText.SetText(label);
        valueText.SetText(value);

    }

    public static InstantiatedResource FindResourceOfType(ResourceData.category type, StorageContainer storageContainer, int minHunger = -1, int count = 1) {
        // If a storage contains a resource of a specified cateogry, return the resource.
        if (storageContainer.inventory != null) {
            InstantiatedResource res = storageContainer.inventory.Find(x => x.resourceData.categoryType == type && x.count - x.reserved >= count && x.resourceData.hungerRegeneration >= minHunger);
            if (res != null) {
                res.reserved += count;
                return res;
            } else return null;
        } else return null;
    }

    public static List<InstantiatedResource> FindResourcesOfType(ResourceData.category type, StorageContainer storageContainer, int minHunger = -1) {
        List<InstantiatedResource> returnList = new List<InstantiatedResource>();
        if (storageContainer.inventory != null) {
            List<InstantiatedResource> res = storageContainer.inventory.FindAll(x => x.resourceData.categoryType == type && x.count - x.reserved > 0 && x.resourceData.hungerRegeneration >= minHunger);
            returnList.AddRange(res);
        }
        return returnList;
    }

    public static List<InstantiatedResource> CompileResourceList(List<StorageContainer> checkableContainers, bool reservedTotal) {
        List<InstantiatedResource> returnList = new List<InstantiatedResource>();
        Debug.Log("STC - Total Storages Checkable: " + checkableContainers.Count);
        foreach (StorageContainer storageContainer in checkableContainers) {
            foreach (InstantiatedResource resource in storageContainer.inventory) {
                InstantiatedResource returnResource = returnList.Find(x => x.resourceData == resource.resourceData);
                int countAddition = resource.count;
                if (reservedTotal) countAddition -= resource.reserved;
                if (returnResource != null) {
                    returnResource.count += countAddition;
                } else {
                    Debug.Log("STC - Resource data: " + resource.resourceData.ID);
                    returnResource = new InstantiatedResource(resource.resourceData, countAddition);
                    returnList.Add(returnResource);
                }
            }
        }

        return returnList;
    }

    public static List<InstantiatedResource> StoragesWithResource(List<StorageContainer> availableStorages, ResourceData resource, int count = 0, int stationary = 1) {
        List<InstantiatedResource> possibleStorages = new List<InstantiatedResource>();
        int itemCount = 0;
        foreach (StorageContainer storageContainer in availableStorages) {
            string text = "STC - Storage " + storageContainer.id + " contains:";
            foreach (InstantiatedResource res in storageContainer.inventory) {
                text += " " + res.resourceData.resourceName + " - " + res.count;
            }
            Debug.Log(text);
        }
        Debug.Log("STC - total storage count for " + resource.resourceName + ": " + availableStorages.Count);
        foreach (StorageContainer storage in availableStorages) {
            Debug.Log("STC - storage " + storage.id + " contains: " + storage.inventory.Count + " instantiated resources");
            InstantiatedResource res = storage.inventory.Find(x => x.resourceData == resource);
            if (res != null) {
                possibleStorages.Add(res);
                itemCount += (res.count - res.reserved);
                if (itemCount >= count) return possibleStorages;
                else Debug.Log("STC - item count = " + itemCount + " & res.count = " + res.count);
            } else Debug.Log("STC - Resource of " + resource.resourceName + " is nulled");
        }
        return null;
    }

    public static List<StorageContainer> FindEmptiestStorages(List<StorageContainer> totalStorages) {
        List<StorageContainer> returnStorages = new List<StorageContainer>();
        foreach (StorageContainer storageContainer in totalStorages) {
            Debug.Log("STC - Checking STC " + storageContainer.id + " with a capacity of " + storageContainer.weightCapacity + ", and a fill of " + storageContainer.weightFill);
            if (storageContainer.weightCapacity - storageContainer.weightFill > 0) {
                returnStorages.Add(storageContainer);
            } else Debug.Log("STC - Storage not passed check");
        }

        // Order all storages with available capacity by their available capacity.
        returnStorages = returnStorages.OrderByDescending(s => (s.weightCapacity - s.weightFill)).ToList<StorageContainer>();
        return returnStorages;
    }

    public static void ChangeInvItemBackground(GameObject invObject, Color colour, bool enabled = true) {
        Image image = invObject.GetComponent<Image>();
        image.color = colour;
    }
}