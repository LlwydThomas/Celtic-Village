using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class DisplayPawnView : MonoBehaviour {
    // Start is called before the first frame update
    public GameObject skillItemPrefab;
    public Slider hungerBar, tirednessBar, healthBar;
    public TextMeshProUGUI hungerText, tirednessText, currentTaskText;
    public Button funcHandlerButton, bedLocationButton, editName;
    public TextMeshProUGUI[] translateableStaticTMPs;
    public TMP_InputField pawnNameText, pawnAgeText;
    private ControllerManager controllerManager;
    public List<GameObject> pawnPanels;
    private Pawn currentItem;
    private SettingsController settings;
    public InventoryDisplayView inventoryDisplayView;
    private UnityAction statusChangeActions, inventoryChangeActions, workerInfoChangeActions;
    private bool listenForChanges;
    public Transform pawnImage;
    public ManagerReferences managerReferences;
    private Image tirednessImage, hungerImage, healthImage;
    private UiManagement uiManagement;
    public Button[] toggledPanelButtons;
    private int previousTab = -1;
    void FormatDropdown() {
        /* TMP_Dropdown newSkillItem = GameObject.Find ("BuildingType").GetComponent<TMP_Dropdown> ();
        newSkillItem.ClearOptions ();
        newSkillItem.AddOptions (translatedTypes); */

    }

    // Update is called once per frame
    private void OnDisable() {
        AmendListerners(false);
        currentItem = null;
    }

    private void Awake() {
        controllerManager = managerReferences.controllerManager;
        settings = controllerManager.settingsController;
        hungerImage = hungerBar.fillRect.transform.GetComponent<Image>();
        tirednessImage = tirednessBar.fillRect.transform.GetComponent<Image>();
        healthImage = healthBar.fillRect.transform.GetComponent<Image>();
    }

    private void OnEnable() {
        TogglePawnPanels(previousTab);
        FormatStaticTextLanguage();
    }

    private void Update() {
        if (currentItem != null) {
            if (Input.GetKeyDown(KeyCode.LeftShift)) {
                if (currentItem.pawnController.CurrentTaskGroup != null) {
                    Debug.Log("DPV - Current pawn (" + currentItem.name + ") is currently " + currentItem.pawnController.CurrentTaskGroup.associatedTasks[0] + " as part of " + currentItem.pawnController.CurrentTaskGroup.taskTypeID);
                }

            }
        }
    }

    private void Start() {
        uiManagement = managerReferences.uiManagement;
        pawnNameText.interactable = false;
        editName.onClick.AddListener(delegate { pawnNameText.interactable = !pawnNameText.interactable; });
        pawnNameText.onEndEdit.AddListener(delegate {
            AmendPawnName(pawnNameText.text);
            uiManagement.TriggerGameControlStoppage(haltControls: 0, timeHalt: -1);
        });
        pawnNameText.onSelect.AddListener(delegate { uiManagement.TriggerGameControlStoppage(haltControls: 1, timeHalt: -1); });
    }

    public void TogglePawnPanels(int index) {
        foreach (GameObject x in pawnPanels) {
            x.SetActive(false);
        }

        index = index == -1 ? 0 : index;
        pawnPanels[index].SetActive(true);
        previousTab = index;
    }

    public void setPawn(GameObject pawn) {
        Pawn current = controllerManager.skillsController.RequestPawnGameObject(pawn);
        Debug.Log("DPV - pawn set to pawn" + current.id);
        currentItem = current;
        //Debug.Log(currentPawnSkills.Count);
        AmendListerners(true);
        DisplayPawn(current);
        RefreshStatusLevels(current.pawnStatus);
        RefreshWorkingInfo(current);
    }

    private void AmendListerners(bool enable) {
        if (currentItem == null) return;
        if (enable) {
            statusChangeActions = () => RefreshStatusLevels(currentItem.pawnStatus);
            inventoryChangeActions = delegate { inventoryDisplayView.PopulateInventoryList(currentItem.storageContainer, managerReferences, 80f); };
            workerInfoChangeActions = delegate { RefreshWorkingInfo(currentItem); };
        }
        if (enable) {
            EventController.StartListening("pawnStatusChange", statusChangeActions);
            EventController.StartListening(currentItem.storageContainer.id + "storageAmended", inventoryChangeActions);
            EventController.StartListening(currentItem.id + "HasNewTaskGroup", workerInfoChangeActions);
        } else {
            EventController.StopListening("pawnStatusChange", statusChangeActions);
            EventController.StopListening(currentItem.storageContainer.id + "storageAmended", inventoryChangeActions);
            EventController.StopListening(currentItem.id + "HasNewTaskGroup", workerInfoChangeActions);
            bedLocationButton.onClick.RemoveAllListeners();
        }
        listenForChanges = enable;

    }

    private void FormatStaticTextLanguage() {
        SettingsFunctions.TranslateTMPItems(controllerManager.settingsController, translateableStaticTMPs);
        foreach (Button toggled in toggledPanelButtons) {
            toggled.gameObject.SetActive(true);
            ToolTipHandler toolTipHandler;
            if (!toggled.gameObject.TryGetComponent<ToolTipHandler>(out toolTipHandler)) {
                toolTipHandler = toggled.gameObject.AddComponent<ToolTipHandler>();
                string translation = toggled.gameObject.name;
                translation = translation.Substring(0, translation.IndexOf("Button"));
                translation = controllerManager.settingsController.TranslateString(translation);
                toolTipHandler.SetTooltipData(translation, 0, null);
            }
        }
    }

    private void AmendPawnName(string newName) {
        if (newName.Length > 0) {
            currentItem.name = newName;
            currentItem.pawnGameObject.name = newName;
        } else {
            pawnNameText.SetTextWithoutNotify(currentItem.name);
        }
        managerReferences.viewManager.pawnListDisplay.AmendPawnNames();
        pawnNameText.interactable = false;
    }

    private void RefreshWorkingInfo(Pawn pawn) {
        TaskGroup currentTaskGroup = pawn.pawnController.CurrentTaskGroup;
        //Debug.Log("DPV - current task group: " + currentTaskGroup.taskTypeID);
        /* TaskData currentTask = null; */

        TaskData taskData = currentTaskGroup != null ? controllerManager.taskController.FindTaskDataByID(currentTaskGroup.taskTypeID) : null;
        string taskText = GeneralFunctions.DescribeCurrentTask(currentTaskGroup, controllerManager.settingsController, taskData);
        currentTaskText.SetText(taskText);

        FunctionHandler currentHandler = pawn.pawnController.PeekPawnHandler();
        funcHandlerButton.onClick.RemoveAllListeners();
        if (currentHandler.id != 1) {
            Build currentEmployer = currentHandler.relatedBuild;
            funcHandlerButton.GetComponentInChildren<TextMeshProUGUI>().SetText(currentEmployer.uniqueName);
            UiManagement uiManagement = managerReferences.uiManagement;
            UnityAction funcHandOnClick = delegate {
                uiManagement.ManageOpenDialogues(true, 0);
                uiManagement.buildingDialogueView.SetBuildingType(currentHandler.relatedBuild);
            };
            funcHandlerButton.onClick.AddListener(funcHandOnClick);
        } else {
            funcHandlerButton.GetComponentInChildren<TextMeshProUGUI>().SetText("N/A");
        }
        Node sleepingSpot = pawn.sleepingNode;
        if (sleepingSpot == null) bedLocationButton.GetComponentInChildren<TextMeshProUGUI>().SetText("N/A");
        else {
            bedLocationButton.GetComponentInChildren<TextMeshProUGUI>().SetText("(" + sleepingSpot.worldPosition.x + ", " + sleepingSpot.worldPosition.y + ")");
            bedLocationButton.onClick.AddListener(delegate { managerReferences.uiManagement.scrollView.PanToLocation(sleepingSpot.worldPosition); });
        }
    }

    public void DisplayPawn(Pawn pawn) {
        // Populate the fields with information pertaining to a specific pawn, populate its inventory and setup the sprite.
        currentItem = pawn;
        pawnNameText.SetTextWithoutNotify(pawn.name);
        pawnAgeText.SetTextWithoutNotify(pawn.age.ToString());
        for (int i = 0; i < pawn.pawnController.clothingSprites.Length; i++) {
            Image image = pawnImage.GetChild(i + 1).GetComponent<Image>();
            image.sprite = pawn.pawnController.clothingSprites[i].sprite;
            image.color = pawn.pawnColours[i];
        }
        //TextMeshProUGUI text = Stats.GetComponentInChildren<TextMeshProUGUI>();
        string insertion = "";
        //PopulateSkillList(pawn.skillsList);
        Debug.Log("StorageContainerID: " + pawn.storageContainer.inventory.Count);
        inventoryDisplayView.PopulateInventoryList(pawn.storageContainer, managerReferences, 80f);
    }

    public void RefreshStatusLevels(PawnStatus pawnStatus) {
        // If pawn status changes whilst this dialogue is active, refresh the values, and adjust the images accordingly.
        hungerBar.value = currentItem.pawnStatus.hungerLevel;
        if (hungerBar.value <= 20) hungerImage.color = Color.red;
        else hungerImage.color = GeneralFunctions.greenSwatch;
        tirednessBar.value = currentItem.pawnStatus.tirednessLevel;
        if (tirednessBar.value <= 20) tirednessImage.color = Color.red;
        else tirednessImage.color = GeneralFunctions.greenSwatch;
        healthBar.value = currentItem.pawnStatus.totalHealth;
        if (healthBar.value <= 20) healthImage.color = Color.red;
        else healthImage.color = GeneralFunctions.greenSwatch;
    }

}