using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "CharParams", menuName = "ScriptableObjects/CharParams", order = 1)]
public class CharParams : ScriptableObject {
    [Header ("Life")]
    [SerializeField] private int _totalHP;
    public int TotalHP => _totalHP;

    [SerializeField] private float _invinciblePeriod;
    /// <summary>
    /// In second.
    /// </summary>
    public float InvinciblePeriod => _invinciblePeriod;

    [SerializeField] private float _beatBackInitSpeed;
    /// <summary>
    /// Magnitude only. The direction is base on hurt direction and derived LifeBase class
    /// </summary>
    public float BeatBackInitSpeed => _beatBackInitSpeed;

    [SerializeField] private float _beatBackPeriod;
    /// <summary>
    /// In second.
    /// </summary>
    public float BeatBackPeriod => _beatBackPeriod;

    [SerializeField] private float _dyingPeriod;
    /// <summary>
    /// In second.
    /// </summary>
    public float DyingPeriod => _dyingPeriod;

    [SerializeField] private float _hpRecoveryPeriod;
    /// <summary>
    /// Recover 1 hp for each hpRecoveryPeriod. In second.
    /// </summary>
    public float HPRecoveryPeriod => _hpRecoveryPeriod;

    [SerializeField] private float _stopCharMinWaitTime;
    /// <summary>
    /// The min wait time when stopping char in order to let most of the visual effect disappear. In second.
    /// </summary>
    public float StopCharMinWaitTime => _stopCharMinWaitTime;

    [Header ("Physics")]
    [SerializeField] private float _gravityScale;
    public float GravityScale => _gravityScale;

    [SerializeField] private float _minFallDownVelocity;
    public float MinFallDownVelocity => _minFallDownVelocity;

    [Header ("Walk")]
    [SerializeField] private float _walkingSpeed;
    public float WalkingSpeed => _walkingSpeed;

    [Header ("Slide")]
    [SerializeField] private float _slideDownVelocity;
    public float SlideDownVelocity => _slideDownVelocity;

    [Header ("Jump")]
    [SerializeField] private float _normalJumpInitSpeed;
    public float NormalJumpInitSpeed => _normalJumpInitSpeed;

    [SerializeField] private float _chargedJumpInitSpeed;
    public float ChargeJumpInitSpeed => _chargedJumpInitSpeed;

    [Header ("Dash")]
    [SerializeField] private float _dashingSpeed;
    public float DashingSpeed => _dashingSpeed;

    [SerializeField] private float _oneShotDashPeriod;
    public float OneShotDashPeriod => _oneShotDashPeriod;

    [SerializeField] private float _dashCoolDownPeriod;
    public float DashCoolDownPeriod => _dashCoolDownPeriod;

    [Header ("Hit")]
    [SerializeField] private float _hitCoolDownPeriod_Normal;
    public float HitCoolDownPeriod_Normal => _hitCoolDownPeriod_Normal;

    [SerializeField] private float _hitCoolDownPeriod_Charged;
    public float HitCoolDownPeriod_Charged => _hitCoolDownPeriod_Charged;

    [SerializeField] private float _hitCoolDownPeriod_Finishing;
    public float HitCoolDownPeriod_Finishing => _hitCoolDownPeriod_Finishing;

    [SerializeField] private float _hitCoolDownPeriod_Drop;
    public float HitCoolDownPeriod_Drop => _hitCoolDownPeriod_Drop;

    [SerializeField] private int _hitDP_Normal;
    public int HitDP_Normal => _hitDP_Normal;

    [SerializeField] private int _hitDP_Charged;
    public int HitDP_Charged => _hitDP_Charged;

    [SerializeField] private int _hitDP_Finishing;
    public int HitDP_Finishing => _hitDP_Finishing;

    [SerializeField] private int _hitDP_Drop;
    public int HitDP_Drop => _hitDP_Drop;

    [SerializeField] private float _dropHitVelocity;
    public float DropHitVelocity => _dropHitVelocity;

    [Header ("Arrow")]
    [SerializeField] private float _arrowCoolDownPeriod_Target;
    public float ArrowCoolDownPeriod_Target => _arrowCoolDownPeriod_Target;

    [SerializeField] private float _arrowCoolDownPeriod_Straight;
    public float ArrowCoolDownPeriod_Straight => _arrowCoolDownPeriod_Straight;

    [SerializeField] private float _arrowCoolDownPeriod_Triple;
    public float ArrowCoolDownPeriod_Triple => _arrowCoolDownPeriod_Triple;

    [SerializeField] private float _arrowInitialSpeed_Target;
    public float ArrowInitialSpeed_Target => _arrowInitialSpeed_Target;

    [SerializeField] private float _arrowInitialSpeed_Straight;
    public float ArrowInitialSpeed_Straight => _arrowInitialSpeed_Straight;

