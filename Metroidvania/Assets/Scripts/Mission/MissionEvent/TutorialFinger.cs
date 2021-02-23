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
    private const string LoopHoldReleaseAnimStateName = "LoopHoldRelease";

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

    #region Tap / HoldRelease

    public void ShowLoopTap_RightScreen () {
        ShowLoopAnim_RightScreen (LoopTapAnimStateName);
    }

    public void ShowLoopHoldRelease_RightScreen () {
        ShowLoopAnim_RightScreen (LoopHoldReleaseAnimStateName);
    }

    private void ShowLoopAnim_RightScreen (string stateName) {
        var totalWidth = SelfCanvas.GetComponent<RectTransform> ().sizeDelta.x;
        var pos = new Vector3 (totalWidth / 4, 0, 0);
        ShowLoopAnim (pos, true, stateName);
    }

    /// <summary>
    /// Currently assumed the target is in the same canvas with this tutorial finger<br />
    /// May need to modify logic if the target is not in the same canvas
    /// </summary>
    public void ShowTap (RectTransform rectTransform) {
        ShowLoopAnim (rectTransform.position, false, LoopTapAnimStateName);
    }

    private void ShowLoopAnim (Vector3 pos, bool isLocalPos, string stateName) {
        StopAllCoroutines ();

        SetFingerPos (pos, isLocalPos);

        finger.SetActive (true);
        animator.Play (stateName);
    }

    #endregion

    #region Drag And Drop

    public void ShowDragAndDrop_LeftScreen (CharEnum.LookDirections directions) {
        var totalWidth = SelfCanvas.GetComponent<RectTransform> ().sizeDelta.x;

        startDragAndDropPos = new Vector3 (-totalWidth / 4, 0, 0);

        var moveLength = totalWidth / 8;
        endDragAndDropPos = startDragAndDropPos;

        var horizontalDirection = directions & (CharEnum.LookDirections.Left | CharEnum.LookDirections.Right);
        switch (horizontalDirection) {
            case CharEnum.LookDirections.Left:
                endDragAndDropPos -= new Vector3 (moveLength, 0, 0);
                break;
            case CharEnum.LookDirections.Right:
                endDragAndDropPos += new Vector3 (moveLength, 0, 0);
                break;
        }

        var verticalDirection = directions & (CharEnum.LookDirections.Down | CharEnum.LookDirections.Up);
        switch (verticalDirection) {
            case CharEnum.LookDirections.Up:
                endDragAndDropPos += new Vector3 (0, moveLength, 0);
                break;
            case CharEnum.LookDirections.Down:
                endDragAndDropPos -= new Vector3 (0, moveLength, 0);
                break;
        }

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
