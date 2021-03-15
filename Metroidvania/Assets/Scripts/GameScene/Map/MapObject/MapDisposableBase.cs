using UnityEngine;

public abstract class MapDisposableBase : MonoBehaviour {

    protected virtual bool IsDisposeWhenMapReset => true;

    private bool isAddedDisposableEventHandlers = false;

    protected abstract void Dispose ();

    protected virtual void Awake () {
        AddDisposableEventHandlers ();
    }

    protected virtual void OnDestroy () {
        RemoveDisposableEventHandlers ();
    }

    private void AddDisposableEventHandlers () {
        if (!isAddedDisposableEventHandlers) {
            isAddedDisposableEventHandlers = true;

            RegisterDisposableEventHandlers ();
        }
    }

    private void RemoveDisposableEventHandlers () {
        if (isAddedDisposableEventHandlers) {
            UnregisterDisposableEventHandlers ();

            isAddedDisposableEventHandlers = false;
        }
    }

    protected virtual void RegisterDisposableEventHandlers () {
        MapManager.MapReseting += MapResetingHandler;
    }

    protected virtual void UnregisterDisposableEventHandlers () {
        MapManager.MapReseting -= MapResetingHandler;
    }

    private void MapResetingHandler () {
        if (IsDisposeWhenMapReset) {
            Dispose ();
        }
    }
}
