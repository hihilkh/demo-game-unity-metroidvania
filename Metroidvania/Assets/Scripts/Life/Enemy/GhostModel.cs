public class GhostModel : EnemyModelBase {
    public override EnemyEnum.EnemyType EnemyType => EnemyEnum.EnemyType.Ghost;
    public override EnemyEnum.MovementType MovementType => EnemyEnum.MovementType.Flying;
}