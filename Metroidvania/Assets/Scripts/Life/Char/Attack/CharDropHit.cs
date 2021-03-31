using HihiFramework.Core;
using UnityEngine;

public class CharDropHit : CharHitBase {
    protected override int BaseDP => Params.HitDP_Drop;

    [SerializeField] private AudioClip dropHitAudioClip;

    public override void StartAttack (Transform refPoint, LifeEnum.HorizontalDirection direction, float charHorizontalSpeed, bool isPlayerAttack, int additionalDP) {
        base.StartAttack (refPoint, direction, charHorizontalSpeed, isPlayerAttack, additionalDP);

        FrameworkUtils.InsertChildrenToParent (refPoint, gameObject, false, false, -1, false);

        // Do not attach an audio source and play by self to prevent SFX is cut by DestroySelf ()
        AudioManager.Instance.PlaySFX (dropHitAudioClip);

        AttackTrigger.HitDropHitSwitch += HitDropHitSwitch;
    }

    private void HitDropHitSwitch (MapSwitch mapSwitch) {
        mapSwitch.Trigger ();
    }
}