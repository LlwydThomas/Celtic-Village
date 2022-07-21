using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ObjectSelectView : MonoBehaviour {
    // Start is called before the first frame update
    private GameObject[] dialogues;
    public bool roofhidden, pawnselected;
    public LayerMask layerMaskActive, layerMaskInactive;
    Vector3 tempStart, tempEnd;
    public ControllerManager controllerManager;
    public bool selectionActive;
    public GameObject panel;
    GraphicRaycaster m_Raycaster;
    PointerEventData m_PointerEventData;
    EventSystem m_EventSystem;
    List<GameObject> selectedItems;
    List<Canvas> canvasList;
    private GameObject currentlySelected;
    public UiManagement uiManager;
    void Start() {
        GeneralEnumStorage.entityLayers = layerMaskInactive;
        EventController.StartListening("allowSelection", delegate { SetSelectionActive(true); });
        EventController.StartListening("disallowSelection", delegate { SetSelectionActive(false); });
        //Object references and variable declarations
        GameObject canvas = GameObject.Find("DialogueCanvas");
        //Fetch the Raycaster from the GameObject (the Canvas)
        m_Raycaster = canvas.GetComponent<GraphicRaycaster>();
        //Fetch the Event System from the Scene
        m_EventSystem = canvas.GetComponent<EventSystem>();
        selectedItems = new List<GameObject>();
        selectionActive = true;
        dialogues = uiManager.dialogues;
    }
    public void RoofToggle() {
        //Check if roofs are currently enabled
        List<Build> buildList = controllerManager.buildingController.ReturnBuildList();
        roofhidden = !roofhidden;
        foreach (Build build in buildList) {
            if (build.buildingReferences.roofObject != null) {
                SpriteRenderer roofSprite = build.buildingReferences.roofObjectSprite;
                roofSprite.enabled = roofhidden;
            }
        }
    }
    public List<GameObject> returnCurrent() {
        return selectedItems;
    }

    public void currentSelection(GameObject item) {
        //Apend a GameObject to the selection list
        selectedItems.Add(item);
    }

    public void HandleSelection() {
        int graphicCount = uiManager.CanvasRaycastCheck(new int[] { 0, 1, 2, 3 });
        if (graphicCount == 0) {
            string avoid = null;
            if (currentlySelected != null) avoid = currentlySelected.tag;
            GameObject hitObject = GameObjectSelector(layerMaskInactive, "Pawn", avoid);
            if (hitObject != null && uiManager.dialoguesAllowed) {
                Debug.Log("OSV - hit tag: " + hitObject.tag);
                DisplayFromGameobject(hitObject);
            } else {
                if (uiManager.closingAllowed) {
                    uiManager.ManageOpenDialogues(false);
                }
            }
        }
    }

    private void DisplayFromGameobject(GameObject hitObject) {
        currentlySelected = hitObject;
        // Determine which dialogue to display dependent on the hit object's tag.
        switch (hitObject.tag) {
            case "Pawn":
                pawnselected = true;
                selectedItems.Add(hitObject);
                // Activate the villager information dialogue, and set the villager displayed to the selected villager.
                uiManager.ManageOpenDialogues(true, 1);
                dialogues[1].GetComponent<DisplayPawnView>().setPawn(hitObject);
                break;
            case "BuildingComponent":
                DisplayFromGameobject(hitObject.transform.parent.gameObject);
                break;
            case "CompleteBuilding":
                selectedItems.Add(hitObject);
                // Activate the building information dialogue and set the building displayed to the selected building.
                uiManager.ManageOpenDialogues(true, 0);
                dialogues[0].GetComponent<BuildingDialogueView>().SetBuildingType(controllerManager.buildingController.RequestBuildFromGameObject(hitObject));
                break;
            case "Tree":
            case "Bush":
            case "Herb":
            case "Crop":
                selectedItems.Add(hitObject);
                // Activate the flora information dialogue and set the flora displayed to the selected flora.
                uiManager.ManageOpenDialogues(true, 4);
                dialogues[4].GetComponent<ObjectInfoView>().SetFloraItem(hitObject);
                break;
            case "Farm":
                selectedItems.Add(hitObject);
                // Activate the farm information dialogue and set the farm displayed to the selected farm.
                uiManager.ManageOpenDialogues(true, 6);
                dialogues[6].GetComponent<FarmingDialogueView>().SetFarmDisplay(controllerManager.farmingController.GameObjectToFarm(hitObject));
                break;
            case "NPC":
                selectedItems.Add(hitObject);
                // Compile an inventory for the NPC and activate the trading dialogue with their inventory.
                NPC npc = controllerManager.nPCController.FindNPCByGameObject(hitObject);
                switch (npc.nPCTypeID) {
                    case 1:
                        uiManager.ManageOpenDialogues(true, 5);
                        NPC.HumanNPC human = npc as NPC.HumanNPC;
                        List<InstantiatedResource> personal = controllerManager.storageController.CompileTotalResourceList();
                        dialogues[5].GetComponent<TradingDialogueView>().BeginTradingDialogue(human.storageContainer.inventory, personal, human.buyModifier, human.sellModifier);
                        break;
                    case 2:
                        NPC.AnimalNPC animal = npc as NPC.AnimalNPC;
                        selectedItems.Add(hitObject);
                        // Activate the flora information dialogue and set the flora displayed to the selected flora.
                        uiManager.ManageOpenDialogues(true, 4);
                        dialogues[4].GetComponent<ObjectInfoView>().SetAnimal(animal);
                        break;
                }
                break;
        }
    }

    public GameObject GameObjectSelector(LayerMask layerMask, string preferredTag = null, string avoidTag = null) {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D[] hits = Physics2D.RaycastAll(ray.origin, ray.direction, Mathf.Infinity, layerMask);
        string debugText = "OSV - Total Hits:";
        foreach (RaycastHit2D hit in hits) debugText += " " + hit.transform.tag + " ";
        if (hits.Length == 0) return null;
        Debug.Log(debugText);
        RaycastHit2D finalHit = hits[0];
        if (hits.Length >= 1) {
            foreach (RaycastHit2D hit1 in hits) {
                if (preferredTag != null) {
                    if (hit1.transform.gameObject.CompareTag(preferredTag)) {
                        finalHit = hit1;
                        break;
                    }
                }
                if (avoidTag != null) {
                    Debug.Log("OSV - Avoid Tag: " + avoidTag);
                    foreach (RaycastHit2D hit2 in hits) {
                        if (!hit2.transform.gameObject.CompareTag(avoidTag)) {
                            finalHit = hit2;
                            break;
                        }
                    }
                }
            }
        }
        Debug.Log("OSV - Total hits: " + hits.Length);
        if (finalHit) return finalHit.transform.gameObject;
        else return null;
    }

    public int DialogueCount() {
        int count = 0;
        foreach (GameObject gameObject in dialogues) {
            if (gameObject.activeSelf) count++;
        }
        return count;
    }

    public void SetSelectionActive(bool active) {
        selectionActive = active;
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown("x")) {
            uiManager.ManageOpenDialogues(false);
        }
        if (Input.GetMouseButtonDown(0)) {
            HandleSelection();
        }
    }
}