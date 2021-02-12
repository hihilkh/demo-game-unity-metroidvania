using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HIHIFramework.UI {
    public class UIDragSingleFollower : UIDragFollower {
        private static UIDragSingleFollower draggingObject;

        private bool CheckCanDrag () {
            if (draggingObject == null || draggingObject == this) {
                return true;
            }

            return false;
        }

        public override void OnBeginDrag (PointerEventData eventData) {
            if (!CheckCanDrag ()) {
                return;
            }

            draggingObject = this;
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

            draggingObject = null;
            base.OnEndDrag (eventData);
        }
    }

}
