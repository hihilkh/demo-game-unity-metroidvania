using UnityEngine;

public abstract class MapInvisibleTriggerBase<T> : MapTriggerBase<T> where T : MapData.InvisibleTriggerData {
    private BoxCollider2D triggerCollider;

    public override void Init (T data) {
        this.Data = data;

        transform.position = new Vector3 (data.collider.pos.x, data.collider.pos.y, GameVariable.GeneralMapItemPosZ);

        triggerCollider = gameObject.GetComponent<BoxCollider2D> ();
        if (triggerCollider == null) {
            triggerCollider = gameObject.AddComponent<BoxCollider2D> ();
        }

        triggerCollider.isTrigger = true;
        triggerCollider.size = data.collider.size;
    }
}