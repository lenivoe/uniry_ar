using System.Collections;
using UnityEngine;

public class AppController : MonoBehaviour {
    public static AppController Inst { get; private set; } = null;
    
    public bool HaveInetConnection { get; private set; } = false;
    
    
    private void Awake() {
        Debug.Assert(Inst == null, "Several singleton instances: " + this + " and " + Inst);

        Inst = this;
        Screen.sleepTimeout = SleepTimeout.NeverSleep; // never turn off screen
        DontDestroyOnLoad(Inst.gameObject);

        Debug.Log("AppSettings initialized");
    }

    private void Start() {
        const float checkingTime = 1;
        StartCoroutine(PdfViewerCoroutine());
        StartCoroutine(CheckConnectionCoroutine(checkingTime));
    }

    
    private IEnumerator PdfViewerCoroutine() {
        while (true) {
            PdfViewer.Inst.MainThreadLoop();
            yield return null;
        }
    }

    private IEnumerator CheckConnectionCoroutine(float checkingTime) {
        while (true) {
            HaveInetConnection = (Application.internetReachability != NetworkReachability.NotReachable);
            yield return new WaitForSecondsRealtime(checkingTime);
        }
    }
}
