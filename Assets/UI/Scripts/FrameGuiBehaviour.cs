using UnityEngine;
using EasyAR;

public class FrameGuiBehaviour : MonoBehaviour {
    public ImageTrackerBaseBehaviour tracker;
    public float enablingDelay = 1;

    private bool needShow = false;
    private float startTime = 0;
    
    void Awake() {
        tracker.TargetLoad += Tracker_TargetLoad;
        tracker.TargetUnload += Tracker_TargetUnload;
    }

    private void Tracker_TargetLoad(ImageTrackerBaseBehaviour imgTracker, ImageTargetBaseBehaviour imgTarget,
        Target arg3, bool arg4)
    {
        imgTarget.TargetFound += OnMarkFound;
        imgTarget.TargetLost += OnMarkLost;
    }

    private void Tracker_TargetUnload(ImageTrackerBaseBehaviour imgTracker, ImageTargetBaseBehaviour imgTarget,
        Target arg3, bool arg4)
    {
        imgTarget.TargetFound -= OnMarkFound;
        imgTarget.TargetLost -= OnMarkLost;
    }



    void OnDestroy() {
        if (tracker != null) {
            tracker.TargetLoad -= Tracker_TargetLoad;
            tracker.TargetUnload -= Tracker_TargetUnload;
            foreach(var target in tracker.LoadedTargetBehaviours) {
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

    private void OnMarkFound(TargetAbstractBehaviour obj) {
        needShow = false;
        SetChildrenActive(false);
    }
    private void OnMarkLost(TargetAbstractBehaviour obj) {
        needShow = true;
        startTime = Time.time;
    }
}
