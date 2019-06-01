using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// асинхронно выводит сообщения на экран
public class MessagerBehaviour : MonoBehaviour {
    private float liveTimeInSec = 0;
    private Text messegeText = null;
    private float startTime = 0;
    private bool needShow = false;
    private bool needHandleMessage = false;
    private string msg = null;

    public void ShowMessege(string message) { ShowMessege(message, 2); }
    public void ShowMessege(string message, float time) {
        msg = message;
        liveTimeInSec = time;
        needShow = true;
        needHandleMessage = true;
    }

    public void Clear() { ShowMessege("", 0); }

    void Start() {
        messegeText = GetComponent<Text>();
        messegeText.enabled = false;
    }
    void Update() {
        if(needHandleMessage) {
            needHandleMessage = false;
            startTime = Time.time;
            messegeText.text = msg;
            messegeText.enabled = true;
        }
        if (needShow && Time.time - startTime >= liveTimeInSec) {
            needShow = false;
            messegeText.enabled = false;
        }
    }
}
