using System;
using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

public class MapCollectableObject : MapTriggerBase<MapData.CollectableData> {
    public static event Action<MapCollectableObject> CollectedEvent;

    [SerializeField] private GameObject baseGO;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer baseSpriteRenderer;
    [SerializeField] private SpriteRenderer additionalSpriteRenderer;

    private const string CollectAnimStateName = "Collect";
    private bool isAddedEnemyEventListeners = false;

    public override void Init (MapData.CollectableData data) {
        this.data = data;

        var collectable = CollectableManager.GetCollectable (data.type);
        if (collectable == null) {
            baseGO.SetActive (false);
            Log.PrintError ("Cannot init map collectable object : Collectable is null.", LogType.MapData);
            return;
        }

        Sprite icon = Resources.Load<Sprite> (collectable.GetIconResourcesName ());
        if (collectable.isWithCircleFrame) {
            additionalSpriteRenderer.sprite = icon;
        } else {
            baseSpriteRenderer.sprite = icon;
        }

        if (data.isFromEnemy) {
            baseGO.SetActive (false);
            if (!isAddedEnemyEventListeners) {
                isAddedEnemyEventListeners = true;
                EnemyModelBase.DiedEvent += EnemyDied;
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

    public Collectable.Type GetCollectableType () {
        return data.type;
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
}