using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public static class SaveFunctions {
    // Start is called before the first frame update
    public static SaveContainer SaveFileReader(string _fileLocation) {
        StreamReader inp_stm = new StreamReader(_fileLocation);
        // TODO: Wrap this in try/catch to handle deserialization exceptions
        string content = inp_stm.ReadToEnd();
        inp_stm.Close();
        return JsonUtility.FromJson<SaveContainer>(content);
    }

    public static SaveGameItem ConvertToSaveGameItem(SaveContainer saveContainer, string filePath) {
        System.DateTime timeSaved = System.DateTime.FromFileTime(saveContainer.timeSaved);
        return new SaveGameItem(filePath, saveContainer.fileName, saveContainer, timeSaved);
    }

    public static List<SaveGameItem> ReturnSaveFiles(string filePath, string orderBy) {
        List<SaveGameItem> saveList = new List<SaveGameItem>();
        string[] filePaths = System.IO.Directory.GetFiles(filePath, "*.json");
        foreach (string path in filePaths) {
            SaveContainer saveContainer = SaveFileReader(path);
            saveList.Add(ConvertToSaveGameItem(saveContainer, path));
        }

        switch (orderBy) {
            case "date":
                saveList = saveList.OrderByDescending(save => save.dateTime).ToList();
                break;
        }

        return saveList;
    }

    public static void FormatSaveGaveItem(GameObject item, SaveGameItem save, UnityAction onClick) {
        item.name = save.fileLocation.Substring(save.fileLocation.LastIndexOf("/"));
        item.transform.GetChild(0).GetComponent<TextMeshProUGUI>().SetText(save.fileName);
        string dateText = save.dateTime.ToString("G");
        System.DateTime.Now.ToString("HH:mm dd/MM");
        item.transform.GetChild(1).GetComponent<TextMeshProUGUI>().SetText(dateText);
        item.GetComponent<Button>().onClick.AddListener(onClick);
    }

}