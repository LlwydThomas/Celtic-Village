using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
public class MapInputHandler : MonoBehaviour {
    // Start is called before the first frame update
    private EventSystem eventSystem;
    PointerEventData pointerEventData;
    public TribeInfo selectedTribe;
    public GameObject sceneDataPasser, tribeSelectionInfo, tribeSelectedNameField;
    
    public TribeData tribeData;
    public List<TribeInfo> tribeInfos = new List<TribeInfo> ();
    private Dictionary<string, TribeInfo> tribeLookup = new Dictionary<string, TribeInfo> ();
    private void Start () {
        tribeInfos = tribeData.tribeColours;
        foreach(TribeInfo x in tribeInfos){
            tribeLookup.Add(x.name, x);
        }
        eventSystem = EventSystem.current;
        pointerEventData = new PointerEventData (eventSystem);
    }

    private void Update () {
        if (Input.GetMouseButtonDown (0)) {
            if (eventSystem.IsPointerOverGameObject ()) {
                Debug.Log (eventSystem.currentSelectedGameObject);
            }
            /*  List<RaycastResult> list = new List<RaycastResult> ();
             eventSystem.RaycastAll (pointerEventData, list);
             foreach (RaycastResult result in list) {
                 Debug.Log (result.gameObject.name);
             } */
        }
    }

    public void SetActiveTribe (string name) {
        selectedTribe = tribeLookup[name];
        tribeSelectedNameField.GetComponent<TextMeshProUGUI>().SetText(selectedTribe.name);
    }

}