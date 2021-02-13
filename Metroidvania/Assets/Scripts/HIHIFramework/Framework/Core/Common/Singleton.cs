using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HIHIFramework.Core {
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour, new() {
        private static object objLock = new object ();
        private static bool IsApplicationQuiting = false;

        private static T instance;
        public static T Instance {
            get {
                // Prevent creating singleton while quiting application
                if (IsApplicationQuiting) {
                    return null;
                }

                lock (objLock) {
                    if (instance == null) {
                        InitSingleton ();
                    }
                }

                return instance;
            }
        }

        private static void InitSingleton () {
            var go = new GameObject ();
            instance = go.AddComponent<T> ();
            go.name = "(Singleton) " + instance.GetType ().Name;
            DontDestroyOnLoad (go);
        }

        private void OnDestroy () {
            IsApplicationQuiting = false;
        }
    }
}