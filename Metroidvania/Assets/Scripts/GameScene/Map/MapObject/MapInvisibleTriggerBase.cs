using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

public abstract class MapInvisibleTriggerBase<T> : MapTriggerBase<T> where T : MapData.InvisibleTriggerData {

    public override void Init (T data) {
        this.data = data;

        transform.position = data.collider.GetPos ();

        var collider = gameObject.GetComponent<BoxCollider2D> ();
        if (collider == null) {
            collider = gameObject.AddComponent<BoxCollider2D> ();
        }

        collider.isTrigger = true;
        collider.size = data.collider.GetSize ();
    }
}