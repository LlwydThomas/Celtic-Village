using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class LoadingBarController : MonoBehaviour {
    // Start is called before the first frame update
    List<LoadingBarInfo> loadingBarInfos = new List<LoadingBarInfo>();
    Dictionary<GameObject, LoadingBarInfo> loadingBarLink = new Dictionary<GameObject, LoadingBarInfo>();
    public GameObject loadingBarPrefab;
    public List<int> ids = new List<int>();
    public Dictionary<int, LoadingBarInfo> loadingBarLookup = new Dictionary<int, LoadingBarInfo>();
    public ManagerReferences manager;
    private ControllerManager controllerManager;
    // Update is called once per frame

    private void Awake() {
        controllerManager = manager.controllerManager;
        EventController.StartListening("gameLoaded", ResetBars);
    }
    void Update() {
        if (loadingBarInfos.Count > 0) {
            float gameSpeed = controllerManager.dateController.GameSpeedReturn();
            Debug.Log("LBC - Game Speed: " + gameSpeed);
            foreach (LoadingBarInfo x in loadingBarInfos.ToArray()) {
                if (x.worldSpaceRelation != null) {
                    x.loadingObject.transform.position = x.worldSpaceRelation.transform.position;
                }
                if (x.tagForDeletion) {
                    Debug.Log("LBC - Attempting to remove loading bar due to cancel");
                    AddOrRemoveBar(x);
                    continue;
                }
                if (x.paused) continue;
                x.loadingBar.value += (Time.deltaTime / x.speedFactor * gameSpeed * 3f);
                if (x.loadingBar.value >= 1) {
                    x.onCompleteActions.Invoke();
                    AddOrRemoveBar(x);
                }
            }
        }
    }

    public List<LoadingBarInfo> ReturnLoadingBars(int id = -1) {
        if (id == -1) return loadingBarInfos;
        else {
            List<LoadingBarInfo> loadingBars = new List<LoadingBarInfo>();
            if (ids.Contains(id)) loadingBars.Add(loadingBarLookup[id]);
            return loadingBars;
        }
    }
    public void ResetBars() {
        int prior = loadingBarInfos.Count;
        Debug.Log("Resetting " + loadingBarInfos.Count + " loading bars...");
        foreach (LoadingBarInfo loadingBar in loadingBarInfos) {
            Debug.Log("Deleting " + loadingBar.loadingObject.name);
            DestroyImmediate(loadingBar.loadingObject);
        }
        loadingBarInfos.Clear();
        loadingBarLink.Clear();
        ids.Clear();
        loadingBarLookup.Clear();
    }

    public LoadingBarInfo PauseOrHideLoadingBar(int id, bool pause) {
        if (!loadingBarLookup.ContainsKey(id)) return null;
        LoadingBarInfo loadingBarInfo = loadingBarLookup[id];
        if (pause) {
            loadingBarInfo.paused = true;
            loadingBarInfo.hidden = true;
            loadingBarInfo.loadingBar.gameObject.SetActive(false);
        } else {
            loadingBarInfo.paused = false;
            loadingBarInfo.hidden = false;
            loadingBarInfo.loadingBar.gameObject.SetActive(true);
        }

        return loadingBarInfo;
    }

    public void AddOrRemoveBar(LoadingBarInfo loadingBarInfo) {
        // If the loading bar is registered, remove its references, and if not, register it. 
        if (loadingBarInfos.Contains(loadingBarInfo)) {
            loadingBarInfos.Remove(loadingBarInfo);
            ids.Remove(loadingBarInfo.ID);
            if (loadingBarInfo.worldSpaceRelation != null)
                loadingBarLink.Remove(loadingBarInfo.worldSpaceRelation);
            loadingBarLookup.Remove(loadingBarInfo.ID);
            DestroyImmediate(loadingBarInfo.loadingObject);
        } else {
            loadingBarInfos.Add(loadingBarInfo);
            ids.Add(loadingBarInfo.ID);
            if (loadingBarInfo.worldSpaceRelation != null)
                loadingBarLink.Add(loadingBarInfo.worldSpaceRelation, loadingBarInfo);
            loadingBarLookup.Add(loadingBarInfo.ID, loadingBarInfo);
        }
    }
    public LoadingBarInfo GenerateLoadingBar(Vector3 position, UnityAction onCompleteActions, float speedFactor, GameObject gameObject = null, float widthFactor = 5f) {
        if (gameObject != null) {
            if (loadingBarLink.ContainsKey(gameObject)) return null;
        }
        GameObject slider = GameObject.Instantiate(loadingBarPrefab, position, Quaternion.identity, this.gameObject.transform);
        onCompleteActions += (() => EventController.TriggerEvent(loadingBarInfos.Count + "BarLoadComplete"));
        LoadingBarInfo loadingBarInfo = new LoadingBarInfo(slider.GetComponentInChildren<Slider>(), onCompleteActions, speedFactor, FindAvailableID(), gameObject, slider);
        AddOrRemoveBar(loadingBarInfo);
        loadingBarInfo.loadingBar.SetValueWithoutNotify(0);
        return loadingBarInfo;
    }

    private int FindAvailableID() {
        int id = Random.Range(1, 10000);
        while (ids.Contains(id)) {
            id = Random.Range(1, 10000);
        }

        return id;
    }

}

[System.Serializable]
public class LoadingBarInfo {
    public Slider loadingBar;
    public UnityAction onCompleteActions;
    public GameObject loadingObject, worldSpaceRelation;
    public float speedFactor;
    public int ID;
    public bool paused, hidden;
    public bool tagForDeletion = false;

    public LoadingBarInfo(Slider _loadingBar, UnityAction _onCompleteActions, float _speedFactor, int _ID, GameObject _worldSpaceRelation, GameObject _loadingObject) {
        speedFactor = _speedFactor * 100;
        loadingBar = _loadingBar;
        worldSpaceRelation = _worldSpaceRelation;
        onCompleteActions = _onCompleteActions;
        ID = _ID;
        loadingObject = _loadingObject;
    }
}