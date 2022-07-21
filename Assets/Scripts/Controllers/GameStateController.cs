using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class GameStateController : MonoBehaviour {

    private NewGameData preparedGameData;
    public GameObject pawnDisplayParent, newGameInfoObject;
    private List<Pawn> pawnList;
    private List<Pawn> defaultPawnList;
    public Button gameStart;
    public GameObject newGameCompiler, mapInputHandler;
    int counter;

    private SceneDataPasser sceneData;
    public StartGameController startGameController;
    public void StartSimulation() {
        // Access the default pawn list as a fallback.
        defaultPawnList = newGameCompiler.GetComponent<NewGameCompiler>().PawnReturn();
        counter = 0;
        pawnList = ExtractPawnData();
        if (pawnList != null) {
            preparedGameData = ExtractNewGameData(pawnList);
            if (preparedGameData != null) startGameController.StartSimulationFromNew(preparedGameData);
        }
    }

    private List<Pawn> ExtractPawnData() {
        List<Pawn> tempList = new List<Pawn>();
        foreach (Transform x in pawnDisplayParent.transform) {
            // Activate all panels for data extraction.
            x.gameObject.SetActive(true);
            // Extract the pawn object from each display panel.
            Pawn extractedPawn = x.gameObject.GetComponent<PawnDisplayHandler>().ReturnPawnData();

            // Determine if the pawn has been changed/initialised.
            if (extractedPawn != null) {
                tempList.Add(extractedPawn);
                counter++;
            } else {
                return null;
            }
        }
        return tempList;
    }

    private NewGameData ExtractNewGameData(List<Pawn> _pawnList) {
        // Create references to all relevant data fields from the start menu.
        TMP_InputField villageName = newGameInfoObject.transform.GetChild(0).gameObject.GetComponentInChildren<TMP_InputField>();
        TMP_InputField mapSeed = newGameInfoObject.transform.GetChild(1).gameObject.GetComponentInChildren<TMP_InputField>();
        TMP_Dropdown difficulty = newGameInfoObject.transform.GetChild(2).gameObject.GetComponentInChildren<TMP_Dropdown>();

        // Scrape all data from these fields.
        string _villagename = villageName.text;
        int _mapSeed;
        if (int.TryParse(mapSeed.text, out _mapSeed)) {
            if (villageName.text != "") {

                Difficulty _difficulty = (Difficulty) difficulty.value;
                Debug.Log("GES - difficulty int: " + difficulty.value + " translated to difficulty of " + _difficulty.ToString());
                TribeInfo tribeInfo = mapInputHandler.GetComponent<MapInputHandler>().selectedTribe;
                Debug.Log("Attempted Seed: " + _mapSeed);
                // Convert this data into a class storing new game information, to be passed to the next scene.
                return new NewGameData(_villagename, _mapSeed, _difficulty, _pawnList, tribeInfo);
            } else return null;
        } else return null;
    }
}