using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class NewGameCompiler : MonoBehaviour {
    public UiManagement uiManagement;
    // Start is called before the first frame update
    public TextMeshProUGUI[] translateables;
    public TMP_Dropdown difficulty;
    public GameObject pawnSelectTemplate, pawnDisplayTemplate;
    public GameObject pawnSelectParent, pawnDisplayParent;
    public GameObject[] InputFields;
    public GameObject pawnCountInt;
    public Button backButton, startButton;
    public Image nameBackground, seedBackground;
    public TMP_InputField nameInput, seedInput;
    public SkillDataList skillDataListObject;
    public List<SkillData> skillDataList;
    public List<Pawn> tempPawnList;
    public SettingsController settingsController;
    int pawnCounter;
    private List<PawnEdit> pawnEdits = new List<PawnEdit>();
    Dictionary<string, string> strings = new Dictionary<string, string>();
    void Start() {
        skillDataList = skillDataListObject.skillDatas;
        tempPawnList = new List<Pawn>();
        foreach (Transform x in pawnDisplayParent.transform) Destroy(x.gameObject);
        foreach (Transform x in pawnSelectParent.transform) Destroy(x.gameObject);
        pawnCounter = 0;

        // Prepare the intitial data.
        FormatLanguage();
        InitialiseListeners();
        CheckPawnCount(1);
    }

    // Update is called once per frame
    void Update() {

    }

    public void FormatLanguage() {
        SettingsFunctions.TranslateTMPItems(settingsController, translateables);
        foreach (TMP_Dropdown.OptionData option in difficulty.options) {
            option.text = settingsController.TranslateString(option.text);
        }
    }

    public void GeneratePawnEdit(int number) {
        if (number == -1) {
            pawnCounter -= 1;
            Transform selectTranform = pawnSelectParent.transform;
            Transform displayTransform = pawnDisplayParent.transform;
            PawnEdit lastPawn = pawnEdits[pawnEdits.Count - 1];
            Destroy(lastPawn.pawnButtonObject);
            Destroy(lastPawn.pawnPanel.transform.parent.gameObject);
            pawnEdits.Remove(lastPawn);
            lastPawn = pawnEdits[pawnEdits.Count - 1];
            SetDisplayActive(lastPawn);
        } else {
            pawnCounter += 1;
            GameObject pawnButtonObject = Instantiate(pawnSelectTemplate, pawnSelectParent.transform, false);
            GameObject displayBox = Instantiate(pawnDisplayTemplate, pawnDisplayParent.transform, false);
            Button pawnButton = pawnButtonObject.GetComponent<Button>();
            PawnEdit pawnEdit = new PawnEdit(displayBox.transform.GetChild(0).gameObject, pawnButtonObject, pawnButton);
            pawnEdits.Add(pawnEdit);
            pawnButton.GetComponentInChildren<TextMeshProUGUI>().SetText(pawnCounter.ToString());
            pawnButton.onClick.AddListener(delegate { SetDisplayActive(pawnEdit); });
            pawnButton.name = "PawnButton" + pawnCounter;
            displayBox.name = "PawnDisplay" + pawnCounter;
            pawnEdit.pawnPanel.SetActive(false);
            if (pawnCounter == 1) {
                SetDisplayActive(pawnEdit);
            }
        }
    }

    private void PopulatePawnDisplay(int index) {

    }

    private void InitialiseListeners() {
        TMP_InputField pawnCountInput = InputFields[0].GetComponent<TMP_InputField>();

        seedInput.onValueChanged.AddListener(delegate {
            Color colour = Color.red;
            if (seedInput.text.Length > 0) {
                int seed;
                if (int.TryParse(seedInput.text, out seed)) {
                    colour = Color.white;
                }
            }
            seedBackground.color = colour;
            DetermineStartButtonInteractable();
        });

        nameInput.onValueChanged.AddListener(delegate {
            Color colour = Color.red;
            if (nameInput.text.Length > 0) colour = Color.white;
            nameBackground.color = colour;
            DetermineStartButtonInteractable();
        });
        backButton.onClick.AddListener(delegate { uiManagement.ManageCanvases(1); });
    }

    private void DetermineStartButtonInteractable() {
        if (nameInput.text.Length > 0 && seedInput.text.Length > 0) startButton.interactable = true;
        else startButton.interactable = false;
    }

    private void SetDisplayActive(PawnEdit pawnEditToActivate = null) {

        foreach (PawnEdit pawnEdit in pawnEdits) {
            pawnEdit.pawnPanel.SetActive(false);
            pawnEdit.buttonImage.color = GeneralFunctions.blackBackground;
        }

        if (pawnEditToActivate != null) {
            pawnEditToActivate.buttonImage.color = GeneralFunctions.greenSwatch;
            pawnEditToActivate.pawnPanel.SetActive(true);
        }
    }

    public void CheckPawnCount(int value) {
        TMP_InputField pawnCountInput = pawnCountInt.GetComponent<TMP_InputField>();
        int count = pawnCounter + value;
        //Debug.Log(pawnCounter);
        if (count >= 1 && count <= 5) {
            if (value == -1) {
                tempPawnList.RemoveAt(tempPawnList.Count - 1);

            } else {
                tempPawnList.Add(PawnCompiler(tempPawnList.Count));

                //Debug.Log(tempPawnList.Count);
            }
            GeneratePawnEdit(value);

            pawnCountInput.text = count.ToString();
        }
    }

    public List<Pawn> PawnReturn() {
        //Debug.Log (tempPawnList.Count);
        return tempPawnList;
    }
    private Pawn PawnCompiler(int id) {
        Pawn newPawn = PawnFunctions.CreateNewPawn(PawnFunctions.RandomPawnName());
        /* List<SkillLevelled> skillsLeveled = new List<SkillLevelled>();
        foreach (SkillData skill in skillDataList) {
            skillsLeveled.Add(new SkillLevelled(skill, 3, false));
        } */
        //string name = settingsController.ReturnPawnNames(1, true) [0];
        return newPawn;
    }
}

public class PawnEdit {
    public GameObject pawnPanel;
    public Button pawnButton;
    public GameObject pawnButtonObject;

    public Image buttonImage;

    public PawnEdit(GameObject _pawnPanel, GameObject _pawnButtonObject, Button _pawnButton) {
        pawnButton = _pawnButton;
        pawnPanel = _pawnPanel;
        pawnButtonObject = _pawnButtonObject;
        buttonImage = _pawnButtonObject.GetComponent<Image>();
    }
}