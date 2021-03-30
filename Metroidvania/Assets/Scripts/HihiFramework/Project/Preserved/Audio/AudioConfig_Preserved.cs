using System.Collections;
using System.Collections.Generic;
using HihiFramework.Core;
using UnityEngine;

public static partial class AudioConfig {
    /// <summary>
    /// Default BGM On Off flag after app installed
    /// </summary>
    public static bool GetIsDefaultBgmOn () {
        return true;
    }

    /// <summary>
    /// Default SFX On Off flag after app installed
    /// </summary>
    public static bool GetIsDefaultSfxOn () {
        return true;
    }

    /// <summary>
    /// Default BGM attenuation in decibel
    /// </summary>
    public static float GetDefaultBgmAttenuation () {
        return 0;
    }

    /// <summary>
    /// Default SFX attenuation in decibel
    /// </summary>
    public static float GetDefaultSfxAttenuation () {
        return 0;
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
            // Currently no BGM
        }

        Log.PrintError ("AudioEnum.BgmType : " + bgmType + " has not been assigned resources name. Return null.", LogTypes.Audio);
        return null;
    }

    /// <summary>
    /// Get the dynamic SFX resources name (without folders) of corresponding DynamicSfxType
    /// </summary>
    public static string GetSfxResourcesName (AudioEnum.DynamicSfxType dynamicSfxType) {
        switch (dynamicSfxType) {
            case AudioEnum.DynamicSfxType.ConfirmBtn: return "ConfirmBtn";
            case AudioEnum.DynamicSfxType.CancelBtn: return "CancelBtn";
        }

        Log.PrintError ("AudioEnum.DynamicSfxType : " + dynamicSfxType + " has not been assigned resources name. Return null.", LogTypes.Audio);
        return null;
    }

    
    /// <summary>
    /// Get the BgmType when going into the game
    /// </summary>
    public static AudioEnum.BgmType GetLandingBgm () {
        return AudioEnum.BgmType.None;
    }
}