    [SerializeField] private float _arrowInitialSpeed_Triple;
    public float ArrowInitialSpeed_Triple => _arrowInitialSpeed_Triple;

    [SerializeField] private int _arrowDP_Target;
    public int ArrowDP_Target => _arrowDP_Target;

    [SerializeField] private int _arrowDP_Straight;
    public int ArrowDP_Straight => _arrowDP_Straight;

    [SerializeField] private int _arrowDP_Triple;
    public int ArrowDP_Triple => _arrowDP_Triple;

    [SerializeField] private float _targetArrowMaxInitShootGradient;
    /// <summary>
    /// tan(theta)
    /// </summary>
    public float TargetArrowMaxInitShootGradient => _targetArrowMaxInitShootGradient;

    [SerializeField] private float _targetArrowMaxTargetDistanceSquare;
    public float TargetArrowMaxTargetDistanceSquare => _targetArrowMaxTargetDistanceSquare;

    [SerializeField] private List<float> _tripleArrowShootingAngleList;
    /// <summary>
    /// In degree
    /// </summary>
    public List<float> TripleArrowShootingAngleList => _tripleArrowShootingAngleList;

    [Header ("Turn")]
    [SerializeField] private float _repelFromWallDistByTurn;
    public float RepelFromWallDistByTurn => _repelFromWallDistByTurn;

    [Header ("Collectable")]
    [SerializeField] private int _hpUpMagnitude;
    /// <summary>
    /// The magnitude for each collected HP Up collectable
    /// </summary>
    public int HPUpMagnitude => _hpUpMagnitude;

    [SerializeField] private int _powerUpMagnitude;
    /// <summary>
    /// The magnitude for each collected Power Up collectable
    /// </summary>
    public int PowerUpMagnitude => _powerUpMagnitude;

    [Header ("Boss")]
    [SerializeField] private int _bossTotalHP;
    public int BossTotalHP => _bossTotalHP;

    [SerializeField] private int _collisionDP;
    public int CollisionDP => _collisionDP;

    [SerializeField] private float _bossDecideActionPeriod;
    public float BossDecideActionPeriod => _bossDecideActionPeriod;

    [SerializeField] private float _bossTurnProbability;
    public float BossTurnProbability => _bossTurnProbability;

    [SerializeField] private float _bossHitAttackHorizontalHalfRange;
    public float BossHitAttackHorizontalHalfRange => _bossHitAttackHorizontalHalfRange;

    [SerializeField] private float _bossDashAttackHorizontalHalfRange;
    public float BossDashAttackHorizontalHalfRange => _bossDashAttackHorizontalHalfRange;

    [SerializeField] private float _bossAllowAttackVerticalHalfRange;
    public float BossAllowAttackVerticalHalfRange => _bossAllowAttackVerticalHalfRange;

    [Header ("Audio Clips")]
    [SerializeField] private AudioClip _walkAudioClip;
    public AudioClip WalkAudioClip => _walkAudioClip;

    [SerializeField] private AudioClip _dashAudioClip;
    public AudioClip DashAudioClip => _dashAudioClip;

    [SerializeField] private AudioClip _jumpAudioClip;
    public AudioClip JumpAudioClip => _jumpAudioClip;

    [SerializeField] private AudioClip _jumpChargingAudioClip;
    public AudioClip JumpChargingAudioClip => _jumpChargingAudioClip;

    [SerializeField] private AudioClip _dropHitChargingAudioClip;
    public AudioClip DropHitChargingAudioClip => _dropHitChargingAudioClip;

    [Header ("Dev Only")]
    [SerializeField] private LifeEnum.HorizontalDirection _initDirection;
    public LifeEnum.HorizontalDirection InitDirection => _initDirection;

    [SerializeField] private bool _isUseDebugCommandSettings;
    public bool IsUseDebugCommandSettings => _isUseDebugCommandSettings;

    [SerializeField] private CharEnum.Command _groundTapCommand;
    public CharEnum.Command GroundTapCommand => _groundTapCommand;

    [SerializeField] private CharEnum.Command _groundHoldCommand;
    public CharEnum.Command GroundHoldCommand => _groundHoldCommand;

    [SerializeField] private CharEnum.Command _groundReleaseCommand;
    public CharEnum.Command GroundReleaseCommand => _groundReleaseCommand;

    [SerializeField] private CharEnum.Command _airTapCommand;
    public CharEnum.Command AirTapCommand => _airTapCommand;

    [SerializeField] private CharEnum.Command _airHoldCommand;
    public CharEnum.Command AirHoldCommand => _airHoldCommand;

    [SerializeField] private CharEnum.Command _airReleaseCommand;
    public CharEnum.Command AirReleaseCommand => _airReleaseCommand;

}