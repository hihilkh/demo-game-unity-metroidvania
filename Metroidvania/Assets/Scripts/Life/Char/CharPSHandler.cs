using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharPSHandler : MonoBehaviour {
    [SerializeField] private List<ParticleSystem> thrusterPSList;
    [SerializeField] private ParticleSystem jumpChargePS;
    [SerializeField] private ParticleSystem dropHitChargePS;

    public void SetThrusterPS (bool isOn) {
        foreach (var ps in thrusterPSList) {
            ps.gameObject.SetActive (isOn);
        }

    }

    public void SetJumpChargePS (bool isOn) {
        jumpChargePS.gameObject.SetActive (isOn);
    }

    public void SetDropHitChargePS (bool isOn) {
        dropHitChargePS.gameObject.SetActive (isOn);
    }
}