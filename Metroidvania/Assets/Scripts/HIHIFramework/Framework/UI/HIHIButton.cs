﻿using System.Collections;
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
        private object info = null;

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
            FrameworkUtils.InsertChildrenToParent (go.transform, textObject, true);

            var textRectTransform = textObject.GetComponent<RectTransform> ();
            textRectTransform.anchorMin = Vector2.zero;
            textRectTransform.anchorMax = Vector2.one;
            textRectTransform.sizeDelta = Vector2.zero;

            // Btn parent
            if (Selection.activeTransform != null) {
                FrameworkUtils.InsertChildrenToParent (Selection.activeTransform, go, true, false, -1, false);
            }

            // Selection
            Selection.activeObject = go;
        }

        protected override void Awake () {
            base.Awake ();

            this.onClick.AddListener (OnClick);
        }

        public void SetOnClickInfo (BtnOnClickType onClickType, object info = null) {
            this.onClickType = onClickType;

            if (info != null) {
                this.info = info;
            }
        }

        private void OnClick () {
            if (!Enum.IsDefined (typeof (BtnOnClickType), onClickType)) {
                Log.PrintError ("HIHIButton onClickType is not yet assigned", LogType.UI | LogType.Input);
                return;
            }

            Log.Print ("OnClick : " + onClickType, LogType.UI | LogType.Input);
            if (info == null) {
                UIEventManager.InvokeEvent (onClickType, this);
            } else {
                UIEventManager.InvokeEvent (onClickType, this, info);
            }

        }
    }
}
