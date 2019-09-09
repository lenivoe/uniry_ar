using System.Linq;

public class ImageTargetBehaviour : EasyAR.ImageTargetBaseBehaviour {
    private static bool hasOnceTracker = true;
    private static EasyAR.ImageTrackerBaseBehaviour tracker = null;

    protected override void Start() {
        // предварительная инициализация (ненавижу EasyAR)
        Name = Path.Split('\\').Last();
        if (Storage == EasyAR.StorageType.App) {
            Storage = EasyAR.StorageType.Assets;
        }
        if (hasOnceTracker) { // установка трекера, в случае, если он на сцене один
            if (tracker == null) {
                var trackers = FindObjectsOfType<EasyAR.ImageTrackerBaseBehaviour>();
                hasOnceTracker = (trackers.Length == 1);
                tracker = trackers[0];
            }
            Bind(tracker);
        }

        base.Start();
    }
}
