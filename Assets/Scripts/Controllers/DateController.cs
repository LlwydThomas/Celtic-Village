using System.Collections.Generic;
using UnityEngine;

public class DateController : MonoBehaviour {

    private float currentDate;

    public string cycle;
    public float baseSpeed;
    private float speed;
    private int hours, minutes, days, months, years, dayLength;
    public float previousSpeed;

    public int monthLength, yearLength;
    public float time, minimumIntensity, maximumIntesity, avgIntensity;

    public int morningEndHour, eveningEndHour;
    public UnityEngine.Rendering.Universal.Light2D screenLight;
    Dictionary<string, string> strings;
    public TimeModel timeModel;
    private bool hourSent = false;
    public ManagerReferences managerReferences;
    private ControllerManager controllerManager;
    private ModelManager modelManager;
    public List<float> rawTimeQueue = new List<float>();

    public float currentSpeed = 0f;
    public float maxSpeed;
    public float timeBetweenChecks;
    private float timeCheckTimer;

    // Start is called before the first frame update

    private void Awake() {
        dayLength = 1440;
        modelManager = managerReferences.modelManager;
        controllerManager = managerReferences.controllerManager;
        timeModel = modelManager.timeModel;
        screenLight = GameObject.Find("ScreenLight").GetComponent<UnityEngine.Rendering.Universal.Light2D>();
        TimeFunctions.InitialiseTimeModel(timeModel, dayLength * monthLength * yearLength, dayLength * monthLength, dayLength, 0);

    }
    private void Start() {
        AmendSpeed(0.5f);
        EventController.StartListening("startTimer", delegate { AmendSpeed(1, true); });
        EventController.StartListening("stopTimer", delegate { AmendSpeed(0, true); });
        avgIntensity = (minimumIntensity + maximumIntesity) / 2f;
        controllerManager.weatherController.AmendSeason();
    }

    // Update is called once per frame

    private void FixedUpdate() {
        // Each fixed frame, increment the raw time and the daily timer, and deduce the hours and minutes based on the timer.
        timeModel.rawTime += Time.deltaTime * timeModel.speed;
        timeCheckTimer += Time.deltaTime;
        // Check if any upcoming time based events have been satisfied.
        if (timeCheckTimer >= timeBetweenChecks && timeModel.speed != 0) {
            string debugText = "DTC - Current time: " + timeModel.rawTime + "; Time Queue: ";
            foreach (float deadline in rawTimeQueue.ToArray()) {
                debugText += deadline + ", ";
                if (timeModel.rawTime >= deadline) {
                    Debug.Log("Deadline has been reached");
                    EventController.TriggerEvent("rawTimeOf" + deadline + "Reached");
                    rawTimeQueue.Remove(deadline);
                }
            }
            if (rawTimeQueue.Count > 0) Debug.Log(debugText);
            timeCheckTimer = 0;
        }

        time += Time.deltaTime * timeModel.speed;
        hours = (int) time / 60;
        minutes = (int) time % 60;
        if (minutes == 0) {
            if (!hourSent) {
                // Only trigger the hour event once, independent of game speed.
                EventController.TriggerEvent("hour");
                hourSent = true;
            }
        } else hourSent = false;

        if (time >= timeModel.dayLength) {
            time = 0;
            days += 1;

            // Roll over to next month, and inform other scripts of the event.
            if (days >= timeModel.daysInMonth) {
                if (months == timeModel.monthsInYear) {
                    years += 1;
                    months = 1;
                    EventController.TriggerEvent("year");
                } else {
                    months += 1;
                    EventController.TriggerEvent("month");
                }
                days = 0;
            } else EventController.TriggerEvent("day");
        }

        // If the current hour is counted as 'night' time, amend light intensity based on the time and the current season. 
        ///screenLight.intensity = CalculateLightIntensity(hours * 60 + minutes);
        controllerManager.weatherController.CalculateLightIntensity();
    }

