﻿using System.Collections;
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

    public CharModel model { get; private set; }
    public Rigidbody2D rb { get; private set; }

    private void Awake () {
        // model
        model = GetComponentInParent<CharModel> ();
        if (model == null) {
            Log.PrintError ("Cannot find corresponding CharacterModel script.");
        }

        // Rigid body
        rb = GetComponentInParent<Rigidbody2D> ();
        if (rb == null) {
            Log.PrintError ("Cannot find corresponding Rigidbody2D.");
        }

        var animator = GetComponent<Animator> ();

        // faceDict
        faceDict = new Dictionary<CharEnum.FaceType, GameObject> ();

        var normalHeadTransform = animator.transform.Find (MeshName_HeadNormal);
        if (normalHeadTransform == null) {
            Log.PrintError ("Cannot find normal head of the character.");
        } else {
            faceDict.Add (CharEnum.FaceType.Normal, normalHeadTransform.gameObject);
        }

        var normalInversedHeadTransform = animator.transform.Find (MeshName_HeadNormalInversed);
        if (normalInversedHeadTransform == null) {
            Log.PrintError ("Cannot find normal inversed head of the character.");
        } else {
            faceDict.Add (CharEnum.FaceType.Normal_Inversed, normalInversedHeadTransform.gameObject);
        }

        var confusedHeadTransform = animator.transform.Find (MeshName_HeadConfused);
        if (confusedHeadTransform == null) {
            Log.PrintError ("Cannot find confused head of the character.");
        } else {
            faceDict.Add (CharEnum.FaceType.Confused, confusedHeadTransform.gameObject);
        }

        var shockedHeadTransform = animator.transform.Find (MeshName_HeadShocked);
        if (shockedHeadTransform == null) {
            Log.PrintError ("Cannot find shocked head of the character.");
        } else {
            faceDict.Add (CharEnum.FaceType.Shocked, shockedHeadTransform.gameObject);
        }

        // bodyPartDict
        bodyPartDict = new Dictionary<CharEnum.BodyPart, GameObject> ();

        var armsTransform = animator.transform.Find (MeshName_Arms);
        if (armsTransform == null) {
            Log.PrintError ("Cannot find arms of the character.");
        } else {
            bodyPartDict.Add (CharEnum.BodyPart.Arms, armsTransform.gameObject);
        }

        var legsTransform = animator.transform.Find (MeshName_Legs);
        if (legsTransform == null) {
            Log.PrintError ("Cannot find legs of the character.");
        } else {
            bodyPartDict.Add (CharEnum.BodyPart.Legs, legsTransform.gameObject);
        }

        var thrustersTransform = animator.transform.Find (MeshName_Thrusters);
        if (thrustersTransform == null) {
            Log.PrintError ("Cannot find thrusters of the character.");
        } else {
            bodyPartDict.Add (CharEnum.BodyPart.Thrusters, thrustersTransform.gameObject);
        }

        var arrowTransform = animator.transform.Find (MeshName_ArrowWeapon);
        if (arrowTransform == null) {
            Log.PrintError ("Cannot find arrow weapon of the character.");
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
            Log.PrintWarning ("FaceDict is null. Cannot set face.");
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
            Log.PrintWarning ("BodyPartDict is null. Cannot set body parts.");
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
        var multiplier = (model.movingDirection == CharEnum.Direction.Right) ? 1 : -1;

        var horizontalSpeed = 0f;

        switch (model.currentHorizontalSpeed) {
            case CharEnum.HorizontalSpeed.Idle:
                horizontalSpeed = 0;
                break;
            case CharEnum.HorizontalSpeed.Walk:
                horizontalSpeed = model.characterParams.walkingSpeed;
                break;
            case CharEnum.HorizontalSpeed.Dash:
                horizontalSpeed = model.characterParams.dashingSpeed;
                break;
        }

        rb.velocity = new Vector3 (horizontalSpeed * multiplier, rb.velocity.y);

        // TODO : Debug usage only
        if (Mathf.Abs (rb.velocity.x) < 1 && Mathf.Abs (rb.velocity.y) < 1) {
            Log.PrintError ("No velocity! ; horizontal speed = " + model.currentHorizontalSpeed);
            //Log.PrintError ("No velocity! Situation = " + model.situation + " ; horizontal speed = " + model.currentHorizontalSpeed);
        }
    }

    /// <param name="x">Input null means using current velocity.x</param>
    /// <param name="y">Input null means using current velocity.y</param>
    public void SetVelocity (float? x, float? y) {
        var velocityX = (x == null) ? rb.velocity.x : (float)x;
        var velocityY = (y == null) ? rb.velocity.y : (float)y;
        rb.velocity = new Vector3 (velocityX, velocityY);
    }

    public void UpdateFacingDirection (bool isNeedOppositeDirection = false) {
        var scale = (model.facingDirection == CharEnum.Direction.Right) ? 1 : -1;

        if (isNeedOppositeDirection) {
            scale = scale * -1;
        }

        rb.transform.localScale = new Vector3 (scale, 1, 1);
    }

    public void ResetGravity () {
        rb.gravityScale = model.characterParams.gravityScale;
    }

    public void RemoveGravity () {
        rb.gravityScale = 0;
    }

    #endregion
}