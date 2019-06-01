using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;


[RequireComponent(typeof(AudioSource))]
public class VideoRecorder : MonoBehaviour {
    //
    // video record control
    //

    public void StartRecord() {
        if (!Everyplay.IsRecordingSupported()) {
            logger += "Video recording is not supported";
        } else if (!Everyplay.IsReadyForRecording()) {
            logger += "Device is not ready for record";
        } else {
            if (PreRecordStarted != null)
                PreRecordStarted();
            Utils.Inst.Shedule(() => Everyplay.StartRecording(), delayBeforeRecordStarted);
        }
    }
    public void StopRecord() {
        Everyplay.StopRecording();
    }
    public void StartStopRecord() {
        if (!Everyplay.IsRecording())
            StartRecord();
        else
            StopRecord();
    }




    class AutoorientationInfoKeeper {
        public AutoorientationInfoKeeper() { SavePermissions(); }

        public void SavePermissions() {
            isAllowedToPortrait = Screen.autorotateToPortrait;
            isAllowedToPortraitUpsideDown = Screen.autorotateToPortraitUpsideDown;
            isAllowedToLandscapeLeft = Screen.autorotateToLandscapeLeft;
            isAllowedToLandscapeRight = Screen.autorotateToLandscapeRight;
        }

        public void RestorePermissions() {
            Screen.autorotateToPortrait = isAllowedToPortrait;
            Screen.autorotateToPortraitUpsideDown = isAllowedToPortraitUpsideDown;
            Screen.autorotateToLandscapeLeft = isAllowedToLandscapeLeft;
            Screen.autorotateToLandscapeRight = isAllowedToLandscapeRight;
        }

        private bool isAllowedToPortrait { get; set; }
        private bool isAllowedToPortraitUpsideDown { get; set; }
        private bool isAllowedToLandscapeLeft { get; set; }
        private bool isAllowedToLandscapeRight { get; set; }
    }
    private AutoorientationInfoKeeper orInfoKeeper = null;

    public Text logText = null;
    private Logger logger = null;

    private AudioSource micSound = null;

    public event Action PreRecordStarted;
    public event Action PostRecordStoped;

    public float delayBeforeRecordStarted = 0.2f;
    public float delayAfterRecordEnded = 0.1f;


#if UNITY_ANDROID && !UNITY_EDITOR
    const int STREAM_VOICE_CALL = 0;
    const int STREAM_SYSTEM = 1;
    const int STREAM_MUSIC = 3;
    const int USE_DEFAULT_STREAM_TYPE = -2147483648;

    private int curAndroidMusicVolume = 0;
    private int minAndroidMusicVolume = 0;
    private bool isAndroidMuted = false;
#endif


    //
    // events
    //

    void Update() {
#if UNITY_ANDROID && !UNITY_EDITOR
        // permanent sound disabling is while video is recording
        if (isAndroidMuted)
            AndroidWrap.AudioManagerObj.Call("setStreamVolume", STREAM_MUSIC, minAndroidMusicVolume, 0);
#endif
    }

    void Start() {
        logger = new Logger(logText);
        Everyplay.SetLowMemoryDevice(true);
        micSound = GetComponent<AudioSource>();
        orInfoKeeper = new AutoorientationInfoKeeper();

#if UNITY_ANDROID && !UNITY_EDITOR
        curAndroidMusicVolume = AndroidWrap.AudioManagerObj.Call<int>("getStreamVolume", STREAM_MUSIC);
        minAndroidMusicVolume = AndroidWrap.AudioManagerObj.Call<int>("getStreamMinVolume", STREAM_MUSIC);
#endif
    }
    void Awake() {
        Everyplay.RecordingStarted += OnRecordStarted;
        Everyplay.RecordingStopped += OnRecordStopped;
        Everyplay.ReadyForRecording += OnReadyForRecording;
        Everyplay.FileReady += OnFileReady;
    }
    
    void OnDestroy() {
        Everyplay.RecordingStarted -= OnRecordStarted;
        Everyplay.RecordingStopped -= OnRecordStopped;
        Everyplay.ReadyForRecording -= OnReadyForRecording;
        Everyplay.FileReady -= OnFileReady;

        StopMicrophone();
        SetSpeakersMute(false);
    }
    void OnRecordStarted() {
        logger += "Recording was started";
        AllowAutorotation(false);
        SetSpeakersMute(true);
        StartMicrophone();
    }
    void OnRecordStopped() {
        logger += "Recording ended";
        StopMicrophone();
        AllowAutorotation(true);
        Utils.Inst.Shedule(() => {
            SetSpeakersMute(false);
            if (PostRecordStoped != null)
                PostRecordStoped();
        }, delayAfterRecordEnded);
        Everyplay.GetFilepath(); // сохранение видео в кеш с вызовом события по завершению
        logger += "File prepearing...";
    }
    void OnReadyForRecording(bool enabled) {
        if (enabled) {
            logger += "Ready for recording";
            Everyplay.ReadyForRecording -= OnReadyForRecording;
        }
    }

    private void OnFileReady(string fileSrc) {
        string fileDst = DateTime.Now.ToString("ddMMyyyy_HHmmssfff") + "{0}.mp4";
        var result = NativeGallery.SaveVideoToGallery(fileSrc, Application.productName, fileDst, errMsg => logger += errMsg);
        logger += "Video saving permission: " + result;
    }

    //
    // tools
    //

    void AllowAutorotation(bool isAllow) {
        if (isAllow)
            orInfoKeeper.RestorePermissions();
        else {
            orInfoKeeper.SavePermissions();
            Screen.autorotateToPortrait =
                Screen.autorotateToPortraitUpsideDown =
                Screen.autorotateToLandscapeLeft =
                Screen.autorotateToLandscapeRight = false;
        }
    }
    

    //
    // microphone record/playing control
    //
    
    private void StartMicrophone() {
        micSound.loop = true;
        micSound.mute = false;
        
        micSound.clip = Microphone.Start(null, true, 10, 44100);
        while (Microphone.GetPosition(null) <= 0) { } // Ждем, пока запись не начнется 
        micSound.Play(); // Проигрываем наш звук
    }
    private void StopMicrophone() {
        Microphone.End(null);
        micSound.Stop();
    }




    //
    // platform dependent code
    //
    
    void SetSpeakersMute(bool isMute) {
#if UNITY_ANDROID && !UNITY_EDITOR
        isAndroidMuted = isMute;
        if (isMute) {
            curAndroidMusicVolume = AndroidWrap.AudioManagerObj.Call<int>("getStreamVolume", STREAM_MUSIC);
        }
        AndroidWrap.AudioManagerObj.Call("setStreamVolume",
            STREAM_MUSIC, isMute ? minAndroidMusicVolume : curAndroidMusicVolume, 0);
#elif UNITY_IPHONE && !UNITY_EDITOR
        // STUB: disable only micro
        StopMicrophone();
#else
        AudioListener.volume = isMute ? 0 : 1;
#endif
    }
}
