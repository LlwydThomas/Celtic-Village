using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class SaveFileView : MonoBehaviour {
    // Start is called before the first frame update
    public ManagerReferences managerReferences;
    private SaveGameItem selectedSaveGame;
    public SaveGameItem SelectedSaveGame {
        get {
            return selectedSaveGame;
        }
        set {
            selectedSaveGame = value;
            SaveGameDisplay display = null;
            if (value != null) display = saveGameDisplays.Find(x => x.saveGameItem == value);
            FormatButtonColour(display);
        }
    }

    public TextMeshProUGUI[] translatables;

    public bool saveSelected;

    public bool saveButtonPresent = true;
    private SceneDataPasser sceneDataPasser;

    List<SaveGameItem> currentSaveGameList = new List<SaveGameItem>();

    public Button loadButton, saveButton, deleteButton;
    // public TMP_InputField saveName;
    public GameObject saveItemPrefab, saveListParent;
    private List<SaveGameDisplay> saveGameDisplays = new List<SaveGameDisplay>();
    public RectTransform saveListRect;
    private void OnEnable() {
        if (!saveButtonPresent) {
            if (sceneDataPasser == null) sceneDataPasser = GameObject.Find("SceneDataPass").GetComponent<SceneDataPasser>();
            saveButton.gameObject.SetActive(false);
        } else saveButton.gameObject.SetActive(true);
        DisplaySavedFiles(SaveFunctions.ReturnSaveFiles(PlayerPrefs.GetString("saveLocation"), "date"));
        SelectedSaveGame = null;
        loadButton.interactable = false;
    }

    private void Start() {
        SettingsFunctions.TranslateTMPItems(managerReferences.controllerManager.settingsController, translatables);
        PrepareButtons();
    }

    private void FixedUpdate() {
        if (saveSelected) {
            if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetMouseButtonDown(1)) {
                saveSelected = false;
                SelectedSaveGame = null;
                saveButton.interactable = false;
                loadButton.interactable = false;
            }
        }
    }
    public void DisplaySavedFiles(List<SaveGameItem> saveGames) {
        currentSaveGameList = saveGames;
        saveSelected = false;
        saveGameDisplays.Clear();
        foreach (Transform child in saveListParent.transform) {
            if (child.name == "SaveFileInput") continue;
            Destroy(child.gameObject);
        }
        foreach (SaveGameItem save in saveGames) {
            GameObject saveItem = GameObject.Instantiate(saveItemPrefab, saveListParent.transform.position, Quaternion.identity, saveListParent.transform);
            SaveGameDisplay saveGameDisplay = new SaveGameDisplay(saveItem, save);
            saveGameDisplays.Add(saveGameDisplay);
            UnityAction action = delegate {
                SetSaveSelection(save);
            };
            SaveFunctions.FormatSaveGaveItem(saveItem, save, action);
        }
        float size = (saveGames.Count * 100f) + (5f * saveGames.Count);
        if (saveButtonPresent) size += 100f;
        GeneralFunctions.SetContentHeight(saveListRect, size, null);
    }

    public void PrepareButtons() {
        if (!saveButtonPresent) loadButton.onClick.AddListener(LoadToDataPasser);
        deleteButton.onClick.AddListener(() => DeleteSaveGame(SelectedSaveGame));
    }

    public void FormatButtonColour(SaveGameDisplay activeSave = null) {
        foreach (SaveGameDisplay saveGame in saveGameDisplays) {
            saveGame.background.color = GeneralFunctions.blackBackground;
        }
        if (activeSave != null) activeSave.background.color = GeneralFunctions.greenSwatch;
    }

    public void LoadToDataPasser() {
        if (sceneDataPasser != null) {
            sceneDataPasser.OverrideSaveGameItem(SelectedSaveGame);
            GameObject.Find("StartGame").GetComponent<StartGameController>().StartSimulationFromLoad(SelectedSaveGame);
        }
    }
    public void DeleteSaveGame(SaveGameItem saveGame) {
        System.IO.File.Delete(saveGame.fileLocation);
        DisplaySavedFiles(SaveFunctions.ReturnSaveFiles(PlayerPrefs.GetString("saveLocation"), "date"));
    }

    private void SetSaveSelection(SaveGameItem saveGame) {
        Debug.Log("SFV - Setting save game selection to " + saveGame.fileName);
        SelectedSaveGame = saveGame;
        saveSelected = true;
        saveButton.interactable = true;
        EnableOrDisableLoadButton(true);
    }

    public void EnableOrDisableLoadButton(bool enable) {
        loadButton.interactable = enable;
    }

}

public class SaveGameDisplay {
    public GameObject saveObject;
    public SaveGameItem saveGameItem;
    public Image background;
    public SaveGameDisplay(GameObject _saveObject, SaveGameItem _saveGameItem) {
        saveGameItem = _saveGameItem;
        saveObject = _saveObject;
        background = saveObject.GetComponent<Image>();
    }
}