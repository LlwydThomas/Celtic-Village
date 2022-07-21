using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class InGameSaveLoad : MonoBehaviour {
    // Start is called before the first frame update

    private SaveGameItem selectedSaveGame;

    private bool saveSelected;

    public SaveFileView saveFileView;
    public Button loadButton, saveButton, deleteButton;
    public TMP_InputField saveName;
    public GameObject saveItemPrefab, saveListParent;
    public ManagerReferences managerReferences;
    private ControllerManager controllerManager;
    private UiManagement uiManagement;

    private void Awake() {
        controllerManager = managerReferences.controllerManager;
        uiManagement = managerReferences.uiManagement;
        saveButton.interactable = false;
    }

    private void Start() {
        PrepareButtons();
    }

    private void OnEnable() {
        saveName.Select();
    }
    public void PrepareButtons() {
        if (saveName != null) {
            saveName.onSelect.AddListener(delegate {
                saveFileView.EnableOrDisableLoadButton(false);
                SetNewFileItemSelected();
                saveName.SetTextWithoutNotify("");
                saveButton.interactable = false;
            });
            saveName.onValueChanged.AddListener(delegate {
                string text = saveName.text;
                if (!string.IsNullOrEmpty(text)) {
                    saveButton.interactable = true;
                } else {
                    saveButton.interactable = false;
                }
            });
        }
        if (saveButton != null) saveButton.onClick.AddListener(SaveSelected);
        if (loadButton != null) {
            loadButton.onClick.AddListener(LoadSelected);
            Debug.Log("LoadButtonReset");
        }
    }

    private void SetNewFileItemSelected() {
        Debug.Log("IGSL - Setting selected save to null.");
        saveFileView.SelectedSaveGame = null;
    }
    public void SaveSelected() {
        if (saveFileView.SelectedSaveGame == null) {
            Debug.Log("IGSL - Attempting to save game with a file name of " + saveName.text);
            if (saveName.text.Length > 0) controllerManager.saveGameController.SaveState(saveName.text, false);
            else uiManagement.warningLogView.AppendMessageToLog("SaveNameRequired", Vector3.zero, 1);
        } else {
            Debug.Log("IGSL - Attempting to save game with a file name of " + saveFileView.SelectedSaveGame.fileName);
            controllerManager.saveGameController.SaveState(saveFileView.SelectedSaveGame.fileName, true);
        }

        saveFileView.DisplaySavedFiles(SaveFunctions.ReturnSaveFiles(PlayerPrefs.GetString("saveLocation"), "date"));
    }

    public void LoadSelected() {
        Debug.Log("IGSL - Passed Select");
        if (saveFileView.saveSelected) {
            controllerManager.saveGameController.BeginLoadFromSave(saveFileView.SelectedSaveGame);
        }
    }
}