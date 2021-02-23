using System;
using UnityEngine;

public class MissionEventTrigger : MapInvisibleTriggerBase<MapData.MissionEventData> {

    public static event Action<MissionEventEnum.EventType> MissionEventTriggered;

    public override void Init (MapData.MissionEventData data) {
        base.Init (data);

        gameObject.layer = GameVariable.PlayerInteractableLayer;
    }

    protected override bool CheckValidTrigger (Collider2D collision) {
        if (collision.tag != GameVariable.PlayerTag) {
            return false;
        }

        return true;
    }

    protected override void OnTriggered () {
        MissionEventTriggered?.Invoke (Data.type);
        Dispose ();
    }
}