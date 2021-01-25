using System;
using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

public class ExitPointTrigger : MonoBehaviour {
    /// <summary>
    /// Input :<br />
    /// int : the entry id that the exit point goes to
    /// </summary>
    public event Action<int> exitEvent;

    private int toEntryId = -1;

    private void OnDestroy () {
        exitEvent = null;
    }

    public void Init (int toEntryId, Action<int> exitEventHandler) {
        this.toEntryId = toEntryId;
        this.exitEvent += exitEventHandler;
    }

    private void OnTriggerEnter2D (Collider2D collision) {
        if (collision.tag != GameVariable.PlayerTag) {
            return;
        }

        if (toEntryId < 0) {
            Log.PrintError ("Not yet assigned toEntryId. Please check.");
            return;
        }

        exitEvent?.Invoke (toEntryId);
    }
}