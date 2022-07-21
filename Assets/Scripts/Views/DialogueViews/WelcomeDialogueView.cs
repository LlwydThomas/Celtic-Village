using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class WelcomeDialogueView : MonoBehaviour {

    public TextMeshProUGUI[] translateables;
    public Button continueButton;
    public ManagerReferences managerReferences;
    public TextMeshProUGUI body, main;
    private ControllerManager controllerManager;
    // Start is called before the first frame update
    private void Start() {
        controllerManager = managerReferences.controllerManager;
    }

    private void OnEnable() {
        if (controllerManager == null) controllerManager = managerReferences.controllerManager;
    }

    public void SetMessageContents(bool welcome, string[] reasonParams = null) {
        continueButton.onClick.RemoveAllListeners();
        if (welcome) {
            main.SetText("WelcomeText");
            body.SetText("WelcomeBody");
            translateables[2].SetText("Continue");
            continueButton.onClick.AddListener(() => managerReferences.uiManagement.ManageOpenDialogues(false));
        } else {
            managerReferences.uiManagement.LockUIElements(1, 1, 1, 1);
            main.SetText("EndGameText");
            body.SetText("EndGameBody");
            translateables[2].SetText("Quit");
            continueButton.onClick.AddListener(() => managerReferences.controllerManager.saveGameController.QuitToTitleScreen());
        }

        SettingsFunctions.TranslateTMPItems(controllerManager.settingsController, translateables);

        if (reasonParams != null) {
            string translatedInsertion = string.Format(body.text, reasonParams);
            body.SetText(translatedInsertion);
        }
    }

    // Update is called once per frame

}