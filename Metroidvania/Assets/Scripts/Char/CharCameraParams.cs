using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "CharCameraParams", menuName = "ScriptableObjects/CharCameraParams", order = 1)]
public class CharCameraParams : ScriptableObject {
    [Header ("User Input")]
    [SerializeField] private float _lookThreshold;
    public float lookThreshold => _lookThreshold;

    [Header ("Camera Control")]
    [SerializeField] private float _camMaxHorizontalMovement;
    public float camMaxHorizontalMovement => _camMaxHorizontalMovement;

    [SerializeField] private float _camMaxVerticalMovement;
    public float camMaxVerticalMovement => _camMaxVerticalMovement;

    [SerializeField] private float _camHorizontalMoveSpeed;
    public float camHorizontalMoveSpeed => _camHorizontalMoveSpeed;

    [SerializeField] private float _camVerticalMoveSpeed;
    public float camVerticalMoveSpeed => _camVerticalMoveSpeed;
}