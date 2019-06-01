using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrientationBehaviour : MonoBehaviour {
    public delegate void OrientationChangedDelegate(ScreenOrientation newOrientation);
    static public event OrientationChangedDelegate OrientationChanged;

    private static ScreenOrientation curOrientation = ScreenOrientation.Portrait;

    private Animator animator = null;
    private int isPortretParamId = 0;

    private Logger logger = null;

    void Start () {
        animator = GetComponent<Animator>();
        isPortretParamId = Animator.StringToHash("IsPortret");
        curOrientation = Screen.orientation;

        GameObject logTextObj = GameObject.Find("LogText");
        logger = new Logger(logTextObj == null ? null : logTextObj.GetComponent<UnityEngine.UI.Text>());

        OrientationChanged += OnOrientationChanged;
        if (OrientationChanged != null)
            OrientationChanged(curOrientation);
    }

    void Update () {
        if(curOrientation != Screen.orientation) {
            curOrientation = Screen.orientation;
            if (OrientationChanged != null)
                OrientationChanged(curOrientation);
        }
    }

    private void OnDestroy() {
        OrientationChanged -= OnOrientationChanged;
    }

    void OnOrientationChanged(ScreenOrientation newOrientation) {
        animator.SetBool(isPortretParamId, curOrientation == ScreenOrientation.Portrait);
        logger += "cur orientation: " + newOrientation;
    }
}
