using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class BuildingDebugView : MonoBehaviour {
    // Start is called before the first frame update
    public BuildingDialogueView building;
    public Transform textParent;
    private Build build;
    private void OnEnable() {
        if (building.gameObject.activeSelf) {
            build = building.RequestCurrentBuild();
            DisplayInfo();
        }
    }

    public void DisplayInfo() {
        GameObject.Instantiate(CreateTextBlock("Total Tasks", build.functionHandler.TaskQueueReturn().Count.ToString()), textParent.position, Quaternion.identity, textParent);

    }

    public GameObject CreateTextBlock(string firstText, string secondText) {
        GameObject parent = new GameObject();
        GameObject firstTextItem = new GameObject();
        GameObject secondTextItem = new GameObject();
        firstTextItem.AddComponent<TextMeshProUGUI>().SetText(firstText);
        secondTextItem.AddComponent<TextMeshProUGUI>().SetText(secondText);
        firstTextItem.transform.parent = parent.transform;
        secondTextItem.transform.parent = parent.transform;
        return parent;
    }
}