using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

public class CharPreparationSMB : CharSMBBase {
    private Dictionary<CharEnum.BodyPart, GameObject> bodyPartDict;
    private const string MeshName_Arms = "Meshes/Arm";
    private const string MeshName_Legs = "Meshes/Leg";
    private const string MeshName_Thrusters = "Meshes/Leg/Thruster";
    private const string MeshName_ArrowWeapon = "Meshes/Arm/Weapon";

    private bool hasAddedEvent = false;

    protected virtual void OnDestroy () {
        if (hasAddedEvent) {
            model.obtainedBodyPartsChangedEvent -= SetBodyParts;
        }
    }

    override public void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        // bodyPartDict
        if (bodyPartDict == null) {
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

            SetBodyParts (model.GetObtainedBodyParts ());

            if (!hasAddedEvent) {
                hasAddedEvent = true;
                model.obtainedBodyPartsChangedEvent += SetBodyParts;
            }
        }
    }

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
}
