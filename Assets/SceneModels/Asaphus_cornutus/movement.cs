using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movement : MonoBehaviour {
    public float speed = 0.05f;
    public float a = 5, b = 3;

    private Vector3 nextPoint;

	void Update () {
        float t = 360 * Mathf.Deg2Rad * (0.23f + speed * Time.time);
        float x = a * Mathf.Cos(t);
        float y = b * Mathf.Sin(2 * t);
        Vector3 dif = (nextPoint - transform.localPosition).normalized;
        transform.localRotation = Quaternion.LookRotation(dif, Vector3.up);
        transform.localPosition = nextPoint;
        nextPoint = new Vector3(x, 0, y);
	}
}
