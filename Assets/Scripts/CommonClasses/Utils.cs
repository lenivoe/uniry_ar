using System;
using System.Collections;
using UnityEngine;

public class Utils : MonoBehaviour {
    public static Utils Inst {
        get {
            if (inst == null) {
                inst = new GameObject("Utils").AddComponent<Utils>();
                DontDestroyOnLoad(inst.gameObject);
            }
            return inst;
        }
    }

    public event Action onQuit;
    
    public void Shedule(Action action, float seconds) {
        inst.StartCoroutine(WaitCoroutine(action, seconds));
    }

    private void OnApplicationQuit() { onQuit?.Invoke(); }


    


    private IEnumerator WaitCoroutine(Action callback, float time/*, float period = 0*/) {
        yield return new WaitForSeconds(time);
        callback();
    }



    private static Utils inst = null;
}
