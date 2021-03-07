using System.Collections.Generic;
using HihiFramework.Core;
using UnityEngine;

public class CharAnimUtils : MonoBehaviour {
    [SerializeField] private CharModel _model;
    public CharModel Model => _model;

    [SerializeField] private Rigidbody2D _rb;
    public Rigidbody2D RB => _rb;

    [SerializeField] private Transform _animBaseTransform;
    public Transform AnimBaseTransform => _animBaseTransform;

    [Header ("Material")]
    [SerializeField] private Material unlitMaterialToClone;
    [SerializeField] private Material stencilBufferMaterial;    // No need to clone. Use it directly for shared material
    

    [Header ("Particle System")]
    [SerializeField] private List<ParticleSystem> thrusterPSList;
    [SerializeField] private ParticleSystem jumpChargePS;
    [SerializeField] private ParticleSystem dropHitChargePS;

    [Header ("Hit Template")]
    [SerializeField] private CharNormalHit _normalHitTemplate;
    public CharNormalHit NormalHitTemplate => _normalHitTemplate;

    [SerializeField] private CharChargedHit _chargedHitTemplate;
    public CharChargedHit ChargedHitTemplate => _chargedHitTemplate;

    [SerializeField] private CharFinishingHit _finishingHitTemplate;
    public CharFinishingHit FinishingHitTemplate => _finishingHitTemplate;

    [SerializeField] private CharDropHit _dropHitTemplate;
    public CharDropHit DropHitTemplate => _dropHitTemplate;

    [Header ("Arrow Template")]
    [SerializeField] private CharTargetArrow _targetArrowTemplate;
    public CharTargetArrow TargetArrowTemplate => _targetArrowTemplate;

    [SerializeField] private CharStraightArrow _straightArrowTemplate;
    public CharStraightArrow StraightArrowTemplate => _straightArrowTemplate;

    [SerializeField] private CharTripleArrow _tripleArrowTemplate;
    public CharTripleArrow TripleArrowTemplate => _tripleArrowTemplate;

    [Header ("RefPoint")]
    [SerializeField] private Transform _refPoint_GeneralHit;
    public Transform RefPoint_GeneralHit => _refPoint_GeneralHit;

    [SerializeField] private Transform _refPoint_SlideHit;
    public Transform RefPoint_SlideHit => _refPoint_SlideHit;

    [SerializeField] private Transform _refPoint_DropHit;
    public Transform RefPoint_DropHit => _refPoint_DropHit;

    [SerializeField] private Transform _refPoint_GeneralShoot;
    public Transform RefPoint_GeneralShoot => _refPoint_GeneralShoot;

    [SerializeField] private Transform _refPoint_SlideShoot;
    public Transform RefPoint_SlideShoot => _refPoint_SlideShoot;

    private readonly Dictionary<CharEnum.FaceType, GameObject> faceDict = new Dictionary<CharEnum.FaceType, GameObject> ();
    private const string MeshName_HeadNormal = "Meshes/Head/head.normal";
    private const string MeshName_HeadNormalInversed = "Meshes/Head/head.normal_inversed";
    private const string MeshName_HeadConfused = "Meshes/Head/head.confused";
    private const string MeshName_HeadShocked = "Meshes/Head/head.shocked";

    private readonly Dictionary<CharEnum.BodyParts, GameObject> bodyPartDict = new Dictionary<CharEnum.BodyParts, GameObject> ();
    private const string MeshName_Arms = "Meshes/Arm";
    private const string MeshName_Legs = "Meshes/Leg";
    private const string MeshName_Thrusters = "Meshes/Leg/Thruster";
    private const string MeshName_ArrowWeapon = "Meshes/Arm/Weapon";

    private Animator animator;

    private bool isNeedCommonUpdate = false;

    private Material charUnlitMaterial;
    private const string CharMaterialAlphaFloatName = "_alpha";

    private void Awake () {
        animator = GetComponent<Animator> ();

        InitFaceDict ();
        InitBodyPartDict ();
        InitCharMaterial ();

        // event handler
        Model.Resetting += ModelResettingHandler;
        Model.ObtainedBodyPartsChanged += ObtainedBodyPartsChangedHandler;
        Model.StatusesChanged += CommonUpdateNeededHandler;
        Model.FacingDirectionChanged += CommonUpdateNeededHandler;
        Model.MovingDirectionChanged += CommonUpdateNeededHandler;
        Model.HorizontalSpeedChanged += CommonUpdateNeededHandler;
    }

    private void Start () {
        SetBodyParts (Model.ObtainedBodyParts);
    }

