using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class WeatherDisplayView : MonoBehaviour {
    public ControllerManager controllerManager;
    private WeatherController weatherController;
    private TimeModel timeModel;
    public TextMeshProUGUI temperatureField;
    public ModelManager modelManager;
    void Start() {
        timeModel = modelManager.timeModel;
        weatherController = controllerManager.weatherController;
    }

    // Update is called once per frame
    private void FixedUpdate() {
        float currentTemperature = modelManager.weatherModel.currentTemperature;

        string output = System.Math.Round(currentTemperature, 1).ToString();
        temperatureField.SetText(output + "°C");
    }
}