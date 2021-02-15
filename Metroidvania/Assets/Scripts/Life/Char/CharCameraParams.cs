using UnityEngine;

[CreateAssetMenu (fileName = "CharCameraParams", menuName = "ScriptableObjects/CharCameraParams", order = 1)]
public class CharCameraParams : ScriptableObject {
    [Header ("User Input")]
    [SerializeField] private float _lookThreshold;
    public float LookThreshold => _lookThreshold;

    [Header ("Camera Control")]
    [SerializeField] private float _camMaxHorizontalMovement;
    public float CamMaxHorizontalMovement => _camMaxHorizontalMovement;

    [SerializeField] private float _camMaxVerticalMovement;
    public float CamMaxVerticalMovement => _camMaxVerticalMovement;

    [SerializeField] private float _camHorizontalMoveSpeed;
    public float CamHorizontalMoveSpeed => _camHorizontalMoveSpeed;

    [SerializeField] private float _camVerticalMoveSpeed;
    public float CamVerticalMoveSpeed => _camVerticalMoveSpeed;
}