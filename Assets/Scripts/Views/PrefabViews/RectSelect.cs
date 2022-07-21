using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
public class RectSelect : MonoBehaviour
{
    // Start is called before the first frame update
    GameObject panel;
    private Vector3 tempStart, tempEnd;
    public GameObject canvas;
    GraphicRaycaster m_Raycaster;
    PointerEventData m_PointerEventData;
    EventSystem m_EventSystem;
    public LayerMask layerMask;
    List<GameObject> selectedItems;
    void Start()
    {
        m_Raycaster = canvas.GetComponent<GraphicRaycaster>();
        //Fetch the Event System from the Scene
        m_EventSystem = canvas.GetComponent<EventSystem>();
    }

    // Update is called once per frame
    private void OnEnable()
    {
        
        
        
        

    }
    void Update()
    {

        m_PointerEventData = new PointerEventData(m_EventSystem);
        //Set the Pointer Event Position to that of the mouse position
        m_PointerEventData.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        //Raycast using the Graphics Raycaster and mouse click position
        m_Raycaster.Raycast(m_PointerEventData, results);
        if (results.Count == 0) {
            if (Input.GetMouseButtonDown(0)) {
                tempStart = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1f));
            }

            if (Input.GetMouseButton(0)) {
                tempEnd = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1f));
                Vector3 middle = (tempEnd + tempStart) / 2f;
                float sizeX = Mathf.Abs(tempStart.x - tempEnd.x);
                float sizeY = Mathf.Abs(tempStart.y - tempEnd.y);
                this.transform.position = middle;
                this.transform.localScale = new Vector2(sizeX, sizeY);
            }


            if (Input.GetMouseButtonUp(0)) {
                Collider2D[] hwn = Physics2D.OverlapBoxAll(this.transform.position, this.transform.localScale, 2f, layerMask) ;
                foreach (Collider2D n in hwn) {
                    Debug.Log(n.name);
                    n.gameObject.GetComponent<SpriteRenderer>().color = Color.red;
                    if (n.CompareTag("Tree")) {
                        //GameObject.Find("Commands").GetComponent<CommandInteraction>().AddToTreeQueue(n.gameObject);
                    }
                }
               this.gameObject.SetActive(false);
            }
        }
        
    }
}