    public float CalculateLightIntensity(float hoursAndMinutes) {
        int dayLength = eveningEndHour - morningEndHour;
        int _hours = (int) hoursAndMinutes / 60;
        if (_hours > morningEndHour && _hours <
            eveningEndHour) return maximumIntesity;
        float _minutes = (float) hoursAndMinutes % 60;
        return TimeFunctions.LightIntensityDeduction(minimumIntensity, maximumIntesity, morningEndHour, eveningEndHour, _hours, _minutes);
    }

    public void AmendSpeed(float factor, bool saveSpeed = false, bool bypassMax = false) {
        if (!bypassMax) {
            float speedMax = GeneralEnumStorage.debugActive ? 10 : maxSpeed;
            factor = Mathf.Clamp(factor, 0, speedMax);
        }
        float currentFactor = Mathf.Round((timeModel.speed / baseSpeed) * 10f) / 10f;
        if (saveSpeed) {
            if (previousSpeed == -1) {
                previousSpeed = currentFactor != 0 ? currentFactor : -1;
            }
            if (factor != 0 && previousSpeed != -1) {
                timeModel.speed = baseSpeed * previousSpeed;
                previousSpeed = -1;
            } else {
                timeModel.speed = baseSpeed * factor;
            }
        } else {
            timeModel.speed = baseSpeed * factor;
            previousSpeed = -1;
        }
        Debug.Log("DTC - prev: " + previousSpeed + " currentSpeed: " + timeModel.speed);
        currentSpeed = factor;
        if (factor >= 5) timeBetweenChecks = 2;
        else timeBetweenChecks = 5;
        EventController.TriggerEvent("gameSpeedChange");
    }

    public void IncreaseOrDecreaseSpeed(float change) {
        float currentSpeed = timeModel.speed / baseSpeed;
        float newSpeed = Mathf.Round((currentSpeed + change) * 10f) / 10f;
        AmendSpeed(newSpeed);
    }

    public int TriggerPause() {
        if ((float) timeModel.speed / (float) baseSpeed == 0) {
            AmendSpeed(1, true);
            return 1;
        } else AmendSpeed(0, true);
        return 0;
    }

    public DateTimeObject AddDateTime(int hours = 0, int days = 0, int months = 0, int years = 0) {
        float newRaw = TimeFunctions.DateAddition(timeModel, hours, days, months, years);
        return TimeFunctions.ConvertDateTimeObject(newRaw, timeModel);
    }

    public DateTimeObject TimeDifference(float rawTime1, float rawTime2 = -1) {
        if (rawTime2 == -1) rawTime2 = ReturnCurrentDateTime().rawTime;
        float rawTimeDifference;
        if (rawTime1 >= rawTime2) {
            rawTimeDifference = rawTime1 - rawTime2;
        } else rawTimeDifference = rawTime2 - rawTime1;

        return TimeFunctions.ConvertDateTimeObject(rawTimeDifference, timeModel);
    }

    public void AppendTimeForNotification(float rawTime, string reason = "") {
        if (!rawTimeQueue.Contains(rawTime)) rawTimeQueue.Add(rawTime);
        if (reason != "") Debug.Log("DTC - Reason: " + reason + " queued at " + rawTime);
    }
    public float GameSpeedReturn() {
        return timeModel.speed;
    }

    public void LoadTime(float rawTime) {
        rawTimeQueue.Clear();
        timeModel.rawTime = rawTime;
        // Use the raw time in order to calculate the current date.
        DateTimeObject dateTimeLoad = TimeFunctions.ConvertDateTimeObject(timeModel.rawTime, timeModel);
        time = (dateTimeLoad.hours * 60) + dateTimeLoad.minutes;
        controllerManager.weatherController.AmendSeason();
    }

    /* private void SetTime(float rawTime) {
        DateTimeObject convertedTime = ConvertDateTimeObject(rawTime);
        time = convertedTime.hoursAndMinutes;
        days = convertedTime.days;
        months = convertedTime.months;
        years = convertedTime.years;
        hours = convertedTime.hours;
        minutes = convertedTime.minutes;
    } */

    public DateTimeObject ReturnCurrentDateTime() {
        return TimeFunctions.ConvertDateTimeObject(timeModel.rawTime, timeModel);
    }

}