using System;
using System.Collections;
using System.Collections.Generic;

namespace HIHIFramework.UI {
    public class UIEventManager {

        private static Dictionary<BtnOnClickType, Action> BtnOnClickDict = new Dictionary<BtnOnClickType, Action> ();

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
    }
}