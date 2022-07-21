using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class ToolTipHeaderView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    private UiManagement UIManager;
    private SettingsController settingsController;
    private GameObject tooltip;
    private string header, text;
    private GameObject relevantGameObject;
    private bool tooltipAllowed = true;
    private IEnumerator displayCoroutine;
    private TextMeshProUGUI toolTipText, toolTipHeader;
    private void Start() {
        UIManager = GameObject.Find("UIManager").GetComponent<UiManagement>();
        settingsController = GameObject.Find("Controllers").GetComponent<ControllerManager>().settingsController;
    }

    // Start is called before the first frame update
    public void OnPointerEnter(PointerEventData pointerEventData) {
        //Debug.Log("TTH - ON POINTER ENTER, value of " + toolTipValue);
        // Show tooltip.
        displayCoroutine = DisplayTooltipCoroutine();
        StartCoroutine(displayCoroutine);
    }
    public void OnPointerExit(PointerEventData pointerEventData) {
        // Hide tooltip.
        StopCoroutine(displayCoroutine);
        HideToolTip();
    }

    IEnumerator DisplayTooltipCoroutine() {
        yield return new WaitForSeconds(0.5f);
        DisplayTooltip(header, text);
    }

    public void SetTooltipData(string _header, string _text, int toolTipNumber, GameObject _relevantGameObject) {
        relevantGameObject = _relevantGameObject;
        if (UIManager == null) UIManager = GameObject.Find("UIManager").GetComponent<UiManagement>();
        header = _header;
        text = _text;
        Debug.Log(UIManager);
        tooltip = UIManager.menuTooltips[toolTipNumber];
        toolTipHeader = tooltip.transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>();
        toolTipText = tooltip.transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>();
    }

    private void DisplayTooltip(string header, string text) {
        if (relevantGameObject != null) {
            if (relevantGameObject.activeSelf) return;
        }
        Debug.Log(tooltip);
        tooltip.SetActive(true);
        toolTipText.SetText(text);
        toolTipHeader.SetText(header);
    }

    private void HideToolTip() {
        tooltip.SetActive(false);
    }

}