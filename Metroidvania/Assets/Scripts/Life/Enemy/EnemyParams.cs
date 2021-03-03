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

    [SerializeField] private float _decelerationFactor;
    /// <summary>
    /// Set this variable from 0 to 1.<br />
    /// When decelerating to idle, the velocity will multiply this factor at each fixed update until the velocity is near to zero.<br />
    /// 0 = stop immediately, 1 = never stop
    /// </summary>
    public float DecelerationFactor => _decelerationFactor;

    [SerializeField] private bool _isDetectChar;
    /// <summary>
    /// If isDetectChar = true, the enemy idle until char go into the char detection area
    /// </summary>
    public bool IsDetectChar => _isDetectChar;

    [Header ("Chase Char")]
    [SerializeField] private bool _isChaseChar;
    /// <summary>
    /// If IsChaseChar = true, the enemy will chase the char if char comes near it
    /// </summary>
    public bool IsChaseChar => _isChaseChar;
    /// <summary>
    /// The period to change the direction of chasing char. In second. <br />
    /// Only work if IsChaseChar = true
    /// </summary>
    [SerializeField] private float _changeChaseCharDirPeriod;
    public float ChangeChaseCharDirPeriod => _changeChaseCharDirPeriod;

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