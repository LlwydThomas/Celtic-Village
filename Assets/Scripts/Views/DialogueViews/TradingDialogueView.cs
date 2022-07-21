using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class TradingDialogueView : MonoBehaviour {
    public ControllerManager controllerManager;
    public ManagerReferences managerReferences;
    public GameObject tradeItemParent, itemKey, tradeItem, taxationField, weightField;
    public Button confirmButton, cancelButton;
    public TMP_InputField tradeBalanceField, tradeWeightField;
    private bool taxation;
    public TextMeshProUGUI itemNameKey, saleValueKey, buyValueKey, youKey, QuantityKey, TraderKey, balanceText, weightText;
    private int totalTradeValue, tradeValueOffset;
    private float totalTradeWeight, currentWeightLimit, currentWeightUsed;
    private List<TradingItemView> tradeItemViews;

    private int currentTradingItemID = -1;

    // Update is called once per frame
    private void OnEnable() {
        EventController.StartListening("tradingQuantityChanged", RecalculateTradeValue);
        tradeItemViews = new List<TradingItemView>();
        //totalTradeValue = 0;
        FormatLanguage();
        FormatButtons();
    }
    private void OnDisable() {
        managerReferences.uiManagement.TriggerGameControlStoppage(haltControls: 0, timeHalt: 0);
        EventController.StopListening("tradingQuantityChanged", RecalculateTradeValue);
        cancelButton.onClick.RemoveAllListeners();
        confirmButton.onClick.RemoveAllListeners();
    }

    private void BeginOneWayTrade(int valueRequired) {

    }

    private void FormatButtons() {
        cancelButton.onClick.AddListener(delegate { ConfirmTransaction(false); });
        confirmButton.onClick.AddListener(delegate { ConfirmTransaction(true); });

    }

    private void FixedUpdate() {
        if (currentTradingItemID != -1) {
            if (Input.GetKeyDown(KeyCode.Tab)) TabToNextTradingItem(currentTradingItemID);
        }
    }
    public void BeginTradingDialogue(List<InstantiatedResource> tradingInventory, List<InstantiatedResource> personalInventory, float buyModifier, float sellModifier, int balanceOffset = 0) {
        ClearTradingScreen();
        managerReferences.uiManagement.TriggerGameControlStoppage(haltControls: 1, timeHalt: 1);
        taxation = balanceOffset != 0 ? true : false;
        tradeValueOffset = balanceOffset;
        int id = 0;
        if (tradingInventory != null) {
            foreach (InstantiatedResource resource in tradingInventory) {
                if (resource.resourceData.tradeable) {
                    GameObject newItem = GameObject.Instantiate(tradeItem, tradeItemParent.transform);
                    TradingItemView itemView = newItem.GetComponent<TradingItemView>();
                    tradeItemViews.Add(itemView);
                    if (personalInventory.FindIndex(x => x.resourceData == resource.resourceData) != -1) {
                        InstantiatedResource personalResource = personalInventory.Find(x => x.resourceData == resource.resourceData);
                        itemView.InitialiseItemValue(id, resource.resourceData, buyModifier, sellModifier, resource.count, personalResource.count, managerReferences);
                        personalInventory.Remove(personalInventory.Find(x => x.resourceData == resource.resourceData));
                    } else {
                        itemView.InitialiseItemValue(id, resource.resourceData, buyModifier, sellModifier, resource.count, 0, managerReferences);
                    }
                    TMP_InputField quantity = itemView.tradeQuantity;
                    quantity.onSelect.AddListener(delegate {
                        itemView.tradeQuantity.SetTextWithoutNotify("");
                        currentTradingItemID = itemView.id;
                        itemView.tradeQuantity.onValueChanged.Invoke(itemView.tradeQuantity.text);
                    });
                    quantity.onDeselect.AddListener(delegate {
                        if (quantity.text == "") itemView.tradeQuantity.SetTextWithoutNotify("0");
                        currentTradingItemID = -1;
                    });
                    id += 1;
                }
            }
        }
        foreach (InstantiatedResource resource in personalInventory) {
            if (resource.resourceData.tradeable) {
                GameObject newItem = GameObject.Instantiate(tradeItem, tradeItemParent.transform);
                TradingItemView itemView = newItem.GetComponent<TradingItemView>();
                tradeItemViews.Add(itemView);
                itemView.InitialiseItemValue(id, resource.resourceData, buyModifier, sellModifier, 0, resource.count, managerReferences);
                TMP_InputField quantity = itemView.tradeQuantity;
                quantity.onSelect.AddListener(delegate {
                    itemView.tradeQuantity.SetTextWithoutNotify("");
                    currentTradingItemID = itemView.id;
                    itemView.tradeQuantity.onValueChanged.Invoke(itemView.tradeQuantity.text);
                });
                quantity.onDeselect.AddListener(delegate {
                    if (quantity.text == "") itemView.tradeQuantity.SetTextWithoutNotify("0");
                    currentTradingItemID = -1;
                });
                id += 1;
            }
        }

        if (tradeItemViews.Count > 0) {
            tradeItemViews[0].tradeQuantity.Select();
        }

        currentWeightLimit = controllerManager.storageController.ReturnTotalStorageCapacity(1);
        currentWeightUsed = controllerManager.storageController.ReturnStorageUsed();
        float size = tradeItemViews.Count * 100f;
        ToggleWeightDisplay(tradingInventory == null);
        GeneralFunctions.SetContentHeight(tradeItemParent.GetComponent<RectTransform>(), size, null);
        RecalculateTradeValue();
    }

    private void ToggleWeightDisplay(bool taxation) {
        taxationField.SetActive(taxation);
        weightField.SetActive(!taxation);
    }

    private void TabToNextTradingItem(int currentID) {
        Debug.Log("TDV - CurrentID " + currentID + " next item id: " + (currentID + 1) + " of total: " + tradeItemViews.Count);
        TradingItemView trading;
        if (tradeItemViews.Count > currentID + 1) trading = tradeItemViews[currentID + 1];
        else trading = tradeItemViews[0];
        trading.tradeQuantity.Select();
        currentTradingItemID = trading.id;
    }

    public void RecalculateTradeValue() {
        totalTradeWeight = 0;
        totalTradeValue = -tradeValueOffset;
        float usedWeight = currentWeightUsed;
        Debug.Log("TDV - Calculating total for " + tradeItemViews.Count + " trade views.");
        foreach (TradingItemView itemView in tradeItemViews) {
            int tradeQuantity;
            if (!int.TryParse(itemView.tradeQuantity.text, out tradeQuantity)) {
                tradeQuantity = 0;
            }
            int specificValue;

            if (tradeQuantity >= 0) {
                specificValue = tradeQuantity * itemView.buyPrice;
                totalTradeValue -= specificValue;
                totalTradeWeight += itemView.currentResource.weightPerItem * tradeQuantity;
            } else {
                specificValue = tradeQuantity * itemView.sellPrice;
                totalTradeValue -= specificValue;
                totalTradeWeight += itemView.currentResource.weightPerItem * tradeQuantity;
            }
        }
        usedWeight = currentWeightUsed + totalTradeWeight;
        if (totalTradeValue < 0 || usedWeight > currentWeightLimit) {
            confirmButton.interactable = false;
        } else {
            confirmButton.interactable = true;
        }

        tradeBalanceField.SetTextWithoutNotify(totalTradeValue.ToString());
        tradeWeightField.SetTextWithoutNotify(usedWeight + " / " + currentWeightLimit);
    }

    public void ConfirmTransaction(bool accept) {
        if (accept) {
            if ((totalTradeWeight + currentWeightUsed) <= currentWeightLimit && totalTradeValue >= 0) {
                // Confirm trade and inform the storage controller of the change.
                List<RequiredResources> outgoingResources = new List<RequiredResources>();
                List<RequiredResources> incomingResources = new List<RequiredResources>();
                foreach (TradingItemView tradeView in tradeItemViews) {
                    int change = int.Parse(tradeView.tradeQuantity.text);
                    RequiredResources newRes = new RequiredResources(tradeView.currentResource, change);
                    if (newRes.count == 0) continue;
                    if (newRes.count < 0) {
                        newRes.count = -newRes.count;
                        outgoingResources.Add(newRes);
                    } else incomingResources.Add(newRes);
                }
                if (controllerManager.storageController.FindAndExtractResourcesFromStorage(outgoingResources) == true) {
                    controllerManager.storageController.OffloadMultipleResources(incomingResources);
                } else return;
                Debug.Log("TDV - trade resulted in " + outgoingResources.Count + " outgoing resources, " + incomingResources.Count + " incoming resources for a total of " + (incomingResources.Count + outgoingResources.Count));
                if (taxation) {
                    controllerManager.eventQueueController.AmendTaxationBacklog(true);
                }
            }
        } else {
            if (taxation) {
                controllerManager.eventQueueController.AmendTaxationBacklog(false, -totalTradeValue);
            }
        }

        ClearTradingScreen();
        managerReferences.uiManagement.ManageOpenDialogues(false);

    }

    public void FormatLanguage() {
        SettingsController settings = controllerManager.settingsController;
        itemNameKey.SetText(settings.TranslateString("ItemName"));
        saleValueKey.SetText(settings.TranslateString("SaleValue"));
        buyValueKey.SetText(settings.TranslateString("BuyValue"));
        youKey.SetText(settings.TranslateString("You"));
        QuantityKey.SetText(settings.TranslateString("Quantity"));
        TraderKey.SetText(settings.TranslateString("Trader"));
    }

    public void ClearTradingScreen() {
        foreach (Transform transform in tradeItemParent.transform) {
            GameObject.Destroy(transform.gameObject);
        }
        taxation = false;
        tradeItemViews.Clear();
    }
}