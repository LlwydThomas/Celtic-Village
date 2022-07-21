using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsController : MonoBehaviour {
    private Dictionary<string, string> stringTrans = new Dictionary<string, string>();
    public Dictionary<string, string> StringTrans {
        get {
            return stringTrans;
        }
        set {
            int currentCount = stringTrans.Count;
            if (value.Count > 0) stringTrans = value;
        }
    }
    private string baseLanguage;

    public bool debugToggle;
    public DifficultySettingsList difficultySettings;
    public Dictionary<Difficulty, DifficultySettings> difficultyLookup = new Dictionary<Difficulty, DifficultySettings>();

    // Initialise the game language firstly, using player preferences.

    private void Awake() {
        stringTrans = new Dictionary<string, string>();
        baseLanguage = PlayerPrefs.GetString("Language", "Cymraeg");
        EnableVsync(PlayerPrefs.GetInt("Vsync", 1));
        Resolution resolution = new Resolution();
        resolution.width = PlayerPrefs.GetInt("ResX", Screen.currentResolution.width);
        resolution.height = PlayerPrefs.GetInt("ResY", Screen.currentResolution.height);
        resolution.refreshRate = PlayerPrefs.GetInt("RefRate", Screen.currentResolution.refreshRate);
        SetResolution(resolution, PlayerPrefs.GetInt("Fullscreen", 1) > 0 ? true : false);
        SetScrollPanSpeed(PlayerPrefs.GetFloat("ScrollSpeed", 10), PlayerPrefs.GetFloat("PanSpeed", 10));
        StringTrans = SettingsFunctions.SetLanguage(baseLanguage);
        PrepareDifficultySettings();
        GeneralEnumStorage.debugActive = debugToggle;
    }

    private void PrepareDifficultySettings() {
        foreach (DifficultySettings diff in difficultySettings.difficultySettings) {
            difficultyLookup.Add(diff.difficulty, diff);
        }
    }

    public DifficultySettings FindDifficultySettings(Difficulty difficulty) {
        if (difficultyLookup.ContainsKey(difficulty)) {
            return difficultyLookup[difficulty];
        } else return difficultySettings.difficultySettings[0];
    }

    public void SetScrollPanSpeed(float scrollSpeed, float panSpeed) {
        Debug.Log("SETC - Setting scrollSpeed to " + scrollSpeed + " and pan speed to: " + panSpeed);
        PlayerPrefs.SetFloat("ScrollSpeed", scrollSpeed);
        PlayerPrefs.SetFloat("PanSpeed", panSpeed);
        if (EventController.instance) EventController.TriggerEvent("settingsChanged");
    }

    public void EnableVsync(int enable) {
        PlayerPrefs.SetInt("Vsync", enable);
        QualitySettings.vSyncCount = enable;
    }

    public void SetResolution(Resolution resolution, bool fullscreen) {
        // Overwrite the player preferences and set the screen resolution the new size.
        Debug.Log("SETC - Setting Resolution to " + resolution.width + "x" + resolution.height + " at " + resolution.refreshRate + "hz, fullscreen: " + fullscreen);
        PlayerPrefs.SetInt("ResX", resolution.width);
        PlayerPrefs.SetInt("ResY", resolution.height);
        PlayerPrefs.SetInt("ResRate", resolution.refreshRate);
        PlayerPrefs.SetInt("Fullscreen", fullscreen ? 1 : 0);
        Screen.SetResolution(resolution.width, resolution.height, fullscreen, resolution.refreshRate);
    }

    public string TranslateString(string key) {
        // Try and return the translated string for the key and if not found return the key itself.
        if (stringTrans.ContainsKey(key)) {
            return stringTrans[key];
        } else if (stringTrans.ContainsKey(key.Trim())) {
            return stringTrans[key.Trim()];
        } else return key;
    }

    public Dictionary<string, string> ReturnStrings() {
        return stringTrans;
    }
}