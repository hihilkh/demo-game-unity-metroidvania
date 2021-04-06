using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using HihiFramework.Core;
using UnityEngine;
using UnityEngine.Audio;

namespace HihiFramework.Audio {
    public abstract class AudioManagerBase<T> : Singleton<T> where T : MonoBehaviour, new() {

        private bool? _currentBgmOnOffFlag = null;
        public bool CurrentBgmOnOffFlag {
            get {
                if (_currentBgmOnOffFlag == null) {
                    if (PlayerPrefs.HasKey (FrameworkVariable.CurrentBgmOnOffFlagKey)) {
                        _currentBgmOnOffFlag = FrameworkUtils.GetPlayerPrefsBool (FrameworkVariable.CurrentBgmOnOffFlagKey);
                    } else {
                        var isDefaultOn = AudioConfig.GetIsDefaultOn (AudioFrameworkEnum.Category.Bgm);

                        // Set player prefs to save the default BGM on off flag
                        FrameworkUtils.SetPlayerPrefsBool (FrameworkVariable.CurrentBgmOnOffFlagKey, isDefaultOn);

                        _currentBgmOnOffFlag = isDefaultOn;
                    }
                }

                return (bool)_currentBgmOnOffFlag;
            }
            private set {
                FrameworkUtils.SetPlayerPrefsBool (FrameworkVariable.CurrentBgmOnOffFlagKey, value);
                _currentBgmOnOffFlag = value;
            }
        }

        private bool? _currentSfxOnOffFlag = null;
        public bool CurrentSfxOnOffFlag {
            get {
                if (_currentSfxOnOffFlag == null) {
                    if (PlayerPrefs.HasKey (FrameworkVariable.CurrentSfxOnOffFlagKey)) {
                        _currentSfxOnOffFlag = FrameworkUtils.GetPlayerPrefsBool (FrameworkVariable.CurrentSfxOnOffFlagKey);
                    } else {
                        var isDefaultOn = AudioConfig.GetIsDefaultOn (AudioFrameworkEnum.Category.Sfx);

                        // Set player prefs to save the default SFX on off flag
                        FrameworkUtils.SetPlayerPrefsBool (FrameworkVariable.CurrentSfxOnOffFlagKey, isDefaultOn);

                        _currentSfxOnOffFlag = isDefaultOn;
                    }
                }

                return (bool)_currentSfxOnOffFlag;
            }
            private set {
                FrameworkUtils.SetPlayerPrefsBool (FrameworkVariable.CurrentSfxOnOffFlagKey, value);
                _currentSfxOnOffFlag = value;
            }
        }

        private int _currentBgmVolumeFactor = FrameworkVariable.MinAudioVolumeFactor - 1;
        public int CurrentBgmVolumeFactor {
            get {
                if (_currentBgmVolumeFactor < FrameworkVariable.MinAudioVolumeFactor) {
                    if (PlayerPrefs.HasKey (FrameworkVariable.CurrentBgmVolumeFactorKey)) {
                        _currentBgmVolumeFactor = PlayerPrefs.GetInt (FrameworkVariable.CurrentBgmVolumeFactorKey);
                    } else {
                        var volumeFactor = AudioConfig.GetDefaultVolumeFactor (AudioFrameworkEnum.Category.Bgm);

                        // Set player prefs to save the default BGM volume factor
                        PlayerPrefs.SetInt (FrameworkVariable.CurrentBgmVolumeFactorKey, volumeFactor);

                        _currentBgmVolumeFactor = volumeFactor;
                    }
                }

                return _currentBgmVolumeFactor;
            }
            private set {
                PlayerPrefs.SetInt (FrameworkVariable.CurrentBgmVolumeFactorKey, value);
                _currentBgmVolumeFactor = value;
            }
        }

        private int _currentSfxVolumeFactor = FrameworkVariable.MinAudioVolumeFactor - 1;
        public int CurrentSfxVolumeFactor {
            get {
                if (_currentSfxVolumeFactor < FrameworkVariable.MinAudioVolumeFactor) {
                    if (PlayerPrefs.HasKey (FrameworkVariable.CurrentSfxVolumeFactorKey)) {
                        _currentSfxVolumeFactor = PlayerPrefs.GetInt (FrameworkVariable.CurrentSfxVolumeFactorKey);
                    } else {
                        var volumeFactor = AudioConfig.GetDefaultVolumeFactor (AudioFrameworkEnum.Category.Sfx);

                        // Set player prefs to save the default SFX volume factor
                        PlayerPrefs.SetInt (FrameworkVariable.CurrentSfxVolumeFactorKey, volumeFactor);

                        _currentSfxVolumeFactor = volumeFactor;
                    }
                }

                return _currentSfxVolumeFactor;
            }
            private set {
                PlayerPrefs.SetInt (FrameworkVariable.CurrentSfxVolumeFactorKey, value);
                _currentSfxVolumeFactor = value;
            }
        }

