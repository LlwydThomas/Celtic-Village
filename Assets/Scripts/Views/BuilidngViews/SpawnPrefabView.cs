using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
public class SpawnPrefabView : MonoBehaviour {

    private int selectedObjectInArray;
    private GameObject currentlySelectedObject;
    public GameObject loadingParent;
    private StructureData[] selectableObjects;
    public LayerMask buildingLayerMask, stageLayerMask, buildingLayerNoWater;
    public static bool isAnObjectSelected = false;
    private StructureData currentStructure = null;
    private GhostReferences currentGhostReferences;
    private int rotationLoc;
    Quaternion rotatePass;
    private ControllerManager controller;
    public ManagerReferences managerReferences;
    public Tilemap topMap;
    private Camera cam;
    private SpriteRenderer currentObjectSprite;
    private bool bypassRequired = false;
    // Use this for initialization
    void Start() {
        topMap = GameObject.Find("TopMap").GetComponent<Tilemap>();
        controller = managerReferences.controllerManager;
        selectableObjects = controller.buildingController.buildingModel.structureDatas.ToArray();
        rotationLoc = 1;
        cam = Camera.main;
    }

    // Update is called once per frame

    void FixedUpdate() {
        if (isAnObjectSelected && currentlySelectedObject != null) {
            // Determine the users mouse position, and convert this to a TileMap cell position.
            Vector2 structOffset = currentStructure.offset;
            Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector3 cellPosition = new Vector3(Mathf.FloorToInt(mousePos.x) + structOffset.x, Mathf.FloorToInt(mousePos.y) + structOffset.y, 1);
            Vector3 centre = currentObjectSprite.bounds.center;
            // Transform the ghost building's GameObject to this location.
            Color prefabColour;
            currentlySelectedObject.transform.position = cellPosition;
            string colourName;
            if (CheckPlacement(centre, currentlySelectedObject, currentStructure)) {
                prefabColour = GeneralEnumStorage.greenGhost;
                colourName = "Green";
            } else {
                prefabColour = GeneralEnumStorage.redGhost;
                colourName = "Red";
            }
            currentGhostReferences.colouredSprite.color = prefabColour;
            Debug.Log("SPV - Checking the placement and amending the colour to " + colourName);

            if (Input.GetMouseButton(1)) {
                ToggleSelection(false);
            }

            if (Input.GetMouseButtonDown(0)) {
                // Determine if the current placement is colliding with environmental objects/other buildings.
                // If the location is suitable for building
                if (CheckPlacement(centre, currentlySelectedObject, currentStructure)) {
                    List<RequiredResources> newList = GeneralFunctions.CopyResourceList(currentStructure.requiredRes);
                    if (controller.storageController.FindAndExtractResourcesFromStorage(newList) || bypassRequired) {
                        GameObject loadBuild = Instantiate(currentStructure.loadingPrefab, currentlySelectedObject.transform.position, currentlySelectedObject.transform.rotation, loadingParent.transform);
                        BeginLoading(loadBuild, currentStructure);
                        if (Input.GetKey(KeyCode.LeftShift)) {
                            BuildPrefab(currentStructure.ID, bypassRequired);
                        } else ToggleSelection(false);
                    }
                    // Instantiate the loading prefab for the building with the same properties as the ghost prefab.
                }
            }
        }
    }

