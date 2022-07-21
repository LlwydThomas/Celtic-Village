using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class IGMView : MonoBehaviour {
    // Start is called before the first frame update
    Dictionary<string, string> strings;
    public GameObject[] gameObjects;
    public Button[] navigationButtons;
    public Transform floraParent;
    public GameObject floraItemPrefab, resourceItemPrefab;
    public GameObject[] helpPanels;
    public GameObject baseNavigation, helpScreen;
    public TextMeshProUGUI[] translateTexts;
    public UiManagement uiManagement;
    public ControllerManager controllerManager;
    void Start() {
        FormatMenu();
    }

    private void OnEnable() {
        uiManagement.TriggerGameControlStoppage(haltControls: 1, timeHalt: 1);
        uiManagement.LockUIElements(1, 1, 1, 0);
        foreach (Transform child in this.transform) {
            if (child.GetSiblingIndex() == 0) child.gameObject.SetActive(true);
            else child.gameObject.SetActive(false);
        }

        FormatHelpPanels();
    }
    private void OnDisable() {
        uiManagement.LockUIElements(0, 0, 0, -1);
        uiManagement.TriggerGameControlStoppage(haltControls: 0, timeHalt: 0);
    }
    void FormatMenu() {
        foreach (GameObject n in gameObjects) {
            //Debug.Log(n.name.Remove(n.name.LastIndexOf("Button")));
            string buttonText = controllerManager.settingsController.TranslateString(n.name.Remove(n.name.LastIndexOf("Button")));
            n.GetComponentInChildren<TextMeshProUGUI>().SetText(buttonText);
        }
        foreach (Button button in navigationButtons) {
            TextMeshProUGUI buttonText = button.gameObject.GetComponentInChildren<TextMeshProUGUI>();
            buttonText.SetText(controllerManager.settingsController.TranslateString(buttonText.text));
        }

        foreach (TextMeshProUGUI text in translateTexts) {
            text.SetText(controllerManager.settingsController.TranslateString(text.text));
        }

        //gameObjects[0].GetComponent<Button>().onClick = controllerManager
    }

    public void SwitchHelpTab(int index) {
        if (!helpScreen.activeSelf) {
            helpScreen.SetActive(true);
            baseNavigation.SetActive(false);
        }
        foreach (GameObject panel in helpPanels) {
            panel.SetActive(false);
        }
        helpPanels[index].SetActive(true);
    }

    private void FormatHelpPanels() {
        foreach (Transform t in floraParent) {
            Destroy(t.gameObject);
        }
        SettingsController set = controllerManager.settingsController;
        List<ResourceData> resources = controllerManager.resourceController.resourceDataList.ResourceDatas;
        List<FloraData> floras = controllerManager.natureController.floraDataList;
        floraParent.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, floras.Count * (150 + 10) + 20);

        foreach (FloraData flora in floras) {
            GameObject newFlora = Instantiate(floraItemPrefab, this.transform.position, Quaternion.identity, floraParent);
            FloraItemReferences floraItemReferences = newFlora.GetComponent<FloraItemReferences>();
            SettingsFunctions.TranslateTMPItems(set, floraItemReferences.labels);
            floraItemReferences.floraIcon.sprite = flora.icon != null ? flora.icon : flora.maturePrefab.GetComponentInChildren<SpriteRenderer>().sprite;
            floraItemReferences.floraName.SetText(set.TranslateString(flora.uniqueType));
            floraItemReferences.floraType.SetText(set.TranslateString(flora.floraCategory.ToString()));
            floraItemReferences.growthTime.SetText(flora.daysToMature.ToString() + " " + set.TranslateString("Days"));
            if (flora.outputResources.Count > 0) {
                RequiredResources floraOutput = flora.outputResources[0].requiredResource;
                floraItemReferences.outputResource.SetText(floraOutput.count + "x " + set.TranslateString(floraOutput.resource.resourceName));
            }
            int[] seasons = NatureFunctions.DetermineFloraGrowthSeasons(flora);
            string[] seasonsText = DetermineTextForSeasons(seasons);
            if (seasonsText.Length > 1) {
                if (seasonsText[0] == seasonsText[1]) floraItemReferences.growthSeasons.SetText(seasonsText[0]);
                else floraItemReferences.growthSeasons.SetText(seasonsText[0] + " - " + seasonsText[1]);
            } else floraItemReferences.growthSeasons.SetText(seasonsText[0]);

            // Format Flora Info Display
        }
        /* foreach (ResourceData res in resources) {
            GameObject newRes = Instantiate(resourceItemPrefab, this.transform.position, Quaternion.identity, floraParent);
            string resName = controllerManager.settingsController.TranslateString(res.resourceName);
            // Format Resource Info Display
        } */
    }

    private string[] DetermineTextForSeasons(int[] seasons) {
        string[] strings = new string[2];
        if (seasons.Length == 1) return new string[] { controllerManager.settingsController.TranslateString("YearRound") };
        bool yearRound = false;
        for (int i = 0; i < seasons.Length; i++) {
            string addition = "";
            switch (seasons[i]) {
                case -1:
                    yearRound = true;
                    break;
                case 0:
                    addition = "Winter";
                    yearRound = false;
                    break;
                case 1:
                    addition = "Spring";
                    yearRound = false;
                    break;
                case 2:
                    addition = "Summer";
                    yearRound = false;
                    break;
                case 3:
                    addition = "Autumn";
                    yearRound = false;
                    break;
            }
            strings[i] = controllerManager.settingsController.TranslateString(addition);
        }
        if (yearRound) return new string[] {
            controllerManager.settingsController.TranslateString("YearRound")
        };
        return strings;
    }

    // Update is called once per frame
    void Update() {

    }
}