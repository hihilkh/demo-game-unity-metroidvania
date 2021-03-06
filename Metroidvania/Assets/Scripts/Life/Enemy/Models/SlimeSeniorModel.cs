using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeSeniorModel : EnemyModelBase {
    public override EnemyEnum.EnemyType EnemyType => EnemyEnum.EnemyType.SlimeSenior;
    public override EnemyEnum.MovementType MovementType => EnemyEnum.MovementType.Walking;
}