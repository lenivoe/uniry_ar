using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoaderBehaviour : MonoBehaviour {
    public int sceneID = 1;
    public float minTimeInSeconds = 2;
    public Image progressBar;

	void Start () {
        progressBar.fillAmount = 0;
        StartCoroutine(LoadSceneCoroutine());
    }
	
	IEnumerator LoadSceneCoroutine() {
        float startTime = Time.time;
        AsyncOperation loading = SceneManager.LoadSceneAsync(sceneID);
        loading.allowSceneActivation = false;
        while(!loading.isDone && loading.progress < 0.9f) {
            if(progressBar != null)
                progressBar.fillAmount = loading.progress;
            yield return null;
        }
        if (progressBar != null)
            progressBar.fillAmount = 1;
        yield return null;
        while(Time.time - startTime < minTimeInSeconds)
            yield return null;
        loading.allowSceneActivation = true;
    }
}
