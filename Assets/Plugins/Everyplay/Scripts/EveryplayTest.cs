using UnityEngine;
using System.Collections;

public class EveryplayTest : MonoBehaviour
{
    private bool isRecording = false;
    private bool isPaused = false;
    private bool isRecordingFinished = false;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        Everyplay.RecordingStarted += RecordingStarted;
        Everyplay.RecordingStopped += RecordingStopped;
    }

    void Destroy()
    {
        Everyplay.RecordingStarted -= RecordingStarted;
        Everyplay.RecordingStopped -= RecordingStopped;
    }

    private void RecordingStarted()
    {
        isRecording = true;
        isPaused = false;
        isRecordingFinished = false;
    }

    private void RecordingStopped()
    {
        isRecording = false;
        isRecordingFinished = true;
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 138, 48), "Everyplay"))
        {
            #if UNITY_EDITOR
            Debug.Log("Everyplay view is not available in the Unity editor. Please compile and run on a device.");
            #endif
        }

        if (isRecording && GUI.Button(new Rect(10, 64, 138, 48), "Stop Recording"))
        {
            Everyplay.StopRecording();
            #if UNITY_EDITOR
            Debug.Log("The video recording is not available in the Unity editor. Please compile and run on a device.");
            #endif
        }
        else if (!isRecording && GUI.Button(new Rect(10, 64, 138, 48), "Start Recording"))
        {
            Everyplay.StartRecording();
            #if UNITY_EDITOR
            Debug.Log("The video recording is not available in the Unity editor. Please compile and run on a device.");
            #endif
        }

        if (isRecording)
        {
            if (!isPaused && GUI.Button(new Rect(10 + 150, 64, 138, 48), "Pause Recording"))
            {
                Everyplay.PauseRecording();
                isPaused = true;
                #if UNITY_EDITOR
                Debug.Log("The video recording is not available in the Unity editor. Please compile and run on a device.");
                #endif
            }
            else if (isPaused && GUI.Button(new Rect(10 + 150, 64, 138, 48), "Resume Recording"))
            {
                Everyplay.ResumeRecording();
                isPaused = false;
                #if UNITY_EDITOR
                Debug.Log("The video recording is not available in the Unity editor. Please compile and run on a device.");
                #endif
            }
        }

        if (isRecordingFinished && GUI.Button(new Rect(10, 118, 138, 48), "Play Last Recording"))
        {
            Everyplay.PlayLastRecording();
            #if UNITY_EDITOR
            Debug.Log("The video playback is not available in the Unity editor. Please compile and run on a device.");
            #endif
        }

        if (isRecording && GUI.Button(new Rect(10, 118, 138, 48), "Take Thumbnail"))
        {
            Everyplay.TakeThumbnail();
            #if UNITY_EDITOR
            Debug.Log("Everyplay take thumbnail is not available in the Unity editor. Please compile and run on a device.");
            #endif
        }

        if (isRecordingFinished && GUI.Button(new Rect(10, 172, 138, 48), "Show sharing modal"))
        {
            Everyplay.ShowSharingModal();
            #if UNITY_EDITOR
            Debug.Log("The sharing modal is not available in the Unity editor. Please compile and run on a device.");
            #endif
        }
    }
}
