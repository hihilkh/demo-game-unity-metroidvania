using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeModel : EnemyModelBase {
    protected override int totalHP => enemyParams.totalHP;
    public override EnemyEnum.MovementType movementType => EnemyEnum.MovementType.Walking;

    public void Start () {
        Init (baseTransform.position, LifeEnum.HorizontalDirection.Right);

        StartCoroutine (Test ());
    }

    private IEnumerator Test () {
        yield return new WaitForSeconds (1F);

        Jump ();
    }
}