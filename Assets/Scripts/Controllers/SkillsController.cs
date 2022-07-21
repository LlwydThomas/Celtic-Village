using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
public class SkillsController : MonoBehaviour {

    public GameObject pawnParent, sceneDataPass, pawnTemplate, pawnListCanvas;
    public SkillDataList skillDataListObject;
    public List<SkillData> skillDataList;
    public int defaultPawnCount;
    public int healthChangeApplied;
    private List<Pawn> initialPawnList = new List<Pawn>();

    [SerializeField]
    ListContainerNames container;
    private SkillModel skillModel;
    public int[] hungerAmendendments, tirednessAmendments;

    public ManagerReferences managerReferences;
    private ControllerManager controllerManager;
    private ModelManager modelManager;
    public bool debug;
    private UnityAction mapAction, statusAction;
    [SerializeField]
    private List<Pawn> pawnList;

    // Start is called before the first frame update
    void Start() {
        // Instantiate and populate the skills model object.
        controllerManager = managerReferences.controllerManager;
        modelManager = managerReferences.modelManager;
        skillDataList = skillDataListObject.skillDatas;
        skillModel = modelManager.skillModel;
        skillModel.skillDataList = skillDataList;
        pawnList = skillModel.pawnList;
        foreach (SkillData skill in skillModel.skillDataList) {
            skillModel.skillDataLookup.Add(skill.ID, skill);
        }

        // Define objects and variables to prepare for the loading of pawns.
        //Debug.Log(container.dataList.Length);
        // Define actions and subscribe to the event controllers triggers.
        //mapAction = () => InitialisePawn(initialPawnList, true);
        EventController.StartListening("day", ForcePawnsToDumpInventory);
        statusAction = () => ProcessStatusChange();
        EventController.StartListening("year", AgeVillagers);
    }

    public void PreparePawnsFromSave(List<Pawn> pawnList) {
        Debug.Log("Loaded Pawn Count " + pawnList.Count);
        foreach (Pawn pawn in pawnList) {
            Debug.Log("Capacity on Load: " + pawn.storageContainer.weightCapacity);
        }

        InitialisePawn(pawnList, true);
    }

    private void ForcePawnsToDumpInventory() {
        foreach (Pawn pawn in skillModel.pawnList) {
            pawn.pawnTaskSystem.AppendStorageDumpTransferTask(controllerManager);
        }
    }

    private void AgeVillagers() {
        foreach (Pawn pawn in skillModel.pawnList.ToArray()) {
            pawn.age += 1;
            float deathProbability = PawnFunctions.FindVillagerDeathChance(pawn);
            deathProbability = 100;
            float roll = Random.Range(0, 100);
            if (roll < deathProbability) {
                KillPawn(pawn, "OldAge");
            }
            Debug.Log("SC - Pawn " + pawn.name + " aged to " + pawn.age + " with a " + deathProbability + " chance of death.");
        }
    }

    public void ProcessStatusChange() {
        DifficultySettings currentSettings = GeneralEnumStorage.currentDifficultySettings;
        if (currentSettings == null) {
            GeneralEnumStorage.currentDifficultySettings = controllerManager.settingsController.FindDifficultySettings(GeneralEnumStorage.currentDifficulty);
            currentSettings = GeneralEnumStorage.currentDifficultySettings;
        }
        List<Pawn> pawnList = PawnFunctions.IncreasePawnStatusLevels(skillModel.pawnList, hungerAmendendments, tirednessAmendments, true);
        foreach (Pawn pawn in pawnList.ToArray()) {
            PawnStatus status = pawn.pawnStatus;
            bool overallCheck = false;
            string possibleDeathCause = "";
            if (status.hungerLevel <= 20) {
                bool complete;
                overallCheck = false;
                // If hunger is below a certain level, instruct the pawn to find food immediately.
                if (pawn.pawnTaskSystem.AmendTaskGroupCount(3, 0) <= 0) {
                    complete = pawn.pawnController.pawnTaskSystem.QueuePersonalTask(managerReferences, 3);
                } else complete = true;
                if (!complete && status.hungerLevel <= 5) {
                    PawnFunctions.ApplyHealthChange(status, -healthChangeApplied);
                    possibleDeathCause = "Starvation";
                }
            } else {
                overallCheck = true;
                if (status.hungerLevel >= 100)
                    EventController.TriggerEvent("pawn" + pawn.id + "FullHunger");
            }
            if (status.tirednessLevel <= 10) {
                bool complete;
                if (pawn.pawnTaskSystem.AmendTaskGroupCount(4, 0) <= 0) {
                    Debug.Log("Queueing an eating task with count of " + pawn.pawnTaskSystem.AmendTaskGroupCount(4, 0));
                    complete = pawn.pawnController.pawnTaskSystem.QueuePersonalTask(managerReferences, 4);
                } else complete = true;
                overallCheck = false;
                if (!complete && status.hungerLevel <= 5) {
                    PawnFunctions.ApplyHealthChange(status, -healthChangeApplied);
                    possibleDeathCause = "LackOfSleep";
                }
            } else {
                overallCheck = overallCheck && true;
                if (status.tirednessLevel >= 100)
                    EventController.TriggerEvent("pawn" + pawn.id + "FullRest");
            }
            if (overallCheck) PawnFunctions.ApplyHealthChange(status, currentSettings.healthChangeApplied);
            else {
                if (status.totalHealth <= 0) KillPawn(pawn, possibleDeathCause);
            }
        }
    }

