using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceController : MonoBehaviour {
    public int firewoodPerPawn = 15;
    public GameObject resList, itemTemplate;
    public ResourceDataList resourceDataList;
    public CraftingDataList craftingDataList;

    public TradingDialogueView tradingDialogue;

    private ResourceModel resourceModel;
    Dictionary<string, string> strings;
    public ManagerReferences managerReferences;
    private ControllerManager controllerManager;
    // Start is called before the first frame update
    void Start() {
        controllerManager = managerReferences.controllerManager;
        resourceModel = managerReferences.modelManager.resourceModel;
        resourceModel.resourceDatas = resourceDataList.ResourceDatas;
        resourceModel.craftingDatas = craftingDataList.craftingDatas;
        foreach (ResourceData x in resourceModel.resourceDatas) {
            resourceModel.resourceDataLookup.Add(x.ID, x);
        }
        foreach (CraftingData craftingData in resourceModel.craftingDatas) {
            resourceModel.craftingDataLookup.Add(craftingData.id, craftingData);
        }

        InitialiseOrLoadResources();
        //Debug.Log(resourceModel.resourceDatas.Count);
        strings = controllerManager.settingsController.ReturnStrings();
    }

    public List<ResourceData> RandomResourceDatas(int count, int[] possibleIds = null, ResourceData.category category = ResourceData.category.Null, bool repeatAllowed = false) {
        List<ResourceData> possibleResources = new List<ResourceData>();
        if (possibleIds != null) {
            foreach (int id in possibleIds) {
                if (resourceModel.resourceDataLookup.ContainsKey(id)) {
                    possibleResources.Add(resourceModel.resourceDataLookup[id]);
                } else Debug.Log("Resource " + id + " cannot be found in the resource data lookup.");
            }
        } else if (category != ResourceData.category.Null) {
            possibleResources = resourceModel.resourceDatas.FindAll(x => x.categoryType == category);
        } else possibleResources = resourceModel.resourceDatas;
        return ResourceFunctions.RandomResourceDatas(count, possibleResources, repeatAllowed);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.F2))
            if (GeneralEnumStorage.debugActive) TestTrading();
    }

    public void TestTrading() {
        List<InstantiatedResource> tradingInvent = new List<InstantiatedResource>();
        tradingInvent.Add(new InstantiatedResource(resourceModel.resourceDataLookup[1], 50));
        tradingInvent.Add(new InstantiatedResource(resourceModel.resourceDataLookup[5], 50));
        tradingDialogue.gameObject.SetActive(true);
        EnableTradingDialogue(tradingInvent);
    }

    public void EnableTradingDialogue(List<InstantiatedResource> tradingInventory, int balanceOffset = 0, float buyModifier = 1f, float sellModifier = 1f) {
        List<InstantiatedResource> personalInventory = controllerManager.storageController.CompileTotalResourceList();
        managerReferences.uiManagement.ManageOpenDialogues(true, 5);
        tradingDialogue.BeginTradingDialogue(tradingInventory, personalInventory, sellModifier, buyModifier, balanceOffset);
    }

    public void InitialiseOrLoadResources(SaveGameItem saveData = null) {
        if (saveData == null) {
            foreach (ResourceData resourceData in resourceModel.resourceDatas) {
                InstantiatedResource newResource = new InstantiatedResource(resourceData, 0);
                resourceModel.resourceInventory.Add(newResource);
            }
        }
    }

    public ResourceModel ReturnResourceModel() {
        return resourceModel;
    }

    public ResourceData LookupResourceData(int id) {
        if (resourceModel.resourceDataLookup.ContainsKey(id)) return resourceModel.resourceDataLookup[id];
        else return null;
    }

    public InstantiatedResource ResourceCountLookup(ResourceData resourceData) {
        return resourceModel.instantiationReference[resourceData];
    }

    public List<InstantiatedResource> ResourceInventoryReturn() {
        return resourceModel.resourceInventory;
    }

    public CraftingData LookupCraftingData(int id) {
        if (resourceModel.craftingDataLookup.ContainsKey(id)) return resourceModel.craftingDataLookup[id];
        else return null;
    }

    public List<CraftingData> CraftingDatasReturn() {
        return resourceModel.craftingDatas;
    }

}

[System.Serializable]
public class CraftingQueueItem {
    public CraftingData craftingData;
    public int craftingDataID;
    public int count;
    public CraftingQueueItem(CraftingData _craftingData, int _count) {
        craftingData = _craftingData;
        count = _count;
        craftingDataID = _craftingData.id;
    }
}