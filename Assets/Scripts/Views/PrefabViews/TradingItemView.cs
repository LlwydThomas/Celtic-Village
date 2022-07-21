using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class TradingItemView : MonoBehaviour {
    public int id;
    public TextMeshProUGUI itemName, saleValue, buyValue, internalQuantity, externalQuantity;
    public int sellPrice, buyPrice;
    public ResourceData currentResource;
    public Image itemIcon;
    public TMP_InputField tradeQuantity;

    public Button increaseQuantity, decreaseQuantity;

    private int internalCount, externalCount;
    public GameObject toolTipTarget;

    public void InitialiseItemValue(int _id, ResourceData resourceData, float buyModifier, float sellModifier, int _externalQuantity, int _internalQuantity, ManagerReferences managerReferences) {
        currentResource = resourceData;
        internalCount = _internalQuantity;
        externalCount = _externalQuantity;
        itemName.SetText(managerReferences.controllerManager.settingsController.TranslateString(resourceData.resourceName));
        itemIcon.sprite = resourceData.icon;
        buyPrice = Mathf.RoundToInt(resourceData.resourceValue * buyModifier);
        sellPrice = Mathf.RoundToInt(resourceData.resourceValue * sellModifier);
        buyValue.SetText(buyPrice.ToString());
        saleValue.SetText(sellPrice.ToString());
        internalQuantity.SetText(_internalQuantity.ToString());
        externalQuantity.SetText(_externalQuantity.ToString());
        tradeQuantity.SetTextWithoutNotify("0");
        id = _id;
        StorageFunctions.AppendResourceTooltip(managerReferences, resourceData, itemName.transform.parent.gameObject, Vector3.zero);
        InitialiseListeners();

    }
    private void InitialiseListeners() {
        tradeQuantity.onValueChanged.AddListener(delegate { AmendQuantityValues(); });
    }

    private void AmendQuantityValues() {
        int tradeBalance;
        int.TryParse(tradeQuantity.text, out tradeBalance);
        int newExternal = externalCount - tradeBalance;
        int newInternal = internalCount + tradeBalance;
        if (newInternal >= 0 && newExternal >= 0) {
            internalQuantity.SetText(newInternal.ToString());
            externalQuantity.SetText(newExternal.ToString());
        } else {
            tradeQuantity.SetTextWithoutNotify("0");
            internalQuantity.SetText(internalCount.ToString());
            externalQuantity.SetText(externalCount.ToString());
        }
        EventController.TriggerEvent("tradingQuantityChanged");
    }
}