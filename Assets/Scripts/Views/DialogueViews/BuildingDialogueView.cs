using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class BuildingDialogueView : MonoBehaviour {
    // Start is called before the first frame update
    public GameObject Circle, typeDropdown;
    public ManagerReferences managerReferences;
    private ControllerManager controllerManager;
    public UiManagement uiManagement;
    private List<WorkerPawnView> workerPawns = new List<WorkerPawnView>();
    public Image updateableImage, iconImage;
    public TextMeshProUGUI workersLabel, workersCount, variableLabel;
    private Build currentItem;
    private BuildingController buildingController;
    private SkillsController skillsController;
    private GameObject radius;
    public TMP_Dropdown buildingPurposeDropdown, variableOutputDropdown;
    public GameObject debugPanel;
    public ScrollRect[] pawnListScrolls;
    public TextMeshProUGUI[] translatableTexts;
    public GameObject pawnItemPrefab, pawnItemParent, possiblePawnItemParent, possiblePawnItemPrefab, expandedDialogue, expandedPurposes;
    public GameObject textBoxButtonPrefab;
    public Button scrollSwitchButton, editName, editPurpose;
    public RectTransform buttonsPanel, purposeRect;
    public InventoryDisplayView inventoryDisplay;
    public TMP_InputField nameInput, variableInput, purposeInput;
    public Button[] toggledPanelButtons;
    public GameObject[] panels;
    public CraftingQueueView craftingQueueView;
    private string eventTriggerString = null;
    private UnityAction eventAction = null;
    private int previousTab = -1;
    [SerializeField]
    private bool bypassRequired;

    private void Awake() {
        controllerManager = managerReferences.controllerManager;
        buildingController = controllerManager.buildingController;
        skillsController = controllerManager.skillsController;
        FormatButtons();
    }

    private void OnDisable() {
        if (eventTriggerString != null) {
            EventController.StopListening(eventTriggerString, eventAction);
        }
        Destroy(radius);
    }

    public void DisplayBuild(Build build) {
        if (build == null) return;
        if (buildingController == null) buildingController = controllerManager.buildingController;
        if (skillsController == null) skillsController = controllerManager.skillsController;
        eventTriggerString = null;
        eventAction = null;
        //Debug.Log("Build " + build.id + " has tasks:" + build.functionHandler.TaskQueueReturn().Count);
        Debug.Log(build.uniqueName);
        ManageActivePanels(build);
        DisplayBuildingImage(build);
        //InitiateDropdown(build);
        InitialisePurposes(build);
        nameInput.SetTextWithoutNotify(build.uniqueName);
        nameInput.interactable = false;
        expandedDialogue.SetActive(false);
        SettingsFunctions.TranslateTMPItems(controllerManager.settingsController, translatableTexts);
        radius = GenerateRadius(radius);
        PopulateVariableField(build.purposeDataID);
        if (build.purposeData.workersRequired) {
            SwapPawnScrolls(allOpen: true);
            GenerateWorkersList(build);
            GeneratePossibleWorkerList(build);
            SwapPawnScrolls(relative: true);
        }
        if (build.purposeData.associatedStorage && build.storageContainer != null) {
            UnityAction inventoryChangeActions = delegate { inventoryDisplay.PopulateInventoryList(build.storageContainer, managerReferences, 80f); };
            inventoryChangeActions.Invoke();
            EventController.StartListening(build.storageContainer.id + "storageAmended", inventoryChangeActions);
        }

        //Debug.Log("Game Object Count: " + build.gameObjectsInRadius.Count);
    }

    private void FormatButtons() {
        scrollSwitchButton.onClick.AddListener(delegate { SwapPawnScrolls(relative: true); });
        editName.onClick.AddListener(delegate { nameInput.interactable = !nameInput.interactable; });
        editPurpose.onClick.AddListener(delegate {
            expandedPurposes.SetActive(!expandedPurposes.activeSelf);
            if (expandedPurposes.activeSelf) InitialisePurposes(currentItem);
        });
        nameInput.onEndEdit.AddListener(delegate {
            AmendBuildName(nameInput.text);
            uiManagement.TriggerGameControlStoppage(haltControls: 0, timeHalt: -1);
        });
        nameInput.onSelect.AddListener(delegate { uiManagement.TriggerGameControlStoppage(haltControls: 1, timeHalt: -1); });

    }

    private void PopulateVariableField(int purposeID) {
        if (currentItem == null) return;
        variableInput.transform.parent.gameObject.SetActive(true);
        SettingsController settings = controllerManager.settingsController;
        string eventTrigger = null;
        switch (purposeID) {
            case 1:
            case 3:
                eventTrigger = "funch" + currentItem.functionHandlerID + "RelevantObjectChange";
                string maxCreations = currentItem.functionHandler.RelevantObjectCount + "/" + currentItem.structureData.maxCreations;
                variableInput.SetTextWithoutNotify(maxCreations);
                variableLabel.SetText("MaxCreations:");
                break;
            case 5:
                eventTrigger = "funch" + currentItem.functionHandlerID + "RelevantObjectChange";
                string maxCrops = currentItem.functionHandler.RelevantObjectCount + "/" + currentItem.structureData.maxCropCount;
                variableInput.SetTextWithoutNotify(maxCrops);
                variableLabel.SetText("MaxCrops:");
                break;
            default:
                variableInput.transform.parent.gameObject.SetActive(false);
                break;
        }

        if (eventAction == null) {
            eventAction = () => PopulateVariableField(purposeID);
            if (eventTrigger != null) {
                eventTriggerString = eventTrigger;
                EventController.StartListening(eventTriggerString, eventAction);
            }
        }

        if (variableInput.transform.parent.gameObject.activeSelf) {
            SettingsFunctions.TranslateTMPItems(controllerManager.settingsController, variableLabel);
        }
    }

    private void AmendBuildName(string newName) {
        currentItem.uniqueName = newName;
        currentItem.buildingGameObject.name = newName;
        nameInput.interactable = false;
    }

    public void ManageActivePanels(Build build) {
        expandedPurposes.SetActive(false);
        bool[] required = new bool[3];
        required[0] = build.purposeData.workersRequired;
        required[1] = build.purposeData.associatedStorage;
        required[2] = (build.purposeData.possibleCraftingRecipes.Count > 0 && build.purposeDataID == 2);
        int count = 0;
        int panelFirstIndex = -1;
        for (int i = 0; i < required.Length; i++) {
            bool req = required[i];
            panels[i].SetActive(false);
            if (req) {
                if (panelFirstIndex == -1) panelFirstIndex = i;
                count++;
                int index = i;
                toggledPanelButtons[i].gameObject.SetActive(true);
                ToolTipHandler toolTipHandler;
                if (!toggledPanelButtons[i].gameObject.TryGetComponent<ToolTipHandler>(out toolTipHandler)) {
                    toolTipHandler = toggledPanelButtons[i].gameObject.AddComponent<ToolTipHandler>();
                    string translation = toggledPanelButtons[i].gameObject.name;
                    translation = translation.Substring(0, translation.IndexOf("Button"));
                    translation = controllerManager.settingsController.TranslateString(translation);
                    toolTipHandler.SetTooltipData(translation, 0, null);
                }
                toggledPanelButtons[i].onClick.AddListener(delegate {
                    TogglePanels(index);
                });
            } else toggledPanelButtons[i].gameObject.SetActive(false);
        }

        if (previousTab != -1) {
            if (required[previousTab]) {
                TogglePanels(previousTab);
                return;
            }
        }
        if (panelFirstIndex != -1) TogglePanels(panelFirstIndex);
        //buttonsPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, count * 70);
    }
    private void TogglePanels(int index = -1) {
        foreach (GameObject panel in panels) {
            panel.SetActive(false);
        }
        if (index != -1) {
            panels[index].SetActive(true);
            if (craftingQueueView.gameObject.activeSelf) craftingQueueView.FormatCraftingList(currentItem, currentItem.functionHandler.craftingQueueItems, BuildingFunctions.CraftingDatasOfQuality(currentItem.purposeData.possibleCraftingRecipes, currentItem.structureData.qualityTier));
            previousTab = index;
        }
    }

    public void SwapPawnScrolls(bool allOpen = false, bool relative = false) {
        if (allOpen) {
            foreach (ScrollRect scroll in pawnListScrolls) scroll.gameObject.SetActive(true);
        } else {
            if (relative) {
                int count = 0;
                foreach (ScrollRect scroll in pawnListScrolls) {
                    if (scroll.gameObject.activeSelf) count++;
                }
                if (count > 1) {
                    foreach (ScrollRect scroll in pawnListScrolls) scroll.gameObject.SetActive(false);
                    pawnListScrolls[0].gameObject.SetActive(true);
                } else {
                    foreach (ScrollRect scroll in pawnListScrolls) scroll.gameObject.SetActive(!scroll.gameObject.activeSelf);
                }
                string workerText;
                if (pawnListScrolls[0].gameObject.activeSelf) workerText = "AssignedPawns:";
                else workerText = "AvailablePawns:";
                workersLabel.SetText(workerText);
                SettingsFunctions.TranslateTMPItems(controllerManager.settingsController, workersLabel);
            }
        }
    }

    public Build RequestCurrentBuild() {
        return currentItem;
    }

    private void GenerateWorkersList(Build build) {
        foreach (Transform x in pawnItemParent.transform) Destroy(x.gameObject);
        int count = 0;
        foreach (Pawn pawn in skillsController.PawnListReturn(false)) {
            Debug.Log("Building Handler: " + build.functionHandlerID + ", Pawn Handler: " + pawn.functionHandlerID);
            if (pawn.functionHandlerID == build.functionHandlerID) {
                GameObject newItem = Instantiate(pawnItemPrefab, pawnItemParent.transform.position, Quaternion.identity, pawnItemParent.transform);
                WorkerPawnView pawnListItem = newItem.GetComponent<WorkerPawnView>();
                workerPawns.Add(pawnListItem);
                pawnListItem.pawnButton.onClick.AddListener(delegate { TriggerPawnDisplay(pawn); });
                pawnListItem.minusButton.onClick.AddListener(delegate { RemoveWorkerStatus(pawn, build); });
                pawnListItem.pawnName.SetText(pawn.name);
                count++;
            }
        }

        if (count == 0) translatableTexts[0].gameObject.SetActive(true);
        else translatableTexts[0].gameObject.SetActive(false);

        pawnItemParent.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 55 * count);
        workersCount.SetText(count + "/" + build.structureData.maxWorkers);
    }

    private void GeneratePossibleWorkerList(Build build) {
        foreach (Transform x in possiblePawnItemParent.transform) Destroy(x.gameObject);
        int count = 0;
        //Debug.Log(skillsController.PawnListReturn(false).Count + " = total possible pawns.");
        List<Pawn> pawnList = skillsController.PawnListReturn(false);
        foreach (Pawn pawn in pawnList) {
            //Debug.Log("Pawn " + pawn.id + " is current assigned to func handler " + pawn.functionHandlerID);
            //Debug.Log("Building Handler: " + build.functionHandlerID + ", Pawn Handler: " + pawn.functionHandlerID);
            if (pawn.functionHandlerID == 1) {
                GameObject newItem = Instantiate(possiblePawnItemPrefab, possiblePawnItemParent.transform.position, Quaternion.identity, possiblePawnItemParent.transform);
                WorkerPawnView pawnListItem = newItem.GetComponent<WorkerPawnView>();
                //workerPawns.Add(pawnListItem);
                pawnListItem.pawnButton.onClick.AddListener(delegate { TriggerPawnDisplay(pawn); });
                pawnListItem.minusButton.onClick.AddListener(delegate { AddWorkerStatus(pawn, build); });
                pawnListItem.pawnName.SetText(pawn.name);
                count++;
            }
        }

        if (count == 0) translatableTexts[1].gameObject.SetActive(true);
        else translatableTexts[1].gameObject.SetActive(false);
        Debug.Log("Total amount of possible workers: " + count);
        possiblePawnItemParent.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 55 * count);
    }

    public void TriggerPawnDisplay(Pawn pawn) {
        uiManagement.ManageOpenDialogues(true, 1);
        uiManagement.dialogues[1].GetComponent<DisplayPawnView>().setPawn(pawn.pawnGameObject);
    }

    public void RemoveWorkerStatus(Pawn pawn, Build build) {
        FunctionHandler generalHandler = controllerManager.buildingController.FindFuncHandlerByID(1);
        if (pawn.pawnController.ReassignFunctionHandler(generalHandler)) {
            GenerateWorkersList(build);
            GeneratePossibleWorkerList(build);
        }
    }

    public void AddWorkerStatus(Pawn pawn, Build build) {
        if (pawn.pawnController.ReassignFunctionHandler(build.functionHandler)) {
            GenerateWorkersList(build);
            GeneratePossibleWorkerList(build);
        }
    }

    public void DisplayBuildingImage(Build build) {
        updateableImage.sprite = build.structureData.buildingSprite;
        iconImage.sprite = build.purposeData.icon;
        Color iconColour = Color.white;
        switch (build.structureDataID) {
            case 1:
            case 2:
            case 3:
                iconColour = Color.black;
                break;
        }
        iconImage.color = iconColour;
    }

    private void InitialisePurposes(Build current) {
        purposeInput.SetTextWithoutNotify(controllerManager.settingsController.TranslateString(currentItem.purposeData.purposeName));
        List<PurposeData> purposeDatas = current.structureData.possiblePurposes;
        foreach (Transform transform in purposeRect.transform) {
            Destroy(transform.gameObject);
        }
        foreach (PurposeData purposeData in purposeDatas) {
            GameObject newItem = GameObject.Instantiate(textBoxButtonPrefab, purposeRect.transform, false);
            string purposeName = controllerManager.settingsController.TranslateString(purposeData.purposeName);
            TextBoxButtonView txtbox = newItem.GetComponent<TextBoxButtonView>();
            txtbox.FormatTextBox(delegate { AmendBuildingPurpose(current, purposeData.ID); }, purposeName, Color.white, 30);

            if (purposeData == current.purposeData) txtbox.background.color = GeneralFunctions.greenSwatch;
            else txtbox.background.color = GeneralFunctions.blackBackground;
        }

        GeneralFunctions.SetContentHeight(purposeRect, 105 * purposeDatas.Count + 10, null);
    }

    public void AmendBuildingPurpose(Build build, int purposeDataID) {
        //TMP_Dropdown dropdown = typeDropdown.GetComponent<TMP_Dropdown>();
        PurposeData purpose = controllerManager.buildingController.PurposeDataLookUp(purposeDataID);

        if (purpose == build.purposeData) return;
        UnityAction onComplete = null;
        UnityAction onCancel = null;
        // Prepare delegates to be ran if confirmation is confirmed.
        onComplete += (delegate {
            controllerManager.buildingController.AmendPurpose(build, purposeDataID, build.functionHandlerID);
            SetBuildingType(build);
            InitialisePurposes(build);
        });
        // Prepare delegates to be ran if confirmation is cancelled.
        //onCancel += (delegate { InitiateDropdown(build); });
        List<RequiredResources> reqs = bypassRequired && GeneralEnumStorage.debugActive ? null : GeneralFunctions.CopyResourceList(purpose.requiredResources);
        if (reqs != null) {
            int multiplier = (int) build.structureData.qualityTier + 1;
            Debug.Log("BDV - Multiplier: " + multiplier);
            foreach (RequiredResources requiredResources in reqs) {
                requiredResources.count *= multiplier;
            }
        }
        string translatedPurpose = controllerManager.settingsController.TranslateString(purpose.purposeName);
        uiManagement.ActivateConfirmationDialogue(build.id + "PurposeTo" + purposeDataID, "ChangeBuildingPurpose", onComplete, onCancel, reqs, stringParams : new string[] { translatedPurpose });

    }

    public void DestroyThis() {
        //Debug.Log("Game Object Count: " + currentItem.gameObjectsInRadius.Count);
        UnityAction action = null;
        action += delegate {
            controllerManager.buildingController.DeleteBuild(currentItem);
            uiManagement.ManageOpenDialogues(false);
        };
        uiManagement.ActivateConfirmationDialogue("build" + currentItem.id + "Deletion", "BuildDeleteInfo", action, null);
    }

    public void SetBuildingType(Build build) {
        //Debug.Log("Name: " + build.uniqueName + ", Obj Count: " + build.gameObjectsInRadius.Count);
        currentItem = build;
        //string math = build.structureData.name;
        DisplayBuild(build);
    }

    private GameObject GenerateRadius(GameObject radius = null) {
        if (radius != null) Destroy(radius);
        GameObject tempCircle = Instantiate(Circle, currentItem.worldPosition, Quaternion.identity, currentItem.buildingGameObject.transform);
        tempCircle.transform.localScale *= currentItem.purposeData.radius * currentItem.structureData.radiusMultiplier * 2f;
        return tempCircle;
    }
}