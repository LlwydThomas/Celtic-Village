using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class HelpScreenView : MonoBehaviour {
    // Start is called before the first frame update
    public TextMeshProUGUI[] translateables;
    public GameObject[] infoPanels;
    public ManagerReferences managerReferences;
    public RectTransform contentRect;
    public ExpansionButtonView[] expansionButtons;
    private void Start() {
        SettingsFunctions.TranslateTMPItems(managerReferences.controllerManager.settingsController, translateables);
        FormatButtons();
    }

    private void OnEnable() {
        foreach (GameObject infoPanel in infoPanels) {
            infoPanel.SetActive(false);
        }

        infoPanels[0].SetActive(true);
    }

    private void FormatButtons() {
        foreach (ExpansionButtonView expansion in expansionButtons) {
            expansion.expansionButton.onClick.AddListener(delegate {
                ToggleExpansionCategories(expansion);
            });
            GeneralFunctions.SetExpansionSize(expansion, 1, 50, 90);
        }

        ToggleExpansionCategories();
    }

    private void ToggleExpansionCategories(ExpansionButtonView expansionButtonView = null) {
        if (expansionButtonView != null) {
            expansionButtonView.resultantList.SetActive(!expansionButtonView.resultantList.activeSelf);
            int charCount = expansionButtonView.resultantList.GetComponentInChildren<TextMeshProUGUI>().text.Length;
            float itemSize = charCount > 400 ? 400 : 250;
            GeneralFunctions.ResizeExpansionButton(expansionButtonView, 1, itemSize);
        } else {
            foreach (ExpansionButtonView expansion in expansionButtons) {
                expansion.resultantList.SetActive(false);
            }
        }
        GeneralFunctions.SetContentHeight(contentRect, FindSize(), new RectTransform[0]);
    }

    private float FindSize() {
        float size = 5;
        foreach (ExpansionButtonView expansionButton in expansionButtons) {
            size += expansionButton.rectTransform.sizeDelta.y + 5;
        }
        return size;
    }
}