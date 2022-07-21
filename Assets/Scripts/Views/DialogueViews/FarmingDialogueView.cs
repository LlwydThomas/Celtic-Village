using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class FarmingDialogueView : MonoBehaviour {
    public LayerMask farmTagLayer;
    public ControllerManager controllerManager;
    public Material lineMat;
    public UiManagement uiManage;
    private bool farmParentSelection;
    private LineRenderer lineRenderer;
    private Farm currentFarm;
    public TextMeshProUGUI cropLabel, farmLabel;
    public TMP_Dropdown cropDropdown;
    public Button farmSelectButton, farmDeleteButton;
    public Image floraImage;
    public TextMeshProUGUI farmSetText;
    private SettingsController settingsController;
    public Sprite emptyOutputSprite;

    public ToolTipHandler outOfSeasonTip;

    public List<FloraData> currentAvailable;
    private void Update() {
        if (farmParentSelection) {
            if (Input.GetMouseButtonDown(0)) {
                HandleFarmParentSelect(uiManage.objectSelectView.GameObjectSelector(farmTagLayer));
            }
            if (Input.GetMouseButtonDown(1)) {
                BeginFarmSelection(false);
            }
        }
    }
    private void OnEnable() {
        if (settingsController == null) settingsController = controllerManager.settingsController;
        TextMeshProUGUI[] translates = new TextMeshProUGUI[] { farmLabel, cropLabel };
        SettingsFunctions.TranslateTMPItems(settingsController, translates);
    }

    public void SetFarmDisplay(Farm farm) {
        currentFarm = farm;
        FormatLanguage();
        farmParentSelection = false;
        PrepareDisplay(farm);
        OverwriteLineRenderer();
        // Set Current display to the clicked farm.
    }
    private void OnDisable() {
        DeleteLineRenderer();
        BeginFarmSelection(false);
    }

    private void DeleteLineRenderer() {
        if (lineRenderer != null) {
            Destroy(lineRenderer.gameObject);
            lineRenderer = null;
        }
    }

    private void OverwriteLineRenderer() {
        DeleteLineRenderer();
        if (currentFarm.farmingHandler != null) {
            lineRenderer = GeneralFunctions.DrawConnectingLine(currentFarm.farmingHandler.relatedBuild.buildingGameObject.transform.position, currentFarm.farmObject.transform.position, lineMat, this.gameObject.transform);
            lineRenderer.name = "FarmConnectionLine";
        }
    }

    private void FormatLanguage() {
        SettingsController settings = controllerManager.settingsController;
        cropLabel.SetText(settings.TranslateString(cropLabel.text));
        farmLabel.SetText(settings.TranslateString(farmLabel.text));
        farmDeleteButton.onClick.AddListener(delegate { DeleteFarm(currentFarm); });
    }

    private void DeleteFarm(Farm farm) {
        if (farm == null) return;
        UnityAction onComplete = null;
        onComplete += delegate {
            uiManage.ManageOpenDialogues(false);
            controllerManager.farmingController.DeleteFarm(farm);
        };
        uiManage.ActivateConfirmationDialogue("farm" + farm.ID + "Deletion", "FarmDeleteInfo", onComplete, null);
    }
    public void BeginFarmSelection(bool active) {
        farmParentSelection = active;
        if (active) {
            uiManage.LockUIElements(1, 1, 1, 1);
            farmSetText.SetText(settingsController.TranslateString("PressToCancel"));
        } else {
            uiManage.LockUIElements(0, 0, 0, 0);
            PrepareDisplay(currentFarm);
        }
    }

    public void PrepareDisplay(Farm farm) {
        if (farm == null) return;
        if (farm.farmingHandler == null) {
            farmSetText.SetText(settingsController.TranslateString("ClickToSet"));
        } else {
            farmSetText.SetText(farm.farmingHandler.relatedBuild.uniqueName);
        }

        PopulateCropDropdown();
    }

    private void PopulateCropDropdown() {
        cropDropdown.ClearOptions();
        cropDropdown.onValueChanged.RemoveAllListeners();
        if (currentFarm.relFuncHandID != -1) {
            SeasonData current = controllerManager.weatherController.SeasonNumReturn();
            cropDropdown.interactable = true;
            FunctionHandler farmHandler = controllerManager.buildingController.FindFuncHandlerByID(currentFarm.relFuncHandID);
            Build build = farmHandler.relatedBuild;
            List<FloraData> structureFloras = build.purposeData.possibleFloraCreations;
            structureFloras = BuildingFunctions.FlorasOfQuality(structureFloras, build.structureData.qualityTier);
            structureFloras.OrderBy(x => x.ID);
            List<string> optionList = new List<string>();
            foreach (FloraData flora in structureFloras) {
                optionList.Add(controllerManager.settingsController.TranslateString(flora.uniqueType));
            }
            cropDropdown.AddOptions(optionList);
            cropDropdown.SetValueWithoutNotify(1);
            cropDropdown.value = structureFloras.IndexOf(currentFarm.floraData);
            cropDropdown.Select();
            cropDropdown.RefreshShownValue();
            if (!currentFarm.floraData.growthSeasons[current.id - 1]) {
                outOfSeasonTip.gameObject.SetActive(true);
                string outOfSeasonText = controllerManager.settingsController.TranslateString("OutOfSeason");
                outOfSeasonTip.SetTooltipData(outOfSeasonText, 0, null);
            } else outOfSeasonTip.gameObject.SetActive(false);

            cropDropdown.onValueChanged.AddListener(delegate { AmendCurrentFarmOutput(currentFarm, structureFloras[cropDropdown.value]); });
            floraImage.sprite = currentFarm.floraData.icon;
        } else {
            cropDropdown.interactable = false;
            floraImage.sprite = emptyOutputSprite;
        }
    }

    private void AmendCurrentFarmOutput(Farm farm, FloraData newFloraData) {
        UnityAction amendFarmOutput = null;
        UnityAction cancelActions = null;
        amendFarmOutput += delegate {
            controllerManager.farmingController.AmendFarmOutput(farm, newFloraData, true);
            PrepareDisplay(farm);
        };

        cancelActions += delegate {
            PrepareDisplay(farm);
        };
        string[] parameters = new string[2];
        parameters[0] = controllerManager.settingsController.TranslateString(farm.floraData.uniqueType);
        parameters[1] = controllerManager.settingsController.TranslateString(newFloraData.uniqueType);
        uiManage.ActivateConfirmationDialogue("Farm" + farm.ID + "AmendOutput", "AmendFarmOutput", amendFarmOutput, cancelActions, stringParams : parameters);

        floraImage.sprite = newFloraData.icon;
    }
    private void HandleFarmParentSelect(GameObject gameObject) {
        if (gameObject != null) {
            // Handle assignment of object.
            Build selectedBuild = controllerManager.buildingController.RequestBuildFromGameObject(gameObject);
            if (selectedBuild != null) {
                if (selectedBuild.purposeData.ID == 5) {
                    controllerManager.farmingController.SetFunctionHandler(currentFarm, selectedBuild.functionHandler);
                    Debug.Log(gameObject.name);
                    PrepareDisplay(currentFarm);
                    BeginFarmSelection(false);
                    OverwriteLineRenderer();
                }
            } else Debug.Log("Build is null.");

        }
    }
}