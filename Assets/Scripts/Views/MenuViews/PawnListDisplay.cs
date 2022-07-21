using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class PawnListDisplay : MonoBehaviour {
    // Start is called before the first frame update
    // Update is called once per frame
    public ManagerReferences managerReferences;
    private UiManagement uiManagement;
    private ControllerManager controller;
    private List<Pawn> pawnList = new List<Pawn>();
    public GameObject pawnIconTemplate, pawnIconParent, pawnScrollView, pawnDisplayDialogue;
    private DisplayPawnView pawnDisplay;
    private Dictionary<Pawn, GameObject> findIconByPawn = new Dictionary<Pawn, GameObject>();

    private void Awake() {
        controller = managerReferences.controllerManager;
        uiManagement = managerReferences.uiManagement;
    }
    public void InitialiseDisplayPawnList(List<Pawn> _pawnList) {
        pawnDisplay = uiManagement.dialogues[1].GetComponent<DisplayPawnView>();
        Transform templateTransform = pawnIconTemplate.transform;
        pawnList = _pawnList;
        int count = pawnList.Count;
        pawnScrollView.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 100 * count);
        pawnIconParent.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 100 * count);
        GenerateIcons();
    }

    private void GenerateIcons() {
        findIconByPawn.Clear();
        foreach (Transform child in pawnIconParent.transform) {
            Destroy(child.gameObject);
        }
        foreach (Pawn x in pawnList) {
            // Add the pawn icon for each pawn.
            GameObject pawnIconItem = Instantiate(pawnIconTemplate, pawnIconParent.transform.position, Quaternion.identity, pawnIconParent.transform);
            Transform imageParent = pawnIconItem.transform.GetChild(0);
            for (int i = 0; i < x.pawnController.clothingSprites.Length; i++) {
                Image pawnImage = imageParent.GetChild(i + 1).GetComponent<Image>();
                pawnImage.sprite = x.pawnController.clothingSprites[i].sprite;
                pawnImage.color = x.pawnController.clothingSprites[i].color;
            }
            findIconByPawn.Add(x, pawnIconItem);
            ToolTipHandler tipHandler = pawnIconItem.AddComponent<ToolTipHandler>();
            tipHandler.SetTooltipData(x.name, 0, null);
            pawnIconItem.GetComponent<Button>().onClick.AddListener(delegate { RequestDisplayPawn(x); });
        }
    }

    public void AmendPawnNames() {
        foreach (Pawn pawn in pawnList) {
            if (findIconByPawn.ContainsKey(pawn)) {
                GameObject iconObject = findIconByPawn[pawn];
                ToolTipHandler toolTipHandler = iconObject.GetComponent<ToolTipHandler>();
                toolTipHandler.SetTooltipData(pawn.name, 0, null);
            }
        }
    }

    private void RequestDisplayPawn(Pawn pawn) {
        if (uiManagement.pawnScrollAllowed) {
            uiManagement.scrollView.PanToLocation(pawn.pawnGameObject.transform.position);
            Debug.Log("Passed scroll allowed");
            uiManagement.ManageOpenDialogues(true, 1);
            pawnDisplay.setPawn(pawn.pawnGameObject);
        }
    }

}