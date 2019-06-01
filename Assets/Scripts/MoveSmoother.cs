using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSmoother : MonoBehaviour {
    public float minDistance = 0.03f;
    public float maxDistance = 1;
    public float smoothDelay = 0.5f;

    private Transform dummy = null;

    // создается болванка, за которой будет следовать текущий объект
    void Start () {
        dummy = new GameObject("dummy").transform;
        dummy.parent = transform.parent;
        dummy.localPosition = transform.localPosition;
        dummy.localRotation = transform.localRotation;

        MoveSmootherHelper helper = dummy.gameObject.AddComponent<MoveSmootherHelper>();
        helper.TargetEnabled += OnTargetEnabled;
        helper.TargetDisabled += OnTargetDisabled;

        transform.parent = null;
	}
    void OnDestroy() {
		if(dummy != null) {
			MoveSmootherHelper helper = dummy.gameObject.GetComponent<MoveSmootherHelper>();
			helper.TargetEnabled -= OnTargetEnabled;
			helper.TargetDisabled -= OnTargetDisabled;
		}
    }
    void Update () {
        float sqrMagn = (dummy.position - transform.position).sqrMagnitude;
        // переместить объект, если он далеко
        if (sqrMagn > maxDistance * maxDistance) {
            transform.position = dummy.position;
            transform.rotation = dummy.rotation;
        } else if (sqrMagn > minDistance * minDistance) {
            // иначе плавно его передвигать, если болванка сдвинулась, не считая погрешности
            float realDelay = 1 - smoothDelay;
            transform.position = Vector3.Slerp(transform.position, dummy.position, realDelay);
            transform.rotation = Quaternion.Slerp(transform.rotation, dummy.rotation, realDelay);
        }
	}


    void OnTargetEnabled() {
        gameObject.SetActive(true);
        transform.position = dummy.position;
        transform.rotation = dummy.rotation;
    }
    void OnTargetDisabled() {
        gameObject.SetActive(false);
    }
}
