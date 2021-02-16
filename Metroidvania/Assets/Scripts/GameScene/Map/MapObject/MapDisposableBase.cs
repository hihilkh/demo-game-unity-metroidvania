using UnityEngine;

public abstract class MapDisposableBase : MonoBehaviour {

    protected virtual bool IsDisposeWhenMapReset => true;

    private bool isAddedEventHandlers = false;

    protected abstract void Dispose ();

    protected virtual void Awake () {
        AddEventHandlers ();
    }

    protected virtual void OnDestroy () {
        RemoveEventHandlers ();
    }

    private void AddEventHandlers () {
        if (!isAddedEventHandlers) {
            isAddedEventHandlers = true;

            MapManager.MapReseting += MapResetingHandler;
        }
    }

    private void RemoveEventHandlers () {
        if (isAddedEventHandlers) {
            MapManager.MapReseting -= MapResetingHandler;

            isAddedEventHandlers = false;
        }
    }

    private void MapResetingHandler () {
        if (IsDisposeWhenMapReset) {
            Dispose ();
        }
    }
}
