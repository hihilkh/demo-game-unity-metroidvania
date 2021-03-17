using System;
using System.Collections;
using HihiFramework.Core;
using UnityEngine;

public class CharCameraModel : MonoBehaviour
{
    [SerializeField] private CharModel charModel;
    [SerializeField] private CharCameraParams camParams;
    [SerializeField] private CharController controller;
    [SerializeField] private GameObject caveCollapsePSBase;

    private Camera cam;
    private AudioListener audioListener;
    private Transform camTransform;

    private Transform originalParentTransform;
    private Vector3 originalLocalPos;

    private Vector2 generalMaxLocalPos;
    private Vector2 generalMinLocalPos;

    private CharEnum.LookDirections currentLookDirections;
    private Vector2 missionMaxGlobalPos;
    private Vector2 missionMinGlobalPos;
    private bool hasSetBoundaries;

    private CameraInputSubEvent currentSubEvent = null;
    private Action missionEventInputFinishedAction = null;

    private bool isDetachedFromChar = false;

    private void Awake () {
        cam = GetComponent<Camera> ();
        audioListener = GetComponent<AudioListener> ();
        camTransform = cam.transform;
        originalParentTransform = camTransform.parent;
        originalLocalPos = camTransform.localPosition;

        generalMaxLocalPos = new Vector2 (originalLocalPos.x + camParams.CamMaxLoopRightMagnitude, originalLocalPos.y + camParams.CamMaxLoopUpMagnitude);
        generalMinLocalPos = new Vector2 (originalLocalPos.x - camParams.CamMaxLoopLeftMagnitude, originalLocalPos.y - camParams.CamMaxLoopDownMagnitude);

        currentLookDirections = CharEnum.LookDirections.None;
        hasSetBoundaries = false;

        if (controller == null) {
            Log.PrintWarning ("Character controller is not assigned.", LogTypes.Char);
        } else {
            controller.Looked += LookedHandler;
        }
    }

    private void LateUpdate () {
        if (isDetachedFromChar) {
            return;
        }

        if (!GameSceneManager.IsGameStarted) {
            return;
        }

        if (currentLookDirections == CharEnum.LookDirections.None && camTransform.localPosition == originalLocalPos) {
            // Skip below update
        } else {
            if (currentSubEvent != null) {
                if (currentLookDirections != currentSubEvent.LookDirections) {
                    // For inputs other than required, treat as no input
                    currentLookDirections = CharEnum.LookDirections.None;
                }
            }

            // Move camera base on looking direction
            var targetPosX = camTransform.localPosition.x;
            var targetPosY = camTransform.localPosition.y;

            var horizontalDeltaPos = camParams.CamHorizontalMoveSpeed * Time.unscaledDeltaTime;
            var horizontalDirection = currentLookDirections & (CharEnum.LookDirections.Left | CharEnum.LookDirections.Right);
            switch (horizontalDirection) {
                case CharEnum.LookDirections.Left:
                    targetPosX = Mathf.Max (targetPosX - horizontalDeltaPos, generalMinLocalPos.x);
                    break;
                case CharEnum.LookDirections.Right:
                    targetPosX = Mathf.Min (targetPosX + horizontalDeltaPos, generalMaxLocalPos.x);
                    break;
                case CharEnum.LookDirections.None:
                    if (targetPosX < originalLocalPos.x) {
                        targetPosX = Mathf.Min (targetPosX + horizontalDeltaPos, originalLocalPos.x);
                    } else {
                        targetPosX = Mathf.Max (targetPosX - horizontalDeltaPos, originalLocalPos.x);
                    }
                    break;
            }

            var verticalDeltaPos = camParams.CamVerticalMoveSpeed * Time.unscaledDeltaTime;
            var verticalDirection = currentLookDirections & (CharEnum.LookDirections.Down | CharEnum.LookDirections.Up);
            switch (verticalDirection) {
                case CharEnum.LookDirections.Down:
                    targetPosY = Mathf.Max (targetPosY - verticalDeltaPos, generalMinLocalPos.y);
                    break;
                case CharEnum.LookDirections.Up:
                    targetPosY = Mathf.Min (targetPosY + verticalDeltaPos, generalMaxLocalPos.y);
                    break;
                case CharEnum.LookDirections.None:
                    if (targetPosY < originalLocalPos.y) {
                        targetPosY = Mathf.Min (targetPosY + verticalDeltaPos, originalLocalPos.y);
                    } else {
                        targetPosY = Mathf.Max (targetPosY - verticalDeltaPos, originalLocalPos.y);
                    }
                    break;
            }

            if (currentSubEvent != null && currentLookDirections == currentSubEvent.LookDirections) {
                if (Mathf.Approximately (targetPosX, camTransform.localPosition.x) && Mathf.Approximately (targetPosY, camTransform.localPosition.y)) {
                    // It means the camera totally moved to the target direction of the sub event
                    missionEventInputFinishedAction?.Invoke ();
                    currentSubEvent = null;
                    missionEventInputFinishedAction = null;
                }
            }
            camTransform.localPosition = new Vector3 (targetPosX, targetPosY, camTransform.localPosition.z);
        }

        if (!hasSetBoundaries) {
            return;
        }

        // ensure the camera is not outside mission boundaries
        if (camTransform.position.x > missionMaxGlobalPos.x) {
            camTransform.position = new Vector3 (missionMaxGlobalPos.x, camTransform.position.y, camTransform.position.z);
        } else if (camTransform.position.x < missionMinGlobalPos.x) {
            camTransform.position = new Vector3 (missionMinGlobalPos.x, camTransform.position.y, camTransform.position.z);
        }

        if (camTransform.position.y > missionMaxGlobalPos.y) {
            camTransform.position = new Vector3 (camTransform.position.x, missionMaxGlobalPos.y, camTransform.position.z);
        } else if (camTransform.position.y < missionMinGlobalPos.y) {
            camTransform.position = new Vector3 (camTransform.position.x, missionMinGlobalPos.y, camTransform.position.z);
        }
    }

