using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class ExpansionButtonView : MonoBehaviour {
    // Start is called before the first frame update
    public Button expansionButton, overallButton;
    public TextMeshProUGUI description;
    public int index;
    public GameObject resultantList;
    public Image icon, backgroundImage;
    public Dictionary<int, GameObject> resultantObjectsDict = new Dictionary<int, GameObject>();
    public List<GameObject> resultantItems;
    public RectTransform rectTransform;
    private float itemSize;
    public void FormatExpansionButton(SettingsController settings, UnityAction onclick, string text, int _index, Sprite _icon, float _itemSize = 50f, bool beginExpanded = false, int expectedItems = -1, float maxFont = 40f, string name = "") {
        if (name != "") this.gameObject.name = name;
        description.SetText(text);
        SettingsFunctions.TranslateTMPItems(settings, description);
        expansionButton.onClick.AddListener(delegate {
            ExpandResultantList();
            onclick();
        });
        itemSize = _itemSize;
        index = _index;
        description.fontSizeMax = maxFont;
        if (_icon != null) {
            icon.sprite = _icon;
        } else icon.gameObject.SetActive(false);

        if (beginExpanded) {
            if (expectedItems != -1) GeneralFunctions.SetExpansionSize(this, expectedItems, itemSize);
            resultantList.SetActive(true);
        } else {
            GeneralFunctions.SetExpansionSize(this, 1, itemSize, 90f);
            resultantList.SetActive(false);
        }
    }

    private void ExpandResultantList() {
        bool active = !resultantList.activeSelf;
        resultantList.SetActive(active);
        if (active) {
            GeneralFunctions.SetExpansionSize(this, resultantList.transform.childCount, itemSize);
        } else {
            GeneralFunctions.SetExpansionSize(this, 1, itemSize, 90f);
        }
    }

    public void RemoveItemFromList(int id, GameObject removedItem) {
        resultantObjectsDict.Remove(id);

    }
}