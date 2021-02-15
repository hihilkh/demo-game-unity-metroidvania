﻿using System;
using HihiFramework.Core;
using UnityEngine;

public class MapCollectableObject : MapTriggerBase<MapData.CollectableData> {
    public static event Action<MapCollectableObject> Collected;

    [SerializeField] private GameObject baseGO;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer baseSpriteRenderer;
    [SerializeField] private SpriteRenderer additionalSpriteRenderer;

    private const string CollectAnimStateName = "Collect";
    private bool isAddedEnemyEventListeners = false;

    public override void Init (MapData.CollectableData data) {
        this.Data = data;

        var collectable = CollectableManager.GetCollectable (data.type);
        if (collectable == null) {
            baseGO.SetActive (false);
            Log.PrintError ("Cannot init map collectable object : Collectable is null.", LogTypes.MapData);
            return;
        }

        var icon = Resources.Load<Sprite> (collectable.GetIconResourcesName ());
        if (collectable.IsWithCircleFrame) {
            additionalSpriteRenderer.sprite = icon;
        } else {
            baseSpriteRenderer.sprite = icon;
        }

        if (data.IsFromEnemy) {
            baseGO.SetActive (false);
            if (!isAddedEnemyEventListeners) {
                isAddedEnemyEventListeners = true;
                EnemyModelBase.Died += EnemyDiedHandler;
            }
        } else {
            baseGO.SetActive (true);
            baseGO.transform.position = new Vector3 (data.pos.x, data.pos.y, GameVariable.GeneralMapItemPosZ);

            // Must set sprite before set Collider in order to get correct collider size
            var collider = gameObject.GetComponent<BoxCollider2D> ();
            if (collider == null) {
                collider = gameObject.AddComponent<BoxCollider2D> ();
            }

            collider.isTrigger = true;
        }
    }

    protected override void OnDestroy () {
        base.OnDestroy ();

        if (isAddedEnemyEventListeners) {
            EnemyModelBase.Died -= EnemyDiedHandler;
        }
    }

    protected override bool CheckValidTrigger (Collider2D collision) {
        if (collision.tag != GameVariable.PlayerTag) {
            return false;
        }

        return true;
    }

    public Collectable.Type GetCollectableType () {
        return Data.type;
    }

    public void StartCollectedAnim (Vector3 startPos, Action onAnimFinished = null) {
        baseGO.transform.position = startPos;
        baseGO.SetActive (true);

        Action onCollectAnimFinished = () => {
            Dispose ();
            onAnimFinished?.Invoke ();
        };

        FrameworkUtils.Instance.StartSingleAnim (animator, CollectAnimStateName, onCollectAnimFinished);
    }

    protected override void OnTriggered () {
        Collected?.Invoke (this);
    }

    #region Events

    private void EnemyDiedHandler (int enemyId) {
        if (Data.IsFromEnemy && Data.fromEnemyId == enemyId) {
            OnTriggered ();
        }
    }

    #endregion
}