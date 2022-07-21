using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
public static class SettingsFunctions {
    // Start is called before the first frame update

    public static void TranslateTMPItems(SettingsController settings, TextMeshProUGUI text) {
        if (text == null) return;
        string textString = text.text;
        string additionText = "";
        string[] escapeChars = new string[] { "?", ":", "..." };

        foreach (string escape in escapeChars) {
            if (textString.Contains(escape)) {
                if (additionText == "") {
                    textString = textString.Substring(0, textString.Length - escape.Length);
                    additionText = escape;
                } else Debug.Log("SF - String has multiple escapes, string name: " + textString + " existing escape is: " + additionText);
            }
        }

        textString.TrimEnd();
        text.SetText(settings.TranslateString(textString) + additionText);
    }
    public static void TranslateTMPItems(SettingsController settings, TextMeshProUGUI[] translatableTextItems) {
        foreach (TextMeshProUGUI textItem in translatableTextItems) {
            TranslateTMPItems(settings, textItem);
        }
    }
    public static Dictionary<string, string> SetLanguage(string baseLanguage) {
        Dictionary<string, string> wordDictionary = new Dictionary<string, string>();
        // Check to see if the language preference has been changed.
        if (baseLanguage != PlayerPrefs.GetString("Language", "Cymraeg")) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        string lang = PlayerPrefs.GetString("Language", "Cymraeg");

        // Import the relevant language files.
        string[] jsonFiles = new string[] { "/Strings.json", "/ResourceNames.json", "/Structures&Purposes.json" };
        string res = Application.streamingAssetsPath + "/Ieithoedd/" + lang;

        foreach (string filename in jsonFiles) {
            string currentFilePath = res + filename;
            try {
                // Read the text from directly from the Json file.
                StreamReader inp_stm = new StreamReader(currentFilePath);
                string content = inp_stm.ReadToEnd();
                ListContainer container = JsonUtility.FromJson<ListContainer>(content);

                // Add all strings from the parsed file, accounting for nested JSON objects.
                List<Translation> translations = container.dataList;
                for (int i = 0; i < translations.Count; i++) {
                    if (!wordDictionary.ContainsKey(translations[i].variable))
                        wordDictionary.Add(translations[i].variable, translations[i].value);
                }
                Debug.Log("SETC - Converted " + translations.Count + " strings from file: " + currentFilePath);
            } catch (System.Exception) {
                Debug.LogError("SETC - Language file cannot be parsed for " + currentFilePath);
                PlayerPrefs.SetString("Language", "Cymraeg");
                SetLanguage(baseLanguage);
                return null;
            }
        }
        Debug.Log("SETC - Language Set To: " + PlayerPrefs.GetString("Language") + ", Total Of: " + wordDictionary.Count + " Strings.");
        return wordDictionary;
    }

}