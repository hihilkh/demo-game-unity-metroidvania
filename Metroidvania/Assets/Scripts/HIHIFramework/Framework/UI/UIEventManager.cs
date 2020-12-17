using System;
using System.Collections;
using System.Collections.Generic;

namespace HIHIFramework.UI {
    public static class UIEventManager {

        private static Dictionary<BtnOnClickType, Action> BtnOnClickDict = new Dictionary<BtnOnClickType, Action> ();
        private static Dictionary<BtnOnClickType, Action<object>> BtnOnClickWithInfoDict = new Dictionary<BtnOnClickType, Action<object>> ();

        #region handler - Action

        public static void AddEventHandler (BtnOnClickType onClickType, Action handler) {
            if (!BtnOnClickDict.ContainsKey (onClickType)) {
                BtnOnClickDict.Add (onClickType, null);
            }

            BtnOnClickDict[onClickType] += handler;
        }

        public static void RemoveEventHandler (BtnOnClickType onClickType, Action handler) {
            if (BtnOnClickDict.ContainsKey (onClickType)) {
                BtnOnClickDict[onClickType] -= handler;

                if (BtnOnClickDict[onClickType] == null) {
                    BtnOnClickDict.Remove (onClickType);
                }
            }
        }

        public static void InvokeEvent (BtnOnClickType onClickType) {
            if (BtnOnClickDict.ContainsKey (onClickType)) {
                BtnOnClickDict[onClickType]?.Invoke ();
            }
        }

        #endregion

        #region handler - Action<object>

        public static void AddEventHandler (BtnOnClickType onClickType, Action<object> handler) {
            if (!BtnOnClickWithInfoDict.ContainsKey (onClickType)) {
                BtnOnClickWithInfoDict.Add (onClickType, null);
            }

            BtnOnClickWithInfoDict[onClickType] += handler;
        }

        public static void RemoveEventHandler (BtnOnClickType onClickType, Action<object> handler) {
            if (BtnOnClickWithInfoDict.ContainsKey (onClickType)) {
                BtnOnClickWithInfoDict[onClickType] -= handler;

                if (BtnOnClickWithInfoDict[onClickType] == null) {
                    BtnOnClickWithInfoDict.Remove (onClickType);
                }
            }
        }

        public static void InvokeEvent (BtnOnClickType onClickType, object info) {
            if (BtnOnClickWithInfoDict.ContainsKey (onClickType)) {
                BtnOnClickWithInfoDict[onClickType]?.Invoke (info);
            }
        }

        #endregion

    }
}