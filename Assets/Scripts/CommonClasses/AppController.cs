using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppController : MonoBehaviour {
    private const float checkConnectionPeriod = 1;
    
    public static bool HaveInetConnection { get; private set; }

    private void Awake() {
        if (inst == null) {
            inst = this;
            DontDestroyOnLoad(inst.gameObject);
            Debug.Log("AppSettings initialized");
        } else {
            Debug.LogError("Using of AppSettings script from editor!! " + gameObject + " " + inst.gameObject);
            return;
        }
        
        Screen.sleepTimeout = SleepTimeout.NeverSleep; // never turn off screen
        HaveInetConnection = false;
    }

    private void Start() {
        if(inst == this) {
            StartCoroutine(PdfViewerCoroutine());
            StartCoroutine(CheckConnectionCoroutine());
        }
    }

    private IEnumerator PdfViewerCoroutine() {
        while (true) {
            PdfViewer.MainThreadLoop();
            yield return null;
        }
    }

    private IEnumerator CheckConnectionCoroutine() {
        while (true) {
            HaveInetConnection = (Application.internetReachability != NetworkReachability.NotReachable);
            yield return new WaitForSecondsRealtime(checkConnectionPeriod);
        }
    }

    private static AppController inst = null;
}
