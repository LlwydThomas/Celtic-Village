using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class OptionsView : MonoBehaviour {
    public bool reloadScene = false;
    public TextMeshProUGUI[] translateables;
    private Resolution[] resolutions;
    public TMP_Dropdown resolution, language;
    public Toggle vsync, fullscreen, autosave, buildingIcons;
    public Slider scrollSpeed, panSpeed;
    public GameObject igmButtons;
    public ManagerReferences managerReferences;
    private void Start() {
        resolutions = Screen.resolutions;
        SettingsFunctions.TranslateTMPItems(managerReferences.controllerManager.settingsController, translateables);
        resolution.ClearOptions();
        List<string> resolutionStrings = new List<string>();
        foreach (Resolution res in resolutions) {
            resolutionStrings.Add(res.width + "x" + res.height + " " + res.refreshRate + "hz");
        }
        resolution.AddOptions(resolutionStrings);
        PopulateOptions();
    }
    private void OnEnable() {
        if (resolutions != null) PopulateOptions();
    }

    private void PopulateOptions() {
        vsync.isOn = PlayerPrefs.GetInt("Vsync") > 0 ? true : false;
        fullscreen.isOn = Screen.fullScreen;
        scrollSpeed.value = PlayerPrefs.GetFloat("ScrollSpeed", 0.1f);
        panSpeed.value = PlayerPrefs.GetFloat("PanSpeed", 0.1f);
        autosave.isOn = PlayerPrefs.GetInt("AutosaveEnabled", 1) == 1 ? true : false;
        buildingIcons.isOn = PlayerPrefs.GetInt("BuildingIconsEnabled", 1) == 1 ? true : false;
        if (language != null) language.SetValueWithoutNotify(language.options.FindIndex(x => x.text == PlayerPrefs.GetString("Language", "Cymraeg")));
        resolution.value = System.Array.IndexOf(resolutions, Screen.currentResolution);
    }

    public void SaveChanges() {
        // Extract information from the options screen and inform the relevant scripts.
        SettingsController settings = managerReferences.controllerManager.settingsController;
        settings.SetResolution(resolutions[resolution.value], fullscreen.isOn);
        int vsyncInt = vsync.isOn == true ? 1 : 0;
        int autosaveOn = autosave.isOn == true ? 1 : 0;
        int iconsOn = buildingIcons.isOn == true ? 1 : 0;
        settings.EnableVsync(vsyncInt);
        settings.SetScrollPanSpeed(scrollSpeed.value, panSpeed.value);
        PlayerPrefs.SetInt("BuildingIconsEnabled", iconsOn);
        PlayerPrefs.SetInt("AutosaveEnabled", autosaveOn);
        if (managerReferences.controllerManager.buildingController != null) {
            managerReferences.controllerManager.buildingController.ToggleIconVisibility(iconsOn);
        }
        if (managerReferences.controllerManager.saveGameController != null) {

        }
        if (language != null) {
            PlayerPrefs.SetString("Language", language.options[language.value].text);
        }
        if (reloadScene) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        igmButtons.SetActive(true);
        this.transform.parent.gameObject.SetActive(false);
        EventController.TriggerEvent("userOptionsAmended");
    }
}