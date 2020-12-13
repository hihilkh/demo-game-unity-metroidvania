using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.UI;
using HIHIFramework.Core;
using TMPro;
using System;

namespace HIHIFramework.UI {

    [CustomEditor (typeof (HIHIButton))]
    public class HIHIButtonEditor : ButtonEditor {
        public override void OnInspectorGUI () {
            base.OnInspectorGUI ();

            var property = serializedObject.FindProperty ("onClickType");
            EditorGUILayout.PropertyField (property);
            serializedObject.ApplyModifiedProperties ();
        }
    }

    public class HIHIButton : Button {
        [SerializeField] private BtnOnClickType onClickType;

        [MenuItem ("GameObject/UI/Button - HIHIFramework")]
        public static void CreateButtonWithTextObject () {
            // Btn
            var go = new GameObject ("Btn", typeof (RectTransform));
            //go.AddComponent<CanvasRenderer> ();
            go.AddComponent<Image> ();
            go.AddComponent<HIHIButton> ();

            // Text
            var textObject = new GameObject ("Text", typeof (RectTransform));
            var text = textObject.AddComponent<TextMeshProUGUI> ();
            text.text = "Button";
            text.color = Color.black;
            text.alignment = TextAlignmentOptions.Center;
            GameUtils.InsertChildrenToParent (go.transform, textObject);

            var textRectTransform = textObject.GetComponent<RectTransform> ();
            textRectTransform.anchorMin = Vector2.zero;
            textRectTransform.anchorMax = Vector2.one;
            textRectTransform.sizeDelta = Vector2.zero;

            // Btn parent
            if (Selection.activeTransform != null) {
                GameUtils.InsertChildrenToParent (Selection.activeTransform, go, false, -1, false);
            }

            // Selection
            Selection.activeObject = go;
        }

        protected override void Awake () {
            base.Awake ();

            this.onClick.AddListener (OnClick);
        }

        public void SetOnClickType (BtnOnClickType onClickType) {
            this.onClickType = onClickType;
        }

        private void OnClick () {
            if (!Enum.IsDefined (typeof (BtnOnClickType), onClickType)) {
                Log.PrintError ("HIHIButton onClickType is not yet assigned");
                return;
            }

            Log.Print ("OnClick : " + onClickType);
            UIEventManager.InvokeEvent (onClickType);
        }
    }
}
