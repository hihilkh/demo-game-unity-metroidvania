using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "TankParams", menuName = "ScriptableObjects/TankParams", order = 1)]
public class TankParams : EnemyParams {
    [Header ("Tank Specific")]
    [SerializeField] private float _touchWallWaitPeriod;
    public float TouchWallWaitPeriod => _touchWallWaitPeriod;
}