        private AudioMixer _bgmAudioMixer = null;
        private AudioMixer bgmAudioMixer {
            get {
                if (_bgmAudioMixer == null) {
                    _bgmAudioMixer = Resources.Load<AudioMixer> (GetAudioMixerResourcesFileFullPath (FrameworkVariable.BgmAudioMixerResourcesName));
                }

                return _bgmAudioMixer;
            }
        }

        private AudioMixer _sfxAudioMixer = null;
        private AudioMixer sfxAudioMixer {
            get {
                if (_sfxAudioMixer == null) {
                    _sfxAudioMixer = Resources.Load<AudioMixer> (GetAudioMixerResourcesFileFullPath (FrameworkVariable.SfxAudioMixerResourcesName));
                }

                return _sfxAudioMixer;
            }
        }

        private AudioSource _bgmAudioSource = null;
        protected AudioSource BgmAudioSource {
            get {
                if (_bgmAudioSource == null) {
                    _bgmAudioSource = gameObject.AddComponent<AudioSource> ();
                    _bgmAudioSource.outputAudioMixerGroup = GetAudioMixerCommonGroup (AudioFrameworkEnum.Category.Bgm);
                    _bgmAudioSource.loop = true;
                    _bgmAudioSource.ignoreListenerVolume = true;
                    _bgmAudioSource.playOnAwake = false;
                }

                return _bgmAudioSource;
            }
        }

        private AudioSource _dynamicSfxAudioSource = null;
        protected AudioSource DynamicSfxAudioSource {
            get {
                if (_dynamicSfxAudioSource == null) {
                    _dynamicSfxAudioSource = gameObject.AddComponent<AudioSource> ();
                    _dynamicSfxAudioSource.outputAudioMixerGroup = GetAudioMixerCommonGroup (AudioFrameworkEnum.Category.Sfx);
                    _dynamicSfxAudioSource.loop = false;
                    _dynamicSfxAudioSource.ignoreListenerVolume = true;
                    _dynamicSfxAudioSource.playOnAwake = false;
                }

                return _dynamicSfxAudioSource;
            }
        }

        protected AudioEnum.BgmType CurrentBgmType { get; private set; } = AudioEnum.BgmType.None;
        private Coroutine bgmFadingCoroutine = null;

        private readonly Dictionary<AudioEnum.DynamicSfxType, AudioClip> dynamicSfxAudioClipDict = new Dictionary<AudioEnum.DynamicSfxType, AudioClip> ();

        #region Initialization

        public void Init (Action<bool> onFinished = null) {
            StartCoroutine (WaitAndInit (onFinished));
        }

        private IEnumerator WaitAndInit (Action<bool> onFinished = null) {
            // Remarks : Delay a frame because AudioMixer.SetFloat somehow doesn't work on Awake ()

            Log.Print ("Start init AudioManager", LogTypes.Audio);

            yield return null;

            InitBgm ();
            InitDynamicSfx ();

            Log.Print ("Successfully init AudioManager", LogTypes.Audio);

            onFinished?.Invoke (true);
        }

        #endregion

        #region Audio Mixer

        protected string GetAudioMixerResourcesFileFullPath (string resourcesName) {
            return Path.Combine (FrameworkVariable.AudioMixerResourcesFolder, resourcesName);
        }

        protected AudioMixer GetAudioMixer (AudioFrameworkEnum.Category category) {
            switch (category) {
                case AudioFrameworkEnum.Category.Bgm:
                    return bgmAudioMixer;
                case AudioFrameworkEnum.Category.Sfx:
                    return sfxAudioMixer;
            }

            Log.PrintError ("There is no defined audio mixer inside the framework with AudioFrameworkEnum.Category : " + category, LogTypes.Audio);
            return null;
        }

        protected AudioMixerGroup GetAudioMixerGroup (AudioFrameworkEnum.Category category, string audioMixerGroupName) {
            var audioMixer = GetAudioMixer (category);
            return GetAudioMixerGroup (audioMixer, audioMixerGroupName);
        }

