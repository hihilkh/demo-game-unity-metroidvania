using System.Collections;
using System.Collections.Generic;
using HihiFramework.Core;
using UnityEngine;

public abstract class SpecialSceneEventDisposableBase : MapDisposableBase {

    protected virtual bool IsDisposeWhenSpecialSceneEventSubSceneChanging => true;

    protected override void RegisterDisposableEventHandlers () {
        base.RegisterDisposableEventHandlers ();
        MissionEventManager.SpecialSceneEventSubSceneChanging += SpecialSceneEventSubSceneChangingHandler;
    }

    protected override void UnregisterDisposableEventHandlers () {
        base.UnregisterDisposableEventHandlers ();
        MissionEventManager.SpecialSceneEventSubSceneChanging -= SpecialSceneEventSubSceneChangingHandler;
    }

    private void SpecialSceneEventSubSceneChangingHandler () {
        if (IsDisposeWhenSpecialSceneEventSubSceneChanging) {
            Dispose ();
        }
    }
}
