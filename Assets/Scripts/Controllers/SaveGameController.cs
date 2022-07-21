using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SaveGameController : MonoBehaviour {
    // Start is called before the first frame update
    public GameObject saveBox;
    [SerializeField]
    public GameObject buildingParent;
    public ManagerReferences managerReferences;
    private ControllerManager controllerManager;

    public bool mapGenerationTesting;
    private float autoSaveTimer;

    private BuildingController buildingController;
    private ModelManager models;

    private string saveLocation;

    private GameObject sceneDataPasser;
    private NewGameData newGameData;

    void Start() {
        if (!PlayerPrefs.HasKey("saveLocation")) PlayerPrefs.SetString("saveLocation", Application.streamingAssetsPath + "/Saves/");
        saveLocation = PlayerPrefs.GetString("saveLocation");
        models = managerReferences.modelManager;
        controllerManager = managerReferences.controllerManager;
        CheckSaveFolderExists(saveLocation);
        CloseDialog();
        buildingController = controllerManager.buildingController;
        sceneDataPasser = GameObject.Find("SceneDataPass");
        if (sceneDataPasser != null) {
            Debug.Log("SGC - SDP is not null");
            HandleDataPasser(sceneDataPasser);
        } else {
            Debug.Log("SGC - SDP is null");
            if (mapGenerationTesting && GeneralEnumStorage.debugActive) {
                List<Pawn> defaultList = new List<Pawn>();
                NewGameData randomGame = new NewGameData("Llyncu", Random.Range(1, 10000), Difficulty.Medium, defaultList, null);
                BeginLoadFromNewGame(randomGame);
            } else {
                var loadList = SaveFunctions.ReturnSaveFiles(saveLocation, "date");
                //Debug.Log("First Save Name: " + loadList[0].fileName);
                if (loadList.Count == 0) QuitToTitleScreen();
                else {
                    BeginLoadFromSave(loadList[0]);
                }
            }
        }
    }

    // Update is called once per frame
    // void Update() {
    //     if (Input.GetButtonDown("Cancel")) {
    //         if (saveBox.activeSelf) {
    //             CloseDialog();
    //         } else OpenDialog();
    //     }
    // }

    private void Update() {
        autoSaveTimer += Time.deltaTime;
        if (autoSaveTimer >= 600) {
            if (PlayerPrefs.GetInt("AutosaveEnabled", 1) == 1) {
                SaveState("autosave", true);
            }
            autoSaveTimer = 0;
        }
    }

    public void HandleDataPasser(GameObject scenePasser) {
        SceneDataPasser sceneData = scenePasser.GetComponent<SceneDataPasser>();
        if (sceneData.saveGameItem != null) {
            BeginLoadFromSave(sceneData.saveGameItem);
        } else {
            BeginLoadFromNewGame(sceneData.currentNewGameData);
        }
    }

    public void OpenDialog() {
        saveBox.SetActive(true);
    }
    public void CloseDialog() {
        saveBox.SetActive(false);
    }

    public void QuitToTitleScreen() {
        SceneManager.LoadScene("Cyntaf");
    }

    public void SaveState(string fileName, bool overridden = false) {
        if (CheckFileExists(fileName) && overridden == false) {
            Debug.Log("Do you want to overwrite this file?");
        } else {

            // Compile lists of all present objects within the game world.
            List<Build> buildingList = controllerManager.buildingController.ReturnBuildList();
            List<FloraItem> floraList = controllerManager.natureController.FloraListReturn(true, FloraData.category.Crop);
            List<Pawn> pawnList = controllerManager.skillsController.PawnListReturn(true);
            List<NPCSaveContainer> npcList = controllerManager.nPCController.ReturnNPCList(true);
            MapSaveData mapSaveData = controllerManager.mapController.ReturnMapSaveData();
            float rawTimer = controllerManager.dateController.ReturnCurrentDateTime().rawTime;
            List<Farm> farmList = controllerManager.farmingController.FarmListReturn();
            Debug.Log(farmList.Count);
            List<InstantiatedEvent> upcomingEvents = models.scheduledEventModel.eventQueue;
            System.DateTime dateTime = System.DateTime.Now;
            long dateLong = dateTime.ToFileTime();
            float lastEventRaw = controllerManager.eventQueueController.LastEventOccurence();

            // Store this lists within a serialisable class.
            SaveContainer container = new SaveContainer(buildingList, rawTimer, lastEventRaw, floraList, pawnList, mapSaveData, dateLong, fileName, farmList, newGameData, upcomingEvents, npcList);
            // Convert the container to JSON and save it to the disk.
            string json = JsonUtility.ToJson(container);
            Debug.Log(json.Length);
            StreamWriter writer = new StreamWriter(saveLocation + fileName + ".json", false);
            writer.WriteLine(json);
            writer.Close();
        }
    }

    public void BeginLoadFromSave(SaveGameItem saveGameItem) {
        Debug.Log("SGC - Beginning Load from save: " + saveGameItem.fileName);
        //Read the text from directly from the test.txt file
        //Debug.Log(container.floraList.Count);
        SaveContainer container = saveGameItem.saveContainer;
        newGameData = container.newGameData;
        Difficulty difficulty = (Difficulty) newGameData.difficulty;
        GeneralEnumStorage.CurrentDifficulty = difficulty;
        GeneralEnumStorage.currentDifficultySettings = controllerManager.settingsController.FindDifficultySettings(difficulty);

        //Debug.Log("Loading Started");
        GenerateMapFromLoad(container.mapData, container.floraList);
        controllerManager.dateController.LoadTime(container.rawTimer);
        buildingController.ResetBuildList(container.buildList);
        controllerManager.farmingController.ResetFarmList(container.farmList);
        controllerManager.nPCController.ResetNPCList(container.nPCs);
        PawnLoader(container.pawnList);
        controllerManager.eventQueueController.InstantiateSavedEvents(container.upcomingEvents, TimeFunctions.ConvertDateTimeObject(container.lastEventRaw, models.timeModel));
        //Debug.Log(container.buildList.Count);

        EventController.TriggerEvent("gameLoaded");
    }

    public void BeginLoadFromNewGame(NewGameData newGame) {
        buildingController.ResetBuildList();
        GeneralEnumStorage.CurrentDifficulty = newGame.difficulty;
        GeneralEnumStorage.currentDifficultySettings = controllerManager.settingsController.FindDifficultySettings(newGame.difficulty);
        newGameData = newGame;
        GenerateMapFromNewGame(newGame);
        PawnLoader(newGame.pawnList);
        controllerManager.dateController.LoadTime(0);
        managerReferences.uiManagement.ManageOpenDialogues(true, 3);
        managerReferences.viewManager.welcomeDialogueView.SetMessageContents(true);
        Debug.Log("SGC - Difficulty set to: " + newGame.difficulty.ToString());
        buildingController.NewGameBuildingCreation(newGame.difficulty);
        controllerManager.nPCController.DetermineAnimalSpawns(3, Vector3.zero, null);
        EventController.TriggerEvent("gameLoaded");
    }

    public List<SaveGameItem> ReturnSaveItems(string filePath, string sortBy) {
        return SaveFunctions.ReturnSaveFiles(saveLocation, sortBy);
    }

    public void GenerateMapFromLoad(MapSaveData mapSaveData, List<FloraItem> savedFlora) {
        controllerManager.mapController.BeginMapLoadFromSave(mapSaveData, savedFlora);
    }

    public void GenerateMapFromNewGame(NewGameData newGame) {
        controllerManager.mapController.BeginMapLoad(newGame);
    }

    public void PawnLoader(List<Pawn> pawnList) {
        controllerManager.skillsController.PreparePawnsFromSave(pawnList);
    }

    public void CheckSaveFolderExists(string _saveLocation) {
        if (!System.IO.Directory.Exists(_saveLocation)) {
            System.IO.Directory.CreateDirectory(_saveLocation);
        }
    }
    public bool CheckFileExists(string fileLocation) {
        Debug.Log(saveLocation + fileLocation);
        if (System.IO.File.Exists(saveLocation + fileLocation)) {
            Debug.Log("File Exists");
            return true;
        } else return false;
    }

    public void EndGame(string[] reasonParamaters) {
        managerReferences.uiManagement.ManageOpenDialogues(true, 3);
        managerReferences.uiManagement.TriggerGameControlStoppage(1, 1);
        managerReferences.viewManager.welcomeDialogueView.SetMessageContents(false, reasonParamaters);
    }

}