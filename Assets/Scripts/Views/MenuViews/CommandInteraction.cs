using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CommandInteraction : MonoBehaviour {
    public GameObject Commands, skillsModel, settingsModel;
    Dictionary<string, string> strings;
    public Animator animator;
    public bool sectionExpanded;
    public Button menuButton;
    private GraphicRaycaster m_Raycaster;
    private PointerEventData m_PointerEventData;
    private EventSystem m_EventSystem;
    private List<RaycastResult> results;
    public GameObject canvas, expandedParent, expandedPanel;

    public Image[] buttonImages;
    private List<SkillData> skillList;

    public ManagerReferences managerReferences;
    private UiManagement uiManagement;
    private ControllerManager controllerManager;
    // Start is called before the first frame update
    void Start() {
        // Initialise the game data.
        uiManagement = managerReferences.uiManagement;
        controllerManager = managerReferences.controllerManager;
        // Reference the graphic raycaster and event system from the UI.
        m_Raycaster = canvas.GetComponent<GraphicRaycaster>();
        m_EventSystem = canvas.GetComponent<EventSystem>();
        sectionExpanded = false;
        SetTooltips(Commands);
        menuButton.onClick.AddListener(delegate { uiManagement.ManageOpenDialogues(false, 2); });
        //MinimiseExpanded();
    }

    public void SetTooltips(GameObject parent) {
        foreach (Transform transform in parent.transform.GetChild(0)) {
            ToolTipHandler tipHandler = transform.gameObject.AddComponent<ToolTipHandler>();
            tipHandler.SetTooltipData(controllerManager.settingsController.TranslateString(transform.gameObject.name.Remove(transform.gameObject.name.IndexOf("Button"))), 0, expandedPanel);
        }
    }

    public void Activate(GameObject expanded) {
        // Set all UI elements to inactive.
        Deactivate();
        if (uiManagement.sideMenuAllowed) {
            // Set the specified section to active.
            expanded.SetActive(true);
            sectionExpanded = true;
        }
    }

    private void SetButtonHighlight(int index = -1) {
        foreach (Image buttonImage in buttonImages) {
            buttonImage.color = GeneralFunctions.blackBackground;
        }
        if (index != -1) buttonImages[index].color = GeneralFunctions.greenSwatch;
    }

    public void ToggleExpanded(int index) {
        if (uiManagement.sideMenuAllowed) {
            uiManagement.ManageOpenDialogues(false);
            uiManagement.ForceCloseTooltip(0);
            SetButtonHighlight(index);
            Debug.Log("SideMenuAllowed: " + uiManagement.sideMenuAllowed);
            if (!sectionExpanded) {
                animator.SetBool("open", true);
                sectionExpanded = true;
            }

            if (animator != null) {
                foreach (Transform child in expandedParent.transform) {
                    child.gameObject.SetActive(false);
                }
                expandedParent.transform.GetChild(index).gameObject.SetActive(true);
            } else Debug.Log("no animator found");

        }
    }

    public void Deactivate() {
        // Set all UI elements to inactive.
        SetButtonHighlight();
        if (animator != null) {
            animator.SetBool("open", false);
            sectionExpanded = false;
        }
    }

    // Update is called once per frame
    void Update() {

        if (Input.GetMouseButtonDown(0)) {
            if (sectionExpanded) {
                results = new List<RaycastResult>();
                m_PointerEventData = new PointerEventData(m_EventSystem);
                m_PointerEventData.position = Input.mousePosition;
                m_Raycaster.Raycast(m_PointerEventData, results);
                //Debug.Log(results.Count);
                if (results.Count == 0) {

                    Deactivate();
                }
            }
        }

    }
}