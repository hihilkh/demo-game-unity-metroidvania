using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSwitch : MapInvisibleTriggerBase<MapData.SwitchData>, IMapTarget {

    public static event Action<MapSwitch> SwitchedOnEvent;

    public override void Init (MapData.SwitchData data) {
        base.Init (data);

        switch (data.switchType) {
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
        if (data.switchType != MapEnum.SwitchType.Normal) {
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
        SwitchedOnEvent?.Invoke (this);
    }

    public void Trigger () {
        OnTriggered ();
    }

    public MapData.HiddenPathData GetHiddenPathData () {
        return data.hiddenPath;
    }

    #region IMapTarget

    public Vector2 GetTargetPos () {
        return transform.position;
    }

    #endregion
}