using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GeneralCompiler : MonoBehaviour {
    // Start is called before the first frame update
    public GameObject LanguageDropdown, LanguageText;
    Dictionary<string, string> strings;
    void Start() {
        strings = GameObject.Find("Settings").GetComponent<SettingsController>().ReturnStrings();
        string curLan = PlayerPrefs.GetString("Language");
        LanguageText.GetComponent<TextMeshProUGUI>().SetText(strings["Lang"]);
        if (curLan == "English") LanguageDropdown.GetComponent<TMP_Dropdown>().value = 1;
        else { LanguageDropdown.GetComponent<TMP_Dropdown>().value = 0; }

        LanguageDropdown.GetComponent<TMP_Dropdown>().onValueChanged.AddListener(delegate { LanguageChange(); });
    }

    private void LanguageChange() {
        string languagechoice = LanguageDropdown.GetComponent<TMP_Dropdown>().options[LanguageDropdown.GetComponent<TMP_Dropdown>().value].text;
        //Debug.Log(LanguageDropdown.GetComponent<TMP_Dropdown>().options[LanguageDropdown.GetComponent<TMP_Dropdown>().value].text);
        PlayerPrefs.SetString("Language", languagechoice);
        //GameObject.Find("Settings").GetComponent<SettingsController>().SetLanguage();
    }

    // Update is called once per frame
    void Update() {

    }
}