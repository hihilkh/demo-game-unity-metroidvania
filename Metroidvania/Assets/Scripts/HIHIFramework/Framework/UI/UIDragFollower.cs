using System;
using HihiFramework.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HihiFramework.UI {
    public class UIDragFollower : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

        public static event Action<UIDragFollower, PointerEventData> BeganDrag;
        public static event Action<UIDragFollower, PointerEventData> Dragging;
        public static event Action<UIDragFollower, PointerEventData> EndedDrag;

        public virtual void OnBeginDrag (PointerEventData eventData) {
            Log.PrintDebug ("OnBeginDrag", LogTypes.UI | LogTypes.Input);
            BeganDrag?.Invoke (this, eventData);
        }

        public virtual void OnDrag (PointerEventData eventData) {
            if (eventData.dragging) {
                transform.position = eventData.position;
                Dragging?.Invoke (this, eventData);
            }
        }

        public virtual void OnEndDrag (PointerEventData eventData) {
            Log.PrintDebug ("OnEndDrag", LogTypes.UI | LogTypes.Input);
            EndedDrag?.Invoke (this, eventData);
        }
    }
}