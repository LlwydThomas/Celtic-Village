using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneDataPasser : MonoBehaviour {
    // Start is called before the first frame update

    public NewGameData currentNewGameData;
    public SaveGameItem saveGameItem;

    private static SceneDataPasser instance;
    public static SceneDataPasser Instance { get { return instance; } }

    private void Awake() {
        if (instance != null && instance != this) {
            DestroyImmediate(this.gameObject);
        } else {
            Debug.Log("SDP - Set to not destroy on load.");
            Object.DontDestroyOnLoad(this);
            instance = this;
        }
    }
    void Start() {

    }

    public void OverrideSaveGameItem(SaveGameItem _saveGameItem) {
        saveGameItem = _saveGameItem;
    }
    public void overrideStoredInfo(NewGameData _newGameData) {
        Debug.Log("SDP - Difficulty in pass set to: " + _newGameData.difficulty.ToString());
        currentNewGameData = _newGameData;
    }

    public NewGameData newGameDataReturn() {
        return currentNewGameData;
    }
}

[System.Serializable]
public class NewGameData {
    public string villageName;
    public int mapSeed;
    public Difficulty difficulty;
    public List<Pawn> pawnList;
    public TribeInfo selectedTribe;
    public List<Pawn> defaultPawnList;

    public NewGameData(string _vilName, int _mapSeed, Difficulty _difficulty, List<Pawn> _pawnList, TribeInfo _tribeInfo) {
        villageName = _vilName;
        mapSeed = _mapSeed;
        difficulty = _difficulty;
        pawnList = _pawnList;
        selectedTribe = _tribeInfo;
    }

}

[System.Serializable]
public enum Difficulty {
    Easy,
    Medium,
    Hard
}