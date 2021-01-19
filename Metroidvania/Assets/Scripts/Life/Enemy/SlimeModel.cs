using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeModel : EnemyModelBase {
    public override EnemyEnum.MovementType movementType => EnemyEnum.MovementType.Walking;

    private void Start () {
        Init (baseTransform.position, LifeEnum.HorizontalDirection.Right);
    }
}