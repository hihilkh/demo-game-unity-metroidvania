using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

public class CharCameraModel : MonoBehaviour
{
    private Camera cam;
    private Transform camTransform;
    [SerializeField] private CharModel charModel;
    [SerializeField] private CharCameraParams camParams;
    [SerializeField] private CharController controller;

    private Vector3 originalLocalPos;

    private Vector2 generalMaxLocalPos;
    private Vector2 generalMinLocalPos;

    private CharEnum.LookDirection currentLookDirection;
    private Vector2 missionMaxGlobalPos;
    private Vector2 missionMinGlobalPos;
    private bool hasSetBoundaries;

    private void Awake () {
        cam = GetComponent<Camera> ();
        camTransform = cam.transform;
        originalLocalPos = camTransform.localPosition;

        generalMaxLocalPos = new Vector2 (originalLocalPos.x + camParams.camMaxHorizontalMovement, originalLocalPos.y + camParams.camMaxVerticalMovement);
        generalMinLocalPos = new Vector2 (originalLocalPos.x - camParams.camMaxHorizontalMovement, originalLocalPos.y - camParams.camMaxVerticalMovement);

        currentLookDirection = CharEnum.LookDirection.None;
        hasSetBoundaries = false;

        if (controller == null) {
            Log.PrintWarning ("Character controller is not assigned.", LogType.Char);
        } else {
            controller.LookEvent += LookAction;
        }
    }

    private void LateUpdate () {
        if (currentLookDirection == CharEnum.LookDirection.None && camTransform.localPosition == originalLocalPos) {
            // Skip below update
        } else {
            // Move camera base on looking direction
            var targetPosX = camTransform.localPosition.x;
            var targetPosY = camTransform.localPosition.y;

            var horizontalDeltaPos = camParams.camHorizontalMoveSpeed * Time.deltaTime;
            var horizontalDirection = currentLookDirection & (CharEnum.LookDirection.Left | CharEnum.LookDirection.Right);
            switch (horizontalDirection) {
                case CharEnum.LookDirection.Left:
                    targetPosX = Mathf.Max (targetPosX - horizontalDeltaPos, generalMinLocalPos.x);
                    break;
                case CharEnum.LookDirection.Right:
                    targetPosX = Mathf.Min (targetPosX + horizontalDeltaPos, generalMaxLocalPos.x);
                    break;
                case CharEnum.LookDirection.None:
                    if (targetPosX < originalLocalPos.x) {
                        targetPosX = Mathf.Min (targetPosX + horizontalDeltaPos, originalLocalPos.x);
                    } else {
                        targetPosX = Mathf.Max (targetPosX - horizontalDeltaPos, originalLocalPos.x);
                    }
                    break;
            }

            var verticalDeltaPos = camParams.camVerticalMoveSpeed * Time.deltaTime;
            var verticalDirection = currentLookDirection & (CharEnum.LookDirection.Down | CharEnum.LookDirection.Up);
            switch (verticalDirection) {
                case CharEnum.LookDirection.Down:
                    targetPosY = Mathf.Max (targetPosY - verticalDeltaPos, generalMinLocalPos.y);
                    break;
                case CharEnum.LookDirection.Up:
                    targetPosY = Mathf.Min (targetPosY + verticalDeltaPos, generalMaxLocalPos.y);
                    break;
                case CharEnum.LookDirection.None:
                    if (targetPosY < originalLocalPos.y) {
                        targetPosY = Mathf.Min (targetPosY + verticalDeltaPos, originalLocalPos.y);
                    } else {
                        targetPosY = Mathf.Max (targetPosY - verticalDeltaPos, originalLocalPos.y);
                    }
                    break;
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

    public void SetMissionBoundaries (Vector2 lowerBound, Vector2 upperBound) {
        var offset = new Vector2 (cam.orthographicSize * (float)Screen.width / (float)Screen.height, cam.orthographicSize);
        missionMaxGlobalPos = upperBound - offset;
        missionMinGlobalPos = lowerBound + offset;

        hasSetBoundaries = true;
    }

    public void UnsetMissionBoundaries () {
        hasSetBoundaries = false;
    }

    private void LookAction (CharEnum.LookDirection direction) {
        currentLookDirection = direction;
    }
}
