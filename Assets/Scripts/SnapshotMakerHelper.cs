using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;


class SnapshotMakerHelper : MonoBehaviour {
    public delegate void PostRenderDelegate(Camera cam);
    public event PostRenderDelegate PostRender;

    private Camera cam = null;

    void Start() {
        cam = GetComponent<Camera>();
    }

    void OnPostRender() {
        if (PostRender != null)
            PostRender(cam);
    }
}

