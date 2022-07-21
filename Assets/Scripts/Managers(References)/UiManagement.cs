using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class UiManagement : MonoBehaviour {
    public GameObject[] menuTooltips;

    public Canvas[] canvases;
    public ConfirmationView confirmationView;
    public GameObject[] dialogues;
    private List<GraphicRaycaster> graphicRaycasters;
    private EventSystem eventSystem;
    public TextMeshProUGUI warningLog;
    public ObjectSelectView objectSelectView;
    private float warningTimer, warningDuration;
    public bool dialoguesAllowed, sideMenuAllowed, pawnScrollAllowed, closingAllowed;
    public bool timeHalted, controlsHalted;
    public TradingDialogueView tradingDialogueView;
    public PawnListDisplay pawnListDisplay;
    public IGMView iGMView;
    public BuildingDialogueView buildingDialogueView;
    public WarningLogView warningLogView;
    public WorldTextView worldTextView;
    public bool closeAllOnLoad;

    public ScrollView scrollView;

    private Queue<ConfirmationItem> confirmationQueue = new Queue<ConfirmationItem>();
    public void TriggerGameControlStoppage(int haltControls = -1, int timeHalt = -1) {
        if (haltControls != -1) {
            controlsHalted = (haltControls == 1 ? true : false);
            if (controlsHalted) EventController.TriggerEvent("stopControls");
            else EventController.TriggerEvent("startControls");
        }
        if (timeHalt != -1) {
            timeHalted = (timeHalt == 1 ? true : false);
            if (timeHalted) EventController.TriggerEvent("stopTimer");
            else EventController.TriggerEvent("startTimer");
        }

        //Debug.Log("UIMAN - AttemptedControl: " + haltControls + " Controls halted: " + controlsHalted + ", Attempted time: " + timeHalt + " Time halted: " + timeHalted);
    }

    private void FixedUpdate() {
        if (confirmationQueue.Count > 0) {
            ConfirmationItem confirmation = confirmationQueue.Peek();
            if (ActivateConfirmationDialogue(confirmation)) {
                confirmationQueue.Dequeue();
            }
        }
    }

    private void Start() {
        graphicRaycasters = new List<GraphicRaycaster>();
        foreach (Canvas canvas in canvases) {
            graphicRaycasters.Add(canvas.GetComponent<GraphicRaycaster>());
        }
        eventSystem = graphicRaycasters[0].GetComponent<EventSystem>();
        LockUIElements(0, 0, 0, 0);
        if (closeAllOnLoad) ManageOpenDialogues(false);
        foreach (GameObject game in menuTooltips) game.SetActive(false);
        //EventController.StartListening("lockDialogues", delegate { LockUIElements(true, false, false); });
        //EventController.StartListening("allowDialogues", delegate { LockUIElements(false, false, false); });
    }

    public void LockUIElements(int dialogueLock, int sideMenuLock, int pawnScrollLock, int closeLock) {
        if (dialogueLock != -1) dialoguesAllowed = dialogueLock > 0 ? false : true;
        if (sideMenuLock != -1) sideMenuAllowed = sideMenuLock > 0 ? false : true;
        if (pawnScrollLock != -1) pawnScrollAllowed = pawnScrollLock > 0 ? false : true;
        if (closeLock != -1) closingAllowed = closeLock > 0 ? false : true;
    }

    public void ManageCanvases(int activeIndex) {
        foreach (Canvas canvas in canvases) canvas.gameObject.SetActive(false);
        canvases[activeIndex].gameObject.SetActive(true);
        if (activeIndex == 1) ManageOpenDialogues(true, 0);
    }

    public void ManageOpenDialogues(bool keepOpen, int index = -1) {
        //Debug.Log(activeCount);
        Debug.Log("UIMAN - Attempting to open index of " + index + " with keep open of: " + keepOpen);
        if (DialogueCount(dialogues) > 0) {
            //Debug.Log("Passed Active");
            foreach (GameObject dialogue in dialogues) {
                dialogue.SetActive(false);
            }

            if (keepOpen && dialoguesAllowed && index != -1) {
                dialogues[index].SetActive(true);
            }
        } else if (index != -1 && dialoguesAllowed) {
            dialogues[index].SetActive(true);
        }
    }

    public int ActiveDialogueIndexReturn() {
        foreach (GameObject dialogue in dialogues) {
            if (dialogue.activeSelf) return System.Array.IndexOf(dialogues, dialogue);
        }
        return -1;
    }

    public bool[] ActiveDialogues() {
        bool[] returnBools = new bool[dialogues.Length];
        for (int i = 0; i < returnBools.Length; i++) {
            if (dialogues[i].activeSelf) returnBools[i] = true;
            else returnBools[i] = false;
        }
        return returnBools;
    }

    public GameObject DialogueLookup(int id) {
        if (dialogues.Length > id) return dialogues[id];
        else return null;
    }

    public int DialogueCount(GameObject[] dialogues) {
        int activeCount = 0;
        foreach (GameObject dialogue in dialogues) {
            if (dialogue.activeSelf) activeCount++;
        }
        return activeCount;
    }

    public void ForceCloseTooltip(int index) {
        GameObject tooltip = menuTooltips[index];
        tooltip.SetActive(false);
    }

    public bool ActivateConfirmationDialogue(string identifier, string confirmationTextVariable, UnityAction completeActions, UnityAction cancelActions, List<RequiredResources> reqs = null, bool useBaseMessage = true, bool overwriteCurrent = false, string[] stringParams = null) {
        ConfirmationItem confirm = new ConfirmationItem(identifier, confirmationTextVariable, completeActions, cancelActions, reqs, useBaseMessage, stringParams);
        return ActivateConfirmationDialogue(confirm, overwriteCurrent);
    }

    private bool ActivateConfirmationDialogue(ConfirmationItem confirmationItem, bool overwriteCurrent = false) {
        if (confirmationView.gameObject.activeSelf && !overwriteCurrent) {
            ConfirmationItem conf = System.Array.Find(confirmationQueue.ToArray(), x => x.identifier == confirmationItem.identifier);
            Debug.Log("UIMAN - Conf is " + confirmationItem.identifier + ", compared to: " + confirmationItem.identifier);
            if (System.Array.Find(confirmationQueue.ToArray(), x => x.identifier == confirmationItem.identifier) == null) {
                if (confirmationView.currentItem.identifier != confirmationItem.identifier) confirmationQueue.Enqueue(confirmationItem);
            }
            return false;
        } else {
            confirmationView.gameObject.SetActive(true);
            confirmationView.SetConfirmContent(confirmationItem);
            closingAllowed = false;
            return true;
        }
    }
    public int CanvasRaycastCheck(int[] ids = null) {
        PointerEventData pointerEventData = new PointerEventData(eventSystem);
        //Set the Pointer Event Position to that of the mouse position
        pointerEventData.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        if (ids == null) {
            foreach (GraphicRaycaster raycaster in graphicRaycasters) {
                raycaster.Raycast(pointerEventData, results);
            }
        } else {
            foreach (int id in ids) {
                graphicRaycasters[id].Raycast(pointerEventData, results);
            }
        }
        return results.Count;
    }

}