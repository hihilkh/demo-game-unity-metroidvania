using System;
using System.Collections.Generic;
using HihiFramework.Core;
using UnityEngine;

public class MapSwitch : MapInvisibleTriggerBase<MapData.SwitchData>, IMapTarget {

    /// <summary>
    /// Input :<br />
    /// MapSwitch : the MapSwitch instance that trigger the event<br />
    /// bool : isSwitchOn (false = switch off)
    /// </summary>
    public static event Action<MapSwitch, bool> Switched;

    private bool isAddedEnemyEventHandlers = false;
    private bool isSwitchedOn = false;
    private bool allowSwitch = true;

    public override void Init (MapData.SwitchData data) {
        base.Init (data);

        switch (data.switchType) {
            case MapEnum.SwitchType.Arrow:
            case MapEnum.SwitchType.Tree:
                gameObject.tag = GameVariable.ArrowSwitchTag;
                gameObject.layer = GameVariable.DefaultLayer;
                break;
            case MapEnum.SwitchType.DropHit:
                gameObject.tag = GameVariable.DropHitSwitchTag;
                gameObject.layer = GameVariable.DefaultLayer;
                break;
            case MapEnum.SwitchType.Enemy:
            case MapEnum.SwitchType.MissionEvent:
                gameObject.tag = GameVariable.DefaultTag;
                gameObject.layer = GameVariable.DefaultLayer;
                break;
            case MapEnum.SwitchType.OnOff:
            default:
                gameObject.tag = GameVariable.DefaultTag;
                gameObject.layer = GameVariable.PlayerInteractableLayer;
                break;
        }

        if (data.switchType == MapEnum.SwitchType.Enemy) {
            gameObject.SetActive (false);
            if (!isAddedEnemyEventHandlers) {
                isAddedEnemyEventHandlers = true;
                EnemyModelBase.Died += EnemyDiedHandler;
            }
        } else if (data.switchType == MapEnum.SwitchType.MissionEvent) {
            gameObject.SetActive (false);
        } else {
            gameObject.SetActive (true);
        }
    }

    protected override void OnDestroy () {
        base.OnDestroy ();

        if (isAddedEnemyEventHandlers) {
            EnemyModelBase.Died -= EnemyDiedHandler;
        }
    }

    protected override bool CheckValidTrigger (Collider2D collision) {
        if (Data.switchType != MapEnum.SwitchType.OnOff) {
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
        Switched?.Invoke (this, isSwitchedOn);
    }

    public void Trigger () {
        OnTriggered ();
    }

    public void SwitchingFinished () {
        if (Data.switchType == MapEnum.SwitchType.OnOff) {
            allowSwitch = true;
        }
    }

    public int GetSwitchId () {
        return Data.id;
    }

    public MapEnum.SwitchType GetSwitchType () {
        return Data.switchType;
    }

    public Vector2Int GetSwitchBasePos () {
        return Data.switchBasePos;
    }

    public List<MapData.HiddenPathData> GetHiddenPathDataList () {
        return Data.hiddenPaths;
    }

    #region Events

    private void EnemyDiedHandler (int enemyId) {
        if (Data.switchType == MapEnum.SwitchType.Enemy && Data.fromEnemyId == enemyId) {
            OnTriggered ();
        }
    }

    #endregion

    #region IMapTarget

    public Vector2 GetTargetPos () {
        return transform.position;
    }

    #endregion
}