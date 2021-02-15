﻿using System;
using UnityEngine;

public class MapExit : MapInvisibleTriggerBase<MapData.ExitData> {
    /// <summary>
    /// Input :<br />
    /// int : the entry id that the exit point goes to
    /// </summary>
    public static event Action<int> ExitReached;

    protected override bool IsDisposeWhenMapReset => false;

    public override void Init (MapData.ExitData data) {
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
        ExitReached?.Invoke (Data.toEntryId);
    }
}