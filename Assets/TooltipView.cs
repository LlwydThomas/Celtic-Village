using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooltipView : MonoBehaviour {
    // Start is called before the first frame update
    private RectTransform rectTransform;
    public int tooltipID;
    private bool fixedPosition = false;

    void Awake() {
        rectTransform = this.GetComponent<RectTransform>();
        DetermineType(tooltipID);
    }

    private void DetermineType(int id) {
        fixedPosition = false;
        switch (id) {
            case 0:
                break;
            case 1:
                break;
        }
    }

    private void OnEnable() {
        DeterminePosition();
    }

    // Update is called once per frame
    void FixedUpdate() {
        DeterminePosition();
    }

    private void DeterminePosition() {
        if (!fixedPosition) {
            float width = rectTransform.rect.width;
            float offsetValue = 20f;
            Vector2 position = Input.mousePosition;
            float pivotX = position.x / Screen.width;
            float finalXPivot = pivotX < 0.5f ? 0 : 1;
            float pivotY = position.y / Screen.height;
            float finalYPivot = pivotY < 0.5f ? 0 : 1;

            float offset = finalXPivot == 1 ? -offsetValue : offsetValue;
            transform.position = position += new Vector2(offset, 0);
            rectTransform.pivot = new Vector2(finalXPivot, pivotY);

        }
    }
}