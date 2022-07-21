using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
public class RectSelectAlt : MonoBehaviour {
    // Start is called before the first frame update
    GameObject panel;
    private Vector3 tempStart, tempEnd;
    public GameObject canvas, tileMapObj, squareDefined;
    GraphicRaycaster m_Raycaster;
    PointerEventData m_PointerEventData;
    EventSystem m_EventSystem;
    public LayerMask layerMask;
    List<GameObject> selectedItems;
    public Rect rect;
    private ControllerManager controllerManager;
    public ManagerReferences managerReferences;
    private System.Action<GameObject, Rect> completedAction;
    Tilemap tileMap;
    int minX, maxX, minY, maxY;
    private bool requiresWalkable, requiredUnoccupied;
    int maxTiles;
    Tile[] tileList;

    private SpriteRenderer sprite;

    // Update is called once per frame
    private void OnEnable() {
        controllerManager = managerReferences.controllerManager;
        sprite = this.GetComponent<SpriteRenderer>();
        this.transform.localScale = new Vector3(0f, 0f, 0f);
        tileMap = tileMapObj.GetComponent<Tilemap>();
        m_Raycaster = canvas.GetComponent<GraphicRaycaster>();
        //Fetch the Event System from the Scene
        m_EventSystem = canvas.GetComponent<EventSystem>();
        m_PointerEventData = new PointerEventData(m_EventSystem);
    }
    void Update() {
        if (completedAction != null) {
            //Set the Pointer Event Position to that of the mouse position
            m_PointerEventData.position = Input.mousePosition;
            List<RaycastResult> results = new List<RaycastResult>();
            int canvasCheck = managerReferences.uiManagement.CanvasRaycastCheck();
            //Raycast using the Graphics Raycaster and mouse click position
            m_Raycaster.Raycast(m_PointerEventData, results);
            if (canvasCheck == 0) {
                Rect rect = new Rect();
                if (Input.GetMouseButtonDown(0)) {
                    // Retrieve the mouse position for the start of the selection rectangle 
                    tempStart = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1f));
                    // Convert this point to the nearest TileMap Cell center
                    tempStart = tileMap.GetCellCenterWorld(new Vector3Int(Mathf.FloorToInt(tempStart.x), Mathf.FloorToInt(tempStart.y), 1));
                    // Offset this value to get the top left of the tile
                    tempStart = tempStart + new Vector3(-0.5f, 0.5f);
                }

                if (Input.GetMouseButtonDown(1)) {
                    this.gameObject.SetActive(false);
                }

                if (Input.GetMouseButton(0)) {
                    if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape)) this.gameObject.SetActive(false);
                    // Retrieve the mouse position for the end of the selection rectangle
                    tempEnd = Camera.main.ScreenToWorldPoint(new Vector3Int(Mathf.FloorToInt(Input.mousePosition.x), Mathf.FloorToInt(Input.mousePosition.y), 1));
                    // Convert this point to the nearest TileMap Cell center
                    tempEnd = tileMap.GetCellCenterWorld(new Vector3Int(Mathf.FloorToInt(tempEnd.x), Mathf.FloorToInt(tempEnd.y), 1));
                    // Offset this value to get the bottom right of the tile
                    tempEnd = tempEnd + new Vector3(0.5f, -0.5f);
                    // Derive the centre of the rectangle, and scale it according to the distance between the start and end points
                    Vector3 middle = (tempEnd + tempStart) / 2f;
                    float sizeX = Mathf.Abs(tempStart.x - tempEnd.x);
                    float sizeY = Mathf.Abs(tempStart.y - tempEnd.y);
                    this.transform.position = middle;
                    this.transform.localScale = new Vector2(sizeX, sizeY);
                    GetSelectionBounds();
                    rect.xMin = minX;
                    rect.xMax = maxX;
                    rect.yMin = minY;
                    rect.yMax = maxY;
                    if (sizeX * sizeY >= maxTiles || !controllerManager.gridController.QueryRectConditions(rect, requiresWalkable, requiredUnoccupied)) {
                        sprite.color = new Color(0.8f, 0.3f, 0.2f, 0.4f);
                    } else sprite.color = new Color(0.3f, 0.7f, 0.6f, 0.4f);
                }

                if (Input.GetMouseButtonUp(0)) {
                    // Use these two points to deteremine max/min values, for use when ammending tiles
                    GetSelectionBounds();
                    rect.xMin = minX;
                    rect.xMax = maxX;
                    rect.yMin = minY;
                    rect.yMax = maxY;
                    Debug.Log("Min " + minX + ", " + minY + " Max " + maxX + ", " + maxY);

                    if (rect.width * rect.height <= maxTiles) {
                        if (!requiresWalkable || controllerManager.gridController.QueryRectConditions(rect, requiresWalkable, requiredUnoccupied)) {
                            // Instantiate a second rectangle with the same properties
                            var newBox = GameObject.Instantiate(squareDefined, this.transform.position, Quaternion.identity);
                            newBox.transform.localScale = this.transform.localScale;
                            completedAction(newBox, rect);
                            completedAction = null;
                        }
                    }
                    this.gameObject.SetActive(false);
                }
            }
        }
    }
    private void OnDisable() {
        completedAction = null;
    }
    public void SetAction(System.Action<GameObject, Rect> _completedAction, bool _requiresWalkable, bool _requiresUnoccupied, int _maxTiles = 1000) {
        completedAction = _completedAction;
        requiresWalkable = _requiresWalkable;
        requiredUnoccupied = _requiresUnoccupied;
        maxTiles = _maxTiles;
        Debug.Log("RectSelect - Action Set");
    }
    void GetSelectionBounds() {
        if (tempStart.x < tempEnd.x) {
            minX = Mathf.RoundToInt(tempStart.x);
            maxX = Mathf.RoundToInt(tempEnd.x);
        } else {
            maxX = Mathf.RoundToInt(tempStart.x);
            minX = Mathf.RoundToInt(tempEnd.x);
        }

        if (tempStart.y < tempEnd.y) {
            minY = Mathf.RoundToInt(tempStart.y);
            maxY = Mathf.RoundToInt(tempEnd.y);
        } else {
            maxY = Mathf.RoundToInt(tempStart.y);
            minY = Mathf.RoundToInt(tempEnd.y);
        }

        /*for (int x = minX; x < maxX; x++) {
            for (int y = minY; y < maxY; y++) {
                //tileMap.SetTile (new Vector3Int (x, y, 0), null);
                //tileMap.SetColor (new Vector3Int (x, y, 0), Color.red);
                Debug.Log (tileMap.GetColor (new Vector3Int (x, y, 0)));
            }
        }*/
    }

}