        protected AudioMixerGroup GetAudioMixerGroup (AudioMixer audioMixer, string audioMixerGroupName) {
            AudioMixerGroup result = null;

            if (audioMixer != null) {
                var matchArray = audioMixer.FindMatchingGroups (audioMixerGroupName);

                if (matchArray != null && matchArray.Length > 0) {
                    result = matchArray[0];
                }
            }

            if (result != null) {
                return result;
            }

            Log.PrintError ("No matched audio mixer group is found. AudioMixer : " + audioMixer.name + " , audioMixerGroupName : " + audioMixerGroupName, LogTypes.Audio);
            return null;
        }

        protected AudioMixerGroup GetAudioMixerMasterGroup (AudioFrameworkEnum.Category category) {
            return GetAudioMixerGroup (category, FrameworkVariable.AudioMixerMasterGroupName);
        }

        protected AudioMixerGroup GetAudioMixerCommonGroup (AudioFrameworkEnum.Category category) {
            return GetAudioMixerGroup (category, FrameworkVariable.AudioMixerCommonGroupName);
        }

        #endregion

        #region Player Prefs Parameters

        private bool GetSavedAudioOnOffFlag (AudioFrameworkEnum.Category category) {
            switch (category) {
                case AudioFrameworkEnum.Category.Bgm: return CurrentBgmOnOffFlag;
                case AudioFrameworkEnum.Category.Sfx: return CurrentSfxOnOffFlag;
            }

            Log.PrintError ("AudioFrameworkEnum.Category : " + category + " has not been assigned audio on off flag cache. Return false.", LogTypes.Audio);
            return false;
        }

        private void SaveAudioOnOffFlag (AudioFrameworkEnum.Category category, bool isOn) {
            switch (category) {
                case AudioFrameworkEnum.Category.Bgm:
                    CurrentBgmOnOffFlag = isOn;
                    return;
                case AudioFrameworkEnum.Category.Sfx:
                    CurrentSfxOnOffFlag = isOn;
                    return;
            }

            Log.PrintError ("AudioFrameworkEnum.Category : " + category + " has not been assigned audio on off flag cache. Do not do anything.", LogTypes.Audio);
            return;
        }

        private int GetSavedVolumeFactor (AudioFrameworkEnum.Category category) {
            switch (category) {
                case AudioFrameworkEnum.Category.Bgm: return CurrentBgmVolumeFactor;
                case AudioFrameworkEnum.Category.Sfx: return CurrentSfxVolumeFactor;
            }

            Log.PrintError ("AudioFrameworkEnum.Category : " + category + " has not been assigned volume factor cache. Return FrameworkVariable.MinAudioVolumeFactor.", LogTypes.Audio);
            return FrameworkVariable.MinAudioVolumeFactor;
        }

        private void SaveVolumeFactor (AudioFrameworkEnum.Category category, int volumeFactor) {
            switch (category) {
                case AudioFrameworkEnum.Category.Bgm:
                    CurrentBgmVolumeFactor = volumeFactor;
                    return;
                case AudioFrameworkEnum.Category.Sfx:
                    CurrentSfxVolumeFactor = volumeFactor;
                    return;
            }

            Log.PrintError ("AudioFrameworkEnum.Category : " + category + " has not been assigned volume factor cache. Do not do anything.", LogTypes.Audio);
            return;
        }

        #endregion

        #region BGM

        private string GetBgmResourcesFileFullPath (string resourcesName) {
            var folder = Path.Combine (FrameworkVariable.AudioResourcesFolder, FrameworkVariable.BgmAudioResourcesSubFolder);
            return Path.Combine (folder, resourcesName);
        }

        /// <summary>
        /// Load the audio clip of corresponding BgmType from Resources folder. Be careful of memory leak when using.
        /// </summary>
        /// <param name="bgmType"></param>
        /// <returns></returns>
        protected AudioClip LoadBgmAudioClip (AudioEnum.BgmType bgmType) {
            if (bgmType == AudioEnum.BgmType.None) {
                Log.Print ("You are loading BGM audio clip of AudioEnum.BgmType.None, which is null.", LogTypes.Audio);
                return null;
            }

            Log.Print ("Load BGM audio clip of AudioEnum.BgmType : " + bgmType, LogTypes.Audio);

            var resourcesFileFullPath = GetBgmResourcesFileFullPath (AudioConfig.GetBgmResourcesName (bgmType));
            var audioClip = Resources.Load<AudioClip> (resourcesFileFullPath);
            if (audioClip == null) {
                Log.PrintWarning ("Fail to load BGM audio clip of AudioEnum.BgmType : " + bgmType + " , resources file path : " + resourcesFileFullPath, LogTypes.Audio);
            }

            return audioClip;
        }

