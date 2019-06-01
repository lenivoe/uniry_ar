using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSmootherHelper : MonoBehaviour {
    public delegate void OnScriptEnableDisable();
    public event OnScriptEnableDisable TargetEnabled;
    public event OnScriptEnableDisable TargetDisabled;

    void OnEnable() {
        if (TargetEnabled != null) {
            TargetEnabled();
        }
    }
    void OnDisable() {
        if (TargetDisabled != null)
            TargetDisabled();
    }
}
