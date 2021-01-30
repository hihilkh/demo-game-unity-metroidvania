using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MapDisposableBase : MonoBehaviour {

    protected virtual bool isDisposeWhenMapReset => true;

    private bool isAddedEventListeners = false;

    protected virtual void Awake () {
        AddEventListeners ();
    }

    protected virtual void OnDestroy () {
        RemoveEventListeners ();
    }

    private void AddEventListeners () {
        if (!isAddedEventListeners) {
            isAddedEventListeners = true;

            MapManager.MapResetingEvent += CheckDispose;
        }
    }

    private void RemoveEventListeners () {
        if (isAddedEventListeners) {
            MapManager.MapResetingEvent -= CheckDispose;

            isAddedEventListeners = false;
        }
    }

    private void CheckDispose () {
        if (isDisposeWhenMapReset) {
            Dispose ();
        }
    }

    protected abstract void Dispose ();
}
