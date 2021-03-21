using System;
using HihiFramework.Core;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor.UI;
#endif

namespace HihiFramework.UI {

#if UNITY_EDITOR
    [CustomEditor (typeof (HihiButton))]
    public class HihiButtonEditor : ButtonEditor {
        public override void OnInspectorGUI () {
            base.OnInspectorGUI ();

            var property = serializedObject.FindProperty ("onClickType");
            EditorGUILayout.PropertyField (property);
            serializedObject.ApplyModifiedProperties ();
        }
    }
#endif

    public class HihiButton : Button {
        [SerializeField] private BtnOnClickType onClickType;
        private object info = null;

#if UNITY_EDITOR
        [MenuItem ("GameObject/UI/Button - HihiFramework")]
        public static void CreateButtonWithTextObject () {
            // Btn
            var go = new GameObject ("Btn", typeof (RectTransform));
            //go.AddComponent<CanvasRenderer> ();
            go.AddComponent<Image> ();
            go.AddComponent<HihiButton> ();

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
#endif

        protected override void Awake () {
            base.Awake ();

            onClick.AddListener (ClickedHandler);
        }

        #region Click related

        public void SetOnClickInfo (BtnOnClickType onClickType, object info = null) {
            this.onClickType = onClickType;

            if (info != null) {
                this.info = info;
            }
        }

        private void ClickedHandler () {
            if (!Enum.IsDefined (typeof (BtnOnClickType), onClickType)) {
                Log.PrintError ("HihiButton onClickType is not yet assigned", LogTypes.UI | LogTypes.Input);
                return;
            }

            Log.Print ("ClickedHandler : " + onClickType, LogTypes.UI | LogTypes.Input);
            if (info == null) {
                UIEventManager.InvokeEvent (onClickType, this);
            } else {
                UIEventManager.InvokeEvent (onClickType, this, info);
            }
        }
        #endregion

        #region Interactable

        public void SetInteractable (bool isInteractable) {
            if (interactable == isInteractable) {
                return;
            }

            interactable = isInteractable;
            if (transition == Transition.ColorTint) {
                var maskColor = isInteractable ? colors.normalColor : colors.disabledColor;

                // Texts
                var texts = GetComponentsInChildren<TextMeshProUGUI> ();
                if (texts != null && texts.Length > 0) {
                    foreach (var text in texts) {
                        text.color = maskColor;
                    }
                }
            }
        }

        #endregion
    }
}
