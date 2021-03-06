using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "GhostKingParams", menuName = "ScriptableObjects/GhostKingParams", order = 1)]
public class GhostKingParams : EnemyParams {
    [Header ("GhostKing Specific")]
    [SerializeField] private float _attackMaxMovementSpeed;
    public float AttackMaxMovementSpeed => _attackMaxMovementSpeed;

    [SerializeField] private float _attackAcceleration;
    public float AttackAcceleration => _attackAcceleration;

    [SerializeField] private float _attackCoolDownPeriod;
    public float AttackCoolDownPeriod => _attackCoolDownPeriod;

    [SerializeField] private float _beforeAttackPeriod;
    public float BeforeAttackPeriod => _beforeAttackPeriod;
}
