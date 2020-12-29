using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HIHIFramework.Core;

public class CharSMBBase : StateMachineBehaviour
{
    private static Dictionary<CharModel, Dictionary<CharEnum.FaceType, GameObject>> FaceDictByCharModel = new Dictionary<CharModel, Dictionary<CharEnum.FaceType, GameObject>> ();
    private const string MeshName_HeadNormal = "Meshes/Head/head.normal";
    private const string MeshName_HeadNormalInversed = "Meshes/Head/head.normal_inversed";
    private const string MeshName_HeadConfused = "Meshes/Head/head.confused";
    private const string MeshName_HeadShocked = "Meshes/Head/head.shocked";

    private static Dictionary<CharModel, Dictionary<CharEnum.BodyPart, GameObject>> BodyPartDictByCharModel = new Dictionary<CharModel, Dictionary<CharEnum.BodyPart, GameObject>> ();
    private const string MeshName_Arms = "Meshes/Arm";
    private const string MeshName_Legs = "Meshes/Leg";
    private const string MeshName_Thrusters = "Meshes/Leg/Thruster";
    private const string MeshName_ArrowWeapon = "Meshes/Arm/Weapon";

    protected CharModel model { get; private set; }

    static CharSMBBase () {
        CharModel.ObtainedBodyPartsChangedEvent += SetBodyParts;
    }

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        // model
        if (model == null) {
            model = animator.GetComponentInParent<CharModel> ();

            if (model == null) {
                Log.PrintError ("Cannot find corresponding CharacterModel script.");
            }
        }

        // faceDict
        if (!FaceDictByCharModel.ContainsKey (model)) {
            var faceDict = new Dictionary<CharEnum.FaceType, GameObject> ();

            var normalHeadTransform = animator.transform.Find (MeshName_HeadNormal);
            if (normalHeadTransform == null) {
                Log.PrintError ("Cannot find normal head of the character.");
            } else {
                faceDict.Add (CharEnum.FaceType.Normal, normalHeadTransform.gameObject);
            }

            //var normalInversedHeadTransform = animator.transform.Find (HeadName_NormalInversed);
            //if (normalInversedHeadTransform == null) {
            //    Log.PrintError ("Cannot find normal inversed head of the character.");
            //} else {
            //    faceDict.Add (CharacterEnum.FaceType.Normal_Inversed, normalInversedHeadTransform.gameObject);
            //}

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

            FaceDictByCharModel.Add (model, faceDict);
        }

        // bodyPartDict
        if (!BodyPartDictByCharModel.ContainsKey (model)) {
            var bodyPartDict = new Dictionary<CharEnum.BodyPart, GameObject> ();

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

            BodyPartDictByCharModel.Add (model, bodyPartDict);

            SetBodyParts (model, model.GetObtainedBodyParts ());
        }
    }

    protected static void SetFace (CharModel charModel, CharEnum.FaceType faceType) {
        if (!FaceDictByCharModel.ContainsKey (charModel)) {
            Log.PrintWarning ("FaceDict is null. Cannot set face.");
            return;
        }

        foreach (var pair in FaceDictByCharModel[charModel]) {
            pair.Value.SetActive (pair.Key == faceType);
        }
    }

    protected static void SetDefaultFace (CharModel charModel) {
        var faceType = CharEnum.FaceType.Normal;
        if ((charModel.GetObtainedBodyParts () & ~CharEnum.BodyPart.Head) == CharEnum.BodyPart.None) {  // Only have head
            faceType = CharEnum.FaceType.Confused;
        }

        SetFace (charModel, faceType);
    }

    private static void SetBodyParts (CharModel charModel, CharEnum.BodyPart parts) {
        if (!BodyPartDictByCharModel.ContainsKey (charModel)) {
            Log.PrintWarning ("BodyPartDict is null. Cannot set body parts.");
            return;
        }

        foreach (var pair in BodyPartDictByCharModel[charModel]) {
            pair.Value.SetActive (parts.HasFlag (pair.Key));
        }

        SetDefaultFace (charModel);
    }
}
