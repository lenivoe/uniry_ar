using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour {
    public float strengh = 0.6f;
    public Transform targetCam;

    private void Start() {
        if (targetCam == null)
            targetCam = Camera.main.transform;
    }

    private void Update () {
        transform.rotation = Quaternion.Lerp(Quaternion.LookRotation(-targetCam.position),
            transform.parent.rotation, 1 - strengh);
	}
}
