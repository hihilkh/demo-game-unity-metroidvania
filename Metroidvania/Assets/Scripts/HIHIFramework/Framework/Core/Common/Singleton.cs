using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HIHIFramework.Core {
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour {

        private static T instance;
        public static T Instance {
            get {
                if (instance == null) {
                    InitSingleton ();
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
    }
}