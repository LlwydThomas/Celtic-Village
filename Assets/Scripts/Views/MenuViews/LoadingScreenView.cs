using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class LoadingScreenView : MonoBehaviour {
    public Slider loadingSlider;
    public TextMeshProUGUI loadingText;
    private bool loading;
    public float loadingSpeed;
    public SettingsController settingsController;
    public Animator animator;
    private AsyncOperation loadingOp;
    public Canvas loadingCanvas;
    public bool loadScene;

    private void OnEnable() {
        loading = false;
        if (loadScene) StructureLoadingInfo();
        else BeginLoadingMidScene();
    }

    private void StructureLoadingInfo() {
        //animator.StartPlayback();
        loadingOp = SceneManager.LoadSceneAsync("Llywydd");
        loadingText.SetText(settingsController.TranslateString(loadingText.text));
        loading = true;
    }

    private void DisableLoading() {
        loadingCanvas.gameObject.SetActive(false);
    }

    private void BeginLoadingMidScene() {
        EventController.StartListening("mapCompleted", DisableLoading);
    }

}