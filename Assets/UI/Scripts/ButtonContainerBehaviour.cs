using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using EasyAR;

// панель с кнопками видна для объектов с тегом ForPhotoAndVideo
public class ButtonContainerBehaviour : MonoBehaviour {
    private List<ImageTargetBehaviour> imgTargets = null;
    private int activeTargetsCount = 0;

    private Animator animator = null;
    private int needShowButtonsId = 0;
    
    // определение всех объектов, для которых нужно показывать панель с кнопками
    private void Start () {
        animator = transform.parent.GetComponent<Animator>();
        if (animator == null) {
            Debug.LogError("Animator not found");
            return;
        }
        needShowButtonsId = Animator.StringToHash("NeedShowButtons");

        GameObject imgTargetContainer = GameObject.Find("ImageTargetContainer");
        ImageTargetBehaviour[] allImgTargets = imgTargetContainer.GetComponentsInChildren<ImageTargetBehaviour>(true);
        if (allImgTargets != null) {
            imgTargets = allImgTargets.Where(target => target.tag == "ForPhotoAndVideo").ToList();
            foreach (var target in imgTargets) {
                target.TargetFound += OnTargetFound;
                target.TargetLost += OnTargetLost;
            }
        }
        if(imgTargets == null || imgTargets.Count == 0)
            Debug.LogWarning("No targets for picture shooting");
    }

    private void OnDestroy() {
        if (imgTargets == null)
            return;
        foreach(var target in imgTargets)
            if(target != null) {
                target.TargetFound -= OnTargetFound;
                target.TargetLost -= OnTargetLost;
            }
    }



    private void OnTargetFound(TargetAbstractBehaviour obj) {
        activeTargetsCount++;
        animator.SetBool(needShowButtonsId, true);
        print("showing buttons");
    }

    private void OnTargetLost(TargetAbstractBehaviour obj) {
        if(--activeTargetsCount == 0)
            animator.SetBool(needShowButtonsId, false);
        print("hiding buttons");
    }
}
