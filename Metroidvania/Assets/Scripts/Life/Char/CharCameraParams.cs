using UnityEngine;

[CreateAssetMenu (fileName = "CharCameraParams", menuName = "ScriptableObjects/CharCameraParams", order = 1)]
public class CharCameraParams : ScriptableObject {
    [Header ("User Input")]
    [SerializeField] private float _lookThreshold;
    public float LookThreshold => _lookThreshold;

    [Header ("Camera Control")]
    [SerializeField] private float _camMaxLoopUpMagnitude;
    public float CamMaxLoopUpMagnitude => _camMaxLoopUpMagnitude;

    [SerializeField] private float _camMaxLoopDownMagnitude;
    public float CamMaxLoopDownMagnitude => _camMaxLoopDownMagnitude;

    [SerializeField] private float _camMaxLoopLeftMagnitude;
    public float CamMaxLoopLeftMagnitude => _camMaxLoopLeftMagnitude;

    [SerializeField] private float _camMaxLoopRightMagnitude;
    public float CamMaxLoopRightMagnitude => _camMaxLoopRightMagnitude;

    [SerializeField] private float _camHorizontalMoveSpeed;
    public float CamHorizontalMoveSpeed => _camHorizontalMoveSpeed;

    [SerializeField] private float _camVerticalMoveSpeed;
    public float CamVerticalMoveSpeed => _camVerticalMoveSpeed;
}