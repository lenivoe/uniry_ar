using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundFrameBehaviour : MonoBehaviour {
    public EasyAR.ImageTrackerBaseBehaviour tracker;
    public float enablingDelay = 1;

    private bool needShow = false;
    private float startTime = 0;
    
    void Awake() {
        tracker.TargetLoad += Tracker_TargetLoad;
        tracker.TargetUnload += Tracker_TargetUnload;
    }

    private void Tracker_TargetLoad(EasyAR.ImageTrackerBaseBehaviour imgTracker, EasyAR.ImageTargetBaseBehaviour imgTarget,
        EasyAR.Target arg3, bool arg4)
    {
        imgTarget.TargetFound += OnMarkFound;
        imgTarget.TargetLost += OnMarkLost;
    }

    private void Tracker_TargetUnload(EasyAR.ImageTrackerBaseBehaviour imgTracker, EasyAR.ImageTargetBaseBehaviour imgTarget,
        EasyAR.Target arg3, bool arg4)
    {
        imgTarget.TargetFound -= OnMarkFound;
        imgTarget.TargetLost -= OnMarkLost;
    }



    void OnDestroy() {
        if (tracker != null) {
            tracker.TargetLoad -= Tracker_TargetLoad;
            tracker.TargetUnload -= Tracker_TargetUnload;
            List<EasyAR.ImageTargetBaseBehaviour> targets = tracker.LoadedTargetBehaviours;
            foreach(var target in targets) {
                target.TargetFound -= OnMarkFound;
                target.TargetLost -= OnMarkLost;
            }
        }
    }
    void Update() {
        if (needShow && Time.time - startTime >= enablingDelay) {
            needShow = false;
            SetChildrenActive(true);
        }
    }

    void SetChildrenActive(bool isEnabled) {
        for (int i = 0; i < transform.childCount; i++) {
            transform.GetChild(i).gameObject.SetActive(isEnabled);
        }
    }

    private void OnMarkFound(EasyAR.TargetAbstractBehaviour obj) {
        needShow = false;
        SetChildrenActive(false);
    }
    private void OnMarkLost(EasyAR.TargetAbstractBehaviour obj) {
        needShow = true;
        startTime = Time.time;
    }
}
