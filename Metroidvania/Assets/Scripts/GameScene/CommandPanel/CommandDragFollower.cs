using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using HIHIFramework.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class CommandDragFollower : UIDragFollower {

    private Vector3 originalLocalPos;
    private Transform baseTransform;
    private Transform draggingBaseTransform;

    /// <param name="originalLocalPos">Use local position to prevent error due to animation of panel</param>
    public void Init (Vector3 originalLocalPos, Transform baseTransform, Transform draggingBaseTransform) {
        this.originalLocalPos = originalLocalPos;
        this.baseTransform = baseTransform;
        this.draggingBaseTransform = draggingBaseTransform;

        transform.localPosition = originalLocalPos;
    }

    public override void OnBeginDrag (PointerEventData eventData) {
        FrameworkUtils.InsertChildrenToParent (draggingBaseTransform, gameObject, true);

        base.OnBeginDrag (eventData);
    }

    public override void OnEndDrag (PointerEventData eventData) {
        base.OnEndDrag (eventData);

        FrameworkUtils.InsertChildrenToParent (baseTransform, gameObject, true);
        transform.localPosition = originalLocalPos;
    }
}