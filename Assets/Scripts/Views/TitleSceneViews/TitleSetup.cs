using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class TitleSetup : MonoBehaviour {
    Dictionary<string, string> strings;
    public Button[] TitleButtons;
    public UiManagement uiManagement;
    public SettingsController settingsController;
    // Start is called before the first frame update
    void Start() {
        PlayerPrefs.SetString("saveLocation", Application.streamingAssetsPath + "/Saves/");
        FormatButtonListeners();
    }

    private void FormatButtonListeners() {
        TitleButtons[0].onClick.AddListener(delegate { uiManagement.ManageCanvases(2); });
        TitleButtons[2].onClick.AddListener(delegate { uiManagement.ManageOpenDialogues(true, 1); });
        TitleButtons[3].onClick.AddListener(Application.Quit);
        TitleButtons[4].onClick.AddListener(delegate { uiManagement.ManageOpenDialogues(true, 2); });
    }

    // Update is called once per frame
    void Update() {

    }
}