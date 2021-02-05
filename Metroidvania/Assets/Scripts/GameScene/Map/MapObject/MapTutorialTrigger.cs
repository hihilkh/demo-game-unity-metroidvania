using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTutorialTrigger : MapInvisibleTriggerBase<MapData.TutorialData> {

    public static event Action<TutorialEnum.GameTutorialType> TriggeredTutorialEvent;

    public override void Init (MapData.TutorialData data) {
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
        TriggeredTutorialEvent?.Invoke (data.type);
    }
}