    private void Reset () {
        Log.Print ("CharAnimUtils : Reset", LogTypes.Animation);
        SetDieFeature (false);
        ResetGravity ();
        UpdateVelocity (0, 0);
    }

    private void Update () {
        // Do update on Update ()
        // - to prevent more than 1 update within a frame
        // - to delay the update (from event handlers) so that all parameters from CharModel is set
        // - Hence, no need to deal with the ordering of setting the parameters in CharModel
        if (isNeedCommonUpdate) {
            isNeedCommonUpdate = false;
            UpdateFace ();
            UpdatePS ();
            UpdateFacingDirection ();
            UpdateVelocityX (null);
        }

        if (RB.velocity.y < Model.Params.MinFallDownVelocity) {
            RB.velocity = new Vector2 (RB.velocity.x, Model.Params.MinFallDownVelocity);
        }
    }

    private void OnDestroy () {
        Model.Resetting -= ModelResettingHandler;
        Model.ObtainedBodyPartsChanged -= ObtainedBodyPartsChangedHandler;
        Model.StatusesChanged -= CommonUpdateNeededHandler;
        Model.FacingDirectionChanged -= CommonUpdateNeededHandler;
        Model.MovingDirectionChanged -= CommonUpdateNeededHandler;
        Model.HorizontalSpeedChanged -= CommonUpdateNeededHandler;
    }

    #region Die feature

    public void SetDieFeature (bool isDie) {
        if (isDie) {
            animator.speed = 0;
            RB.bodyType = RigidbodyType2D.Static;
        } else {
            animator.speed = 1;
            RB.bodyType = RigidbodyType2D.Dynamic;
            SetCharAlpha (1);
        }
    }

    #endregion

    #region Face related

    private void InitFaceDict () {
        var normalHeadTransform = transform.Find (MeshName_HeadNormal);
        if (normalHeadTransform == null) {
            Log.PrintError ("Cannot find normal head of the character.", LogTypes.Animation);
        } else {
            faceDict.Add (CharEnum.FaceType.Normal, normalHeadTransform.gameObject);
        }

        var normalInversedHeadTransform = transform.Find (MeshName_HeadNormalInversed);
        if (normalInversedHeadTransform == null) {
            Log.PrintError ("Cannot find normal inversed head of the character.", LogTypes.Animation);
        } else {
            faceDict.Add (CharEnum.FaceType.Normal_Inversed, normalInversedHeadTransform.gameObject);
        }

        var confusedHeadTransform = transform.Find (MeshName_HeadConfused);
        if (confusedHeadTransform == null) {
            Log.PrintError ("Cannot find confused head of the character.", LogTypes.Animation);
        } else {
            faceDict.Add (CharEnum.FaceType.Confused, confusedHeadTransform.gameObject);
        }

        var shockedHeadTransform = transform.Find (MeshName_HeadShocked);
        if (shockedHeadTransform == null) {
            Log.PrintError ("Cannot find shocked head of the character.", LogTypes.Animation);
        } else {
            faceDict.Add (CharEnum.FaceType.Shocked, shockedHeadTransform.gameObject);
        }
    }

    private void UpdateFace () {
        if (Model.IsBeatingBack || Model.IsDying) {
            SetFace (CharEnum.FaceType.Confused);
        } else if (Model.GetIsInStatuses (CharEnum.Statuses.Sliding)) {
            SetFace (CharEnum.FaceType.Normal_Inversed);
        } else {
            var faceType = CharEnum.FaceType.Normal;
            if ((Model.ObtainedBodyParts & ~CharEnum.BodyParts.Head) == CharEnum.BodyParts.None) {  // Only have head
                faceType = CharEnum.FaceType.Confused;
            }

            SetFace (faceType);
        }
    }

    private void SetFace (CharEnum.FaceType faceType) {
        foreach (var pair in faceDict) {
            pair.Value.SetActive (pair.Key == faceType);
        }
    }

    #endregion

    #region Body Parts

    private void InitBodyPartDict () {
        var armsTransform = transform.Find (MeshName_Arms);
        if (armsTransform == null) {
            Log.PrintError ("Cannot find arms of the character.", LogTypes.Animation);
        } else {
            bodyPartDict.Add (CharEnum.BodyParts.Arms, armsTransform.gameObject);
        }

        var legsTransform = transform.Find (MeshName_Legs);
        if (legsTransform == null) {
            Log.PrintError ("Cannot find legs of the character.", LogTypes.Animation);
        } else {
            bodyPartDict.Add (CharEnum.BodyParts.Legs, legsTransform.gameObject);
        }

        var thrustersTransform = transform.Find (MeshName_Thrusters);
        if (thrustersTransform == null) {
            Log.PrintError ("Cannot find thrusters of the character.", LogTypes.Animation);
        } else {
            bodyPartDict.Add (CharEnum.BodyParts.Thrusters, thrustersTransform.gameObject);
        }

        var arrowTransform = transform.Find (MeshName_ArrowWeapon);
        if (arrowTransform == null) {
            Log.PrintError ("Cannot find arrow weapon of the character.", LogTypes.Animation);
        } else {
            bodyPartDict.Add (CharEnum.BodyParts.Arrow, arrowTransform.gameObject);
        }
    }

