using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class HandleSideMenu : MonoBehaviour {
    public Canvas sideMenuCanvas;
    GraphicRaycaster graphicRaycaster;
    EventSystem eventSystem;
    PointerEventData ped;

    // Start is called before the first frame update
    void Start() {
        eventSystem = sideMenuCanvas.GetComponent<EventSystem>();
        graphicRaycaster = sideMenuCanvas.GetComponent<GraphicRaycaster>();
        ped = new PointerEventData(eventSystem);
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            //Set the Pointer Event Position to that of the mouse position
            ped.position = Input.mousePosition;
            List<RaycastResult> results = new List<RaycastResult>();
            //Raycast using the Graphics Raycaster and mouse click position
            graphicRaycaster.Raycast(ped, results);
            if (results.Count == 0) {
                this.gameObject.SetActive(false);
            }
        }
    }
}