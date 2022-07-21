using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class GameSetupController : MonoBehaviour {
    // Start is called before the first frame update
    public bool currentEditing;
    public GameObject[] tabbableInputs;
    private int currentIndex;
    private GameObject currentItem;
    void Start () {
        currentEditing = false;
    }

    // Update is called once per frame
    void Update () {
        DetectInput();
    }

    public void BeginEditing (int index) {
        currentEditing = true;
        currentItem = tabbableInputs[index];
        currentIndex = index;
    }

    private void DetectInput () {
        if (currentEditing) {
            if (Input.GetKeyDown (KeyCode.Return) || Input.GetKeyDown (KeyCode.Tab)) {
                if (currentIndex < tabbableInputs.Length - 1) {
                    BeginEditing (currentIndex + 1);
                    currentItem.GetComponent<TMP_InputField> ().Select ();
                    currentItem.GetComponent<TMP_InputField> ().ActivateInputField ();
                } else {
                    BeginEditing (0);
                    currentItem.GetComponent<TMP_InputField> ().Select ();
                    currentItem.GetComponent<TMP_InputField> ().ActivateInputField ();
                }

            }
        }
    }
}