using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class InventoryItemReferences : MonoBehaviour {
    // Start is called before the first frame update
    public GameObject iconObject, itemInfo, deleteButtonObject;
    public Image icon, backgroundImage;
    public TextMeshProUGUI countText, resourceNameText;
    public Button deleteButton;

    public void SetActiveObjects(bool iconActive, bool countActive, bool deleteActive) {
        iconObject.SetActive(iconActive);
        countText.gameObject.SetActive(countActive);
        deleteButtonObject.SetActive(deleteActive);
    }
}