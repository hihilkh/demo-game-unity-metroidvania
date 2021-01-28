using System;
using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

public class MapExit : MapInvisibleTriggerBase<MapData.ExitData> {
    /// <summary>
    /// Input :<br />
    /// int : the entry id that the exit point goes to
    /// </summary>
    public static event Action<int> ExitedEvent;

    protected override bool CheckValidTrigger (Collider2D collision) {
        if (collision.tag != GameVariable.PlayerTag) {
            return false;
        }

        return true;
    }

    protected override void OnTriggered () {
        ExitedEvent?.Invoke (data.toEntryId);
    }
}