    public void ManuallyAmendStatusLevels(Pawn pawn, int hungerIncrease = 0, int tirednessIncrease = 0) {
        PawnStatus status = pawn.pawnStatus;
        if (hungerIncrease == 0 && tirednessIncrease == 0) return;
        if (hungerIncrease > 0) {
            status.hungerLevel = PawnFunctions.BindStatusValues(status.hungerLevel, hungerIncrease);
        }
        if (tirednessIncrease > 0) {
            status.tirednessLevel = PawnFunctions.BindStatusValues(status.tirednessLevel, tirednessIncrease);
        }
        EventController.TriggerEvent("pawnStatusChange");
    }

    public void RemovePawn(Pawn pawn) {
        skillModel.pawnObjectConnect.Remove(pawn.pawnGameObject);
        skillModel.pawnList.Remove(pawn);
        SetFunctionHandler(pawn, 1);
        Destroy(pawn.pawnGameObject);

        // Reformat the top pawn list.
        if (skillModel.pawnList.Count == 0) {
            string[] textParams = new string[] { controllerManager.settingsController.TranslateString("NoRemainingPawns") };
            controllerManager.saveGameController.EndGame(textParams);
        } else managerReferences.viewManager.pawnListDisplay.InitialiseDisplayPawnList(skillModel.pawnList);
    }

    public List<Pawn> FindPawnFromFunctionHandler(int funcID) {
        List<Pawn> subscribedPawns = skillModel.pawnList.FindAll(x => x.functionHandlerID == funcID);
        return subscribedPawns;
    }

    private void KillPawn(Pawn pawn, string cause) {
        cause = controllerManager.settingsController.TranslateString(cause);
        cause = cause.ToLower();
        Debug.Log("SC - Pawn " + pawn.name + " has died due to " + cause + ".");
        managerReferences.uiManagement.warningLogView.AppendMessageToLog("PawnHasDied", pawn.pawnGameObject.transform.position, duration : 400f, insertionStrings : new string[] { pawn.name, cause });
        RemovePawn(pawn);
    }

    public void InitialisePawn(List<Pawn> pawnList, bool clear) {
        if (clear) ResetPawnList();
        if (pawnList.Count == 0) {
            pawnList = PawnFunctions.DefaultPawnListReturn(skillModel.skillDataList, defaultPawnCount);
        }

        Vector3 spawnStart = new Vector3(0, 0);
        List<Node> nodeList = controllerManager.gridController.FindWalkableSquare(spawnStart, false, pawnList.Count, 3, 3);
        Debug.Log("Node list has found " + nodeList.Count + " possible spawn Locations.");
        Debug.Log("How many pawnnss? " + pawnList.Count);

        for (int i = 0; i < pawnList.Count; i++) {
            Pawn pawn = pawnList[i];
            if (pawn.name == "") {
                string pawnName = PawnFunctions.RandomPawnName();
                if (pawnName != null) pawn.name = pawnName;
            }
            if (pawn.id == -1) pawn.id = FindAvailableID();
            Vector3 spawnPos;
            if (pawn.saveLocation == Vector3.zero && nodeList.Count >= i + 1) spawnPos = nodeList[i].worldPosition;
            else {
                Debug.Log("Adding the pawn at save position, from new");
                spawnPos = pawnList[i].saveLocation;
            }
            GameObject instantiatedPawn = Instantiate(pawnTemplate, spawnPos, Quaternion.identity, pawnParent.transform);
            controllerManager.storageController.RegisterStorage(pawn.storageContainer, pawn.storageContainer.inventory);
            if (pawn.sleepingNodeID != -1) pawn.sleepingNode = controllerManager.gridController.LookupNodeFromID(pawn.sleepingNodeID);
            /* foreach (SkillLevelled skill in pawnList[i].skillsList) {
                AmendSkillLevels(pawnList[i], skill.skillData, 0);
            } */
            AppendPawn(pawn, instantiatedPawn, pawn.functionHandlerID);
        }
        //EventController.StopListening("mapCompleted", mapAction);
        pawnListCanvas.GetComponent<PawnListDisplay>().InitialiseDisplayPawnList(skillModel.pawnList);
        EventController.StopListening("hour", statusAction);
        EventController.StartListening("hour", statusAction);
        //EventController.StartListening("mapCompleted", mapAction);
    }

