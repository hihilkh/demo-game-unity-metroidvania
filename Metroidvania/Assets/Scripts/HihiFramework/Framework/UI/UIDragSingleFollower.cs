using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HihiFramework.UI {
    public class UIDragSingleFollower : UIDragFollower {
        private static UIDragSingleFollower DraggingObject;

        private bool CheckCanDrag () {
            if (DraggingObject == null || DraggingObject == this) {
                return true;
            }

            return false;
        }

        public override void OnBeginDrag (PointerEventData eventData) {
            if (!CheckCanDrag ()) {
                return;
            }

            DraggingObject = this;
            base.OnBeginDrag (eventData);
        }

        public override void OnDrag (PointerEventData eventData) {
            if (!CheckCanDrag ()) {
                return;
            }

            base.OnDrag (eventData);
        }

        public override void OnEndDrag (PointerEventData eventData) {
            if (!CheckCanDrag ()) {
                return;
            }

            DraggingObject = null;
            base.OnEndDrag (eventData);
        }
    }

}
