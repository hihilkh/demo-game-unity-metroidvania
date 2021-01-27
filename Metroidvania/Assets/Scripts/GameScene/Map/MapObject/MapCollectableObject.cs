using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCollectableObject : MapTriggerBase<MapData.CollectableData> {

    public static event Action<MapCollectable.Type> CollectedEvent;

    public override void Init (MapData.CollectableData data) {
        this.data = data;

        var spriteRenderer = gameObject.GetComponent<SpriteRenderer> ();
        if (spriteRenderer == null) {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer> ();
        }

        // TODO : Set sprite


        if (data.isFromEnemy) {
            gameObject.SetActive (false);
            // TODO : Get Enemy and listen to the diedEvent
        } else {
            gameObject.SetActive (true);
            transform.position = data.GetPos ();

            var collider = gameObject.GetComponent<BoxCollider2D> ();
            if (collider == null) {
                collider = gameObject.AddComponent<BoxCollider2D> ();
            }

            collider.isTrigger = true;
        }

    }

    protected override bool CheckValidTrigger (Collider2D collision) {
        if (collision.tag != GameVariable.PlayerTag) {
            return false;
        }

        return true;
    }

    protected override void OnTriggered () {
        CollectedEvent?.Invoke (data.GetCollectableType ());
    }
}