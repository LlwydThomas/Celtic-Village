using System;
using System.Collections;
using System.Collections.Generic;
using HSVPicker;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class PawnDisplayHandler : MonoBehaviour {
    // Start is called before the first frame update
    public NewGameCompiler newGameCompiler;
    public GameObject nameObject, ageObject, backgroundObject, visibleHandlerObject;
    private ControllerManager controllerManager;
    public SettingsController settingsController;
    public Button[] buttons;
    private Image[] buttonBackgrounds;
    private Color[] colours = new Color[4];
    public ColorPicker colorPicker;
    private Image currentSprite;
    public Image[] clothingRenderers;
    public TMP_InputField nameField, ageField;
    public TextMeshProUGUI[] translatables;
    public GameObject[] basicInfoObjects;
    private List<Pawn> pawnlist;
    private Pawn currentPawn;
    private List<SkillLevelled> calculatedSkills;
    public List<SkillData> skills;
    private bool formDataSet = false;

    public Button randomiseButton;
    private void OnDisable() {
        ageField.DeactivateInputField();
        nameField.DeactivateInputField();
    }

    private void OnEnable() {
        ageField.ActivateInputField();
        nameField.ActivateInputField();
    }
    void Start() {
        skills = new List<SkillData>();
        buttonBackgrounds = new Image[buttons.Length];
        pawnlist = new List<Pawn>();
        //foreach (Transform x in skillScroll.transform) DestroyImmediate(x.gameObject);
        newGameCompiler = GameObject.Find("NewGamePanel").GetComponent<NewGameCompiler>();
        settingsController = GameObject.Find("Settings").GetComponent<SettingsController>();
        pawnlist = newGameCompiler.PawnReturn();
        randomiseButton.onClick.AddListener(RandomiseColours);
        //Debug.Log (pawnlist.Count);
        //int index = int.Parse();
        Debug.Log("Sibling Index: " + this.transform.GetSiblingIndex());
        if (pawnlist != null) PopulatePawnBasicInfo(this.transform.GetSiblingIndex());
        ButtonFormat();
        SettingsFunctions.TranslateTMPItems(settingsController, translatables);
    }

    private void RandomiseColours() {
        Color[] randoms = PawnFunctions.RandomiseColours(4);
        for (int i = 0; i < clothingRenderers.Length; i++) {
            Color colour = randoms[i];
            clothingRenderers[i].color = colour;
        }

        ButtonFormat();
    }

    private void ButtonFormat() {
        for (int i = 0; i < buttons.Length; i++) {
            Button currentButton = buttons[i];
            currentButton.onClick.RemoveAllListeners();
            Debug.Log("PDH - i: " + i + ", " + currentButton.name + ", " + clothingRenderers[i].gameObject.name);
            Image image = clothingRenderers[i];
            buttonBackgrounds[i] = currentButton.gameObject.GetComponent<Image>();
            buttonBackgrounds[i].color = image.color;
            currentButton.onClick.AddListener(delegate {
                Debug.Log("PDH - " + currentButton.name);
                AllowColourEditing(image, currentButton);
                colorPicker.AssignColor(image.color);
            });

            Debug.Log("PDH - I after listener " + i + " on button " + currentButton.name);
        }

        ageField.onEndEdit.AddListener(delegate { CheckAgeInt(ageField); });
    }

    private void CheckAgeInt(TMP_InputField ageField) {
        string text = ageField.text;
        int age;

        if (int.TryParse(text, out age)) {
            if (age >= 75 || age <= 18) {
                ageField.SetTextWithoutNotify(currentPawn.age.ToString());
            }
        } else ageField.SetTextWithoutNotify(currentPawn.age.ToString());
        Debug.Log("PDH - text: " + text + " age: " + age.ToString());
    }

    private void AllowColourEditing(Image image, Button button) {
        colorPicker.onValueChanged.RemoveAllListeners();
        currentSprite = image;
        Debug.Log("PDH - Allow colour editing run for " + currentSprite.gameObject.name);
        colorPicker.onValueChanged.AddListener(delegate {
            currentSprite.color = colorPicker.CurrentColor;
            button.gameObject.GetComponent<Image>().color = colorPicker.CurrentColor;
        });
    }

    private void PopulatePawnBasicInfo(int index) {
        Debug.Log("PDH - Pawn List Count: " + pawnlist.Count + ", index: " + index);
        currentPawn = pawnlist[index];
        nameField.SetTextWithoutNotify(currentPawn.name);
        ageField.SetTextWithoutNotify(currentPawn.age.ToString());
        formDataSet = true;
        for (int i = 0; i < clothingRenderers.Length; i++) {
            clothingRenderers[i].color = currentPawn.pawnColours[i];
        }
    }

    public Pawn ReturnPawnData() {
        calculatedSkills = new List<SkillLevelled>();
        PawnStatus pawnStatus = new PawnStatus(1, 1);
        // Body, Hair, Legs, Shirt
        for (int i = 0; i < clothingRenderers.Length; i++) {
            colours[i] = clothingRenderers[i].color;
        }
        int age;
        string name;
        if (!int.TryParse(ageField.text, out age)) age = 21;
        Pawn newPawn;
        if (formDataSet) {
            newPawn = new Pawn(nameField.text, age, colours, pawnStatus, currentPawn.id);
        } else newPawn = currentPawn;

        return newPawn;
    }
}