    public void InitialisePawn(Pawn pawn, Vector3 location) {
        //pawn.saveLocation = location;
        InitialisePawn(new List<Pawn>() { pawn }, false);
    }
    public Pawn RequestPawnGameObject(GameObject gameObject) {
        return skillModel.pawnObjectConnect[gameObject];
    }

    public void SetFunctionHandler(Pawn pawn, int functionHandlerID) {
        FunctionHandler function = controllerManager.buildingController.FindFuncHandlerByID(functionHandlerID);
        Debug.Log("Setting pawn" + pawn.id + " to Function Handler " + functionHandlerID + "; func =" + function);
        if (function != null) {
            pawn.functionHandlerID = functionHandlerID;
            pawn.pawnController.ReassignFunctionHandler(function);
        } else {
            FunctionHandler generalHandler = controllerManager.buildingController.FindFuncHandlerByID(1);
            pawn.functionHandlerID = generalHandler.id;
            pawn.pawnController.ReassignFunctionHandler(generalHandler);
        }
    }
    public int GetItemNum() {
        return skillModel.pawnList.Count + 1;
    }

    public List<SkillData> GetSkillList() {
        return skillModel.skillDataList;
    }
    public List<Pawn> PawnListReturn(bool savePosition) {
        if (savePosition) {
            foreach (Pawn pawn in skillModel.pawnList) pawn.saveLocation = pawn.pawnGameObject.transform.position;
        }
        return skillModel.pawnList;
    }

    public void ResetPawnList() {
        foreach (Pawn pawn in skillModel.pawnList) DestroyImmediate(pawn.pawnGameObject);
        skillModel.pawnList.Clear();
        skillModel.pawnObjectConnect.Clear();
    }
    public void AppendPawn(Pawn pawn, GameObject gameObject, int funcID) {
        gameObject.name = "pawn" + pawn.id;
        pawn.pawnGameObject = gameObject;
        pawn.pawnController = pawn.pawnGameObject.GetComponent<PawnController>();
        if (pawn.pawnColours == null) pawn.pawnColours = PawnFunctions.RandomiseColours(4);
        for (int j = 0; j < pawn.pawnColours.Length; j++) {
            pawn.pawnController.clothingSprites[j].color = pawn.pawnColours[j];
        }
        pawn.pawnController.SetPawnInModel(pawn);
        skillModel.pawnList.Add(pawn);
        skillModel.pawnObjectConnect.Add(gameObject, pawn);
        SetFunctionHandler(pawn, funcID);
    }

    public Pawn CreateNewPawn(Vector3 location) {
        Debug.Log("New pawn has been added");
        Pawn newPawn = PawnFunctions.CreateNewPawn(PawnFunctions.RandomPawnName());
        InitialisePawn(newPawn, location);
        return newPawn;
    }

    public int FindAvailableID() {
        int tempID = Random.Range(0, 9999);
        while (skillModel.pawnIDs.Contains(tempID)) {
            tempID = Random.Range(0, 9999);
        }
        return tempID;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Insert)) {
            if (GeneralEnumStorage.debugActive) AgeVillagers();
        }
    }

    // Structure for save data

    [System.Serializable]
    public class ListContainerNames {
        public string[] dataList;
    }
}

// Class definitions.