        private void InitBgm () {
            ChangeBgm (AudioConfig.GetLandingBgm ());

            SetBgmOnOff (GetSavedAudioOnOffFlag (AudioFrameworkEnum.Category.Bgm));
        }

        public void ChangeBgm (AudioEnum.BgmType bgmType) {
            if (bgmType == CurrentBgmType) {
                Log.PrintDebug ("The bgmType to change (" + bgmType + ") equal to CurrentBgmType. No need to do any action.", LogTypes.Audio);
                return;
            }

            var isNeedToUnloadAssets = BgmAudioSource.clip != null;

            BgmAudioSource.clip = LoadBgmAudioClip (bgmType);
            if (BgmAudioSource.clip != null) {
                BgmAudioSource.Play ();
            } else {
                BgmAudioSource.Stop ();
            }

            CurrentBgmType = bgmType;

            if (isNeedToUnloadAssets) {
                Resources.UnloadUnusedAssets ();
            }
        }

        /// <summary>
        /// If calling this method while already doing fade BGM action, it would totally override the current fade BGM action<br />
        /// i.e., some <paramref name="onFadeOutFinished"/> or <paramref name="onFadeInFinished"/> action of <b>previous</b> fade BGM call may not be triggered.
        /// </summary>
        public void ChangeBgmWithFading (AudioEnum.BgmType bgmType, Action onFadeOutFinished, Action onFadeInFinished) {
            ChangeBgmWithFading (bgmType, AudioConfig.DefaultBgmFadingScale, AudioConfig.DefaultBgmFadingTime, onFadeOutFinished, onFadeInFinished);
        }

        /// <summary>
        /// If calling this method while already doing fade BGM action, it would totally override the current fade BGM action<br />
        /// i.e., some <paramref name="onFadeOutFinished"/> or <paramref name="onFadeInFinished"/> action of <b>previous</b> fade BGM call may not be triggered.
        /// </summary>
        public void ChangeBgmWithFading (AudioEnum.BgmType bgmType, AudioFrameworkEnum.VolumeScale fadingVolumeScale = AudioConfig.DefaultBgmFadingScale, float fadingTime = AudioConfig.DefaultBgmFadingTime, Action onFadeOutFinished = null, Action onFadeInFinished = null) {
            var isAlreadyFading = false;
            if (bgmFadingCoroutine != null) {
                isAlreadyFading = true;
                StopCoroutine (bgmFadingCoroutine);
                bgmFadingCoroutine = null;
            }

            if (bgmType == CurrentBgmType) {
                onFadeOutFinished?.Invoke ();

                if (isAlreadyFading) {
                    Log.PrintDebug ("The bgmType to change (" + bgmType + ") equal to CurrentBgmType but the BGM is fading. Resume the volume by fading in.", LogTypes.Audio);
                    FadeInBgm (fadingVolumeScale, fadingTime, onFadeInFinished);
                } else {
                    Log.PrintDebug ("The bgmType to change (" + bgmType + ") equal to CurrentBgmType. No need to do any action.", LogTypes.Audio);
                    onFadeInFinished?.Invoke ();
                }
                return;
            }

            Action onFadeOutBgmFinished = () => {
                onFadeOutFinished?.Invoke ();
                ChangeBgm (bgmType);
                FadeInBgm (fadingVolumeScale, fadingTime, onFadeInFinished);
            };

            FadeOutBgm (fadingVolumeScale, fadingTime, onFadeOutBgmFinished);
        }

        #region FadeIn FadeOut methods

        public void FadeOutBgm (AudioFrameworkEnum.VolumeScale fadingVolumeScale, float fadingTime, Action onFinished = null) {
            if (!GetSavedAudioOnOffFlag (AudioFrameworkEnum.Category.Bgm)) {
                Log.Print ("BGM is on mute. No need to do fade out BGM action.", LogTypes.Audio);
                onFinished?.Invoke ();
                return;
            }

            var initialAttenuation = GetAttenuation (AudioFrameworkEnum.Category.Bgm);
            var destAttenuation = FrameworkVariable.AudioMixerAttenuation_LowerBound;

            bgmFadingCoroutine = StartCoroutine (FadeBgmCoroutine (initialAttenuation, destAttenuation, fadingVolumeScale, fadingTime, onFinished));
        }

