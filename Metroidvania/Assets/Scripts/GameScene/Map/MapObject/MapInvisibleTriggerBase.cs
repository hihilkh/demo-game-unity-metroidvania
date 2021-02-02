using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

public abstract class MapInvisibleTriggerBase<T> : MapTriggerBase<T> where T : MapData.InvisibleTriggerData {
    protected BoxCollider2D triggerCollider;

    public override void Init (T data) {
        this.data = data;

        transform.position = new Vector3 (data.collider.pos.x, data.collider.pos.y, GameVariable.GeneralMapItemPosZ);

        triggerCollider = gameObject.GetComponent<BoxCollider2D> ();
        if (triggerCollider == null) {
            triggerCollider = gameObject.AddComponent<BoxCollider2D> ();
        }

        triggerCollider.isTrigger = true;
        triggerCollider.size = data.collider.size;
    }
}