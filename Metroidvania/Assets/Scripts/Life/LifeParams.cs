using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeParams : ScriptableObject {
    [Header ("LifeParams")]
    [SerializeField] private int _totalHP;
    public int totalHP => _totalHP;

    [SerializeField] private float _invincibleTime;
    /// <summary>
    /// In second.
    /// </summary>
    public float invincibleTime => _invincibleTime;

    [SerializeField] private float _beatBackInitSpeed;
    /// <summary>
    /// Magnitude only. The direction is base on hurt direction and derived LifeBase class
    /// </summary>
    public float beatBackInitSpeed => _beatBackInitSpeed;
}
