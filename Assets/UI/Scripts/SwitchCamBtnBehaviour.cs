using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EasyAR;
using System;
using ArCamDevType = EasyAR.CameraDeviceBaseBehaviour.DeviceType;


public class SwitchCamBtnBehaviour : MonoBehaviour {
    // camera switch params
    public float maxSwitchTime = 6;
    public float tryingTime = 0.05f;
    public uint diffrentFramesCount = 6;

    // for camera switch
    private Texture2D checkingCamTex = null;
    private CameraDeviceBehaviour arCam = null;
    private RawImage transitionBackground = null;
    private SnapshotMaker snapshooter = null;

    // for button view
    private Toggle toggle = null;
    private UnityEngine.UI.Image image = null;
    private Sprite rotateBackSprite = null;
    private Sprite rotateFrontSprite = null;


    void Start() {
        arCam = FindObjectOfType<CameraDeviceBehaviour>();
        transitionBackground = transform.parent.GetComponentInParent<RawImage>();
        transitionBackground.enabled = false;
        snapshooter = FindObjectOfType<SnapshotMaker>();

        // загрузка спрайтов для разных состояний кнопки
        const string rotateBackSpritePath = "buttons/rotate_back";
        rotateBackSprite = Resources.Load<Sprite>(rotateBackSpritePath);
        const string rotateFrontSpritePath = "buttons/rotate_front";
        rotateFrontSprite = Resources.Load<Sprite>(rotateFrontSpritePath);
        
        image = GetComponent<UnityEngine.UI.Image>();
        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(delegate {
            // смена спрайта
            image.sprite = toggle.isOn ? rotateBackSprite : rotateFrontSprite;
            // выключение ui на время переключения камеры
            SetUiActive(false);
            // показ заглушки на время переключения камеры
            snapshooter.ScreenTexMade += OnCreenshotMade;
            snapshooter.RefreshScreenshotRenderTex();
            snapshooter.Screenshoot(); // шаг 1: скриним экран, ставим на заглушку
        });
    }

    void OnDestroy() {
        snapshooter.ScreenTexMade -= OnCreenshotMade;
        snapshooter.PhotoTexMade -= OnPhotoMade;
    }

    void SetUiActive(bool isActive) {
        Selectable[] buttons = transform.parent.GetComponentsInChildren<Selectable>();
        for (int i = 0; i < buttons.Length; i++)
            buttons[i].interactable = isActive;
    }

    private void OnCreenshotMade(Texture2D tex) {
        snapshooter.ScreenTexMade -= OnCreenshotMade;
        transitionBackground.texture = tex;
        transitionBackground.enabled = true;
        StartCoroutine(ShowStubPlane());
    }

    private void OnPhotoMade(Texture2D tex) {
        checkingCamTex = tex;
    }

    private IEnumerator ShowStubPlane() {
        snapshooter.PhotoTexMade += OnPhotoMade;

        bool isBack = arCam.CameraDeviceType == ArCamDevType.Back;
        arCam.CameraDeviceType = isBack ? ArCamDevType.Front : ArCamDevType.Back;
        arCam.Close();
        arCam.OpenAndStart();
        
        checkingCamTex = null;
        Vector2Int size = new Vector2Int(Screen.width / 16, Screen.height / 16);
        ulong prevHash = 0;
        uint curDiffrFramesCount = 0;
        float startTime = Time.time;
        while ((Time.time - startTime < maxSwitchTime) && (curDiffrFramesCount < diffrentFramesCount)) {
            snapshooter.Snapshoot(size);
            while (checkingCamTex == null)
                yield return null;
            ulong hash = CalcTexHash(checkingCamTex);
            if (hash != prevHash)
                curDiffrFramesCount++;
            else
                curDiffrFramesCount = 0;
            prevHash = hash;
            checkingCamTex = null;
            yield return new WaitForSeconds(tryingTime);
        }

        snapshooter.PhotoTexMade -= OnPhotoMade;
        transitionBackground.enabled = false;
        SetUiActive(true);
    }

    private ulong CalcTexHash(Texture2D tex) {
        // resize
        TextureScale.Bilinear(tex, 8, 8);
        
        // get grayscale
        Color[] clrs = tex.GetPixels();
        float[] grays = new float[clrs.Length];
        for(int i = 0; i < clrs.Length; i++)
            grays[i] = clrs[i].grayscale;

        // find average
        float average = 0;
        for (int i = 0; i < grays.Length; i++)
            average += grays[i];
        average /= grays.Length;

        // calc hash (with binarisation)
        ulong hash = 0;
        for (int i = 0; i < grays.Length; i++)
            hash = (hash << 1) | (grays[i] >= average ? 1ul : 0ul);

        return hash;
    }
}

