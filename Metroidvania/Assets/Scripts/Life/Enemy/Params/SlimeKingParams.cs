using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "SlimeKingParams", menuName = "ScriptableObjects/SlimeKingParams", order = 1)]
public class SlimeKingParams : EnemyParams {
    [Header ("SlimeKing Specific")]
    [SerializeField] private Vector2 _attackJumpInitVelocity;
    public Vector2 AttackJumpInitVelocity => _attackJumpInitVelocity;

    [SerializeField] private float _attackHorizontalHalfRange;
    public float AttackHorizontalHalfRange => _attackHorizontalHalfRange;
}
