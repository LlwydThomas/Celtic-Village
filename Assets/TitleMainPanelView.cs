using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class TitleMainPanelView : MonoBehaviour {
    // Start is called before the first frame update
    public ManagerReferences managerReferences;
    public TextMeshProUGUI[] translatables;

    public TextMeshProUGUI versionField;
    void Start() {
        SettingsFunctions.TranslateTMPItems(managerReferences.controllerManager.settingsController, translatables);
    }

    // Update is called once per frame
    void Update() {

    }
}