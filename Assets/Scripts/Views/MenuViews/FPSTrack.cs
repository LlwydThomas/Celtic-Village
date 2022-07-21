using System.Collections;
using TMPro;
using UnityEngine;
public class FPSTrack : MonoBehaviour {

    // Attach this to a GUIText to make a frames/second indicator.
    //
    // It calculates frames/second over each updateInterval,
    // so the display does not keep changing wildly.
    //
    // It is also fairly accurate at very low FPS counts (<10).
    // We do this not by simply counting frames per interval, but
    // by accumulating FPS for each frame. This way we end up with
    // correct overall FPS even if the interval renders something like
    // 5.5 frames.

    public float updateInterval = 0.5F;

    private float accum = 0; // FPS accumulated over the interval
    private int frames = 0; // Frames drawn over the interval
    private float timeleft; // Left time for current interval

    private float timer, frameAverage;
    TextMeshProUGUI guiText;
    void Start() {
        guiText = gameObject.GetComponent<TextMeshProUGUI>();
    }

    void Update() {
        float timeChange = Time.smoothDeltaTime;
        timer = timer >= 0 ? timer -= timeChange : updateInterval;

        if (timer <= 0) frameAverage = (int) (1f / timeChange);
        string format = string.Format("{0:F1}", frameAverage);
        guiText.text = format;

        if (frameAverage < 30) {
            if (frameAverage < 10) guiText.color = Color.red;
            else guiText.color = Color.yellow;
        } else guiText.color = Color.green;
    }
}