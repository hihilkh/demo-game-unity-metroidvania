using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureBoxModel : EnemyModelBase {
    public override EnemyEnum.EnemyType EnemyType => EnemyEnum.EnemyType.TreasureBox;
    public override EnemyEnum.MovementType MovementType => EnemyEnum.MovementType.Flying;
}