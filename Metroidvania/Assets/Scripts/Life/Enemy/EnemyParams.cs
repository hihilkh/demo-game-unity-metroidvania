using UnityEngine;

[CreateAssetMenu (fileName = "EnemyParams", menuName = "ScriptableObjects/EnemyParams", order = 1)]
public class EnemyParams : ScriptableObject {
    [Header ("Life")]
    [SerializeField] private int _totalHP;
    public int TotalHP => _totalHP;

    [SerializeField] private float _invinciblePeriod;
    /// <summary>
    /// In second.<br />
    /// If InvinciblePeriod = 0, do not trigger invincible.
    /// </summary>
    public float InvinciblePeriod => _invinciblePeriod;

    [SerializeField] private float _beatBackInitSpeed;
    /// <summary>
    /// Magnitude only. The direction is base on hurt direction and derived LifeBase class.<br />
    /// If BeatBackInitSpeed = 0, do not trigger beat back.
    /// </summary>
    public float BeatBackInitSpeed => _beatBackInitSpeed;

    [Header ("Movement")]
    [SerializeField] private float _maxMovementSpeed;
    /// <summary>
    /// For Walking MovementType, it is the horizontal walking speed. For Flying MovementType, it is the flying speed.
    /// </summary>
    public float MaxMovementSpeed => _maxMovementSpeed;

    [SerializeField] private bool _isSpeedAccelerate;
    /// <summary>
    /// If IsSpeedAccelerate = false, the speed would have shape changes
    /// </summary>
    public bool IsSpeedAccelerate => _isSpeedAccelerate;

    [SerializeField] private float _acceleration;
    /// <summary>
    /// Only work if IsSpeedAccelerate = true
    /// </summary>
    public float Acceleration => _acceleration;

    [Header ("Jump")]
    [SerializeField] private float _jumpInitSpeed;
    /// <summary>
    /// Only work in Walking MovementType.
    /// </summary>
    public float JumpInitSpeed => _jumpInitSpeed;

    [SerializeField] private bool _isJumpRecursively;
    /// <summary>
    /// Only work in Walking MovementType.
    /// </summary>
    public bool IsJumpRecursively => _isJumpRecursively;

    [SerializeField] private float _recursiveJumpPeriod;
    /// <summary>
    /// In second. Only work in Walking MovementType and IsSpeedAccelerate = true
    /// </summary>
    public float RecursiveJumpPeriod => _recursiveJumpPeriod;

    [Header ("Attack")]
    [SerializeField] private int _collisionDP;
    public int CollisionDP => _collisionDP;
}