    private void SetBodyParts (CharEnum.BodyParts parts) {
        foreach (var pair in bodyPartDict) {
            pair.Value.SetActive (parts.HasFlag (pair.Key));
        }

        UpdateFace ();
    }

    #endregion

    #region Material

    private void InitCharMaterial () {
        var renderers = GetComponentsInChildren<SkinnedMeshRenderer> ();

        if (renderers == null || renderers.Length <= 0) {
            Log.PrintError ("Cannot find SkinnedMeshRenderer of the character.", LogTypes.Animation);
            return;
        }

        charUnlitMaterial = new Material (unlitMaterialToClone);
        var materials = new Material[2];
        materials[0] = charUnlitMaterial;
        materials[1] = stencilBufferMaterial;
        foreach (var renderer in renderers) {
            renderer.materials = materials;
        }
    }

    public void SetCharAlpha (float alpha) {
        charUnlitMaterial.SetFloat (CharMaterialAlphaFloatName, alpha);
    }

    #endregion

    #region Particle System

    public void SetThrusterPS (bool isOn) {
        foreach (var ps in thrusterPSList) {
            ps.gameObject.SetActive (isOn);
        }
    }

    private void SetJumpChargePS (bool isOn) {
        jumpChargePS.gameObject.SetActive (isOn);
    }

    private void SetDropHitChargePS (bool isOn) {
        dropHitChargePS.gameObject.SetActive (isOn);
    }

    private void UpdatePS () {
        SetThrusterPS (Model.GetIsInStatuses (CharEnum.Statuses.Dashing));
        SetJumpChargePS (Model.GetIsInStatuses (CharEnum.Statuses.JumpCharging));
        SetDropHitChargePS (Model.GetIsInStatuses (CharEnum.Statuses.DropHitCharging));
    }
    #endregion

    #region Movement Related

    public float GetVelocityXByCurrentHorizontalSpeed (bool magnitudeOnly = false) {
        var velocityX = 0f;

        switch (Model.CurrentHorizontalSpeed) {
            case CharEnum.HorizontalSpeed.Idle:
                velocityX = 0;
                break;
            case CharEnum.HorizontalSpeed.Walk:
                velocityX = Model.Params.WalkingSpeed;
                break;
            case CharEnum.HorizontalSpeed.Dash:
                velocityX = Model.Params.DashingSpeed;
                break;
        }

        if (magnitudeOnly) {
            return velocityX;
        } else {
            var multiplier = (Model.MovingDirection == LifeEnum.HorizontalDirection.Right) ? 1 : -1;
            return velocityX * multiplier;
        }
    }

    /// <param name="x">Input null means determined by model.currentHorizontalSpeed</param>
    public void UpdateVelocity (float? x, float y) {
        if (RB.bodyType == RigidbodyType2D.Static) {
            return;
        }

        var velocityX = (x == null) ? GetVelocityXByCurrentHorizontalSpeed () : (float)x;

        RB.velocity = new Vector3 (velocityX, y);
    }

    /// <param name="x">Input null means determined by model.currentHorizontalSpeed</param>
    public void UpdateVelocityX (float? x) {
        var velocityX = (x == null) ? GetVelocityXByCurrentHorizontalSpeed () : (float)x;

        UpdateVelocity (velocityX, RB.velocity.y);
    }

    private void UpdateFacingDirection () {
        var scale = (Model.FacingDirection == LifeEnum.HorizontalDirection.Right) ? 1 : -1;

        if (Model.GetIsInStatuses (CharEnum.Statuses.Sliding)) {
            scale = scale * -1;
        }

        AnimBaseTransform.localScale = new Vector3 (scale, 1, 1);
    }

    public void ResetGravity () {
        RB.gravityScale = Model.Params.GravityScale;
    }

    public void RemoveGravity () {
        RB.gravityScale = 0;
    }

    #endregion

    #region Events

    private void ModelResettingHandler () {
        Reset ();
    }

    private void ObtainedBodyPartsChangedHandler (CharEnum.BodyParts parts) {
        SetBodyParts (parts);
    }

    private void CommonUpdateNeededHandler () {
        isNeedCommonUpdate = true;
    }

    #endregion
}