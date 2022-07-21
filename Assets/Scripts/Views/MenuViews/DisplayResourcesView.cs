using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class DisplayResourcesView : MonoBehaviour {
    // Start is called before the first frame update
    public GameObject resourceItemPrefab, resourceItemParent, expandButtonPrefab;
    private ControllerManager controllerManager;
    public ManagerReferences managerReferences;

    [SerializeField]
    private List<ResourceDisplayItem> resourceDisplays = new List<ResourceDisplayItem>();
    private List<ExpansionButtonView> expansionButtonViews = new List<ExpansionButtonView>();
    public List<GameObject> expansionObjects;
    public Sprite[] categoryIcons;
    public GameObject contentSizeObject;
    public RectTransform resourceParent, contentRect;

    public List<InstantiatedResource> previousList = null;

    private void OnEnable() {
        if (controllerManager == null) controllerManager = managerReferences.controllerManager;
        EventController.StartListening("resourceChange", DisplayTotalResources);
        DisplayTotalResources();
    }

    private void OnDisable() {
        EventController.StopListening("resourceChange", DisplayTotalResources);
    }

    private void ResetResourceView() {

    }

    public void DisplayTotalResources() {
        // Clear any remaining resource items.
        if (resourceDisplays.Count == 0) {
            foreach (Transform t in resourceParent.transform) {
                Destroy(t.gameObject);
            }
        }
        // Determine which items have been removed since the inventory was last displayed.
        List<InstantiatedResource> removedItems = null;
        List<InstantiatedResource> resourceList = controllerManager.storageController.CompileTotalResourceList(stationary: -1);
        if (previousList != null) {
            removedItems = previousList.FindAll(x => resourceList.Find(y => y.resourceID == x.resourceID) == null);
        }

        previousList = resourceList;
        List<TextMeshProUGUI> descTexts = new List<TextMeshProUGUI>();
        System.Array enums = System.Enum.GetValues(typeof(ResourceData.category));
        foreach (ResourceData.category category in System.Enum.GetValues(typeof(ResourceData.category))) {
            if (category == ResourceData.category.Null) continue;
            int index = (int) category;
            ResourceDisplayItem currentView;
            List<InstantiatedResource> relevantResources = resourceList.FindAll(x => x.resourceData.categoryType == category);
            if (resourceDisplays.Find(x => x.expansionButton.index == index) == null) {
                GameObject newExpansion = Instantiate(expandButtonPrefab, resourceItemParent.transform, false);
                ExpansionButtonView expansion = newExpansion.GetComponent<ExpansionButtonView>();
                ResourceDisplayItem resourceDisplay = new ResourceDisplayItem(expansion, relevantResources);
                resourceDisplays.Add(resourceDisplay);
                currentView = resourceDisplay;
                currentView.expansionButton.FormatExpansionButton(controllerManager.settingsController, FindAndSetContentHeight, category.ToString(), index, categoryIcons[index - 1], 50f, maxFont : 25f, name: "Resources" + category.ToString());
                expansion.resultantList.SetActive(false);
                //GeneralFunctions.SetExpansionsMaxFont(resourceDisplays);
            } else {
                currentView = resourceDisplays.Find(x => x.expansionButton.index == index);
                currentView.resources = relevantResources;
            }
            Transform itemParent = currentView.expansionButton.resultantList.transform;
            ResourceDisplayItem res = currentView;
            GameObject button = res.expansionButton.expansionButton.gameObject;
            relevantResources.OrderBy(x => x.resourceData.subCategory);
            if (relevantResources.Count > 0) {
                Vector3 offset = new Vector3(-300f, 0, 0);
                button.SetActive(true);
                foreach (InstantiatedResource instantiated in relevantResources) {
                    string resName = controllerManager.settingsController.TranslateString(instantiated.resourceData.resourceName);
                    if (currentView.expansionButton.resultantObjectsDict.ContainsKey(instantiated.resourceID)) {
                        GameObject existing = currentView.expansionButton.resultantObjectsDict[instantiated.resourceID];
                        StorageFunctions.FormatInventoryItem(existing, resName, instantiated.count, true, Color.white, instantiated.resourceData.iconColour, 80f, instantiated.resourceData.icon, 25f);
                    } else {
                        GameObject newItem = GameObject.Instantiate(resourceItemPrefab, itemParent, false);
                        StorageFunctions.FormatInventoryItem(newItem, resName, instantiated.count, true, Color.white, instantiated.resourceData.iconColour, 80f, instantiated.resourceData.icon, 25f);
                        StorageFunctions.AppendResourceTooltip(managerReferences, instantiated.resourceData, newItem, offset);
                        currentView.expansionButton.resultantObjectsDict.Add(instantiated.resourceData.ID, newItem);
                    }
                }
            } else button.SetActive(false);
            if (removedItems != null) {
                Debug.Log("DRV - Removed item count: " + removedItems.Count);
                foreach (InstantiatedResource instantiatedResource in removedItems) {
                    Debug.Log("DRV - Removed item: " + instantiatedResource.resourceData.resourceName);
                    GameObject existing = null;
                    if (currentView.expansionButton.resultantObjectsDict.ContainsKey(instantiatedResource.resourceID)) {
                        existing = currentView.expansionButton.resultantObjectsDict[instantiatedResource.resourceID];
                    }
                    if (existing == null) continue;
                    currentView.expansionButton.RemoveItemFromList(instantiatedResource.resourceID, existing);
                    Destroy(existing);
                }

                removedItems = null;
            }
            GeneralFunctions.ResizeExpansionButton(currentView.expansionButton, relevantResources.Count, 50);
        }
        FindAndSetContentHeight();
    }

    private void FindAndSetContentHeight() {
        GeneralFunctions.SetContentHeight(contentRect, FindSize(), new RectTransform[] { resourceParent });
    }
    private float FindSize() {
        // Finds the overall size of the items displayed.
        float size = 5;
        foreach (ResourceDisplayItem res in resourceDisplays) {
            size += res.expansionButton.rectTransform.sizeDelta.y + 5;
        }
        Debug.Log("DRV - Overall size of " + size + " from " + resourceDisplays.Count + " resdisplays.");
        return size;
    }
    /*   private int InitialisePrefabValues(GameObject _gameObject, InstantiatedResource _resource) {

          ResourceData resource = controllerManager.resourceController.LookupResourceData(_resource.resourceID);
          _gameObject.name = resource.resourceName;
          GameObject icon = _gameObject.transform.GetChild(0).transform.GetChild(1).gameObject;
          if (resource.icon != null) {
              Image spriteReference = icon.GetComponent<Image>();
              spriteReference.sprite = resource.icon;
          }
          icon.GetComponent<ToolTipHandler>().SetTooltipData(controllerManager.settingsController.TranslateString(resource.resourceName), 0);
          gameObject.GetComponentInChildren<TextMeshProUGUI>().SetText(_resource.count.ToString());
          return _resource.count;
      } */

}

[System.Serializable]
public class ResourceDisplayItem {
    public int typeID;
    public ExpansionButtonView expansionButton;
    public List<InstantiatedResource> resources = null;
    public List<RequiredResources> resourceReqs = null;

    public List<CraftingData.ResourceByCategory> resourcesByCategory = null;
    public ResourceDisplayItem(ExpansionButtonView _expansionButtonView, List<InstantiatedResource> resList) {
        typeID = 1;
        expansionButton = _expansionButtonView;
        resources = resList;
    }
    public ResourceDisplayItem(ExpansionButtonView _expansionButtonView, List<RequiredResources> resList) {
        typeID = 2;
        expansionButton = _expansionButtonView;
        resourceReqs = resList;
    }
    public ResourceDisplayItem(ExpansionButtonView _expansionButtonView, List<CraftingData.ResourceByCategory> resList) {
        typeID = 3;
        expansionButton = _expansionButtonView;
        resourcesByCategory = resList;
    }

}