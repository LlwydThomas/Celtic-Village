using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class ConfirmationView : MonoBehaviour {
    public ManagerReferences managerReferences;
    private ControllerManager controllerManager;
    private UiManagement uiManagement;
    public Button confirmButton, cancelButton;
    private bool requiresResources;
    private List<RequiredResources> requiredResources = new List<RequiredResources>();
    public InventoryDisplayView inventoryDisplayView;
    public TextMeshProUGUI confirmText, cancelText, confirmationInfo;
    public UnityAction onCompleteActions;
    public UnityAction onCancelActions;
    public GameObject requiredItemsPanel, requiredItemsContent, itemListPrefab;
    private RectTransform rectTransform;
    public ConfirmationItem currentItem;
    // Start is called before the first frame update

    private void Awake() {
        if (controllerManager == null) controllerManager = managerReferences.controllerManager;
        if (uiManagement == null) uiManagement = managerReferences.uiManagement;
        rectTransform = this.GetComponent<RectTransform>();
    }

    private void OnDisable() {
        uiManagement.TriggerGameControlStoppage(haltControls: 0, timeHalt: 0);
        uiManagement.closingAllowed = true;
    }

    public bool SetConfirmContent(ConfirmationItem confirmationItem) {
        currentItem = confirmationItem;
        //Debug.Log("Complete Actions Count: " + completeActions.Count);
        uiManagement.TriggerGameControlStoppage(haltControls: 1, timeHalt: 1);
        onCompleteActions = confirmationItem.completeActions;
        onCancelActions = confirmationItem.cancelActions;
        PopulateRequiredItemList(confirmationItem.resources);
        PrepareText(confirmationItem.confirmationTextVariable, confirmationItem.useBaseMessage, confirmationItem.confirmationTextParameters);
        return true;
    }

    // Allow for external cancelling (if another confirmation is required), whilst not allowing for automatic confirmation.
    public void ForceConfirmationCancel() {
        Complete(true);
    }

    private void FixedUpdate() {
        if (Input.GetKeyDown(KeyCode.Y)) {
            Complete(false);
        }
        if (Input.GetKeyDown(KeyCode.N)) {
            Complete(true);
        }
    }

    private void PopulateRequiredItemList(List<RequiredResources> reqs = null) {
        confirmButton.interactable = true;
        if (reqs != null) {
            if (reqs.Count > 0) {
                requiredItemsPanel.gameObject.SetActive(true);
                string[] labels = new string[2] { "CostName", "AmountText" };
                inventoryDisplayView.PopulateInventoryList(reqs, managerReferences, false, 80f, true, false, labels);
                GeneralFunctions.SetContentHeight(rectTransform, 600, null);
                if (!inventoryDisplayView.requiredItemsMet) confirmButton.interactable = false;
            } else {
                GeneralFunctions.SetContentHeight(rectTransform, 300, null);
                requiredItemsPanel.gameObject.SetActive(false);
            }
            requiredResources = reqs;
        } else {
            GeneralFunctions.SetContentHeight(rectTransform, 300, null);
            requiredItemsPanel.gameObject.SetActive(false);
        }
    }

    private void PrepareButtons() {
        cancelButton.onClick.RemoveAllListeners();
        confirmButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(delegate { Complete(true); });
        confirmButton.onClick.AddListener(delegate { Complete(false); });
    }

    private void PrepareText(string confirmationTextVariable, bool useBaseMessage = true, string[] textParams = null) {
        // Preparation of translated display strings related to the confirmation.
        SettingsController settings = controllerManager.settingsController;
        confirmText.SetText(settings.TranslateString("Confirm") + " (Y)");
        cancelText.SetText(settings.TranslateString("Cancel") + " (N)");
        confirmationTextVariable = settings.TranslateString(confirmationTextVariable);
        if (textParams != null) confirmationTextVariable = string.Format(confirmationTextVariable, textParams);
        string baseInfo = "";
        if (useBaseMessage) baseInfo = settings.TranslateString("BaseConfirmInfo") + " ";
        confirmationInfo.SetText(baseInfo + confirmationTextVariable);
    }

    private void Cancel() {
        this.gameObject.SetActive(false);
    }

    private void OnEnable() {
        PrepareButtons();
    }

    private void Complete(bool cancel) {
        UnityAction invokableActions;
        if (cancel) invokableActions = onCancelActions;
        else invokableActions = onCompleteActions;
        Debug.Log("CNFV - Attemping to complete: " + cancel);
        if (!cancel && requiredResources.Count > 0) {
            Debug.Log("CNFV - Attempting to exctract " + requiredResources.Count + " resources for amend purpose.");
            if (managerReferences.controllerManager.storageController.FindAndExtractResourcesFromStorage(requiredResources, true)) {
                InvokeAction(invokableActions);
            } else InvokeAction(onCancelActions);
        } else {
            InvokeAction(invokableActions);
        }
        this.gameObject.SetActive(false);
        uiManagement.TriggerGameControlStoppage(haltControls: 0, timeHalt: 0);
    }

    private void InvokeAction(UnityAction actions) {
        if (actions == null) return;
        actions.Invoke();
    }
}

public class ConfirmationItem {
    public string confirmationTextVariable;
    public string[] confirmationTextParameters;
    public UnityAction completeActions;
    public UnityAction cancelActions;
    public List<RequiredResources> resources;
    public bool useBaseMessage;
    public string identifier;

    public ConfirmationItem(string _identifier, string _confirmationTextVariable, UnityAction _completeActions, UnityAction _cancelActions, List<RequiredResources> _resources = null, bool _useBaseMessage = true, string[] _confirmationTextParameters = null) {
        identifier = _identifier;
        completeActions = _completeActions;
        cancelActions = _cancelActions;
        confirmationTextParameters = _confirmationTextParameters;
        resources = _resources;
        confirmationTextVariable = _confirmationTextVariable;
        useBaseMessage = _useBaseMessage;
    }
}