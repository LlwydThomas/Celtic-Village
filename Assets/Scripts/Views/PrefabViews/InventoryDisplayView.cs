using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class InventoryDisplayView : MonoBehaviour {
    // Start is called before the first frame update
    public GameObject inventoryItemParent, inventoryItemPrefab, debugObject;
    public TextMeshProUGUI resNameLabel, invFillText, countLabel;
    public RectTransform rect;
    List<ResourceData> resources;
    public bool debug, requiredItemsMet;
    private bool debugActive;
    private List<GameObject> inventoryItems = new List<GameObject>();
    private StorageContainer storageContainer = null;
    private ControllerManager controllerManager;

    // Update is called once per frame
    public void PopulateInventoryList(StorageContainer storage, ManagerReferences managerReferences, float itemHeight) {
        debugActive = GeneralEnumStorage.debugActive && debug;
        PopulateTotalFill(storage.weightFill, storage.weightCapacity);
        if (debugObject != null) {
            PrepareDebugPanel(debugActive, storage, managerReferences.controllerManager);
        }
        storageContainer = storage;
        List<RequiredResources> requiredResources = GeneralFunctions.CopyResourceList(storage.inventory);
        PopulateInventoryList(requiredResources, managerReferences, true, itemHeight);
    }

    public void PopulateInventoryList(List<RequiredResources> requiredResources, ManagerReferences managerReferences, bool showWeight, float height, bool highlightAvailable = false, bool hideBackground = true, string[] labelOverrides = null) {
        controllerManager = managerReferences.controllerManager;
        if (labelOverrides != null) {
            if (labelOverrides.Length == 2) {
                resNameLabel.text = labelOverrides[0];
                countLabel.text = labelOverrides[1];
            }
        }
        requiredItemsMet = true;

        if (!debugActive) {
            DisableDebugPanel();
        }
        foreach (Transform n in inventoryItemParent.transform) {
            if (n.gameObject.name != "DebugItemAdd")
                GameObject.Destroy(n.gameObject);
        }
        if (!showWeight) invFillText.gameObject.SetActive(false);
        inventoryItems.Clear();
        PrepareText(controllerManager.settingsController);
        List<InstantiatedResource> totalList = controllerManager.storageController.CompileTotalResourceList();
        foreach (RequiredResources res in requiredResources) {
            if (res.count == 0) continue;
            GameObject newInvItem = Instantiate(inventoryItemPrefab, inventoryItemParent.transform, false);
            inventoryItems.Add(newInvItem);

            newInvItem.name = res.resource.resourceName;
            string skillTrans = controllerManager.settingsController.TranslateString(newInvItem.name);
            string countString = res.count.ToString();
            if (showWeight) countString += "(" + (res.resource.weightPerItem * res.count).ToString() + ")";
            Color colour = Color.white;
            if (highlightAvailable) {
                if (StorageFunctions.CheckIfResourceAvailable(res, totalList)) colour = Color.green;
                else {
                    colour = Color.red;
                    requiredItemsMet = false;
                }
            }

            InventoryItemReferences itemReferences = StorageFunctions.FormatInventoryItem(newInvItem, skillTrans, countString, hideBackground, colour, res.resource.iconColour, height, res.resource.icon, 30, deleteActive : storageContainer != null);
            if (storageContainer != null) {
                UnityAction confirmAction = null;
                confirmAction += delegate {
                    if (controllerManager.storageController.TryDeleteItemFromStorage(storageContainer, res)) {
                        EventController.TriggerEvent("resourceChange");
                    }
                };
                itemReferences.deleteButton.onClick.AddListener(delegate {
                    string[] replace = new string[] { res.count.ToString(), skillTrans };
                    managerReferences.uiManagement.ActivateConfirmationDialogue("DeleteRes" + res.resource.ID, "DeleteResource", confirmAction, null, useBaseMessage : true, stringParams : replace);
                });
            }
        }
        int count = debugActive ? inventoryItems.Count + 1 : inventoryItems.Count;
        float size = (count * (height + 10f));
        GeneralFunctions.SetContentHeight(rect, size, null);
    }

    private void PrepareDebugPanel(bool debug, StorageContainer storage, ControllerManager controller) {
        Debug.Log("IDV - Debug panel prepared for storage: " + storage.id);
        if (!debug) {
            debugObject.SetActive(false);
            return;
        }

        debugObject.SetActive(true);
        TMP_Dropdown drop = debugObject.transform.GetChild(1).GetComponent<TMP_Dropdown>();
        drop.ClearOptions();
        List<string> options = new List<string>();
        resources = controller.resourceController.ReturnResourceModel().resourceDatas;
        foreach (ResourceData resource in resources) {
            options.Add(resource.resourceName);
        }
        drop.AddOptions(options);

        TMP_InputField input = debugObject.transform.GetChild(2).GetComponent<TMP_InputField>();
        input.onEndEdit.RemoveAllListeners();
        input.onEndEdit.AddListener(delegate {
            AmendStorage(controller, storage, drop.value, int.Parse(input.text));
            input.text = "0";
        });
    }

    private void DisableDebugPanel() {
        debugObject.SetActive(false);
    }

    public void AmendStorage(ControllerManager controller, StorageContainer storage, int resIndex, int count) {

        ResourceData res = resources[resIndex];
        Debug.Log("Trying to add " + count + " " + res.resourceName);
        StorageFunctions.TryAmendStorage(storage, res, count);
    }

    private void PrepareText(SettingsController settings) {
        SettingsFunctions.TranslateTMPItems(settings, new TextMeshProUGUI[] { countLabel, resNameLabel });
    }

    public void PopulateTotalFill(float fill, float cap) {
        invFillText.SetText(fill + "/" + cap);
    }
}