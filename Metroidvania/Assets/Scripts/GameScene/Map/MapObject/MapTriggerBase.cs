using HihiFramework.Core;
using UnityEngine;

public abstract class MapTriggerBase<T> : MapDisposableBase {

    protected T Data { get; set; }

    public abstract void Init (T data);
    protected abstract bool CheckValidTrigger (Collider2D collision);
    protected abstract void OnTriggered ();

    private void OnTriggerEnter2D (Collider2D collision) {
        if (!GameSceneManager.IsGameStarted) {
            return;
        }

        if (Data == null) {
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