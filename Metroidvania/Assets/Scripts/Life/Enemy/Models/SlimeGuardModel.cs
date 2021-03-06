using UnityEngine;

public class SlimeGuardModel : EnemyModelBase {
    [SerializeField] private Collider2D arrowDefendCollider;

    public override EnemyEnum.EnemyType EnemyType => EnemyEnum.EnemyType.SlimeGuard;
    public override EnemyEnum.MovementType MovementType => EnemyEnum.MovementType.Walking;

    protected override void StartInvincible () {
        base.StartInvincible ();

        arrowDefendCollider.enabled = false;
    }

    protected override void StopInvincible () {
        base.StopInvincible ();

        arrowDefendCollider.enabled = true;
    }
}