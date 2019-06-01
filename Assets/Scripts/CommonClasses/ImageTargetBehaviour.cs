using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

public class ImageTargetBehaviour : EasyAR.ImageTargetBaseBehaviour {
    private static bool hasOnceTracker = true;
    private static EasyAR.ImageTrackerBaseBehaviour tracker = null;

    protected override void Start() {
        Name = Path.Split('\\').Last();
        if (Storage == EasyAR.StorageType.App)
            Storage = EasyAR.StorageType.Assets;
        if (hasOnceTracker) { // установка трекера, в случае, если он на сцене один
            if (tracker == null) {
                var trackers = FindObjectsOfType<EasyAR.ImageTrackerBaseBehaviour>();
                if (hasOnceTracker = (trackers.Length == 1))
                    Bind(tracker = trackers[0]);
            } else {
                Bind(tracker);
            }
        }
        base.Start();
    }
}
