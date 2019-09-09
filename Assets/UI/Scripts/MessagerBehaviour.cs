using UnityEngine;
using UnityEngine.UI;

// асинхронно выводит сообщения на экран
public class MessagerBehaviour : MonoBehaviour {
    private enum State { PRESHOW, SHOW, PREHIDE, HIDE }

    private Text messegeGui = null;
    
    private State state = State.HIDE;
    private string msg = null;
    private float liveTimeInSec = 0;
    private float startTime;


    public void SetMessege(string message, float time = 2) {
        msg = message;
        liveTimeInSec = time;
        state = State.PRESHOW;
    }

    public void Clear() { state = State.PREHIDE; }


    void Start() {
        messegeGui = GetComponent<Text>();
        HideGui();
    }
    
    void Update() {
        switch(state) {
            case State.SHOW:
                if((Time.time - startTime) >= liveTimeInSec) {
                    state = State.PREHIDE;
                }
                break;
            case State.PREHIDE:
                HideGui();
                state = State.HIDE;
                break;
            case State.PRESHOW:
                ShowGui();
                state = State.SHOW;
                break;
        }
    }

    private void ShowGui() {
        startTime = Time.time;
        messegeGui.text = msg;
        messegeGui.enabled = true;
    }

    private void HideGui() {
        messegeGui.enabled = false;
    }
}