        public void FadeInBgm (AudioFrameworkEnum.VolumeScale fadingVolumeScale, float fadingTime, Action onFinished = null) {
            if (!GetSavedAudioOnOffFlag (AudioFrameworkEnum.Category.Bgm)) {
                Log.Print ("BGM is on mute. No need to do fade in BGM action.", LogTypes.Audio);
                onFinished?.Invoke ();
                return;
            }

            var initialAttenuation = GetAttenuation (AudioFrameworkEnum.Category.Bgm);
            var destVolumeFactor = GetSavedVolumeFactor (AudioFrameworkEnum.Category.Bgm);
            var destAttenuation = CalculateAttenuation (AudioFrameworkEnum.Category.Bgm, destVolumeFactor);

            bgmFadingCoroutine = StartCoroutine (FadeBgmCoroutine (initialAttenuation, destAttenuation, fadingVolumeScale, fadingTime, onFinished));
        }

        private IEnumerator FadeBgmCoroutine (float initialAttenuation_Decibel, float destAttenuation_Decibel, AudioFrameworkEnum.VolumeScale fadingVolumeScale, float fadingTime, Action onFinished = null) {
            Action onFadingFinished = () => {
                bgmFadingCoroutine = null;
                onFinished?.Invoke ();
            };

            if (fadingTime <= 0) {
                SetAttenuation (AudioFrameworkEnum.Category.Bgm, AudioFrameworkEnum.VolumeScale.Decibel, destAttenuation_Decibel);
                onFadingFinished.Invoke ();
                yield break;
            }

            var initialAttenuation = ConvertVolumeScale (AudioFrameworkEnum.VolumeScale.Decibel, fadingVolumeScale, initialAttenuation_Decibel);
            var destAttenuation = ConvertVolumeScale (AudioFrameworkEnum.VolumeScale.Decibel, fadingVolumeScale, destAttenuation_Decibel);

            float startTime = Time.unscaledTime;
            float progress = 0;
            do {
                var attenuation = Mathf.Lerp (initialAttenuation, destAttenuation, progress);
                SetAttenuation (AudioFrameworkEnum.Category.Bgm, fadingVolumeScale, attenuation);
                yield return null;
                progress = (Time.unscaledTime - startTime) / fadingTime;
            }
            while (progress < 1);

            SetAttenuation (AudioFrameworkEnum.Category.Bgm, AudioFrameworkEnum.VolumeScale.Decibel, destAttenuation_Decibel);
            onFadingFinished.Invoke ();
        }

        #endregion

        #endregion

        #region SFX

        private string GetSfxResourcesFileFullPath (string resourcesName) {
            var folder = Path.Combine (FrameworkVariable.AudioResourcesFolder, FrameworkVariable.SfxAudioResourcesSubFolder);
            return Path.Combine (folder, resourcesName);
        }

        /// <summary>
        /// Load or get cached audio clip of corresponding DynamicSfxType from Resources folder. Once loaded, it will save to cache for reuse.
        /// </summary>
        protected AudioClip LoadDynamicSfxAudioClip (AudioEnum.DynamicSfxType dynamicSfxType) {
            if (dynamicSfxType == AudioEnum.DynamicSfxType.None) {
                Log.Print ("You are loading SFX audio clip of AudioEnum.DynamicSfxType.None, which is null.", LogTypes.Audio);
                return null;
            }

            if (!dynamicSfxAudioClipDict.ContainsKey (dynamicSfxType)) {
                Log.Print ("Load SFX audio clip of AudioEnum.DynamicSfxType : " + dynamicSfxType, LogTypes.Audio);

                var resourcesFileFullPath = GetSfxResourcesFileFullPath (AudioConfig.GetSfxResourcesName (dynamicSfxType));
                var audioClip = Resources.Load<AudioClip> (resourcesFileFullPath);
                if (audioClip == null) {
                    Log.PrintWarning ("Fail to load SFX audio clip of AudioEnum.DynamicSfxType : " + dynamicSfxType + " , resources file path : " + resourcesFileFullPath, LogTypes.Audio);
                }

                dynamicSfxAudioClipDict.Add (dynamicSfxType, audioClip);
            }

            return dynamicSfxAudioClipDict[dynamicSfxType];
        }

