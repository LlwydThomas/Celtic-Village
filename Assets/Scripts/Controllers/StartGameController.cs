using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class StartGameController : MonoBehaviour {
    // Start is called before the first frame update
    public SceneDataPasser sceneDataPasser;
    private NewGameData loadGameData;
    public UiManagement ui;
    public Canvas loadingScreen;
    public void StartSimulationFromLoad(SaveGameItem save) {
        // Access the default pawn list as a fallback.
        FindSceneDataPasser();
        sceneDataPasser.OverrideSaveGameItem(save);
        AsyncLoadGame();
    }

    private void AsyncLoadGame() {
        ui.ManageCanvases(0);
    }

    public void StartSimulationFromNew(NewGameData newGameData) {
        FindSceneDataPasser();
        sceneDataPasser.overrideStoredInfo(newGameData);
        AsyncLoadGame();
    }

    public void FindSceneDataPasser() {
        if (sceneDataPasser != null) return;
        sceneDataPasser = GameObject.Find("SceneDataPass").GetComponent<SceneDataPasser>();
    }
}