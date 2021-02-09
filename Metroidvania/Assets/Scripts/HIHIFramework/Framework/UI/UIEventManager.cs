using System;
using System.Collections;
using System.Collections.Generic;

namespace HIHIFramework.UI {
    public static class UIEventManager {

        private static Dictionary<BtnOnClickType, Action<HIHIButton>> BtnOnClickDict = new Dictionary<BtnOnClickType, Action<HIHIButton>> ();
        private static Dictionary<BtnOnClickType, Action<HIHIButton, object>> BtnOnClickWithInfoDict = new Dictionary<BtnOnClickType, Action<HIHIButton, object>> ();

        #region handler - Action

        public static void AddEventHandler (BtnOnClickType onClickType, Action<HIHIButton> handler) {
            if (!BtnOnClickDict.ContainsKey (onClickType)) {
                BtnOnClickDict.Add (onClickType, null);
            }

            BtnOnClickDict[onClickType] += handler;
        }

        public static void RemoveEventHandler (BtnOnClickType onClickType, Action<HIHIButton> handler) {
            if (BtnOnClickDict.ContainsKey (onClickType)) {
                BtnOnClickDict[onClickType] -= handler;

                if (BtnOnClickDict[onClickType] == null) {
                    BtnOnClickDict.Remove (onClickType);
                }
            }
        }

        public static void InvokeEvent (BtnOnClickType onClickType, HIHIButton btn) {
            if (BtnOnClickDict.ContainsKey (onClickType)) {
                BtnOnClickDict[onClickType]?.Invoke (btn);
            }
        }

        #endregion

        #region handler - Action<object>

        public static void AddEventHandler (BtnOnClickType onClickType, Action<HIHIButton, object> handler) {
            if (!BtnOnClickWithInfoDict.ContainsKey (onClickType)) {
                BtnOnClickWithInfoDict.Add (onClickType, null);
            }

            BtnOnClickWithInfoDict[onClickType] += handler;
        }

        public static void RemoveEventHandler (BtnOnClickType onClickType, Action<HIHIButton, object> handler) {
            if (BtnOnClickWithInfoDict.ContainsKey (onClickType)) {
                BtnOnClickWithInfoDict[onClickType] -= handler;

                if (BtnOnClickWithInfoDict[onClickType] == null) {
                    BtnOnClickWithInfoDict.Remove (onClickType);
                }
            }
        }

        public static void InvokeEvent (BtnOnClickType onClickType, HIHIButton btn, object info) {
            if (BtnOnClickWithInfoDict.ContainsKey (onClickType)) {
                BtnOnClickWithInfoDict[onClickType]?.Invoke (btn, info);
            }
        }

        #endregion

    }
}