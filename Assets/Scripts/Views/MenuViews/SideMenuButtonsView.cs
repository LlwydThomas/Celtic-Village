using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class SideMenuButtonsView : MonoBehaviour {
    // Start is called before the first frame update   
    public GameObject buildingMenuParent, commandMenuParent, InfoMenu;
    public SpawnPrefabView spawnPrefabView;
    public GameObject buttonPrefab, expandedButtonPrefab, resourceItemPrefab;
    public RectTransform contentRect, resourceParent;
    List<GameObject> buttonList = new List<GameObject>();
    public List<GameObject> expansionObjects = new List<GameObject>();
    private List<ExpansionButtonView> expansionButtonViews = new List<ExpansionButtonView>();
    private List<ResourceDisplayItem> resourceDisplays = new List<ResourceDisplayItem>();
    private List<StructureData> structureDatas = new List<StructureData>();
    public ManagerReferences manager;
    private ControllerManager controllerManager;
    public bool bypassRequired;

    private void OnEnable() {
        if (controllerManager == null) controllerManager = manager.controllerManager;
        AmendResourceItemColour();
        EventController.StartListening("resourceChange", AmendResourceItemColour);
        GeneralFunctions.SetContentHeight(contentRect, FindSize(), new RectTransform[] { resourceParent });
    }

    private void Start() {
        GenerateBuildingButtons();
    }

    private void OnDisable() {
        EventController.StopListening("resourceChange", AmendResourceItemColour);
    }
    public void GenerateBuildingButtons() {
        // Remove all building buttons and repopulate based on the structure data list.
        foreach (Transform child in resourceParent.transform) {
            Destroy(child.gameObject);
        }
        resourceDisplays.Clear();
        structureDatas = manager.modelManager.buildingModel.structureDatas;
        Debug.Log("Creating Buttons for " + structureDatas.Count + " structures.");
        foreach (StructureData structure in structureDatas) {
            GameObject buildButton = Instantiate(expandedButtonPrefab, resourceParent.transform, false);
            ExpansionButtonView expansionButtonView = buildButton.GetComponent<ExpansionButtonView>();
            expansionButtonViews.Add(expansionButtonView);
            expansionObjects.Add(expansionButtonView.resultantList);
            RectTransform rect = expansionButtonView.gameObject.GetComponent<RectTransform>();
            Debug.Log("SMBV - Structure Resource req: " + structure.requiredRes.Count);
            ResourceDisplayItem resourceDisplay = new ResourceDisplayItem(expansionButtonView, structure.requiredRes);
            resourceDisplay.resourceReqs = structure.requiredRes;
            resourceDisplays.Add(resourceDisplay);
            if (expansionButtonView != null) {
                expansionButtonView.FormatExpansionButton(controllerManager.settingsController, delegate {
                    FindAndSetContentHeight();
                }, controllerManager.settingsController.TranslateString(structure.structureName), structureDatas.IndexOf(structure), structure.buildingIcon, maxFont : 25f, name: "Building" + structure.structureName);
            }
            //GeneralFunctions.SetExpansionSize(expansionButtonView, structure.requiredRes.Count, 50, 90);
            Transform itemParent = resourceDisplay.expansionButton.resultantList.transform;
            if (structure.requiredRes.Count > 0) {
                foreach (RequiredResources instantiated in structure.requiredRes) {
                    if (!expansionButtonView.resultantObjectsDict.ContainsKey(instantiated.resource.ID)) {
                        string resName = controllerManager.settingsController.TranslateString(instantiated.resource.resourceName);
                        GameObject newItem = GameObject.Instantiate(resourceItemPrefab, itemParent, false);
                        StorageFunctions.FormatInventoryItem(newItem, resName, instantiated.count, false, Color.white, instantiated.resource.iconColour, 80f, instantiated.resource.icon, 25f);
                        expansionButtonView.resultantObjectsDict.Add(instantiated.resource.ID, newItem);
                    }

                }
            } else expansionButtonView.expansionButton.gameObject.SetActive(false);
            expansionButtonView.resultantList.SetActive(false);
            GeneralFunctions.ResizeExpansionButton(expansionButtonView, structure.requiredRes.Count, 50f);

            string header = controllerManager.settingsController.TranslateString(structure.structureName);
            string text = controllerManager.settingsController.TranslateString(structure.structureName + "Description");
            BuildingFunctions.AppendHeaderToolTip(header, text, expansionButtonView.backgroundImage.gameObject);
            expansionButtonView.overallButton.onClick.AddListener(delegate {
                spawnPrefabView.BuildPrefab(structure.ID, GeneralEnumStorage.debugActive && bypassRequired);
                manager.uiManagement.ForceCloseTooltip(2);
            });
        }

        AmendResourceItemColour();
        GeneralFunctions.SetContentHeight(contentRect, FindSize(), new RectTransform[] { resourceParent });
    }

    private void AmendResourceItemColour() {
        // Firstly, check that all structure buttons have been created.
        if (resourceDisplays.Count == structureDatas.Count) {
            List<InstantiatedResource> totalResources = controllerManager.storageController.CompileTotalResourceList(stationary: -1, reservedTotal : true);
            foreach (ResourceDisplayItem displayItem in resourceDisplays) {
                bool requirementsMet = true;
                Button overallButton = displayItem.expansionButton.overallButton;
                Image background = displayItem.expansionButton.backgroundImage;
                foreach (RequiredResources req in displayItem.resourceReqs) {
                    // Determine whether each required resource is currently in the player's storage.
                    GameObject relevantObj = displayItem.expansionButton.resultantObjectsDict[req.resource.ID];
                    Color colour;
                    InstantiatedResource resource = totalResources.Find(x => x.resourceData == req.resource);
                    if (resource != null) {
                        if (resource.count >= req.count) colour = Color.green;
                        else colour = Color.red;
                    } else {
                        Debug.Log("SMBV - Unable to find resource from " + req.resource.resourceName);
                        colour = Color.red;
                    }
                    if (colour == Color.red) requirementsMet = false;
                    // Set the count text's colour to red if the resource is not available.
                    relevantObj.transform.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>().color = colour;
                }
                // Only enable the build button if all resources have been located.
                if (requirementsMet || (bypassRequired && GeneralEnumStorage.debugActive)) {
                    overallButton.interactable = true;
                    background.color = Color.white;
                } else {
                    overallButton.interactable = false;
                    background.color = Color.red;
                }
            }
        }
    }

    private float FindSize() {
        float size = 5;
        foreach (ResourceDisplayItem res in resourceDisplays) {
            size += res.expansionButton.rectTransform.sizeDelta.y + 5;
        }
        return size;
    }

    private void FindAndSetContentHeight() {
        GeneralFunctions.SetContentHeight(contentRect, FindSize(), new RectTransform[] { resourceParent });
    }

}