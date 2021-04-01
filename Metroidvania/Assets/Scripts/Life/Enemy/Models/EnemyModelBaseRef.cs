using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyModelBaseRef : MonoBehaviour {
    [SerializeField] private Animator _animator;
    public Animator Animator => _animator;

    [SerializeField] private EnemyAnimSMBUtils _animUtils;
    public EnemyAnimSMBUtils AnimUtils => _animUtils;

    [SerializeField] private Transform _targetRefPoint;
    public Transform TargetRefPoint => _targetRefPoint;

    [SerializeField] private EnemyCharDetection _charDetection;
    public EnemyCharDetection CharDetection => _charDetection;

    [SerializeField] private EnemyAudioUtils _audioUtils;
    public EnemyAudioUtils AudioUtils => _audioUtils;
}
