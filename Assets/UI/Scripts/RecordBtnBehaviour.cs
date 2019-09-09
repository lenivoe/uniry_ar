using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecordBtnBehaviour : MonoBehaviour {
    private Button button = null;
    private Selectable photoBtn = null;
    private Selectable switchCamBtn = null;
    private Animator animator = null;
    private int isRecordingParamId = 0;
    private VideoRecorder videoRecorder = null;
    private AudioSource clickSound = null;
    private MessagerBehaviour messager = null;

    public void OnRecordButtonClick() {
        if (!Everyplay.IsRecordingSupported()) {
            messager.SetMessege("Запись видео не поддерживается вашим устройством");
        } else if (!Everyplay.IsReadyForRecording()) {
            messager.SetMessege("Необходимо разовое подключение к интернету");
        } else {
            animator.SetBool(isRecordingParamId, !animator.GetBool(isRecordingParamId));
            photoBtn.interactable = !photoBtn.interactable;
            switchCamBtn.interactable = !switchCamBtn.interactable;

            videoRecorder.ToggleRecord();
        }
    }

    void Start() {
        button = GetComponent<Button>();
        button.onClick.AddListener(delegate { OnRecordButtonClick(); });

        photoBtn = transform.parent.Find("Photo").GetComponent<Selectable>();
        switchCamBtn = transform.parent.Find("ChangeCam").GetComponent<Selectable>();

        animator = transform.parent.GetComponentInParent<Animator>();
        isRecordingParamId = Animator.StringToHash("IsRecording");

        clickSound = GetComponent<AudioSource>();
        videoRecorder = FindObjectOfType<VideoRecorder>();
        videoRecorder.PreRecordStarted += OnRecordStart;
        videoRecorder.PostRecordStoped += OnRecordStop;

        messager = transform.parent.parent.Find("Messager").GetComponent<MessagerBehaviour>();
    }
    void OnDestroy() {
        videoRecorder.PreRecordStarted -= OnRecordStart;
        videoRecorder.PostRecordStoped -= OnRecordStop;
    }

    //
    // video record callbacks
    //

    private void OnRecordStart() {
        clickSound.Play();
    }
    private void OnRecordStop() {
        clickSound.Play();
        messager.SetMessege("Видео сохранено");
    }
}
