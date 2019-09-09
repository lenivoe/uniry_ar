using UnityEngine;
using UnityEngine.UI;
using System;

using AndroidWrap;

[RequireComponent(typeof(AudioSource))]
public class VideoRecorder : MonoBehaviour {
    public Text logText;
    public float delayBeforeRecordStarted = 0.2f;
    public float delayAfterRecordEnded = 0.1f;



    public event Action PreRecordStarted;
    public event Action PostRecordStoped;

    private class AutoorientationInfoKeeper {
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
    private Logger logger = null;
    private AudioSource micSound = null;

    private bool isSpeakersMuted = false;


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
    public void ToggleRecord() {
        if (!Everyplay.IsRecording())
            StartRecord();
        else
            StopRecord();
    }


    //
    // events
    //

    void Update() {
        // hack: permanent sound disabling is while video is recording
        MuteSpeakers(isSpeakersMuted);
    }

    void Start() {
        logger = new Logger(logText);
        Everyplay.SetLowMemoryDevice(true);
        micSound = GetComponent<AudioSource>();
        orInfoKeeper = new AutoorientationInfoKeeper();

        CacheVolumesValues();
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
        MuteSpeakers(false);
    }

    void OnRecordStarted() {
        logger += "Recording was started";
        AllowAutorotation(false);
        MuteSpeakers(true);
        StartMicrophone();
    }

    void OnRecordStopped() {
        logger += "Recording ended";
        StopMicrophone();
        AllowAutorotation(true);
        Utils.Inst.Shedule(() => {
            MuteSpeakers(false);
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



    void MuteSpeakers(bool isEnabled) {
        if(isSpeakersMuted != isEnabled) {
            MuteSpeakersPlatformed(isSpeakersMuted = isEnabled);
        }
    }


    //
    // platform dependent code
    //

#if UNITY_EDITOR
    void MuteSpeakersPlatformed(bool isEnabled) { AudioListener.volume = isEnabled ? 0 : 1; }
    void CacheVolumesValues() { }

#elif UNITY_IPHONE
    // STUB: only micro disables
    void MuteSpeakersPlatformed(bool isEnabled) { StopMicrophone(); }
    void CacheVolumesValues() { }

#elif UNITY_ANDROID
    const int STREAM_VOICE_CALL = 0;
    const int STREAM_SYSTEM = 1;
    const int STREAM_MUSIC = 3;
    const int USE_DEFAULT_STREAM_TYPE = -2147483648;

    private int curVolume = 0;
    private int minVolume = 0;



    void MuteSpeakersPlatformed(bool isEnabled) {
        if (isEnabled) {
            curVolume = Obj.AudioManager.Call<int>("getStreamVolume", STREAM_MUSIC);
            Obj.AudioManager.Call("setStreamVolume", STREAM_MUSIC, minVolume, 0);
        } else {
            Obj.AudioManager.Call("setStreamVolume", STREAM_MUSIC, curVolume, 0);
        }
    }

    void CacheVolumesValues() {
        curVolume = Obj.AudioManager.Call<int>("getStreamVolume", STREAM_MUSIC);
        minVolume = Obj.AudioManager.Call<int>("getStreamMinVolume", STREAM_MUSIC);
    }

#endif

}