        private void InitDynamicSfx () {
            SetSfxOnOff (GetSavedAudioOnOffFlag (AudioFrameworkEnum.Category.Sfx));
        }

        public void PlayDynamicSFX (AudioEnum.DynamicSfxType dynamicSfxType) {
            if (dynamicSfxType == AudioEnum.DynamicSfxType.None) {
                // Do not do anything
                return;
            }

            var audioClip = LoadDynamicSfxAudioClip (dynamicSfxType);
            if (audioClip != null) {
                DynamicSfxAudioSource.PlayOneShot (audioClip);
            } else {
                Log.PrintWarning ("Audio clip of AudioEnum.DynamicSfxType : " + dynamicSfxType + " is null. Do not play SFX.", LogTypes.Audio);
            }
        }

        /// <summary>
        /// Use dynamic SFX audio source to play SFX to prevent some problems (e.g. the game object of the audio source is destroyed)
        /// </summary>
        public void PlaySFX (AudioClip audioClip) {
            if (audioClip != null) {
                DynamicSfxAudioSource.PlayOneShot (audioClip);
            } else {
                Log.PrintWarning ("Audio clip is null. Do not play SFX.", LogTypes.Audio);
            }
        }

        #endregion

        #region Volume (Attenuation)

        #region Decibel Convertion

        /// <returns>Notes : For <paramref name="from"/> = Linear, <paramref name="to"/> = Decibel and <paramref name="fromValue"/> is smaller or equal to 0, directly return <b>FrameworkVariable.AudioMixerAttenuation_LowerBound</b> in order to prevent calculation error.</returns>
        protected static float ConvertVolumeScale (AudioFrameworkEnum.VolumeScale from, AudioFrameworkEnum.VolumeScale to, float fromValue) {
            switch (from) {
                case AudioFrameworkEnum.VolumeScale.Decibel:
                    switch (to) {
                        case AudioFrameworkEnum.VolumeScale.Decibel:
                            return fromValue;
                        case AudioFrameworkEnum.VolumeScale.Linear:
                            return Mathf.Pow (10F, fromValue / 20F);
                    }
                    break;
                case AudioFrameworkEnum.VolumeScale.Linear:
                    switch (to) {
                        case AudioFrameworkEnum.VolumeScale.Decibel:
                            if (fromValue <= 0) {
                                return FrameworkVariable.AudioMixerAttenuation_LowerBound;
                            }

                            return 20F * Mathf.Log10 (fromValue);
                        case AudioFrameworkEnum.VolumeScale.Linear:
                            return fromValue;
                    }
                    break;
            }

            Log.PrintError ("ConvertVolumeScale from " + from + " to " + to + " has not yet been implemented logic. return fromValue.", LogTypes.Audio);
            return fromValue;
        }

        #endregion

        /// <summary>
        /// Calculate attenuation from given volume factor by the scaling of AudioConfig.VolumeFactorScale.
        /// </summary>
        /// <returns>
        /// In decibel
        /// </returns>
        protected static float CalculateAttenuation (AudioFrameworkEnum.Category category, int volumeFactor) {
            var attenuation_LowerBound = ConvertVolumeScale (AudioFrameworkEnum.VolumeScale.Decibel, AudioConfig.VolumeFactorScale, FrameworkVariable.AudioMixerAttenuation_LowerBound);
            var attenuation_UpperBound = Mathf.Min (FrameworkVariable.AudioMixerAttenuation_UpperBound, AudioConfig.GetAttenuationUpperBound (category));
            attenuation_UpperBound = ConvertVolumeScale (AudioFrameworkEnum.VolumeScale.Decibel, AudioConfig.VolumeFactorScale, attenuation_UpperBound);

            volumeFactor = Mathf.Clamp (volumeFactor, FrameworkVariable.MinAudioVolumeFactor, FrameworkVariable.MaxAudioVolumeFactor);
            var progress = volumeFactor / (FrameworkVariable.MaxAudioVolumeFactor - FrameworkVariable.MinAudioVolumeFactor);

            var attenuation = Mathf.Lerp (attenuation_LowerBound, attenuation_UpperBound, progress);
            return ConvertVolumeScale (AudioConfig.VolumeFactorScale, AudioFrameworkEnum.VolumeScale.Decibel, attenuation);
        }

