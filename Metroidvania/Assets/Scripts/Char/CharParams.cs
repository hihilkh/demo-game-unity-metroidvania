using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "CharParams", menuName = "ScriptableObjects/CharParams", order = 1)]
public class CharParams : ScriptableObject {
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

    [SerializeField] private float _dropHitVelocity;
    public float dropHitVelocity => _dropHitVelocity;

    [Header ("Arrow")]
    [SerializeField] private float _arrowCoolDownPeriod_Target;
    public float arrowCoolDownPeriod_Target => _arrowCoolDownPeriod_Target;

    [SerializeField] private float _arrowCoolDownPeriod_Straight;
    public float arrowCoolDownPeriod_Straight => _arrowCoolDownPeriod_Straight;

    [SerializeField] private float _arrowCoolDownPeriod_Triple;
    public float arrowCoolDownPeriod_Triple => _arrowCoolDownPeriod_Triple;

    [Header ("Turn")]
    [SerializeField] private float _repelFromWallDistByTurn;
    public float repelFromWallDistByTurn => _repelFromWallDistByTurn;
}