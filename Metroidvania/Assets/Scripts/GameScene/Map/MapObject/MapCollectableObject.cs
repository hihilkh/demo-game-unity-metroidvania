using System;
using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

public class MapCollectableObject : MapTriggerBase<MapData.CollectableData> {

    public static event Action<MapCollectableObject> CollectedEvent;

    private bool isAddedEventListeners = false;

    public override void Init (MapData.CollectableData data) {
        this.data = data;

        var spriteRenderer = gameObject.GetComponent<SpriteRenderer> ();
        if (spriteRenderer == null) {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer> ();
        }

        // TODO : Set sprite

        if (data.isFromEnemy) {
            gameObject.SetActive (false);
            if (!isAddedEventListeners) {
                isAddedEventListeners = true;
                EnemyModelBase.DiedEvent += EnemyDied;
            }
        } else {
            gameObject.SetActive (true);
            transform.position = new Vector3 (data.pos.x, data.pos.y, GameVariable.GeneralMapItemPosZ);

            var collider = gameObject.GetComponent<BoxCollider2D> ();
            if (collider == null) {
                collider = gameObject.AddComponent<BoxCollider2D> ();
            }

            collider.isTrigger = true;
        }
    }

    private void OnDestroy () {
        if (isAddedEventListeners) {
            EnemyModelBase.DiedEvent -= EnemyDied;
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

    private void EnemyDied (int enemyId) {
        if (data.isFromEnemy && data.fromEnemyId == enemyId) {
            OnTriggered ();
        }
    }

    public MapCollectable.Type GetCollectableType () {
        return data.type;
    }
}