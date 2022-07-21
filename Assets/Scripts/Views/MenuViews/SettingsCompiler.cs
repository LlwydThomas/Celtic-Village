using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsCompiler : MonoBehaviour {
    public SettingsController settingsController;
    public UiManagement uiManagement;
    public string baseLang;
    // Start is called before the first frame update
    public Button[] settingsRootButtons;

    public GameObject[] tabs;

    void Start() {
        tabs[0].SetActive(true);
        FormatAllButtons();
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            this.gameObject.SetActive(false);
        }
        if (PlayerPrefs.GetString("Language") != baseLang) {

        }
    }

    private void FormatAllButtons() {
        settingsRootButtons[0].gameObject.GetComponentInChildren<TextMeshProUGUI>().SetText(settingsController.TranslateString("Back"));
        foreach (Button button in settingsRootButtons) {
            TextMeshProUGUI text = button.gameObject.GetComponentInChildren<TextMeshProUGUI>();
            text.SetText(settingsController.TranslateString(text.text));
        }

        settingsRootButtons[0].onClick.AddListener(delegate { uiManagement.ManageCanvases(1); });
    }

    public void ExpandTab(int index) {
        foreach (GameObject tab in tabs) {
            tab.SetActive(false);
        }
        tabs[index].SetActive(true);
    }

}