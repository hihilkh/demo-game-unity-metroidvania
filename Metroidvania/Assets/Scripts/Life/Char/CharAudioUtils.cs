using System.Collections;
using System.Collections.Generic;
using HihiFramework.Core;
using UnityEngine;

public class CharAudioUtils : MonoBehaviour {
    [Header ("AudioSources")]
    [SerializeField] private AudioSource walkAudioSource;
    [SerializeField] private AudioSource dashAudioSource;
    [SerializeField] private AudioSource jumpAudioSource;
    [SerializeField] private AudioSource jumpChargingAudioSource;
    [SerializeField] private AudioSource dropHitChargingAudioSource;
    [SerializeField] private AudioSource beatBackAudioSource;
    [SerializeField] private AudioSource dieAudioSource;

    private void Awake () {
        GameUtils.TimeStopped += TimeStoppedHandler;
        GameUtils.TimeResumed += TimeResumedHandler;
    }

    private void OnDestroy () {
        GameUtils.TimeStopped -= TimeStoppedHandler;
        GameUtils.TimeResumed -= TimeResumedHandler;
    }

    private void PlaySfx (AudioSource audioSource) {
        if (!audioSource.isActiveAndEnabled) {
            Log.Print ("Char PlaySfx failed. audioSource : " + audioSource.name + " is not yet active and enabled.", LogTypes.Char | LogTypes.Audio);
            return;
        }

        Log.PrintDebug ("Char PlaySfx : " + audioSource.clip.name, LogTypes.Char | LogTypes.Audio);
        audioSource.Play ();
    }

    private void StopSfx (AudioSource audioSource) {
        Log.PrintDebug ("Char StopSfx : " + audioSource.clip.name, LogTypes.Char | LogTypes.Audio);
        audioSource.Stop ();
    }

    private void PlayOneShotSfx (AudioSource audioSource) {
        if (!audioSource.isActiveAndEnabled) {
            Log.Print ("Char PlayOneShotSfx failed. audioSource : " + audioSource.name + " is not yet active and enabled.", LogTypes.Char | LogTypes.Audio);
            return;
        }

        Log.PrintDebug ("Char PlayOneShotSfx : " + audioSource.clip.name, LogTypes.Char | LogTypes.Audio);
        audioSource.PlayOneShot (audioSource.clip);
    }

    private void PauseNonOneShotSfx () {
        Log.PrintDebug ("Char PauseNonOneShotSfx", LogTypes.Char | LogTypes.Audio);

        dashAudioSource.Pause ();
        jumpChargingAudioSource.Pause ();
        dropHitChargingAudioSource.Pause ();
    }

    private void UnPauseNonOneShotSfx () {
        Log.PrintDebug ("Char UnPauseNonOneShotSfx", LogTypes.Char | LogTypes.Audio);

        dashAudioSource.UnPause ();
        jumpChargingAudioSource.UnPause ();
        dropHitChargingAudioSource.UnPause ();
    }

    #region Exposed methods

    public void PlayWalkSfx () {
        PlayOneShotSfx (walkAudioSource);
    }

    public void PlayJumpSfx () {
        PlayOneShotSfx (jumpAudioSource);
    }

    public void PlayDashSfx () {
        PlaySfx (dashAudioSource);
    }

    public void StopDashSfx () {
        StopSfx (dashAudioSource);
    }

    public void PlayJumpChargingSfx () {
        PlaySfx (jumpChargingAudioSource);
    }

    public void StopJumpChargingSfx () {
        StopSfx (jumpChargingAudioSource);
    }

    public void PlayDropHitChargingSfx () {
        PlaySfx (dropHitChargingAudioSource);
    }

    public void StopDropHitChargingSfx () {
        StopSfx (dropHitChargingAudioSource);
    }

    public void PlayBeatBackSfx () {
        PlayOneShotSfx (beatBackAudioSource);
    }

    public void PlayDieSfx () {
        PlayOneShotSfx (dieAudioSource);
    }

    #endregion

    #region Time Control

    private void TimeStoppedHandler () {
        PauseNonOneShotSfx ();
    }

    private void TimeResumedHandler () {
        UnPauseNonOneShotSfx ();
    }

    #endregion
}