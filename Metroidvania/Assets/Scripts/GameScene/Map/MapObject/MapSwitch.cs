using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSwitch : MapInvisibleTriggerBase<MapData.SwitchData> {

    public static event Action<MapData.HiddenPathData> SwitchedOnEvent;

    public override void Init (MapData.SwitchData data) {
        base.Init (data);

        switch (data.GetSwitchType ()) {
            case MapEnum.SwitchType.Arrow:
                gameObject.tag = GameVariable.ArrowSwitchTag;
                break;
            case MapEnum.SwitchType.DropHit:
                gameObject.tag = GameVariable.DropHitSwitchTag;
                break;
            case MapEnum.SwitchType.Normal:
            default:
                gameObject.tag = GameVariable.DefaultTag;
                break;
        }
    }

    protected override bool CheckValidTrigger (Collider2D collision) {
        if (data.GetSwitchType () != MapEnum.SwitchType.Normal) {
            // Trigger by Arrow attack or drop hit attack
            return false;
        }

        // MapEnum.SwitchType.Normal
        if (collision.tag != GameVariable.PlayerTag) {
            return false;
        }

        return true;
    }

    protected override void OnTriggered () {
        SwitchedOnEvent?.Invoke (data.hiddenPath);
    }

    public void Trigger () {
        OnTriggered ();
    }
}