    private bool CheckPlacement(Vector3 centre, GameObject currentSelected, StructureData currentStructure) {
        Vector3 centreVector = cam.WorldToScreenPoint(centre);
        Ray ray = cam.ScreenPointToRay(centreVector);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, stageLayerMask);
        LayerMask relevantMask;
        if (currentStructure.requiredTiles.Length > 0) relevantMask = buildingLayerNoWater;
        else relevantMask = buildingLayerMask;
        if (hit) {
            if (hit.transform.CompareTag("BuildStage")) {
                float radius = currentObjectSprite.bounds.size.x / 2f;
                Debug.Log("SPV - Building Radius: " + radius);
                if (!Physics2D.OverlapCircle(centre, radius, relevantMask)) {
                    //Debug.Log(loadingPrefab.name);
                    //Debug.Log(loadParent.name);
                    if (currentStructure.requiredTiles.Length > 0) {
                        currentlySelectedObject.transform.rotation = Quaternion.identity;
                        //List<Node> nodes = controller.gridController.CheckGameObjectNodes(currentGhostReferences.waterCollider);
                        List<Node> baseNodes = controller.gridController.CheckGameObjectNodes(currentGhostReferences.baseCollider);
                        //List<Node> remainingNodes = baseNodes.FindAll(x => !nodes.Contains(x));
                        Debug.Log("SPV - A total of " + baseNodes.Count + " nodes found for structure " + currentStructure.structureName);

                        foreach (RequiredTiles required in currentStructure.requiredTiles) {
                            int remaining = required.count;
                            foreach (Node node in baseNodes) {
                                if (required.allowedTiles.Contains(node.associatedTile)) remaining -= 1;
                            }
                            if (remaining > 0) {
                                Debug.Log("SPV - Reason for failure: Tile Count not met");
                                return false;
                            }
                        }
                    }
                    return true;
                } else {
                    Debug.Log("SPV - Reason for failure: Placement not clear");
                    return false;
                }
            } else return false;
        } else return false;
    }

    public void BeginLoading(GameObject loadingObject, StructureData structure) {
        Transform environment = controller.buildingController.buildingParent.transform;
        Vector3 position = loadingObject.transform.position;
        // Initialise variables
        UnityAction onCompleteActions = null;
        onCompleteActions += (() => HandleLoadingCompletion(loadingObject, environment, structure));
        // Create the loading bar for the building, which on completion will instantiate the final building.
        controller.loadingBarController.GenerateLoadingBar(position, onCompleteActions, 1f, loadingObject);
    }

    public void HandleLoadingCompletion(GameObject loading, Transform buildParent, StructureData structure) {
        // On completion of the build instantiate the final prefab, and register the build with the building controller.
        BuildingController buildingController = controller.buildingController;
        GameObject completed = Instantiate(structure.completedStructure, loading.transform.position, this.transform.rotation, buildParent) as GameObject;
        int id = structure.possiblePurposes[0].ID;
        buildingController.InitialiseNewBuild(completed, structure.ID, id);
        Destroy(loading);
    }

    private void ToggleSelection(bool on) {
        if (!on) {
            Destroy(currentlySelectedObject.gameObject);
            currentlySelectedObject = null;
            currentStructure = null;
        }
        isAnObjectSelected = on;
    }
    public void BuildPrefab(int structureIndex, bool _bypassRequired) {
        bypassRequired = _bypassRequired && GeneralEnumStorage.debugActive;
        // Toggle roofs on, to ensure they don't desync.
        if (this.transform.childCount == 0) {
            currentStructure = controller.buildingController.StructureDataLookUp(structureIndex);
            Vector3 mousePosition = Input.mousePosition.normalized;
            Vector3 tiledPosition = topMap.GetCellCenterLocal(new Vector3Int(Mathf.FloorToInt(mousePosition.x), Mathf.FloorToInt(mousePosition.y), Mathf.FloorToInt(mousePosition.z)));
            if (StorageFunctions.CheckIfResourcesAvailable(currentStructure.requiredRes, controller.storageController.CompileTotalResourceList(reservedTotal: true, stationary: -1)) || bypassRequired) {
                // Determine where to instantiate the prefab.
                isAnObjectSelected = true;
                // Instantiate at given point, to begin the placement script
                currentlySelectedObject = (GameObject) Instantiate(currentStructure.ghostPrefab, tiledPosition, Quaternion.identity, this.transform);
                currentGhostReferences = currentlySelectedObject.GetComponent<GhostReferences>();
                currentObjectSprite = currentGhostReferences.colouredSprite;
            } else {
                managerReferences.uiManagement.warningLogView.AppendMessageToLog("LackingResourcesForBuild", Vector3.zero, 50);
            }
        }
    }
}