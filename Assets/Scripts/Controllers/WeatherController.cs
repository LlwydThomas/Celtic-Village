using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.ParticleSystemJobs;
public class WeatherController : MonoBehaviour {

    public ManagerReferences managerReferences;
    private ControllerManager controllerManager;
    private ModelManager modelManager;
    private WeatherModel weatherModel;
    public SeasonDataList seasonDataList;
    [SerializeField]
    private DailyWeatherData dailyWeather;
    public ParticleView particleView;
    public string particleListener = "";
    public float temperatureFluctuationMax;
    public float maximumIntesity, minimumIntensity;
    public UnityEngine.Rendering.Universal.Light2D screenLight;

    private void Awake() {
        controllerManager = managerReferences.controllerManager;
        modelManager = managerReferences.modelManager;

        weatherModel = modelManager.weatherModel;
        InitialiseScriptableObjects();

        EventController.StartListening("hour", delegate { TemperatureCalculation(dailyWeather); });
        EventController.StartListening("mapCompleted", delegate {
            AmendSeason();
            particleView.AlignParticleSystems(modelManager.gridModel);
        });
    }

    private void Start() {
        EventController.StartListening("month", delegate { AmendSeason(); });
        EventController.StartListening("day", delegate { CalculateWeatherData(weatherModel.currentSeason); });
        CalculateWeatherData(weatherModel.currentSeason);
    }

    public float TemperatureCalculation(DailyWeatherData dailyWeather) {
        DateTimeObject dateTime = controllerManager.dateController.ReturnCurrentDateTime();
        SeasonData season = weatherModel.currentSeason;
        weatherModel.currentTemperature = TimeFunctions.TemperatureDeduction(dailyWeather.averageTemperature, season.minTemp, season.maxTemp, season.morningEnd, season.eveningStart, dateTime.hours, dateTime.minutes);
        return weatherModel.currentTemperature;
    }

    public float CalculateLightIntensity() {
        DateTimeObject currentDateTime = controllerManager.dateController.ReturnCurrentDateTime();
        SeasonData currentSeason = weatherModel.currentSeason;
        int dayLength = currentSeason.eveningStart - currentSeason.morningEnd;
        float intensity;
        if (currentDateTime.hours > currentSeason.morningEnd && currentDateTime.hours <
            currentSeason.eveningStart) intensity = maximumIntesity;
        else {
            intensity = TimeFunctions.LightIntensityDeduction(minimumIntensity, maximumIntesity, currentSeason.morningEnd, currentSeason.eveningStart, currentDateTime.hours, currentDateTime.minutes);
        }
        screenLight.intensity = intensity;
        return intensity;
    }

    public DailyWeatherData ReturnDailyWeatherData() {
        return dailyWeather;
    }

    private void InitialiseScriptableObjects() {
        weatherModel.seasonDatas = seasonDataList.SeasonDatas;
        foreach (SeasonData season in weatherModel.seasonDatas) weatherModel.seasonDataLookup.Add(season.id, season);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.F12))
            if (GeneralEnumStorage.debugActive) AmendSeason();
    }

    public int DaysSinceRainReturn() {
        return weatherModel.daysSinceRain;
    }

    public void AmendSeason(int month = -1) {
        if (month == -1) month = controllerManager.dateController.ReturnCurrentDateTime().months;
        //Debug.Log(weatherModel.season);
        Debug.Log(weatherModel);
        weatherModel.currentSeason = weatherModel.seasonDataLookup[TimeFunctions.DeduceSeason(modelManager.timeModel, month)];
        CalculateWeatherData(weatherModel.currentSeason);
        TemperatureCalculation(dailyWeather);
    }

    public void CalculateWeatherData(SeasonData season) {
        DateTimeObject currentDateTime = controllerManager.dateController.ReturnCurrentDateTime();
        if (dailyWeather.season != null && TimeFunctions.CompareDateTimes(dailyWeather.dateOfWeatherData, currentDateTime)) return;
        Debug.Log("WHC - Passes the compare date times check.");
        DateController date = controllerManager.dateController;
        dailyWeather = TimeFunctions.CalculateWeatherData(season, currentDateTime, temperatureFluctuationMax);
        if (dailyWeather != null) {
            if (dailyWeather.precipitaion) {
                dailyWeather.startTime = date.AddDateTime(hours: Random.Range(1, 12)).rawTime;
                date.AppendTimeForNotification(dailyWeather.startTime, "Starting Rainfall");
                ListenForRain(true);
            } else weatherModel.daysSinceRain += 1;
            dailyWeather.rainfallQueued = true;
        }
        /* if (season != null) {

            dailyWeather.rainfallQueued = false;
            bool precipitaion, rainfallQueued;
            if (Random.Range(0.0f, 0.1f) <= season.chanceOfPrecipiation) precipitaion = true;
            else precipitaion = false;
            int precipitationLength = 0;
            float startDate = -1;
            if (precipitaion) {
                precipitationLength = Random.Range(1, 10);
                startDate = date.AddDateTime(hours: Random.Range(1, 12)).rawTime;
                date.AppendTimeForNotification(startDate, "Starting Rainfall");
            } else {
                weatherModel.daysSinceRain += 1;
            }
            rainfallQueued = true;
            float averageTemperature = (season.maxTemp + season.minTemp) / 2 + Random.Range(-temperatureFluctuationMax, temperatureFluctuationMax);

            dailyWeather = new DailyWeatherData(season, currentDateTime, precipitaion, precipitationLength, averageTemperature, startDate);
            if (dailyWeather.precipitaion) ListenForRain(true);
            dailyWeather.rainfallQueued = rainfallQueued;
        } */
    }

    public SeasonData SeasonNumReturn() {
        return weatherModel.currentSeason;
    }

    public void BeginOrEndParticles(int typeIndex, bool begin) {
        if (begin) {
            particleView.ManageParticleSystems(typeIndex);
            EventController.StopListening(particleListener, delegate {
                BeginOrEndParticles(0, true);
            });
            DateTimeObject endTime = controllerManager.dateController.AddDateTime(hours: dailyWeather.precipitationLength);
            controllerManager.dateController.AppendTimeForNotification(endTime.rawTime, "Ending Rainfall");
            particleListener = "rawTimeOf" + endTime.rawTime + "Reached";
            weatherModel.daysSinceRain = 0;
            EventController.StartListening(particleListener, delegate { BeginOrEndParticles(0, false); });

        } else {
            EventController.StopListening(particleListener, delegate { BeginOrEndParticles(0, false); });
            particleView.ManageParticleSystems(-1);
        }

    }
    public void ListenForRain(bool activate) {
        if (activate) {
            if (dailyWeather.precipitaion && dailyWeather.startTime != -1) {
                particleListener = "rawTimeOf" + dailyWeather.startTime + "Reached";
                EventController.StartListening(particleListener, delegate { BeginOrEndParticles(0, true); });
            }
        }
    }

}

[System.Serializable]
public class DailyWeatherData {
    public SeasonData season;
    public bool precipitaion;
    public DateTimeObject dateOfWeatherData;
    public int precipitationLength;
    public float averageTemperature;
    public bool rainfallQueued;
    public float startTime;

    public DailyWeatherData(SeasonData _season, DateTimeObject timeStamp, bool _precipitation, int _precipitationLength, float _averageTemperature, float startDate = -1) {
        season = _season;
        precipitaion = _precipitation;
        dateOfWeatherData = timeStamp;
        precipitationLength = _precipitationLength;
        averageTemperature = _averageTemperature;
        startTime = startDate;
    }

}