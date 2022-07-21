using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class ObjectInfoView : MonoBehaviour {
    public ManagerReferences managerReferences;
    private ControllerManager controllerManager;
    private UiManagement uiManagement;
    public TextMeshProUGUI itemName, growthText;
    private SettingsController settingsController;
    [SerializeField]
    private FloraItem currentFlora;
    private NPC.AnimalNPC currentAnimal;
    public string currentObjectType;
    public Button infoButton;
    public Slider growthSlider, healthSlider;
    public TextMeshProUGUI[] translateTexts;
    private UnityAction action;
    private void Awake() {
        controllerManager = managerReferences.controllerManager;
        uiManagement = managerReferences.uiManagement;
        settingsController = controllerManager.settingsController;

    }

    private void Start() {
        SettingsFunctions.TranslateTMPItems(controllerManager.settingsController, translateTexts);
    }
    private void OnEnable() {
        infoButton.onClick.AddListener(OpenHelpScreen);
    }
    private void OnDisable() {
        infoButton.onClick.RemoveAllListeners();
        if (currentFlora != null) {
            EventController.StopListening("growthProcessed", action);
        }
    }

    private void OpenHelpScreen() {
        // Open the In Game Menu and display the help screen.
        uiManagement.ManageOpenDialogues(true, 2);
        IGMView iGMView = uiManagement.iGMView;
        iGMView.SwitchHelpTab(1);
    }

    public void RefreshGrowth(FloraItem floraItem) {
        if (settingsController == null) return;
        if (floraItem != null) {
            float growth = floraItem.growthPercentage / 100f;
            float health = floraItem.floraHealth / 100f;
            PrepareDisplay(floraItem.floraData.uniqueType, true, true);
            growthSlider.SetValueWithoutNotify(growth);
            healthSlider.SetValueWithoutNotify(health);
            Image sliderImage = healthSlider.fillRect.transform.GetComponent<Image>();
            if (health <= 0.2f) sliderImage.color = Color.red;
            else sliderImage.color = GeneralFunctions.greenSwatch;
        }
    }

    public void SetAnimal(NPC.AnimalNPC animal) {
        currentObjectType = "Animal";
        if (animal.nPCTypeID == 2) {
            PrepareDisplay(animal.animalData.uniqueName, false, false, false);
        } else {
            NullifySelection();
            uiManagement.ManageOpenDialogues(false);
        }
    }

    public void SetFloraItem(GameObject gameObject) {
        currentObjectType = "Flora";
        FloraItem floraItem = controllerManager.natureController.GameObjectToFloraItem(gameObject);
        Debug.Log(floraItem.uniqueName);
        if (floraItem != null) {
            currentFlora = floraItem;
            action = delegate { RefreshGrowth(currentFlora); };
            EventController.StartListening("growthProcessed", action);
            RefreshGrowth(floraItem);
        } else {
            NullifySelection();
            uiManagement.ManageOpenDialogues(false);
        }
    }

    private void PrepareDisplay(string _itemName, bool _healthSlider, bool _growthSlider, bool _infoButton = true) {
        string translatedName = settingsController.TranslateString(_itemName);
        itemName.SetText(translatedName);
        healthSlider.gameObject.SetActive(_healthSlider);
        growthSlider.gameObject.SetActive(_growthSlider);
        if (!_healthSlider && !_growthSlider) {
            healthSlider.transform.parent.gameObject.SetActive(false);
        } else {
            healthSlider.transform.parent.gameObject.SetActive(true);
        }
        infoButton.gameObject.SetActive(_infoButton);
    }

    private void NullifySelection() {
        currentFlora = null;
        currentAnimal = null;
        currentObjectType = "";
    }
}