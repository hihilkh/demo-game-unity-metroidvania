using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostModel : EnemyModelBase {
    public override EnemyEnum.MovementType movementType => EnemyEnum.MovementType.Flying;
}