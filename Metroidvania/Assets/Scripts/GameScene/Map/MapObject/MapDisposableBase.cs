using UnityEngine;

public abstract class MapDisposableBase : MonoBehaviour {

    protected virtual bool IsDisposeWhenMapReset => true;

    private bool isAddedEventListeners = false;

    protected abstract void Dispose ();

    protected virtual void Awake () {
        AddEventListeners ();
    }

    protected virtual void OnDestroy () {
        RemoveEventListeners ();
    }

    private void AddEventListeners () {
        if (!isAddedEventListeners) {
            isAddedEventListeners = true;

            MapManager.MapReseting += MapResetingHandler;
        }
    }

    private void RemoveEventListeners () {
        if (isAddedEventListeners) {
            MapManager.MapReseting -= MapResetingHandler;

            isAddedEventListeners = false;
        }
    }

    private void MapResetingHandler () {
        if (IsDisposeWhenMapReset) {
            Dispose ();
        }
    }
}