        protected void Mute (AudioFrameworkEnum.Category category) {
            Log.Print ("Mute audio : AudioFrameworkEnum.Category : " + category, LogTypes.Audio);

            var audioMixer = GetAudioMixer (category);
            Mute (audioMixer);
        }

        protected void UnMute (AudioFrameworkEnum.Category category) {
            Log.Print ("UnMute audio : AudioFrameworkEnum.Category : " + category, LogTypes.Audio);

            var volumeFactor = GetSavedVolumeFactor (category);
            SetAttenuation (category, AudioFrameworkEnum.VolumeScale.Decibel, CalculateAttenuation (category, volumeFactor));
        }

        /// <summary>
        /// Mute an audio mixer.<br />
        /// With this method, you need to handle the attenuation value before mute manually.
        /// For the audio mixer of AudioFrameworkEnum.Category, please use <b>Mute(AudioFrameworkEnum.Category)</b> and <b>UnMute(AudioFrameworkEnum.Category)</b> instead.
        /// </summary>
        /// <returns>The attenuation value before mute</returns>
        protected float Mute (AudioMixer audioMixer) {
            Log.Print ("Mute AudioMixer : " + audioMixer.name, LogTypes.Audio);
            var attenuationBefore = GetAttenuation (audioMixer);
            SetAttenuation (audioMixer, AudioFrameworkEnum.VolumeScale.Decibel, FrameworkVariable.AudioMixerAttenuation_LowerBound);

            return attenuationBefore;
        }

        /// <summary>
        /// Set attenuation.<br />
        /// <paramref name="attenuationValue"/> should be match with <paramref name="volumeScale"/>.<br />
        /// Notes : If <paramref name="volumeScale"/> = Linear, it requires extra conversion step into decibel.
        /// </summary>
        protected void SetAttenuation (AudioFrameworkEnum.Category category, AudioFrameworkEnum.VolumeScale volumeScale, float attenuationValue) {
            var audioMixer = GetAudioMixer (category);
            SetAttenuation (audioMixer, volumeScale, attenuationValue);
        }

        /// <summary>
        /// Set attenuation.<br />
        /// <paramref name="attenuationValue"/> should be match with <paramref name="volumeScale"/>.<br />
        /// Notes : If <paramref name="volumeScale"/> = Linear, it requires extra conversion step into decibel.
        /// </summary>
        protected void SetAttenuation (AudioMixer audioMixer, AudioFrameworkEnum.VolumeScale volumeScale, float attenuationValue) {
            var decibelValue = ConvertVolumeScale (volumeScale, AudioFrameworkEnum.VolumeScale.Decibel, attenuationValue);
            audioMixer.SetFloat (FrameworkVariable.MasterAttenuationExposedParam, decibelValue);
        }

        /// <returns>In decibel</returns>
        protected float GetAttenuation (AudioFrameworkEnum.Category category) {
            var audioMixer = GetAudioMixer (category);
            return GetAttenuation (audioMixer);
        }

        /// <returns>In decibel</returns>
        protected float GetAttenuation (AudioMixer audioMixer) {
            float result;
            if (audioMixer.GetFloat (FrameworkVariable.MasterAttenuationExposedParam, out result)) {
                return result;
            }

            Log.PrintError ("Get current attenuation of audioMixer : " + audioMixer.name + " failed. Return FrameworkVariable.AudioMixerAttenuation_LowerBound.", LogTypes.Audio);
            return FrameworkVariable.AudioMixerAttenuation_LowerBound;
        }

        #endregion

        #region Exposed methods for settings panel

        public void SetBgmOnOff (bool isOn) {
            if (isOn) {
                UnMute (AudioFrameworkEnum.Category.Bgm);
            } else {
                Mute (AudioFrameworkEnum.Category.Bgm);
            }
        }

        public void SaveBgmOnOffSetting (bool isOn) {
            SaveAudioOnOffFlag (AudioFrameworkEnum.Category.Bgm, isOn);
        }

        public void SetSfxOnOff (bool isOn) {
            if (isOn) {
                UnMute (AudioFrameworkEnum.Category.Sfx);
            } else {
                Mute (AudioFrameworkEnum.Category.Sfx);
            }
        }

        public void SaveSfxOnOffSetting (bool isOn) {
            SaveAudioOnOffFlag (AudioFrameworkEnum.Category.Sfx, isOn);
        }

        // TODO : Add attenuation control methods for settings panel

        #endregion

    }
}