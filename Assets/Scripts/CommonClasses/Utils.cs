using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils : MonoBehaviour {
    public static Utils Inst {
        get {
            if (inst == null) {
                GameObject utilsObj = new GameObject("Utils");
                inst = utilsObj.AddComponent<Utils>();
                DontDestroyOnLoad(utilsObj);
            }
            return inst;
        }
    }

    public event Action onQuit;
    

    private void OnApplicationQuit() {
        if (onQuit != null)
            onQuit();
    }


    public void Shedule(Action action, float seconds) {
        inst.StartCoroutine(WaitCoroutine(action, seconds));
    }


    private IEnumerator WaitCoroutine(Action callback, float time/*, float period = 0*/) {
        yield return new WaitForSeconds(time);
        callback();
    }



    private static Utils inst = null;
}
