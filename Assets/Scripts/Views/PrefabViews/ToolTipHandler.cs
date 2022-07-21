using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class ToolTipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    private UiManagement UIManager;
    private SettingsController settingsController;
    private GameObject tooltip;
    private string toolTipValue;
    private GameObject relevantGameObject;
    private bool tooltipAllowed = true;
    private IEnumerator displayCoroutine;
    private TextMeshProUGUI toolTipText;
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
        DisplayTooltip(toolTipValue);
    }

    public void SetTooltipData(string value, int toolTipNumber, GameObject _relevantGameObject) {
        relevantGameObject = _relevantGameObject;
        if (UIManager == null) UIManager = GameObject.Find("UIManager").GetComponent<UiManagement>();
        toolTipValue = value;
        Debug.Log(UIManager);
        tooltip = UIManager.menuTooltips[toolTipNumber];
        toolTipText = tooltip.GetComponentInChildren<TextMeshProUGUI>();
    }

    private void DisplayTooltip(string value) {
        if (relevantGameObject != null) {
            if (relevantGameObject.activeSelf) return;
        }
        Debug.Log(tooltip);
        tooltip.SetActive(true);
        toolTipText.SetText(value);
    }

    private void HideToolTip() {
        tooltip.SetActive(false);
    }

}