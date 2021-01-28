using System;
using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

public class MapCollectableObject : MapTriggerBase<MapData.CollectableData> {

    public static event Action<MapCollectableObject> CollectedEvent;

    public override void Init (MapData.CollectableData data) {
        Init (data, null);
    }

    public void Init (MapData.CollectableData data, EnemyModelBase enemy) {
        this.data = data;

        var spriteRenderer = gameObject.GetComponent<SpriteRenderer> ();
        if (spriteRenderer == null) {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer> ();
        }

        // TODO : Set sprite

        if (data.isFromEnemy) {
            gameObject.SetActive (false);
            if (enemy == null) {
                Log.PrintError ("The enemy reference is null. Cannot listen to enemy diedEvent", LogType.MapData);
            } else {
                enemy.diedEvent += OnTriggered;
            }
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
        CollectedEvent?.Invoke (this);
    }

    public MapCollectable.Type GetCollectableType () {
        return data.GetCollectableType ();
    }
}