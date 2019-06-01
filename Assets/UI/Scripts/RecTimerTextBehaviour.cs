using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecTimerTextBehaviour : MonoBehaviour {
    private float startTime = 0;
    private Text recTimeText = null;

    void Start() {
        recTimeText = GetComponent<Text>();
    }
    void OnEnable() {
        startTime = Time.unscaledTime;
	}
    void Update() {
        float recTime = Time.unscaledTime - startTime;
        recTimeText.text = string.Format("{0:00}:{1:00}", recTime / 60, recTime % 60);
    }
}
