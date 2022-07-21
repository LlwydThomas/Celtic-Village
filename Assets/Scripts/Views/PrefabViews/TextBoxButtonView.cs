using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class TextBoxButtonView : MonoBehaviour {
    // Start is called before the first frame update
    public Image background;
    public Button button;
    public float textSize;
    public TextMeshProUGUI textField;
    public void FormatTextBox(UnityAction onclick, string text, Color textColour, float textSize) {
        textField.SetText(text);
        button.onClick.AddListener(onclick);
        textField.color = textColour;
        textField.fontSizeMax = textSize;
    }
}