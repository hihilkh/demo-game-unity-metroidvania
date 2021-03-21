using System;
using System.Collections.Generic;

namespace HihiFramework.UI {
    public static class UIEventManager {

        private static readonly Dictionary<BtnOnClickType, Action<HihiButton>> BtnOnClickDict = new Dictionary<BtnOnClickType, Action<HihiButton>> ();
        private static readonly Dictionary<BtnOnClickType, Action<HihiButton, object>> BtnOnClickWithInfoDict = new Dictionary<BtnOnClickType, Action<HihiButton, object>> ();

        #region handler - Action

        public static void AddEventHandler (BtnOnClickType onClickType, Action<HihiButton> handler) {
            if (!BtnOnClickDict.ContainsKey (onClickType)) {
                BtnOnClickDict.Add (onClickType, null);
            }

            BtnOnClickDict[onClickType] += handler;
        }

        public static void RemoveEventHandler (BtnOnClickType onClickType, Action<HihiButton> handler) {
            if (BtnOnClickDict.ContainsKey (onClickType)) {
                BtnOnClickDict[onClickType] -= handler;

                if (BtnOnClickDict[onClickType] == null) {
                    BtnOnClickDict.Remove (onClickType);
                }
            }
        }

        public static void InvokeEvent (BtnOnClickType onClickType, HihiButton sender) {
            if (BtnOnClickDict.ContainsKey (onClickType)) {
                BtnOnClickDict[onClickType]?.Invoke (sender);
            }
        }

        #endregion

        #region handler - Action<object>

        public static void AddEventHandler (BtnOnClickType onClickType, Action<HihiButton, object> handler) {
            if (!BtnOnClickWithInfoDict.ContainsKey (onClickType)) {
                BtnOnClickWithInfoDict.Add (onClickType, null);
            }

            BtnOnClickWithInfoDict[onClickType] += handler;
        }

        public static void RemoveEventHandler (BtnOnClickType onClickType, Action<HihiButton, object> handler) {
            if (BtnOnClickWithInfoDict.ContainsKey (onClickType)) {
                BtnOnClickWithInfoDict[onClickType] -= handler;

                if (BtnOnClickWithInfoDict[onClickType] == null) {
                    BtnOnClickWithInfoDict.Remove (onClickType);
                }
            }
        }

        public static void InvokeEvent (BtnOnClickType onClickType, HihiButton sender, object info) {
            if (BtnOnClickWithInfoDict.ContainsKey (onClickType)) {
                BtnOnClickWithInfoDict[onClickType]?.Invoke (sender, info);
            }
        }

        #endregion

    }
}