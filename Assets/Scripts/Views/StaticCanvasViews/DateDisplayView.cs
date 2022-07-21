using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DateDisplayView : MonoBehaviour {
    // Start is called before the first frame update
    private DateController dateController;
    private UiManagement ui;
    private TimeModel timeModel;
    public TextMeshProUGUI textFieldDate, textFieldTime;
    private ModelManager modelManager;

    [SerializeField]
    private DateTimeObject currentDateTime;
    public ManagerReferences managerReferences;
    private ControllerManager controllerManager;
    SettingsController settingsController;
    public Button pauseButton, playButton, decreaseSpeedButton, increaseSpeedButton;
    public Image pauseBackground, playBackground;
    SeasonData previousSeason = null;
    void Start() {
        modelManager = managerReferences.modelManager;
        timeModel = modelManager.timeModel;
        controllerManager = managerReferences.controllerManager;
        dateController = controllerManager.dateController;
        ui = managerReferences.uiManagement;
        settingsController = controllerManager.settingsController;
        FormatButtons();
        EventController.StartListening("gameSpeedChange", ColourButtonsBasedOnSpeed);
    }

    // Update is called once per frame
    private void FixedUpdate() {
        currentDateTime = dateController.ReturnCurrentDateTime();
        if (modelManager.weatherModel.currentSeason != null) {
            SeasonData currentSeason = modelManager.weatherModel.currentSeason;
            if (currentSeason != previousSeason) {
                string date = settingsController.TranslateString(currentSeason.uniqueName) + " " + settingsController.TranslateString("Year") + " " + currentDateTime.years;
                textFieldDate.SetText(date);
            }
            previousSeason = currentSeason;
        }
        string time = DateStringConvert(currentDateTime.hours) + ":" + DateStringConvert(currentDateTime.minutes);
        textFieldTime.SetText(time);
    }

    private void FormatButtons() {
        pauseButton.onClick.AddListener(delegate {
            if (!ui.controlsHalted) {
                dateController.AmendSpeed(0);
            }
        });
        playButton.onClick.AddListener(delegate {
            if (!ui.controlsHalted) {
                float gameSpeed = dateController.GameSpeedReturn();
                if (gameSpeed == 0) dateController.AmendSpeed(1);
            }
        });
        increaseSpeedButton.onClick.AddListener(delegate {
            if (!ui.controlsHalted) {
                dateController.IncreaseOrDecreaseSpeed(0.5f);
            }
        });
        decreaseSpeedButton.onClick.AddListener(delegate {
            if (!ui.controlsHalted) {
                dateController.IncreaseOrDecreaseSpeed(-0.5f);
            }
        });
    }

    public void ColourButtonsBasedOnSpeed() {
        float currentSpeed = dateController.GameSpeedReturn();
        if (currentSpeed > 0) {
            pauseBackground.color = GeneralFunctions.blackBackground;
            playBackground.color = GeneralFunctions.greenSwatch;
        } else {
            pauseBackground.color = GeneralFunctions.greenSwatch;
            playBackground.color = GeneralFunctions.blackBackground;
        }
    }

    private string DateStringConvert(int subject) {
        if (subject >= 10) {
            return subject.ToString();
        } else return "0" + subject;
    }
}