using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class WorldTextView : MonoBehaviour {
    // Start is called before the first frame update
    private List<TextObject> texts = new List<TextObject>();
    private float checkTime = 1f, timer = 0;

    public GameObject textPrefab;
    private void FixedUpdate() {
        timer += Time.deltaTime;
        if (timer >= checkTime) {
            foreach (TextObject text in texts.ToArray()) {
                text.duration -= Time.deltaTime;
                if (text.duration <= 0) {
                    Destroy(text.textMeshProUGUI.gameObject.transform.parent.gameObject);
                    texts.Remove(text);
                }
            }
        }
    }

    public void AppendTextItem(Vector3 location, string text, Color colour, float duration, int fontsize) {
        GameObject textObject = GameObject.Instantiate(textPrefab, location, Quaternion.identity, this.transform);
        TextMeshProUGUI textMesh = textObject.GetComponentInChildren<TextMeshProUGUI>();
        textMesh.SetText(text);
        texts.Add(new TextObject(textMesh, duration));
    }

    private class TextObject {
        public TextMeshProUGUI textMeshProUGUI;
        public float duration;
        public TextObject(TextMeshProUGUI text, float dur) {
            textMeshProUGUI = text;
            duration = dur;
        }
    }
}