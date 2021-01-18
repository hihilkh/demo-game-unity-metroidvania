using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "EnemyParams", menuName = "ScriptableObjects/EnemyParams", order = 1)]
public class EnemyParams : ScriptableObject {
    [Header ("Life")]
    [SerializeField] private int _totalHP;
    public int totalHP => _totalHP;

    [SerializeField] private float _invincibleTime;
    /// <summary>
    /// In second.
    /// </summary>
    public float invincibleTime => _invincibleTime;

    [SerializeField] private float _beatBackInitSpeed;
    /// <summary>
    /// Magnitude only. The direction is base on MovementType and collisionNormal
    /// </summary>
    public float beatBackInitSpeed => _beatBackInitSpeed;

    [Header ("Movement")]
    [SerializeField] private float _movementSpeed;
    /// <summary>
    /// For Walking MovementType, it is the horizontal walking speed. For Flying MovementType, it is the flying speed.
    /// </summary>
    public float movementSpeed => _movementSpeed;

    [Header ("Jump")]
    [SerializeField] private float _jumpInitSpeed;
    /// <summary>
    /// Only work in Walking MovementType.
    /// </summary>
    public float jumpInitSpeed => _jumpInitSpeed;

    [SerializeField] private float _recursiveJumpPeriod;
    /// <summary>
    /// In second. Smaller than zero means do not jump recursively. Only work in Walking MovementType.
    /// </summary>
    public float recursiveJumpPeriod => _recursiveJumpPeriod;
}