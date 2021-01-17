using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "EnemyParams", menuName = "ScriptableObjects/EnemyParams", order = 1)]
public class EnemyParams : ScriptableObject {
    [Header ("Life")]
    [SerializeField] private int _totalHP;
    public int totalHP => _totalHP;

    [Header ("Movement")]
    [SerializeField] private float _movementSpeed;
    /// <summary>
    /// For Walking MovementType, it is the horizontal walking speed. For Flying MovementType, it is the flying speed.
    /// </summary>
    public float movementSpeed => _movementSpeed;

    [Header ("Jump")]
    [SerializeField] private float _jumpInitSpeed;
    public float jumpInitSpeed => _jumpInitSpeed;
}