using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

public abstract class MapTriggerBase<T> : MapDisposableBase {

    protected T data;

    public abstract void Init (T data);
    protected abstract bool CheckValidTrigger (Collider2D collision);
    protected abstract void OnTriggered ();

    private void OnTriggerEnter2D (Collider2D collision) {
        if (data == null) {
            Log.PrintError ("Not yet initialized. Please check.");
            return;
        }

        if (!CheckValidTrigger (collision)) {
            return;
        }

        OnTriggered ();
    }

    #region MapDisposableBase

    protected override void Dispose () {
        if (gameObject != null) {
            Destroy (gameObject);
        }
    }

    #endregion
}