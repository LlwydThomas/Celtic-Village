using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class CraftingQueueView : MonoBehaviour {
    // Start is called before the first frame update
    public ManagerReferences managerReferences;
    private ControllerManager controllerManager;
    public TextMeshProUGUI[] translatableTexts;
    public GameObject craftingItemPrefab, expandableDialogue, expandButtonPrefab, inventoryItemPrefab;
    public TMP_InputField countSelect;
    public Transform craftingItemParent, inputOutputParent, recipeParent;
    public RectTransform overallExpansion, inputRect, craftingQueueRect, recipeListRect;
    private List<ResourceDisplayItem> resourceDisplays = new List<ResourceDisplayItem>();
    private List<ExpansionButtonView> recipeExpands = new List<ExpansionButtonView>();
    public Button confirmButton, cancelButton, toggleExpanded;
    private List<CraftingData> craftingDatas;
    private System.Action contentHeight;
    private CraftingData crafting;

    private Build current;
    private void Start() {
        controllerManager = managerReferences.controllerManager;
        SettingsFunctions.TranslateTMPItems(controllerManager.settingsController, translatableTexts);
        countSelect.gameObject.GetComponent<ToolTipHandler>().SetTooltipData("Set to -1 for unlimited crafting", 0, null);
    }

    private void OnDisable() {
        if (current != null) EventController.StopListening(current.id + "CraftingQueueChange", delegate { PopulateCraftingScroll(current.functionHandler.craftingQueueItems); });
    }

    public void FormatCraftingList(Build build, List<CraftingQueueItem> craftingQueue, List<CraftingData> craftingDatasAvailable) {
        if (controllerManager == null) controllerManager = managerReferences.controllerManager;
        Debug.Log("CQV - Formatting craft list for build " + build.id + " with a total of " + craftingDatasAvailable.Count + " recipes available and a queue size of " + craftingQueue.Count);
        craftingDatas = craftingDatasAvailable;
        current = build;
        toggleExpanded.onClick.RemoveAllListeners();
        toggleExpanded.onClick.AddListener(delegate { ToggleExpandedView(true); });
        PopulateCraftingScroll(craftingQueue);
        //FormatCountSelect();
        RecipeSelectBegin();
        if (crafting == null) crafting = craftingDatas[0];
        FormatRecipeColours(crafting.id);
        PopulateInputOutputResources(crafting);
        EventController.StartListening(build.id + "CraftingQueueChange", delegate { PopulateCraftingScroll(build.functionHandler.craftingQueueItems); });
    }

    private void PopulateCraftingScroll(List<CraftingQueueItem> craftingQueueItems) {
        foreach (Transform t in craftingItemParent) {
            Destroy(t.gameObject);
        }
        foreach (CraftingQueueItem craftingQueue in craftingQueueItems) {
            GameObject game = GameObject.Instantiate(craftingItemPrefab, craftingItemParent, false);
            game.transform.GetChild(0).GetComponentInChildren<Button>().onClick.AddListener(delegate { RemoveCraftingOrder(craftingQueue); });
            StorageFunctions.FormatInventoryItem(game.transform.GetChild(1).gameObject, craftingQueue.craftingData.craftingRecipeName, craftingQueue.count, true, Color.white, Color.white, 80f, maxFont : 30);
            //StorageFunctions.FormatInventoryItem()
        }

        craftingQueueRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, craftingQueueItems.Count * 100);
    }

    private void RecipeSelectBegin() {
        PopulateCraftingRecipeList(craftingDatas);
        PopulateInputOutputResources(craftingDatas[0]);
    }

    private void PopulateCraftingRecipeList(List<CraftingData> craftingDatas) {
        if (recipeExpands.Count > 0) ClearRecipeList();
        foreach (CraftingData.CraftingCategory category in System.Enum.GetValues(typeof(CraftingData.CraftingCategory))) {
            List<CraftingData> currentList = craftingDatas.FindAll(x => x.craftingCategory == category);
            Debug.Log("CQV - Category name: " + category.ToString() + ", datas found: " + currentList.Count);
            if (currentList.Count > 0) {
                GameObject catExpand = Instantiate(expandButtonPrefab, recipeParent.transform, false);
                int index = (int) category;
                ExpansionButtonView expansion = catExpand.GetComponent<ExpansionButtonView>();
                recipeExpands.Add(expansion);
                expansion.FormatExpansionButton(controllerManager.settingsController, FindAndSetContentRecipe, category.ToString(), index, null, beginExpanded : true, expectedItems : currentList.Count, maxFont : 25f, name: "Recipes" + category.ToString());
                SettingsFunctions.TranslateTMPItems(controllerManager.settingsController, expansion.description);
                foreach (CraftingData craftingData in currentList) {
                    GameObject newItem = GameObject.Instantiate(inventoryItemPrefab, expansion.resultantList.transform, false);
                    string resName = controllerManager.settingsController.TranslateString(craftingData.craftingRecipeName);
                    StorageFunctions.FormatInventoryItem(newItem, resName, -1, false, Color.white, Color.white, 80f, maxFont : 28f);
                    expansion.resultantObjectsDict.Add(craftingData.id, newItem);
                    expansion.resultantItems.Add(newItem);
                    Button button = newItem.AddComponent<Button>();
                    button.onClick.AddListener(delegate {
                        PopulateInputOutputResources(craftingData);
                        FormatRecipeColours(craftingData.id);
                        crafting = craftingData;
                    });
                }
            }
        }
        FindAndSetContentRecipe();
    }

    private void FormatRecipeColours(int id) {
        foreach (ExpansionButtonView expansion in recipeExpands) {
            foreach (GameObject game in expansion.resultantItems) {
                StorageFunctions.ChangeInvItemBackground(game, GeneralFunctions.blackBackground, true);
            }
            if (expansion.resultantObjectsDict.ContainsKey(id)) StorageFunctions.ChangeInvItemBackground(expansion.resultantObjectsDict[id], GeneralFunctions.greenSwatch, true);
        }
    }

    private void FindAndSetContentRecipe() {
        GeneralFunctions.SetContentHeight(recipeListRect, FindRecipeListsSize(), null);
    }
    private void FindAndSetContentInput() {
        GeneralFunctions.SetContentHeight(inputRect, FindSize(), null);
    }

    private float FindRecipeListsSize() {
        float size = 10;
        foreach (ExpansionButtonView expansion in recipeExpands) {
            size += expansion.rectTransform.sizeDelta.y;
        }
        Debug.Log("CQV - Size Calculation: " + size);
        return size;
    }

    private void ClearRecipeList() {
        foreach (ExpansionButtonView expansion in recipeExpands) {
            Destroy(expansion.gameObject);
        }

        recipeExpands.Clear();
    }

    public void PopulateInputOutputResources(CraftingData craftingData) {
        // Clear any remaining resource items.
        foreach (Transform child in inputOutputParent.transform) {
            Destroy(child.gameObject);
        }
        resourceDisplays.Clear();
        FormatCountSelect();
        GameObject inputExpand = Instantiate(expandButtonPrefab, inputOutputParent.transform, false);
        GameObject outputExpand = Instantiate(expandButtonPrefab, inputOutputParent.transform, false);
        ExpansionButtonView inputExpandView = inputExpand.GetComponent<ExpansionButtonView>();
        ExpansionButtonView outputExpandView = outputExpand.GetComponent<ExpansionButtonView>();
        RectTransform inputRect = inputExpand.GetComponent<RectTransform>();
        RectTransform outputRect = outputExpand.GetComponent<RectTransform>();

        ResourceDisplayItem inputDisplay;
        int expectedInputs;
        if (craftingData.categoryInputs.Count > 0) {
            expectedInputs = craftingData.categoryInputs.Count;
            inputDisplay = new ResourceDisplayItem(inputExpandView, craftingData.categoryInputs);
        } else {
            expectedInputs = craftingData.inputs.Count;
            inputDisplay = new ResourceDisplayItem(inputExpandView, craftingData.inputs);
        }
        resourceDisplays.Add(inputDisplay);
        ResourceDisplayItem outputDisplay = new ResourceDisplayItem(outputExpandView, craftingData.outputs);
        resourceDisplays.Add(outputDisplay);
        SettingsController settings = controllerManager.settingsController;
        inputExpandView.FormatExpansionButton(settings, FindAndSetContentInput, "InputResources", 0, null, beginExpanded : true, expectedItems : expectedInputs, maxFont : 25f);
        outputExpandView.FormatExpansionButton(settings, FindAndSetContentInput, "OutputResources", 1, null, beginExpanded : true, expectedItems : craftingData.outputs.Count, maxFont : 25f);

        List<InstantiatedResource> totalResources = controllerManager.storageController.CompileTotalResourceList();
        Vector3 offset = new Vector3(-300f, 0, 0);
        if (inputDisplay.typeID == 3) {
            FormatCategoryInput(inputDisplay);
        } else {
            foreach (RequiredResources req in inputDisplay.resourceReqs) {
                Color colour;
                GameObject newItem = GameObject.Instantiate(inventoryItemPrefab, inputDisplay.expansionButton.resultantList.transform, false);
                string resName = controllerManager.settingsController.TranslateString(req.resource.resourceName);
                if (StorageFunctions.CheckIfResourceAvailable(req, totalResources)) colour = Color.green;
                else colour = Color.red;
                StorageFunctions.FormatInventoryItem(newItem, resName, req.count, false, colour, req.resource.iconColour, 80f, req.resource.icon, 28f);
                StorageFunctions.AppendResourceTooltip(managerReferences, req.resource, newItem, offset);
            }
        }

        foreach (RequiredResources req in outputDisplay.resourceReqs) {
            GameObject newItem = GameObject.Instantiate(inventoryItemPrefab, outputDisplay.expansionButton.resultantList.transform, false);
            string resName = controllerManager.settingsController.TranslateString(req.resource.resourceName);
            StorageFunctions.FormatInventoryItem(newItem, resName, req.count, false, Color.white, req.resource.iconColour, 80f, req.resource.icon, 28f);
            StorageFunctions.AppendResourceTooltip(managerReferences, req.resource, newItem, offset);
        }
        FindAndSetContentInput();
    }

    private void ToggleExpandedView(bool active) {
        if (active) {
            expandableDialogue.SetActive(true);
            FormatExpandedButtons();
        } else expandableDialogue.SetActive(false);
    }

    private void FormatCategoryInput(ResourceDisplayItem inputDisplay) {
        SettingsController settings = controllerManager.settingsController;
        foreach (CraftingData.ResourceByCategory req in inputDisplay.resourcesByCategory) {
            Color colour;
            GameObject newItem = GameObject.Instantiate(inventoryItemPrefab, inputDisplay.expansionButton.resultantList.transform, false);
            string resName = "(" + settings.TranslateString("Any") + ") ";
            resName += settings.TranslateString(req.subCategory.ToString());
            if (controllerManager.storageController.CheckIfSubCategoryInStorage(req)) colour = Color.green;
            else colour = Color.red;
            StorageFunctions.FormatInventoryItem(newItem, resName, req.count, false, colour, Color.white, 80f, maxFont : 28f);
        }
    }

    private int FindCount(ResourceDisplayItem resourceDisplay) {
        switch (resourceDisplay.typeID) {
            case 1:
                return resourceDisplay.resources.Count;
            case 2:
                return resourceDisplay.resourceReqs.Count;
            case 3:
                return resourceDisplay.resourcesByCategory.Count;
        }
        return -1;
    }
    private void FormatCountSelect() {
        countSelect.onEndEdit.AddListener(delegate { ClampCount(countSelect); });
    }
    private float FindSize() {
        float size = 5;
        foreach (ResourceDisplayItem res in resourceDisplays) {
            size += res.expansionButton.rectTransform.sizeDelta.y + 5;
        }
        return size;
    }

    private void ClampCount(TMP_InputField count) {
        int converted;
        Debug.Log("CQV - count text:" + count.text);
        if (count.text == "∞") return;
        if (int.TryParse(count.text, out converted)) {
            Debug.Log("CQV - Int parsed with value of " + converted);
            if (converted < 0) {
                count.SetTextWithoutNotify("∞");
            }
        } else count.SetTextWithoutNotify("");
    }

    private void FormatExpandedButtons() {
        Debug.Log("CQV - FormattingExpandedButtons");
        cancelButton.onClick.RemoveAllListeners();
        confirmButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(delegate { ToggleExpandedView(false); });
        confirmButton.onClick.AddListener(delegate {
            int count;
            if (countSelect.text == "∞") count = -1;
            else count = int.Parse(countSelect.text);
            AppendNewCraftOrder(crafting, count);
            ToggleExpandedView(false);
        });
    }

    private void AppendNewCraftOrder(CraftingData craftingData, int count) {
        CraftingQueueItem queueItem = new CraftingQueueItem(craftingData, count);
        current.functionHandler.AppendCraftOrder(queueItem);
        PopulateCraftingScroll(current.functionHandler.craftingQueueItems);
    }

    private void RemoveCraftingOrder(CraftingQueueItem craftingQueue) {
        current.functionHandler.ReduceCraftingOrder(craftingQueue);
        PopulateCraftingScroll(current.functionHandler.craftingQueueItems);
    }

}