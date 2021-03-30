using System.Collections.Generic;
using System.IO;
using HihiFramework.Core;
using UnityEngine;
using UnityEngine.Audio;

namespace HihiFramework.Audio {
    public abstract class AudioManagerBase<T> : Singleton<T> where T : MonoBehaviour, new() {

        private const float MinAudioMixerAttenuation_Decibel = -80F;
        private const float MaxAudioMixerAttenuation_Decibel = 20F;
        private const float MinAudioMixerAttenuation_SoundIntensityScale = 0.0001F;  // -80 = 20 x log(0.0001)
        private const float MaxAudioMixerAttenuation_SoundIntensityScale = 10F;      // 20 = 20 x log(10)

        private const float FallbackAttenuation = 0;    // In decibel

        private bool? _currentBgmOnOffFlag = null;
        private bool currentBgmOnOffFlag {
            get {
                if (_currentBgmOnOffFlag == null) {
                    if (PlayerPrefs.HasKey (FrameworkVariable.CurrentBgmOnOffFlagKey)) {
                        _currentBgmOnOffFlag = FrameworkUtils.GetPlayerPrefsBool (FrameworkVariable.CurrentBgmOnOffFlagKey);
                    } else {
                        var isDefaultOn = AudioConfig.GetIsDefaultBgmOn ();

                        // Set player prefs to save the default BGM on off flag
                        FrameworkUtils.SetPlayerPrefsBool (FrameworkVariable.CurrentBgmOnOffFlagKey, isDefaultOn);

                        _currentBgmOnOffFlag = isDefaultOn;
                    }
                }

                return (bool)_currentBgmOnOffFlag;
            }
            set {
                FrameworkUtils.SetPlayerPrefsBool (FrameworkVariable.CurrentBgmOnOffFlagKey, value);
                _currentBgmOnOffFlag = value;
            }
        }

        private bool? _currentSfxOnOffFlag = null;
        private bool currentSfxOnOffFlag {
            get {
                if (_currentSfxOnOffFlag == null) {
                    if (PlayerPrefs.HasKey (FrameworkVariable.CurrentSfxOnOffFlagKey)) {
                        _currentSfxOnOffFlag = FrameworkUtils.GetPlayerPrefsBool (FrameworkVariable.CurrentSfxOnOffFlagKey);
                    } else {
                        var isDefaultOn = AudioConfig.GetIsDefaultSfxOn ();

                        // Set player prefs to save the default SFX on off flag
                        FrameworkUtils.SetPlayerPrefsBool (FrameworkVariable.CurrentSfxOnOffFlagKey, isDefaultOn);

                        _currentSfxOnOffFlag = isDefaultOn;
                    }
                }

                return (bool)_currentSfxOnOffFlag;
            }
            set {
                FrameworkUtils.SetPlayerPrefsBool (FrameworkVariable.CurrentSfxOnOffFlagKey, value);
                _currentSfxOnOffFlag = value;
            }
        }

        private float _currentBgmAttenuation = MinAudioMixerAttenuation_Decibel - 1;    // In decibel
        private float currentBgmAttenuation {
            get {
                if (_currentBgmAttenuation < MinAudioMixerAttenuation_Decibel) {
                    if (PlayerPrefs.HasKey (FrameworkVariable.CurrentBgmAttenuationKey)) {
                        _currentBgmAttenuation = PlayerPrefs.GetFloat (FrameworkVariable.CurrentBgmAttenuationKey);
                    } else {
                        var attenuation = AudioConfig.GetDefaultBgmAttenuation ();

                        // Set player prefs to save the default BGM on off flag
                        PlayerPrefs.SetFloat (FrameworkVariable.CurrentBgmAttenuationKey, attenuation);

                        _currentBgmAttenuation = attenuation;
                    }
                }

                return _currentBgmAttenuation;
            }
            set {
                PlayerPrefs.SetFloat (FrameworkVariable.CurrentBgmAttenuationKey, value);
                _currentBgmAttenuation = value;
            }
        }

