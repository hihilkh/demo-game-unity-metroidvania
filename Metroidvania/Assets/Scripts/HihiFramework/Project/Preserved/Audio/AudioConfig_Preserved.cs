using System.Collections;
using System.Collections.Generic;
using HihiFramework.Audio;
using HihiFramework.Core;
using UnityEngine;

public static partial class AudioConfig {
    /// <summary>
    /// Default On Off flag of corresponding audio category after app installed
    /// </summary>
    public static bool GetIsDefaultOn (AudioFrameworkEnum.Category category) {
        return true;
    }

    /// <summary>
    /// Default volume factor of corresponding audio category
    /// </summary>
    public static int GetDefaultVolumeFactor (AudioFrameworkEnum.Category category) {
        return 100;
    }

    /// <summary>
    /// The attenuation (in decibel) of corresponding audio category when it is with maximum volume factor.<br />
    /// The range should be within <b>FrameworkVariable.AudioMixerAttenuation_LowerBound</b> and <b>FrameworkVariable.AudioMixerAttenuation_UpperBound</b>.
    /// </summary>
    public static float GetAttenuationUpperBound (AudioFrameworkEnum.Category category) {
        switch (category) {
            case AudioFrameworkEnum.Category.Bgm:
                return -5;
            default:
                return 0;
        }
    }

    /// <summary>
    /// Get the BGM resources name (without folders) of corresponding BgmType
    /// </summary>
    public static string GetBgmResourcesName (AudioEnum.BgmType bgmType) {
        if (bgmType == AudioEnum.BgmType.None) {
            // No BGM for BgmType.None
            Log.PrintWarning ("You are trying to get BGM resources name of AudioEnum.BgmType.None, which should have no BGM. Return null.", LogTypes.Audio);
            return null;
        }

        switch (bgmType) {
            case AudioEnum.BgmType.General: return "Wanderlust";
            case AudioEnum.BgmType.CaveCollapsing: return "TheEpic2";
        }

        Log.PrintError ("AudioEnum.BgmType : " + bgmType + " has not been assigned resources name. Return null.", LogTypes.Audio);
        return null;
    }

    /// <summary>
    /// Get the dynamic SFX resources name (without folders) of corresponding DynamicSfxType
    /// </summary>
    public static string GetSfxResourcesName (AudioEnum.DynamicSfxType dynamicSfxType) {
        if (dynamicSfxType == AudioEnum.DynamicSfxType.None) {
            // No SFX for DynamicSfxType.None
            Log.PrintWarning ("You are trying to get SFX resources name of AudioEnum.DynamicSfxType.None, which should have no SFX. Return null.", LogTypes.Audio);
            return null;
        }

        switch (dynamicSfxType) {
            case AudioEnum.DynamicSfxType.ConfirmBtn: return "ConfirmBtn";
            case AudioEnum.DynamicSfxType.CancelBtn: return "CancelBtn";
            case AudioEnum.DynamicSfxType.StartGame: return "StartGame";
        }

        Log.PrintError ("AudioEnum.DynamicSfxType : " + dynamicSfxType + " has not been assigned resources name. Return null.", LogTypes.Audio);
        return null;
    }

    
    /// <summary>
    /// Get the BgmType when going into the game
    /// </summary>
    public static AudioEnum.BgmType GetLandingBgm () {
        return AudioEnum.BgmType.General;
    }

    /// <summary>
    /// The scale for calculation between volume factor and attenuation.<br />
    /// Refer to <b>AudioManagerBase.CalculateAttenuation()</b> for details.
    /// </summary>
    public const AudioFrameworkEnum.VolumeScale VolumeFactorScale = AudioFrameworkEnum.VolumeScale.Linear;

    /// <summary>
    /// The time used by <b>AudioManagerBase.ChangeBgmWithFading()</b> as default fading time
    /// </summary>
    public const float DefaultBgmFadingTime = 1F;

    /// <summary>
    /// The scale used by <b>AudioManagerBase.ChangeBgmWithFading()</b> as default fading volume scale
    /// </summary>
    public const AudioFrameworkEnum.VolumeScale DefaultBgmFadingScale = AudioFrameworkEnum.VolumeScale.Decibel;
}