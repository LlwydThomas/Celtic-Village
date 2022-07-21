using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class EventQueueController : MonoBehaviour {
    // Start is called before the first frame update
    public ManagerReferences managerReferences;
    private ControllerManager controllerManager;
    public ScheduledEventList scheduledEventList;
    public int minDaysSinceLastEvent;
    public float eventBaseChance, dateOffsetDivisor;
    public int[] difficultyMaxBacklog = new int[3];
    private ScheduledEventModel scheduledEventModel;

    [SerializeField]
    public List<InstantiatedEvent> eventQueue = new List<InstantiatedEvent>();
    [SerializeField]
    private List<EventCondition> currentEventConditions = new List<EventCondition>();

    private void Awake() {
        controllerManager = managerReferences.controllerManager;
        scheduledEventModel = managerReferences.modelManager.scheduledEventModel;
        eventQueue = scheduledEventModel.eventQueue;
        ReadEventScriptables();
    }
    private void Start() {
        EventController.StartListening("day", DetermineFutureEvents);
        EventController.StartListening("day", delegate { EventPicker(new List<int>() { 5 }); });
        EventController.StartListening("month", delegate { EventPicker(new List<int>() { 4 }); });
    }

    private void ReadEventScriptables() {
        scheduledEventModel.scheduledEventList = scheduledEventList;
        scheduledEventModel.scheduledEvents = scheduledEventList.scheduledEvents;
        foreach (ScheduledEvent scheduled in scheduledEventModel.scheduledEvents) {
            scheduledEventModel.scheduledEventLookup.Add(scheduled.ID, scheduled);
        }
    }

    public void DetermineFutureEvents() {
        // Determine how long it has been since the previous event
        DateTimeObject lastEvent = scheduledEventModel.lastOccurringEvent;
        DateTimeObject dateDifference;
        if (lastEvent == null) {
            dateDifference = controllerManager.dateController.ReturnCurrentDateTime();
        } else dateDifference = controllerManager.dateController.TimeDifference(lastEvent.rawTime);

        float eventChance = AiFunctions.CalculateProbabilityOfEvent(dateDifference, minDaysSinceLastEvent, dateOffsetDivisor, eventBaseChance, managerReferences.modelManager.timeModel.dayLength);

        if (Random.Range(0f, 1f) <= eventChance) {
            EventPicker(new List<int>() { 1, 2, 3 });
        }
    }

    private void AppendRegularEvent(int id) {
        DetermineCurrentConditions();
        ScheduledEvent scheduled = LookupScheduledEventByID(id);
        DateTimeObject targetDate = controllerManager.dateController.AddDateTime(hours: 1);
        Debug.Log("Enqueued regular event of " + scheduled.uniqueName + " at time of " + targetDate.rawTime);
        if (scheduled != null) {
            switch (id) {
                case 4:
                    AppendEventToQueue(scheduled, targetDate, Vector3.negativeInfinity);
                    break;

            }
        }
    }

    private void Update() {
        if (GeneralEnumStorage.debugActive) {
            if (Input.GetKeyDown(KeyCode.F11)) {
                AppendRegularEvent(4);
            }

            if (Input.GetKeyDown(KeyCode.KeypadMinus)) {
                string[] reason = new string[1] { controllerManager.settingsController.TranslateString("TaxationLimitReached") };
                controllerManager.saveGameController.EndGame(reason);
            }

        }

        if (GeneralEnumStorage.developerOptions || GeneralEnumStorage.debugActive) {
            if (Input.GetKey(KeyCode.LeftShift)) {
                if (Input.GetKeyDown(KeyCode.T)) {
                    EventPicker(new List<int>() { 1 }, 0);
                } else if (Input.GetKeyDown(KeyCode.A)) {
                    EventPicker(new List<int>() { 5 }, 0);
                } else if (Input.GetKeyDown(KeyCode.C)) {
                    EventPicker(new List<int>() { 3 }, 0);
                }
            }
        }
    }

    public ScheduledEvent LookupScheduledEventByID(int id) {
        if (scheduledEventModel.scheduledEventLookup.ContainsKey(id)) return scheduledEventModel.scheduledEventLookup[id];
        else return null;
    }

    private void EventPicker(List<int> possibleIDs = null, int setTime = -1) {
        DetermineCurrentConditions();
        List<ScheduledEvent> availableEvents = new List<ScheduledEvent>();
        List<ScheduledEvent> availableIndependent = null;
        List<ScheduledEvent> availableDependent = null;
        if (possibleIDs == null) {
            possibleIDs = new List<int>();
            foreach (ScheduledEvent scheduledEvent in scheduledEventModel.scheduledEvents) {
                possibleIDs.Add(scheduledEvent.ID);
            }
        }

        // Determine which events' conditions have been met.

        foreach (int id in possibleIDs) {
            ScheduledEvent scheduledEvent = LookupScheduledEventByID(id);
            if (scheduledEvent != null) {
                bool allMet = true;
                foreach (EventCondition condition in scheduledEvent.eventConditions) {
                    EventCondition currentCondition = currentEventConditions.Find(x => x.conditionIdentifier == condition.conditionIdentifier);
                    if (currentCondition != null) {
                        int currentValue = currentCondition.value;
                        if (!GeneralFunctions.CompareConditions(condition.comparisonValue, condition.value, currentValue)) {
                            allMet = false;
                        }
                    } else allMet = false;
                }
                if (allMet) {
                    availableEvents.Add(scheduledEvent);
                }
            }
        }

        Debug.Log("EQC - Total avaiable events: " + availableEvents.Count);
        if (availableEvents.Count == 0) return;
        availableIndependent = availableEvents.FindAll(x => x.independentEvent);
        availableDependent = availableEvents.FindAll(x => !x.independentEvent);
        // Normalise probabilities based on available events;
        if (availableDependent != null) {
            float[] input = new float[availableDependent.Count];
            for (int i = 0; i < availableDependent.Count; i++) {
                input[i] = availableDependent[i].percentageChance;
            }
            int index = GeneralFunctions.PickRandomValueFromChanceArray(input, Random.Range(0f, 1f));
            if (index != -1 && availableDependent.Count > index) {
                int hoursUntil = setTime == -1 ? Random.Range(1, 6) : setTime;
                DateTimeObject targetDate = controllerManager.dateController.AddDateTime(hours: hoursUntil);
                AppendEventToQueue(availableDependent[index], targetDate, null);
            }
        }
        if (availableIndependent != null) IndependentEventQueuer(availableIndependent);
    }

    private void IndependentEventQueuer(List<ScheduledEvent> possibleEvents) {
        foreach (ScheduledEvent possibleEvent in possibleEvents) {
            Debug.Log("EQC - Attempting to queue " + possibleEvent.uniqueName);
            float randomRoll = Random.Range(0f, 1f);
            if (randomRoll < possibleEvent.percentageChance) {
                DateTimeObject targetDate = controllerManager.dateController.AddDateTime(hours: Random.Range(1, 6));
                AppendEventToQueue(possibleEvent, targetDate, null);
            }
        }
    }

    private void ExectuteEvent(InstantiatedEvent relevantEvent) {
        Debug.Log("EQC - Event of type (" + relevantEvent.proposedEvent.uniqueName + ") has been triggered." + "with an id of " + relevantEvent.id);
        Vector3 pos = relevantEvent.position != null ? relevantEvent.position.Value : Vector3.zero;
        managerReferences.uiManagement.warningLogView.AppendMessageToLog(relevantEvent.proposedEvent.uniqueName + "Occurred", pos);
        if (relevantEvent.scheduledActions != null) relevantEvent.scheduledActions.Invoke();
        scheduledEventModel.eventQueue.Remove(relevantEvent);
        scheduledEventModel.instantiatedEventLookup.Remove(relevantEvent.id);
        EventController.StopListening("rawTimeOf" + relevantEvent.scheduledTime.rawTime + "Reached", () => ExectuteEvent(relevantEvent));
    }

    public void InstantiateSavedEvents(List<InstantiatedEvent> events, DateTimeObject lastEvent) {
        scheduledEventModel.eventQueue.Clear();
        foreach (InstantiatedEvent currentEvent in events) {
            Debug.Log("EQC - Event Queue Count: " + scheduledEventModel.eventQueue.Count + ", current: " + currentEvent.scheduledEventID);
            ScheduledEvent scheduled = scheduledEventModel.scheduledEventLookup[currentEvent.scheduledEventID];
            AppendEventToQueue(scheduled, currentEvent.scheduledTime, currentEvent.position);
        }
        if (scheduledEventModel.lastOccurringEvent == null && lastEvent != null) scheduledEventModel.lastOccurringEvent = lastEvent;

        Debug.Log("EQC - New Event Queue Count: " + scheduledEventModel.eventQueue.Count);
    }

    private void AppendEventToQueue(ScheduledEvent scheduledEvent, DateTimeObject deadline, Vector3? location) {
        int id = FindAvailableID();
        int x = managerReferences.modelManager.gridModel.width;
        int y = managerReferences.modelManager.gridModel.height;
        Vector3? loc = location;
        if (scheduledEvent.locRequired && loc == null) loc = new Vector3(Random.Range(-x / 2, x / 2), Random.Range(-y / 2, y / 2));
        InstantiatedEvent newEvent = new InstantiatedEvent(id, DetermineRequiredEvents(scheduledEvent, loc), deadline, scheduledEvent);
        newEvent.position = loc;
        scheduledEventModel.eventQueue.Add(newEvent);
        scheduledEventModel.instantiatedEventLookup.Add(id, newEvent);
        controllerManager.dateController.AppendTimeForNotification(deadline.rawTime, "EventTrigger");
        EventController.StartListening("rawTimeOf" + deadline.rawTime + "Reached", () => ExectuteEvent(newEvent));
        Debug.Log(scheduledEvent.uniqueName + " has been scheduled to run at " + deadline.hours + "hrs/" + deadline.days + "dys, with the id of " + id);
        if (!scheduledEvent.independentEvent) scheduledEventModel.lastOccurringEvent = deadline;
    }

    public float LastEventOccurence() {
        DateTimeObject lastEvent = scheduledEventModel.lastOccurringEvent;
        if (lastEvent != null) {
            return lastEvent.rawTime;
        } else return 0;
    }

    private int FindAvailableID() {
        int attempt = Random.Range(1, 9999);
        while (scheduledEventModel.instantiatedEventLookup.ContainsKey(attempt)) {
            attempt = Random.Range(1, 9999);
        }
        return attempt;
    }

    private UnityAction DetermineRequiredEvents(ScheduledEvent scheduledEvent, Vector3? loc1) {
        UnityAction returnEvent = null;
        UiManagement uiManagement = managerReferences.uiManagement;
        string confirmationVariable = "";
        Vector3 loc = loc1 == null ? loc = Vector3.negativeInfinity : loc1.Value;

        switch (scheduledEvent.ID) {
            case 1:
                // Trader Entry;
                returnEvent += (delegate { controllerManager.nPCController.SpawnNPC(new List<InstantiatedResource>(), 1500, loc); });
                break;
            case 2:
                confirmationVariable = "NewVillagerConfirmInfo";
                returnEvent += (delegate { controllerManager.skillsController.CreateNewPawn(loc); });
                break;
            case 3:
                // Crop Failure;
                returnEvent += delegate {
                    controllerManager.farmingController.ProcessCropFailure();
                };
                break;
            case 4:
                int buildingWeight = currentEventConditions.Find(x => x.conditionIdentifier == EventCondition.ConditionIdentifier.TotalBuildingWeight).value;
                int totalWealth = currentEventConditions.Find(x => x.conditionIdentifier == EventCondition.ConditionIdentifier.TotalItemWealth).value;
                int value = ResourceFunctions.DetermineTaxValue(buildingWeight, totalWealth, GeneralEnumStorage.currentDifficulty);
                int taxationBacklog = scheduledEventModel.taxationBacklog;
                returnEvent += (delegate { controllerManager.resourceController.EnableTradingDialogue(null, value + taxationBacklog); });
                break;
            case 5:
                //Animal Spawning
                returnEvent += delegate {
                    controllerManager.nPCController.DetermineAnimalSpawns(Random.Range(1, 4), loc);
                };
                break;
        }

        if (scheduledEvent.confirmationRequired && confirmationVariable != "") {
            string identifier = scheduledEvent.uniqueName + scheduledEvent.ID + "at" + loc;
            UnityAction confirmList = null;
            confirmList += (delegate { uiManagement.ActivateConfirmationDialogue(identifier, confirmationVariable, returnEvent, null, useBaseMessage : false); });
            return confirmList;
        }
        return returnEvent;
    }

    private void DetermineCurrentConditions() {
        if (currentEventConditions.Count != System.Enum.GetValues(typeof(EventCondition.ConditionIdentifier)).Length) {
            foreach (EventCondition.ConditionIdentifier conditionID in System.Enum.GetValues(typeof(EventCondition.ConditionIdentifier))) {
                Debug.Log("Searching for: " + conditionID.ToString());
                if (currentEventConditions.Find(x => x.conditionIdentifier == conditionID) == null) {
                    currentEventConditions.Add(new EventCondition(conditionID, 0));
                }
            }
        }

        List<Build> buildList = controllerManager.buildingController.ReturnBuildList();

        float totalValue = controllerManager.storageController.ReturnValueOfItems(false);
        currentEventConditions.Find(x => x.conditionIdentifier == EventCondition.ConditionIdentifier.TotalItemWealth).value = (int) totalValue;

        int totalPopulation = controllerManager.skillsController.PawnListReturn(false).Count;
        currentEventConditions.Find(x => x.conditionIdentifier == EventCondition.ConditionIdentifier.TotalPopulation).value = (int) totalPopulation;

        int totalBuildings = buildList.Count;
        currentEventConditions.Find(x => x.conditionIdentifier == EventCondition.ConditionIdentifier.TotalBuildings).value = (int) totalBuildings;

        int totalBuildingWeight = 0;
        foreach (Build build in buildList) {
            totalBuildingWeight += build.structureData.buildingWeight;
        }
        currentEventConditions.Find(x => x.conditionIdentifier == EventCondition.ConditionIdentifier.TotalBuildingWeight).value = (int) totalBuildingWeight;
    }

    public int AmendTaxationBacklog(bool reset, int change = 0) {
        if (reset) scheduledEventModel.taxationBacklog = 0;
        else {
            scheduledEventModel.taxationBacklog += change;
            Difficulty difficulty = GeneralEnumStorage.currentDifficulty;
            DifficultySettings difficultySettings = controllerManager.settingsController.FindDifficultySettings(difficulty);
            int maxValue = 10000;
            switch (difficulty) {
                case Difficulty.Easy:
                    maxValue = difficultyMaxBacklog[0];
                    break;
                case Difficulty.Medium:
                    maxValue = difficultyMaxBacklog[1];
                    break;
                case Difficulty.Hard:
                    maxValue = difficultyMaxBacklog[2];
                    break;
            }

            if (scheduledEventModel.taxationBacklog >= difficultySettings.backLogLimit) {
                string[] reason = new string[1] { controllerManager.settingsController.TranslateString("TaxationLimitReached") };
                controllerManager.saveGameController.EndGame(reason);
            } else {
                if (scheduledEventModel.taxationBacklog >= (float) difficultySettings.backLogLimit / 2f) {
                    managerReferences.uiManagement.warningLogView.AppendMessageToLog("TaxationWarningMessage", Vector3.zero);
                }
            }
        }
        return scheduledEventModel.taxationBacklog;
    }
}