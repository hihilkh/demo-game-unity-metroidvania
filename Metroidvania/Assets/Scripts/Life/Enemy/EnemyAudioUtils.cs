using System.Collections;
using System.Collections.Generic;
using HihiFramework.Core;
using UnityEngine;

public class EnemyAudioUtils : MonoBehaviour {
    [Header ("AudioSources")]
    [SerializeField] private AudioSource jumpAudioSource;
    [SerializeField] private AudioSource beatBackAudioSource;
    [SerializeField] private AudioSource dieAudioSource;

    private void PlayOneShotSfx (AudioSource audioSource) {
        if (!audioSource.isActiveAndEnabled) {
            Log.Print (gameObject.name + " : PlayOneShotSfx failed. audioSource : " + audioSource.name + " is not yet active and enabled.", LogTypes.Enemy | LogTypes.Audio);
            return;
        }

        Log.PrintDebug (gameObject.name + " : PlayOneShotSfx : " + audioSource.clip.name, LogTypes.Enemy | LogTypes.Audio);
        audioSource.PlayOneShot (audioSource.clip);
    }

    #region Exposed methods

    public void PlayJumpSfx () {
        PlayOneShotSfx (jumpAudioSource);
    }

    public void PlayBeatBackSfx () {
        PlayOneShotSfx (beatBackAudioSource);
    }

    public void PlayDieSfx () {
        PlayOneShotSfx (dieAudioSource);
    }

    #endregion
}
