using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "CharParams", menuName = "ScriptableObjects/CharParams", order = 1)]
public class CharParams : LifeParams {
    [SerializeField] private float _beatBackPeriod;
    /// <summary>
    /// In second.
    /// </summary>
    public float beatBackPeriod => _beatBackPeriod;

    [SerializeField] private float _dyingPeriod;
    /// <summary>
    /// In second.
    /// </summary>
    public float dyingPeriod => _dyingPeriod;

    [SerializeField] private float _hpRecoveryPeriod;
    /// <summary>
    /// Recover 1 hp for each hpRecoveryPeriod. In second.
    /// </summary>
    public float hpRecoveryPeriod => _hpRecoveryPeriod;

    [Header ("Physics")]
    [SerializeField] private float _gravityScale;
    public float gravityScale => _gravityScale;

    [SerializeField] private float _minFallDownVelocity;
    public float minFallDownVelocity => _minFallDownVelocity;

    [Header ("Walk")]
    [SerializeField] private float _walkingSpeed;
    public float walkingSpeed => _walkingSpeed;

    [Header ("Slide")]
    [SerializeField] private float _slideDownVelocity;
    public float slideDownVelocity => _slideDownVelocity;

    [Header ("Jump")]
    [SerializeField] private float _normalJumpInitSpeed;
    public float normalJumpInitSpeed => _normalJumpInitSpeed;

    [SerializeField] private float _chargedJumpInitSpeed;
    public float chargeJumpInitSpeed => _chargedJumpInitSpeed;

    [Header ("Dash")]
    [SerializeField] private float _dashingSpeed;
    public float dashingSpeed => _dashingSpeed;

    [SerializeField] private float _oneShotDashPeriod;
    public float oneShotDashPeriod => _oneShotDashPeriod;

    [SerializeField] private float _dashCoolDownPeriod;
    public float dashCoolDownPeriod => _dashCoolDownPeriod;

    [Header ("Hit")]
    [SerializeField] private float _hitCoolDownPeriod_Normal;
    public float hitCoolDownPeriod_Normal => _hitCoolDownPeriod_Normal;

    [SerializeField] private float _hitCoolDownPeriod_Charged;
    public float hitCoolDownPeriod_Charged => _hitCoolDownPeriod_Charged;

    [SerializeField] private float _hitCoolDownPeriod_Finishing;
    public float hitCoolDownPeriod_Finishing => _hitCoolDownPeriod_Finishing;

    [SerializeField] private float _hitCoolDownPeriod_Drop;
    public float hitCoolDownPeriod_Drop => _hitCoolDownPeriod_Drop;

    [SerializeField] private int _hitDP_Normal;
    public int hitDP_Normal => _hitDP_Normal;

    [SerializeField] private int _hitDP_Charged;
    public int hitDP_Charged => _hitDP_Charged;

    [SerializeField] private int _hitDP_Finishing;
    public int hitDP_Finishing => _hitDP_Finishing;

    [SerializeField] private int _hitDP_Drop;
    public int hitDP_Drop => _hitDP_Drop;

    [SerializeField] private float _dropHitVelocity;
    public float dropHitVelocity => _dropHitVelocity;

    [Header ("Arrow")]
    [SerializeField] private float _arrowCoolDownPeriod_Target;
    public float arrowCoolDownPeriod_Target => _arrowCoolDownPeriod_Target;

    [SerializeField] private float _arrowCoolDownPeriod_Straight;
    public float arrowCoolDownPeriod_Straight => _arrowCoolDownPeriod_Straight;

    [SerializeField] private float _arrowCoolDownPeriod_Triple;
    public float arrowCoolDownPeriod_Triple => _arrowCoolDownPeriod_Triple;

    [SerializeField] private float _arrowInitialSpeed_Target;
    public float arrowInitialSpeed_Target => _arrowInitialSpeed_Target;

    [SerializeField] private float _arrowInitialSpeed_Straight;
    public float arrowInitialSpeed_Straight => _arrowInitialSpeed_Straight;

    [SerializeField] private float _arrowInitialSpeed_Triple;
    public float arrowInitialSpeed_Triple => _arrowInitialSpeed_Triple;

    [SerializeField] private int _arrowDP_Target;
    public int arrowDP_Target => _arrowDP_Target;

    [SerializeField] private int _arrowDP_Straight;
    public int arrowDP_Straight => _arrowDP_Straight;

    [SerializeField] private int _arrowDP_Triple;
    public int arrowDP_Triple => _arrowDP_Triple;

    [SerializeField] private float _targetArrowMaxElevationAngle;
    /// <summary>
    /// In degree
    /// </summary>
    public float targetArrowMaxElevationAngle => _targetArrowMaxElevationAngle;

    [SerializeField] private List<float> _tripleArrowShootingAngleList;
    /// <summary>
    /// In degree
    /// </summary>
    public List<float> tripleArrowShootingAngleList => _tripleArrowShootingAngleList;

    [Header ("Turn")]
    [SerializeField] private float _repelFromWallDistByTurn;
    public float repelFromWallDistByTurn => _repelFromWallDistByTurn;

    [Header ("Dev Only")]
    [SerializeField] private LifeEnum.HorizontalDirection _initDirection;
    public LifeEnum.HorizontalDirection initDirection => _initDirection;

    [SerializeField] private CharEnum.Command _groundTapCommand;
    public CharEnum.Command groundTapCommand => _groundTapCommand;

    [SerializeField] private CharEnum.Command _groundHoldCommand;
    public CharEnum.Command groundHoldCommand => _groundHoldCommand;

    [SerializeField] private CharEnum.Command _groundReleaseCommand;
    public CharEnum.Command groundReleaseCommand => _groundReleaseCommand;

    [SerializeField] private CharEnum.Command _airTapCommand;
    public CharEnum.Command airTapCommand => _airTapCommand;

    [SerializeField] private CharEnum.Command _airHoldCommand;
    public CharEnum.Command airHoldCommand => _airHoldCommand;

    [SerializeField] private CharEnum.Command _airReleaseCommand;
    public CharEnum.Command airReleaseCommand => _airReleaseCommand;

}