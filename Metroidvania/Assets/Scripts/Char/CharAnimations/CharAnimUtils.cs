using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

public class CharAnimUtils : MonoBehaviour
{
    private Dictionary<CharEnum.FaceType, GameObject> faceDict;
    private const string MeshName_HeadNormal = "Meshes/Head/head.normal";
    private const string MeshName_HeadNormalInversed = "Meshes/Head/head.normal_inversed";
    private const string MeshName_HeadConfused = "Meshes/Head/head.confused";
    private const string MeshName_HeadShocked = "Meshes/Head/head.shocked";

    private Dictionary<CharEnum.BodyPart, GameObject> bodyPartDict;
    private const string MeshName_Arms = "Meshes/Arm";
    private const string MeshName_Legs = "Meshes/Leg";
    private const string MeshName_Thrusters = "Meshes/Leg/Thruster";
    private const string MeshName_ArrowWeapon = "Meshes/Arm/Weapon";

    [SerializeField] private CharModel _model;
    public CharModel model => _model;

    [SerializeField] private Rigidbody2D _rb;
    public Rigidbody2D rb => _rb;

    [Header ("Hit Template")]
    [SerializeField] private CharNormalHit _normalHitTemplate;
    public CharNormalHit normalHitTemplate => _normalHitTemplate;

    [SerializeField] private CharChargedHit _chargedHitTemplate;
    public CharChargedHit chargedHitTemplate => _chargedHitTemplate;

    [SerializeField] private CharFinishingHit _finishingHitTemplate;
    public CharFinishingHit finishingHitTemplate => _finishingHitTemplate;

    [SerializeField] private CharDropHit _dropHitTemplate;
    public CharDropHit dropHitTemplate => _dropHitTemplate;

    [Header ("Arrow Template")]
    [SerializeField] private CharTargetArrow _targetArrowTemplate;
    public CharTargetArrow targetArrowTemplate => _targetArrowTemplate;

    [SerializeField] private CharStraightArrow _straightArrowTemplate;
    public CharStraightArrow straightArrowTemplate => _straightArrowTemplate;

    [SerializeField] private CharTripleArrow _tripleArrowTemplate;
    public CharTripleArrow tripleArrowTemplate => _tripleArrowTemplate;

    [Header ("RefPoint")]
    [SerializeField] private Transform _refPoint_GeneralHit;
    public Transform refPoint_GeneralHit => _refPoint_GeneralHit;

    [SerializeField] private Transform _refPoint_SlideHit;
    public Transform refPoint_SlideHit => _refPoint_SlideHit;

    [SerializeField] private Transform _refPoint_DropHit;
    public Transform refPoint_DropHit => _refPoint_DropHit;

    [SerializeField] private Transform _refPoint_GeneralShoot;
    public Transform refPoint_GeneralShoot => _refPoint_GeneralShoot;

    [SerializeField] private Transform _refPoint_SlideShoot;
    public Transform refPoint_SlideShoot => _refPoint_SlideShoot;

    private void Awake () {
        var animator = GetComponent<Animator> ();

        // faceDict
        faceDict = new Dictionary<CharEnum.FaceType, GameObject> ();

        var normalHeadTransform = animator.transform.Find (MeshName_HeadNormal);
        if (normalHeadTransform == null) {
            Log.PrintError ("Cannot find normal head of the character.", LogType.Animation);
        } else {
            faceDict.Add (CharEnum.FaceType.Normal, normalHeadTransform.gameObject);
        }

        var normalInversedHeadTransform = animator.transform.Find (MeshName_HeadNormalInversed);
        if (normalInversedHeadTransform == null) {
            Log.PrintError ("Cannot find normal inversed head of the character.", LogType.Animation);
        } else {
            faceDict.Add (CharEnum.FaceType.Normal_Inversed, normalInversedHeadTransform.gameObject);
        }

        var confusedHeadTransform = animator.transform.Find (MeshName_HeadConfused);
        if (confusedHeadTransform == null) {
            Log.PrintError ("Cannot find confused head of the character.", LogType.Animation);
        } else {
            faceDict.Add (CharEnum.FaceType.Confused, confusedHeadTransform.gameObject);
        }

        var shockedHeadTransform = animator.transform.Find (MeshName_HeadShocked);
        if (shockedHeadTransform == null) {
            Log.PrintError ("Cannot find shocked head of the character.", LogType.Animation);
        } else {
            faceDict.Add (CharEnum.FaceType.Shocked, shockedHeadTransform.gameObject);
        }

        // bodyPartDict
        bodyPartDict = new Dictionary<CharEnum.BodyPart, GameObject> ();

        var armsTransform = animator.transform.Find (MeshName_Arms);
        if (armsTransform == null) {
            Log.PrintError ("Cannot find arms of the character.", LogType.Animation);
        } else {
            bodyPartDict.Add (CharEnum.BodyPart.Arms, armsTransform.gameObject);
        }

        var legsTransform = animator.transform.Find (MeshName_Legs);
        if (legsTransform == null) {
            Log.PrintError ("Cannot find legs of the character.", LogType.Animation);
        } else {
            bodyPartDict.Add (CharEnum.BodyPart.Legs, legsTransform.gameObject);
        }

        var thrustersTransform = animator.transform.Find (MeshName_Thrusters);
        if (thrustersTransform == null) {
            Log.PrintError ("Cannot find thrusters of the character.", LogType.Animation);
        } else {
            bodyPartDict.Add (CharEnum.BodyPart.Thrusters, thrustersTransform.gameObject);
        }

        var arrowTransform = animator.transform.Find (MeshName_ArrowWeapon);
        if (arrowTransform == null) {
            Log.PrintError ("Cannot find arrow weapon of the character.", LogType.Animation);
        } else {
            bodyPartDict.Add (CharEnum.BodyPart.Arrow, arrowTransform.gameObject);
        }

        // event handler
        model.obtainedBodyPartsChangedEvent += SetBodyParts;
    }

