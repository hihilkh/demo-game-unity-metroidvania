using System;
using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HIHIFramework.UI {
    public class UIDragFollower : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

        public static event Action<UIDragFollower, PointerEventData> BeganDragEvent;
        public static event Action<UIDragFollower, PointerEventData> EndedDragEvent;

        public virtual void OnBeginDrag (PointerEventData eventData) {
            Log.PrintDebug ("OnBeginDrag", LogType.UI | LogType.Input);
            BeganDragEvent?.Invoke (this, eventData);
        }

        public virtual void OnDrag (PointerEventData eventData) {
            if (eventData.dragging) {
                transform.position = eventData.position;
            }
        }

        public virtual void OnEndDrag (PointerEventData eventData) {
            Log.PrintDebug ("OnEndDrag", LogType.UI | LogType.Input);
            EndedDragEvent?.Invoke (this, eventData);
        }
    }
}