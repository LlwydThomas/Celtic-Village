using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class ScrollView : MonoBehaviour {

    public Tilemap centre;
    Vector3 centerPt;
    public float baseScrollSpeed, basePanSpeed, minOrtho, maxOrtho, baseSpeed, dragSpeed;
    private float lengthx, lengthy, speed, targetOrtho;
    public GameObject[] UICanvases;
    List<GraphicRaycaster> uiDetect = new List<GraphicRaycaster>();
    PointerEventData m_PointerEventData;
    EventSystem m_EventSystem;
    public bool movementEnabled = true, automaticScrolling = false;
    private Vector3 targetLocation, dragOrigin;
    private Camera cam;

    [SerializeField]
    private float scrollSpeed, panSpeed;

    public ManagerReferences managerReferences;
    private ControllerManager controllerManager;

    // Start is called before the first frame update
    void Start() {
        cam = Camera.main;
        QualitySettings.vSyncCount = 1;
        AmendSpeeds();
        lengthx = centre.size.x / 2.5f;
        lengthy = centre.size.y / 2;
        centerPt = centre.transform.position;
        targetOrtho = cam.orthographicSize;
        transform.position = centerPt;
        controllerManager = managerReferences.controllerManager;
        foreach (GameObject x in UICanvases) uiDetect.Add(x.GetComponent<GraphicRaycaster>());
        EventController.StartListening("stopControls", delegate { ChangeEnabledState(false); });
        EventController.StartListening("startControls", delegate { ChangeEnabledState(true); });
        EventController.StartListening("settingsChanged", delegate { AmendSpeeds(); });
    }

    private void AmendSpeeds() {
        panSpeed = PlayerPrefs.GetFloat("PanSpeed", 1f) * basePanSpeed;
        scrollSpeed = PlayerPrefs.GetFloat("ScrollSpeed", 1f) * baseScrollSpeed;
    }

    // Update is called once per frame
    void FixedUpdate() {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (movementEnabled) {
            cam.orthographicSize = Mathf.MoveTowards(Camera.main.orthographicSize, targetOrtho, scrollSpeed);
            DragFunction();
            MoveCamera();
        }

        if (scroll != 0) {
            int results = managerReferences.uiManagement.CanvasRaycastCheck();
            //Raycast using the Graphics Raycaster and mouse click position
            if (results == 0) {
                targetOrtho -= scroll * scrollSpeed;
                targetOrtho = Mathf.Clamp(targetOrtho, minOrtho, maxOrtho);
            }
        }

    }

    public void MoveCamera() {
        Vector3 newPos;
        if (automaticScrolling) {
            ProcessPanning();
        } else {
            Vector3 movement = new Vector3(Input.GetAxis("Horizontal"),
                Input.GetAxis("Vertical"), 0f);
            newPos = transform.position + (movement * panSpeed * Time.deltaTime);
            Vector3 offset = newPos - centerPt;
            transform.position = new Vector3(Mathf.Clamp(offset.x, 0 - lengthx, lengthx), Mathf.Clamp(offset.y, 0 - lengthy, lengthy), -1);
            // Debug.DrawLine(new Vector3(centerPt.x - lengthx, centerPt.y - lengthy), new Vector3(centerPt.x + lengthx, centerPt.y + lengthy));
        }

    }

    private void DragFunction() {
        if (Input.GetKeyDown(KeyCode.Mouse1)) {
            dragOrigin = Input.mousePosition;
            return;
        }

        if (!Input.GetKey(KeyCode.Mouse1)) return;
        Vector3 targetPos = Camera.main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
        Vector3 move = new Vector3(targetPos.x, targetPos.y, 0) * panSpeed * Time.deltaTime;
        transform.Translate(move, Space.World);
    }

    public void PanToLocation(Vector3 location) {
        Debug.Log("Panning to location " + location);
        automaticScrolling = true;
        //Vector3 screenPosition = cam.WorldToScreenPoint(location);
        targetLocation = new Vector3(location.x, location.y, -1f);
    }

    public void ProcessPanning() {
        if (transform.position == targetLocation || targetLocation == Vector3.zero) {
            automaticScrolling = false;
            targetLocation = Vector3.zero;
            return;
        }
        Vector3 target = targetLocation;
        //target = new Vector3(Mathf.Clamp(target.x * speed, -lengthx, lengthx), Mathf.Clamp(target.y * speed, -lengthy, lengthy), -1f);
        Vector3 lerpPos = Vector3.Lerp(transform.position, target, panSpeed);
        transform.position = new Vector3(lerpPos.x, lerpPos.y, -1f);
        //transform.position = target;
    }

    public void ChangeEnabledState(bool enable) {
        movementEnabled = enable;
    }
}