    public void ResetPos () {
        camTransform.localPosition = originalLocalPos;
    }

    public void SetMissionBoundaries (Vector2 lowerBound, Vector2 upperBound) {
        var offset = new Vector2 (cam.orthographicSize * (float)Screen.width / (float)Screen.height, cam.orthographicSize);
        missionMaxGlobalPos = upperBound - offset;
        missionMinGlobalPos = lowerBound + offset;

        hasSetBoundaries = true;
    }

    public void UnsetMissionBoundaries () {
        hasSetBoundaries = false;
    }

    public void SetAudioListener (bool isEnable) {
        if (audioListener != null) {
            audioListener.enabled = isEnable;
        }
    }

    public void SetCameraInputMissionEvent (CameraInputSubEvent subEvent, Action onInputFinished = null) {
        currentSubEvent = subEvent;
        missionEventInputFinishedAction = onInputFinished;
    }

    #region Particle system

    public void SetCaveCollapseEffect (bool isActive) {
        caveCollapsePSBase.SetActive (isActive);
    }

    #endregion

    #region attach / detach

    public void DetachFromCharAndSetPos (Vector2 pos) {
        FrameworkUtils.InsertChildrenToParent (null, camTransform, false);
        camTransform.position = new Vector3 (pos.x, pos.y, camTransform.position.z);
        isDetachedFromChar = true;
    }

    public void AttachBackToChar () {
        FrameworkUtils.InsertChildrenToParent (originalParentTransform, camTransform, false);
        camTransform.localPosition = originalLocalPos;

        // Remarks :
        // - Delay update to prevent the case that actually not yet set the position and thus change position by LateUpdate ()
        // - Do not use StartCoroutine by self because this script may sometimes become disabled
        Action onWaitFinished = () => {
            isDetachedFromChar = false;
        };

        FrameworkUtils.Instance.Wait (null, onWaitFinished);
    }

    #endregion

    #region Events

    private void LookedHandler (CharEnum.LookDirections directions) {
        currentLookDirections = directions;
    }

    #endregion
}
