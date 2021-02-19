using System;
using System.Collections;
using HihiFramework.Core;
using UnityEngine;

public class TutorialFinger : MonoBehaviour {

    [SerializeField] private GameObject finger;
    [SerializeField] private Animator animator;

    private Canvas _selfCanvas = null;
    private Canvas SelfCanvas {
        get {
            if (_selfCanvas == null) {
                _selfCanvas = GetComponentInParent<Canvas> ();
            }

            return _selfCanvas;
        }
    }

    private const string FinderDownAnimStateName = "FingerDown";
    private const string FinderUpAnimStateName = "FingerUp";
    private const string LoopTapAnimStateName = "LoopTap";

    private const float LoopDragAndDropWaitPeriod = 1f;
    private const float DragAndDropPeriod = 1f;

    private Vector3 startDragAndDropPos;
    private Vector3 endDragAndDropPos;
    private bool isDragAndDropUsingLocalPos;

    private void OnDestroy () {
        StopAllCoroutines ();
    }

    #region Common

    public void Hide () {
        StopAllCoroutines ();
        finger.SetActive (false);
    }
    
    private void SetFingerPos (Vector3 pos, bool isLocalPos) {
        if (isLocalPos) {
            gameObject.transform.localPosition = pos;
        } else {
            gameObject.transform.position = pos;
        }
    }

    #endregion

    #region Tap

    public void ShowLoopTap_RightScreen () {
        var totalWidth = SelfCanvas.GetComponent<RectTransform> ().sizeDelta.x;
        var pos = new Vector3 (totalWidth / 4, 0, 0);
        ShowTap (pos, true);
    }

    /// <summary>
    /// Currently assumed the target is in the same canvas with this tutorial finger<br />
    /// May need to modify logic if the target is not in the same canvas
    /// </summary>
    public void ShowTap (RectTransform rectTransform) {
        ShowTap (rectTransform.position, false);
    }

    private void ShowTap (Vector3 pos, bool isLocalPos) {
        StopAllCoroutines ();

        SetFingerPos (pos, isLocalPos);

        finger.SetActive (true);
        animator.Play (LoopTapAnimStateName);
    }

    #endregion

    #region Drag And Drop

    public void ShowDragAndDrop_LeftToRight () {
        var totalWidth = SelfCanvas.GetComponent<RectTransform> ().sizeDelta.x;

        startDragAndDropPos = new Vector3 (-totalWidth / 4, 0, 0);
        endDragAndDropPos = Vector3.zero;
        isDragAndDropUsingLocalPos = true;

        StopAllCoroutines ();

        StartCoroutine (DragAndDropRecursive ());
    }

    /// <summary>
    /// Currently assumed the target is in the same canvas with this tutorial finger<br />
    /// May need to modify logic if the target is not in the same canvas
    /// </summary>
    public void ShowDragAndDrop (RectTransform startRectTransform, RectTransform endRectTransform) {
        startDragAndDropPos = startRectTransform.position;
        endDragAndDropPos = endRectTransform.position;
        isDragAndDropUsingLocalPos = false;

        StopAllCoroutines ();

        StartCoroutine (DragAndDropRecursive ());
    }

    private IEnumerator DragAndDropRecursive () {
        SetFingerPos (startDragAndDropPos, isDragAndDropUsingLocalPos);
        finger.SetActive (true);

        bool isContinue = false;

        FrameworkUtils.Instance.StartSingleAnim (animator, FinderDownAnimStateName, () => isContinue = true);

        yield return new WaitUntil (() => isContinue);

        var startTime = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup - startTime < DragAndDropPeriod) {
            var pos = Vector3.Lerp (startDragAndDropPos, endDragAndDropPos, (Time.realtimeSinceStartup - startTime) / DragAndDropPeriod);
            SetFingerPos (pos, isDragAndDropUsingLocalPos);
            yield return null;
        }

        SetFingerPos (endDragAndDropPos, isDragAndDropUsingLocalPos);
        isContinue = false;
        FrameworkUtils.Instance.StartSingleAnim (animator, FinderUpAnimStateName, () => isContinue = true);

        yield return new WaitUntil (() => isContinue);

        yield return new WaitForSecondsRealtime (LoopDragAndDropWaitPeriod);

        StartCoroutine (DragAndDropRecursive ());
    }

    #endregion
}
