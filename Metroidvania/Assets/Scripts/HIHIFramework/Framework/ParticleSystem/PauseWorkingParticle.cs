using UnityEngine;

namespace HihiFramework.PS {
    public class PauseWorkingParticle : MonoBehaviour {
        private ParticleSystem ps;

        private void Awake () {
            ps = GetComponent<ParticleSystem> ();
        }
        // Update is called once per frame
        private void Update () {
            if (Time.timeScale == 0) {
                ps.Simulate (Time.unscaledDeltaTime, true, false);
            }
        }
    }
}
