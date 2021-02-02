using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSwitch : MapInvisibleTriggerBase<MapData.SwitchData>, IMapTarget {

    /// <summary>
    /// Input :<br />
    /// MapSwitch : the MapSwitch instance that trigger the event<br />
    /// bool : isSwitchOn (false = switch off)
    /// </summary>
    public static event Action<MapSwitch, bool> SwitchedEvent;

    private bool isAddedEnemyEventListeners = false;
    private bool isSwitchedOn = false;
    private bool allowSwitch = true;

    public override void Init (MapData.SwitchData data) {
        base.Init (data);

        switch (data.switchType) {
            case MapEnum.SwitchType.Arrow:
                gameObject.tag = GameVariable.ArrowSwitchTag;
                break;
            case MapEnum.SwitchType.DropHit:
                gameObject.tag = GameVariable.DropHitSwitchTag;
                break;
            case MapEnum.SwitchType.OnOff:
            case MapEnum.SwitchType.Enemy:
            default:
                gameObject.tag = GameVariable.DefaultTag;
                break;
        }

        if (data.switchType == MapEnum.SwitchType.Enemy) {
            gameObject.SetActive (false);
            if (!isAddedEnemyEventListeners) {
                isAddedEnemyEventListeners = true;
                EnemyModelBase.DiedEvent += EnemyDied;
            }
        } else {
            gameObject.SetActive (true);
        }
    }

    protected override void OnDestroy () {
        base.OnDestroy ();

        if (isAddedEnemyEventListeners) {
            EnemyModelBase.DiedEvent -= EnemyDied;
        }
    }

    protected override bool CheckValidTrigger (Collider2D collision) {
        if (data.switchType != MapEnum.SwitchType.OnOff) {
            // Trigger by other ways
            return false;
        }

        // MapEnum.SwitchType.Normal
        if (collision.tag != GameVariable.PlayerTag) {
            return false;
        }

        return true;
    }

    protected override void OnTriggered () {
        if (!allowSwitch) {
            return;
        }

        allowSwitch = false;
        isSwitchedOn = !isSwitchedOn;
        SwitchedEvent?.Invoke (this, isSwitchedOn);
    }

    public void Trigger () {
        OnTriggered ();
    }

    public void FinishSwitched () {
        if (data.switchType == MapEnum.SwitchType.OnOff) {
            allowSwitch = true;
        }
    }

    private void EnemyDied (int enemyId) {
        if (data.switchType == MapEnum.SwitchType.Enemy && data.fromEnemyId == enemyId) {
            OnTriggered ();
        }
    }

    public MapEnum.SwitchType GetSwitchType () {
        return data.switchType;
    }

    public Vector2Int GetSwitchBasePos () {
        return data.switchBasePos;
    }

    public List<MapData.HiddenPathData> GetHiddenPathDataList () {
        return data.hiddenPaths;
    }

    #region IMapTarget

    public Vector2 GetTargetPos () {
        return transform.position;
    }

    #endregion
}