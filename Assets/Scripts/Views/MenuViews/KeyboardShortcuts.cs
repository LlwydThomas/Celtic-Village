using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardShortcuts : MonoBehaviour {
    public ManagerReferences managerReferences;
    private UiManagement uiManagement;
    private ControllerManager controllerManager;
    private int gameSpeed = 1;

    private void Start() {
        uiManagement = managerReferences.uiManagement;
        controllerManager = managerReferences.controllerManager;
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.F5)) {
            controllerManager.saveGameController.SaveState("quicksave", true);
        }

        if (Input.GetKeyDown(KeyCode.Escape)) {

            Debug.Log("Total active dialogues: " + uiManagement.ActiveDialogueIndexReturn());
            if (uiManagement.ActiveDialogueIndexReturn() == -1) {
                uiManagement.ManageOpenDialogues(false, 2);
            } else {
                if (uiManagement.closingAllowed) {
                    uiManagement.ManageOpenDialogues(false);
                }
            }
        }

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.F1)) {
            GeneralEnumStorage.developerOptions = !GeneralEnumStorage.developerOptions;
            controllerManager.gridController.ToggleNodeIconVisibility(0);
        }

        if (!uiManagement.controlsHalted) {
            if (Input.GetKeyDown(KeyCode.H)) {
                managerReferences.viewManager.objectSelectView.RoofToggle();
            }
            if (Input.GetKeyDown(KeyCode.Equals)) {
                controllerManager.dateController.IncreaseOrDecreaseSpeed(0.5f);
            }
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Equals)) {
                gameSpeed = 15;
                controllerManager.dateController.AmendSpeed(gameSpeed);
            }
            if (Input.GetKeyDown(KeyCode.Minus)) {
                controllerManager.dateController.IncreaseOrDecreaseSpeed(-0.5f);
            }
            if (Input.GetKeyDown(KeyCode.Space)) {
                gameSpeed = controllerManager.dateController.TriggerPause();
            }
            if (Input.GetKeyDown(KeyCode.Tab)) {
                uiManagement.warningLogView.ToggleExpandedMessages();
            }
            if (Input.GetKeyDown(KeyCode.F2)) {
                controllerManager.buildingController.ShowOrHidePurposeIcons();
            }
        }
    }

}