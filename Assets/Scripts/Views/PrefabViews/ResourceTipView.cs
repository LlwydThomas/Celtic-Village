using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class ResourceTipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    private ManagerReferences managerReferences;
    private UiManagement UIManager;
    private SettingsController settingsController;
    private GameObject tooltip;
    private ResourceData toolTipValue;
    private GameObject relevantGameObject;
    private bool tooltipAllowed = true;
    public bool active;
    private Rect screenRect;
    private RectTransform current;
    private IEnumerator displayCoroutine;
    private TextMeshProUGUI toolTipText;

    // Start is called before the first frame update
    public void OnPointerEnter(PointerEventData pointerEventData) {
        // Show tooltip.
        displayCoroutine = DisplayTooltipCoroutine();
        StartCoroutine(displayCoroutine);
    }
    public void OnPointerExit(PointerEventData pointerEventData) {
        // Hide tooltip.
        StopCoroutine(displayCoroutine);
        HideToolTip();
        active = false;
    }

    private void SetReferences(ManagerReferences _managerReferences) {
        managerReferences = _managerReferences;
        if (UIManager == null) UIManager = managerReferences.uiManagement;
        if (settingsController == null) settingsController = managerReferences.controllerManager.settingsController;
    }

    IEnumerator DisplayTooltipCoroutine() {
        yield return new WaitForSeconds(0.5f);
        active = true;
        DisplayResource(toolTipValue);
    }

    public void SetTooltipData(ManagerReferences managerReferences, ResourceData resource, int toolTipNumber, GameObject _relevantGameObject) {
        screenRect = new Rect(0f, 0f, Screen.width, Screen.height);
        relevantGameObject = _relevantGameObject;
        SetReferences(managerReferences);
        toolTipValue = resource;
        Debug.Log(UIManager);
        tooltip = UIManager.menuTooltips[toolTipNumber];
        current = tooltip.GetComponent<RectTransform>();
    }

    private void DisplayResource(ResourceData resource) {
        if (relevantGameObject != null && relevantGameObject.activeSelf) return;
        tooltip.SetActive(true);
        Transform basicInfo = tooltip.transform.GetChild(0);
        basicInfo.GetComponentInChildren<TextMeshProUGUI>().SetText(settingsController.TranslateString(resource.resourceName));
        basicInfo.GetChild(1).GetComponent<Image>().sprite = resource.icon;

        Transform itemStats = tooltip.transform.GetChild(1);
        StorageFunctions.FormatTextWithValue(itemStats.GetChild(0).gameObject, settingsController.TranslateString("Hunger"), resource.hungerRegeneration.ToString(), 28);
        StorageFunctions.FormatTextWithValue(itemStats.GetChild(1).gameObject, settingsController.TranslateString("Weight"), resource.weightPerItem.ToString(), 28);
        StorageFunctions.FormatTextWithValue(itemStats.GetChild(2).gameObject, settingsController.TranslateString("Value"), resource.resourceValue.ToString(), 28);
    }

    private void HideToolTip() {
        tooltip.SetActive(false);
    }

    private void CheckBounds() {
        Vector3[] objectCorners = new Vector3[4];
        current.GetWorldCorners(objectCorners);
        foreach (Vector3 corner in objectCorners) {
            if (!screenRect.Contains(corner)) {
                tooltip.transform.position = new Vector3(0, 0, 0);
            }
        }

    }

}