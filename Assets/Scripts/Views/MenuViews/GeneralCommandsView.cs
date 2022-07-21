using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GeneralCommandsView : MonoBehaviour {
    // Start is called before the first frame update
    public Button farmDefineButton, cutWoodButton, harvestPlantButton, killAnimalsButton;
    public ControllerManager controller;

    public RectTransform contentRect;
    public Transform commandParent;

    private void OnEnable() {
        GeneralFunctions.SetContentHeight(contentRect, FindSize(), null);
    }

    private void Start() {
        InitialiseButtons();
    }
    public void InitialiseButtons() {
        farmDefineButton.onClick.AddListener(controller.farmingController.BeginFarmSelection);
        cutWoodButton.onClick.AddListener(delegate { controller.farmingController.BeginFloraSelection(new FloraData.category[] { FloraData.category.Tree }); });
        harvestPlantButton.onClick.AddListener(delegate { controller.farmingController.BeginFloraSelection(new FloraData.category[] { FloraData.category.Bush, FloraData.category.Herb }); });
        killAnimalsButton.onClick.AddListener(delegate {
            controller.nPCController.BeginNPCSelection(2);
        });
        List<Button> buttons = new List<Button>() { farmDefineButton, cutWoodButton, harvestPlantButton, killAnimalsButton };

        foreach (Button command in buttons) {
            TextMeshProUGUI text = command.gameObject.GetComponentInChildren<TextMeshProUGUI>();
            SettingsFunctions.TranslateTMPItems(controller.settingsController, text);
        }
    }

    private float FindSize() {
        float height = 0;
        foreach (Transform transform in commandParent.transform) {
            height += transform.gameObject.GetComponent<RectTransform>().rect.height;
        }
        return height;
    }
}