    private void Start()
    {
        ResetGravity ();
        SetBodyParts (model.GetObtainedBodyParts ());
    }

    private void OnDestroy () {
        model.obtainedBodyPartsChangedEvent -= SetBodyParts;
    }

    #region Face related

    public void SetFace (CharEnum.FaceType faceType) {
        if (faceDict == null) {
            Log.PrintWarning ("FaceDict is null. Cannot set face.", LogType.Animation);
            return;
        }

        foreach (var pair in faceDict) {
            pair.Value.SetActive (pair.Key == faceType);
        }
    }

    public void SetDefaultFace () {
        var faceType = CharEnum.FaceType.Normal;
        if ((model.GetObtainedBodyParts () & ~CharEnum.BodyPart.Head) == CharEnum.BodyPart.None) {  // Only have head
            faceType = CharEnum.FaceType.Confused;
        }

        SetFace (faceType);
    }

    #endregion

    #region Body Parts

    private void SetBodyParts (CharEnum.BodyPart parts) {
        if (bodyPartDict == null) {
            Log.PrintWarning ("BodyPartDict is null. Cannot set body parts.", LogType.Animation);
            return;
        }

        foreach (var pair in bodyPartDict) {
            pair.Value.SetActive (parts.HasFlag (pair.Key));
        }

        SetDefaultFace ();
    }

    #endregion

    #region Movement Related

    public void UpdateHorizontalVelocity () {
        var velocityX = GetVelocityXByCurrentHorizontalSpeed ();

        rb.velocity = new Vector3 (velocityX, rb.velocity.y);
    }

    public float GetVelocityXByCurrentHorizontalSpeed (bool magnitudeOnly = false) {
        var velocityX = 0f;

        switch (model.currentHorizontalSpeed) {
            case CharEnum.HorizontalSpeed.Idle:
                velocityX = 0;
                break;
            case CharEnum.HorizontalSpeed.Walk:
                velocityX = model.charParams.walkingSpeed;
                break;
            case CharEnum.HorizontalSpeed.Dash:
                velocityX = model.charParams.dashingSpeed;
                break;
        }

        if (magnitudeOnly) {
            return velocityX;
        } else {
            var multiplier = (model.movingDirection == CharEnum.Direction.Right) ? 1 : -1;
            return velocityX * multiplier;
        }
    }

    /// <param name="x">Input null means determined by model.currentHorizontalSpeed</param>
    public void SetVelocity (float? x, float y) {
        var velocityX = (x == null) ? GetVelocityXByCurrentHorizontalSpeed () : (float)x;

        rb.velocity = new Vector3 (velocityX, y);
    }

    /// <param name="x">Input null means determined by model.currentHorizontalSpeed</param>
    public void SetVelocityX (float? x) {
        var velocityX = (x == null) ? GetVelocityXByCurrentHorizontalSpeed () : (float)x;

        rb.velocity = new Vector3 (velocityX, rb.velocity.y);
    }

    public void UpdateFacingDirection (bool isNeedOppositeDirection = false) {
        var scale = (model.facingDirection == CharEnum.Direction.Right) ? 1 : -1;

        if (isNeedOppositeDirection) {
            scale = scale * -1;
        }

        rb.transform.localScale = new Vector3 (scale, 1, 1);
    }

    public void ResetGravity () {
        rb.gravityScale = model.charParams.gravityScale;
    }

    public void RemoveGravity () {
        rb.gravityScale = 0;
    }

    #endregion
}