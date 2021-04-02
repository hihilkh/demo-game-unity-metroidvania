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

        protected AudioEnum.BgmType CurrentBgmType { get; private set; }

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
            LoadBgm (AudioConfig.GetLandingBgm (), false);

            SetBgmOnOff (GetSavedAudioOnOffFlag (AudioFrameworkEnum.Category.Bgm));
        }

        protected void LoadBgm (AudioEnum.BgmType bgmType, bool isChangingBgm) {
            if (isChangingBgm) {
                // TODO : Fade in out effect
                if (bgmType == CurrentBgmType) {
                    Log.PrintDebug ("The bgmType to change (" + bgmType + ") equal to CurrentBgmType. No need to do any action.", LogTypes.Audio);
                    return;
                }

                BgmAudioSource.clip = LoadBgmAudioClip (bgmType);
                Resources.UnloadUnusedAssets ();    // Unload unused BGM audio clip
            } else {
                BgmAudioSource.clip = LoadBgmAudioClip (bgmType);
            }

            if (bgmType != AudioEnum.BgmType.None) {
                BgmAudioSource.Play ();
            } else {
                BgmAudioSource.Stop ();
            }

            CurrentBgmType = bgmType;
        }

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

        /// <returns>If <paramref name="linear"/> is smaller or equal to 0, directly return <b>FrameworkVariable.AudioMixerAttenuation_LowerBound</b> in order to prevent calculation error.</returns>
        protected static float ConvertLinearToDecibel (float linear) {
            if (linear <= 0) {
                return FrameworkVariable.AudioMixerAttenuation_LowerBound;
            }

            return 20F * Mathf.Log10 (linear);
        }

        protected static float ConvertDecibelToLinear (float decibel) {
            return Mathf.Pow (10F, decibel / 20F);
        }

        #endregion

        /// <summary>
        /// Calculate attenuation from given volume factor by the scaling of AudioConfig.VolumeFactorScale.
        /// </summary>
        /// <returns>
        /// Value in decibel if AudioConfig.VolumeFactorScale = Decibel<br />
        /// Value in sound intensity if AudioConfig.VolumeFactorScale = Linear
        /// </returns>
        protected static float CalculateAttenuation (AudioFrameworkEnum.Category category, int volumeFactor) {
            var attenuation_LowerBound = FrameworkVariable.AudioMixerAttenuation_LowerBound;
            var attenuation_UpperBound = Mathf.Min (FrameworkVariable.AudioMixerAttenuation_UpperBound, AudioConfig.GetAttenuationUpperBound (category));

            switch (AudioConfig.VolumeFactorScale) {
                case AudioFrameworkEnum.VolumeScale.Decibel:
                    // No need to convert anything
                    break;
                case AudioFrameworkEnum.VolumeScale.Linear:
                    attenuation_LowerBound = ConvertDecibelToLinear (attenuation_LowerBound);
                    attenuation_UpperBound = ConvertDecibelToLinear (attenuation_UpperBound);
                    break;
                default:
                    Log.PrintError ("VolumeScale : " + AudioConfig.VolumeFactorScale + " has not yet been implemented CalculateAttenuation logic. Treat as Decibel.", LogTypes.Audio);
                    break;
            }

            volumeFactor = Mathf.Clamp (volumeFactor, FrameworkVariable.MinAudioVolumeFactor, FrameworkVariable.MaxAudioVolumeFactor);
            var progress = volumeFactor / (FrameworkVariable.MaxAudioVolumeFactor - FrameworkVariable.MinAudioVolumeFactor);
            return Mathf.Lerp (attenuation_LowerBound, attenuation_UpperBound, progress);
        }

        protected void Mute (AudioFrameworkEnum.Category category) {
            Log.Print ("Mute audio : AudioFrameworkEnum.Category : " + category, LogTypes.Audio);

            var audioMixer = GetAudioMixer (category);
            Mute (audioMixer);
        }

        protected void UnMute (AudioFrameworkEnum.Category category) {
            Log.Print ("UnMute audio : AudioFrameworkEnum.Category : " + category, LogTypes.Audio);

            var audioMixer = GetAudioMixer (category);
            var volumeFactor = GetSavedVolumeFactor (category);
            SetAttenuation (audioMixer, AudioConfig.VolumeFactorScale, CalculateAttenuation (category, volumeFactor));
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
            SetAttenuation (audioMixer, FrameworkVariable.AudioMixerAttenuation_LowerBound);

            return attenuationBefore;
        }

        /// <summary>
        /// Set attenuation by decibel. If you want to set with linear scale, use <b>SetAttenuation_Linear</b> instead.
        /// </summary>
        protected void SetAttenuation (AudioMixer audioMixer, float decibelValue) {
            SetAttenuation (audioMixer, AudioFrameworkEnum.VolumeScale.Decibel, decibelValue);
        }

        /// <summary>
        /// Set attenuation by linear. Compare to <b>SetAttenuation</b> (set by decibel), it requires conversion into decibel to set the value.
        /// </summary>
        protected void SetAttenuation_Linear (AudioMixer audioMixer, float linearValue) {
            SetAttenuation (audioMixer, AudioFrameworkEnum.VolumeScale.Linear, linearValue);
        }

        protected void SetAttenuation (AudioMixer audioMixer, AudioFrameworkEnum.VolumeScale volumeScale, float attenuationValue) {
            var decibelValue = attenuationValue;

            switch (volumeScale) {
                case AudioFrameworkEnum.VolumeScale.Decibel:
                    break;
                case AudioFrameworkEnum.VolumeScale.Linear:
                    decibelValue = ConvertLinearToDecibel (attenuationValue);
                    break;
                default:
                    Log.PrintError ("VolumeScale : " + volumeScale + " has not yet been implemented SetAttenuation logic. Treat as Decibel.", LogTypes.Audio);
                    break;
            }

            audioMixer.SetFloat (FrameworkVariable.MasterAttenuationExposedParam, decibelValue);
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