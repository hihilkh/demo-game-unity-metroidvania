using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HIHIFramework.Core;

public class CharSMBBase : StateMachineBehaviour
{
    private Dictionary<CharEnum.FaceType, GameObject> faceDict;
    private const string MeshName_HeadNormal = "Meshes/Head/head.normal";
    private const string MeshName_HeadNormalInversed = "Meshes/Head/head.normal_inversed";
    private const string MeshName_HeadConfused = "Meshes/Head/head.confused";
    private const string MeshName_HeadShocked = "Meshes/Head/head.shocked";

    protected CharModel model { get; private set; }

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
        if (faceDict == null) {
            faceDict = new Dictionary<CharEnum.FaceType, GameObject> ();

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
        }
    }

    protected void SetFace (CharEnum.FaceType faceType) {
        if (faceDict == null) {
            Log.PrintWarning ("FaceDict is null. Cannot set face.");
            return;
        }

        foreach (var pair in faceDict) {
            pair.Value.SetActive (pair.Key == faceType);
        }
    }

    protected void SetDefaultFace () {
        var faceType = CharEnum.FaceType.Normal;
        if ((model.GetObtainedBodyParts () & ~CharEnum.BodyPart.Head) == CharEnum.BodyPart.None) {  // Only have head
            faceType = CharEnum.FaceType.Confused;
        }

        SetFace (faceType);
    }
}
