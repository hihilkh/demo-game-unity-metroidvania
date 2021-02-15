using UnityEngine;

namespace HihiFramework.Core {
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour, new() {
        private static object objLock = new object ();
        private static bool IsApplicationQuiting = false;

        private static T _Instance;
        public static T Instance {
            get {
                // Prevent creating singleton while quiting application
                if (IsApplicationQuiting) {
                    return null;
                }

                lock (objLock) {
                    if (_Instance == null) {
                        InitSingleton ();
                    }
                }

                return _Instance;
            }
        }

        private static void InitSingleton () {
            var go = new GameObject ();
            _Instance = go.AddComponent<T> ();
            go.name = "(Singleton) " + _Instance.GetType ().Name;
            DontDestroyOnLoad (go);
        }

        private void OnDestroy () {
            IsApplicationQuiting = false;
        }
    }
}