        private float _currentSfxAttenuation = MinAudioMixerAttenuation_Decibel - 1;    // In decibel
        private float currentSfxAttenuation {
            get {
                if (_currentSfxAttenuation < MinAudioMixerAttenuation_Decibel) {
                    if (PlayerPrefs.HasKey (FrameworkVariable.CurrentSfxAttenuationKey)) {
                        _currentSfxAttenuation = PlayerPrefs.GetFloat (FrameworkVariable.CurrentSfxAttenuationKey);
                    } else {
                        var attenuation = AudioConfig.GetDefaultSfxAttenuation ();

                        // Set player prefs to save the default BGM on off flag
                        PlayerPrefs.SetFloat (FrameworkVariable.CurrentSfxAttenuationKey, attenuation);

                        _currentSfxAttenuation = attenuation;
                    }
                }

                return _currentSfxAttenuation;
            }
            set {
                PlayerPrefs.SetFloat (FrameworkVariable.CurrentSfxAttenuationKey, value);
                _currentSfxAttenuation = value;
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

        public void Init () {
            Log.Print ("Start init AudioManager", LogTypes.Audio);

            InitBgm ();
            InitDynamicSfx ();

            Log.Print ("Successfully init AudioManager", LogTypes.Audio);
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

        public bool GetSavedAudioOnOffFlag (AudioFrameworkEnum.Category category) {
            switch (category) {
                case AudioFrameworkEnum.Category.Bgm: return currentBgmOnOffFlag;
                case AudioFrameworkEnum.Category.Sfx: return currentSfxOnOffFlag;
            }

            Log.PrintError ("AudioFrameworkEnum.Category : " + category + " has not been assigned audio on off flag cache. Return false.", LogTypes.Audio);
            return false;
        }

        private void SaveAudioOnOffFlag (AudioFrameworkEnum.Category category, bool isOn) {
            switch (category) {
                case AudioFrameworkEnum.Category.Bgm:
                    currentBgmOnOffFlag = isOn;
                    return;
                case AudioFrameworkEnum.Category.Sfx:
                    currentSfxOnOffFlag = isOn;
                    return;
            }

            Log.PrintError ("AudioFrameworkEnum.Category : " + category + " has not been assigned audio on off flag cache. Do not do anything.", LogTypes.Audio);
            return;
        }

        /// <returns>In decibel</returns>
        public float GetSavedAttenuation (AudioFrameworkEnum.Category category) {
            switch (category) {
                case AudioFrameworkEnum.Category.Bgm: return currentBgmAttenuation;
                case AudioFrameworkEnum.Category.Sfx: return currentSfxAttenuation;
            }

            Log.PrintError ("AudioFrameworkEnum.Category : " + category + " has not been assigned attenuation cache. Return FallbackAttenuation.", LogTypes.Audio);
            return FallbackAttenuation;
        }

        /// <param name="attenuation">In decibel</param>
        private void SaveAttenuation (AudioFrameworkEnum.Category category, float attenuation) {
            switch (category) {
                case AudioFrameworkEnum.Category.Bgm:
                    currentBgmAttenuation = attenuation;
                    return;
                case AudioFrameworkEnum.Category.Sfx:
                    currentSfxAttenuation = attenuation;
                    return;
            }

            Log.PrintError ("AudioFrameworkEnum.Category : " + category + " has not been assigned attenuation cache. Do not do anything.", LogTypes.Audio);
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
            SetAndSaveAudioOnOff (AudioFrameworkEnum.Category.Bgm, GetSavedAudioOnOffFlag (AudioFrameworkEnum.Category.Bgm), false);
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
            SetAndSaveAudioOnOff (AudioFrameworkEnum.Category.Sfx, GetSavedAudioOnOffFlag (AudioFrameworkEnum.Category.Sfx), false);
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

        #endregion

        #region Volume (Attenuation)

        #region Decibel Convertion

        protected float ConvertLinearToDecibel (float linear) {
            // Just allow from MinAudioMixerValue_SoundIntensityScale to MaxAudioMixerValue_SoundIntensityScale
            linear = Mathf.Clamp (linear, MinAudioMixerAttenuation_SoundIntensityScale, MaxAudioMixerAttenuation_SoundIntensityScale);

            return 20F * Mathf.Log10 (linear);
        }

        protected float ConvertDecibelToLinear (float decibel) {
            // Just allow from MinAudioMixerValue_Decibel to MaxAudioMixerValue_Decibel
            decibel = Mathf.Clamp (decibel, MinAudioMixerAttenuation_Decibel, MaxAudioMixerAttenuation_Decibel);

            return Mathf.Pow (10F, decibel / 20F);
        }

        #endregion

        public void SwitchAudioOnOff (AudioFrameworkEnum.Category category) {
            var isOn = !GetSavedAudioOnOffFlag (category);
            SetAndSaveAudioOnOff (category, isOn, true);
        }

        protected void SetAndSaveAudioOnOff (AudioFrameworkEnum.Category category, bool isOn, bool isSaveToPlayerPrefs = true) {
            Log.Print ("Set audio on off : AudioFrameworkEnum.Category : " + category + " ; isOn : " + isOn + " ; isSaveToPlayerPrefs : " + isSaveToPlayerPrefs, LogTypes.Audio);
            if (isSaveToPlayerPrefs) {
                SaveAudioOnOffFlag (category, isOn);
            }

            var audioMixer = GetAudioMixer (category);

            if (isOn) {
                SetAttenuation (audioMixer, GetSavedAttenuation (category));
            } else {
                Mute (audioMixer);
            }
        }

        /// <summary>
        /// Mute an audio mixer.<br />
        /// It does not save the setting to PlayerPrefs. If you want to set audio on/off of a AudioFrameworkEnum.Category and save the setting to PlayerPrefs,
        /// use <b>SetAndSaveAudioOnOff</b> instead.<br />
        /// it also does not remember the attenuation value before mute. If you want the framework to handle it, use <b>SetAndSaveAudioOnOff</b> instead.
        /// </summary>
        /// <returns>The attenuation value before setting to off</returns>
        protected float Mute (AudioMixer audioMixer) {
            var attenuationBefore = GetAttenuation (audioMixer);
            SetAttenuation (audioMixer, MinAudioMixerAttenuation_Decibel);

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

        private void SetAttenuation (AudioMixer audioMixer, AudioFrameworkEnum.VolumeScale volumeScale, float attenuationValue) {
            var decibelValue = attenuationValue;

            switch (volumeScale) {
                case AudioFrameworkEnum.VolumeScale.Decibel:
                    break;
                case AudioFrameworkEnum.VolumeScale.Linear:
                    decibelValue = ConvertLinearToDecibel (attenuationValue);
                    break;
                default:
                    Log.PrintWarning ("VolumeScale : " + volumeScale + " has not yet been implemented SetAttenuation logic. Treat as Decibel.", LogTypes.Audio);
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

            Log.PrintError ("Get current attenuation of audioMixer : " + audioMixer.name + " failed. Return FallbackAttenuation.", LogTypes.Audio);
            return FallbackAttenuation;
        }

        #endregion
    }
}