using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using HIHIFramework.Core;

public static class GameProgress {

    public static Dictionary<int, MissionProgress> MissionProgressDict { get; private set; }
    public static List<CharacterEnum.Command> EnabledCommandList { get; private set; }

    #region Load Progress

    public static void LoadProgress () {
        LoadMissionProgressList ();
        LoadEnabledCommandList ();
    }

    #endregion

    #region getter

    public static MissionProgress GetMissionProgress (int missionId) {
        if (MissionProgressDict == null) {
            return null;
        }

        if (MissionProgressDict.ContainsKey (missionId)) {
            return MissionProgressDict[missionId];
        } else {
            return null;
        }
    }

    #endregion

    #region update

    public static void UpdateMissionProgress (int missionId, MissionProgress missionProgress) {
        if (missionProgress == null) {
            Log.PrintWarning ("The input MissionProgress is null. Do not update anything.");
            return;
        }

        if (MissionProgressDict == null) {
            MissionProgressDict = new Dictionary<int, MissionProgress> ();
        }

        if (MissionProgressDict.ContainsKey (missionId)) {
            MissionProgressDict[missionId] = missionProgress;
        } else {
            MissionProgressDict.Add (missionId, missionProgress);
        }

        SaveMissionProgressList ();
    }

    public static void EnableCommand (CharacterEnum.Command command) {
        if (EnabledCommandList == null) {
            EnabledCommandList = new List<CharacterEnum.Command> ();
        }

        if (!EnabledCommandList.Contains (command)) {
            EnabledCommandList.Add (command);
        }

        SaveEnabledCommandList ();
    }

    #endregion

    #region PlayerPrefs (Save / Load)

    private static void SaveMissionProgressList () {
        var json = "";
        if (MissionProgressDict != null && MissionProgressDict.Count > 0) {
            var allMissionProgress = new AllMissionProgress (MissionProgressDict);
            json = JsonUtility.ToJson (allMissionProgress);
        }

        PlayerPrefs.SetString (GameVariable.AllMissionProgressKey, json);
    }

    private static void LoadMissionProgressList () {
        var json = PlayerPrefs.GetString (GameVariable.AllMissionProgressKey, null);

        if (string.IsNullOrEmpty (json)) {
            MissionProgressDict = new Dictionary<int, MissionProgress> ();
        } else {
            var allMissionProgress = new AllMissionProgress ();
            allMissionProgress = JsonUtility.FromJson<AllMissionProgress> (json);
            MissionProgressDict = allMissionProgress.ConvertToDict ();
        }
    }

    private static void SaveEnabledCommandList () {
        var str = "";
        if (EnabledCommandList != null && EnabledCommandList.Count > 0) {
            var tempList = new List<int> ();
            foreach (var command in EnabledCommandList) {
                tempList.Add ((int)command);
            }

            str = string.Join (FrameworkVariable.DefaultDelimiter, tempList);
        }

        PlayerPrefs.SetString (GameVariable.EnabledCommandListKey, str);
    }

    private static void LoadEnabledCommandList () {
        var str = PlayerPrefs.GetString (GameVariable.EnabledCommandListKey, null);

        if (string.IsNullOrEmpty (str)) {
            EnabledCommandList = new List<CharacterEnum.Command> ();
        } else {
            var array = str.Split (new string[] { FrameworkVariable.DefaultDelimiter }, System.StringSplitOptions.None);
            foreach (var commandIntStr in array) {
                EnabledCommandList.Add ((CharacterEnum.Command)int.Parse (commandIntStr));
            }
        }
    }

    #endregion

}
