using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using HIHIFramework.Core;

public static class UserManager {

    public static Dictionary<int, MissionProgress> MissionProgressDict { get; private set; }
    public static List<CharEnum.Command> EnabledCommandList { get; private set; }

    #region PlayerPrefs

    public static int SelectedMissionId {
        get { return PlayerPrefs.GetInt (GameVariable.SelectedMissionIdKey, -1); }
        set { PlayerPrefs.SetInt (GameVariable.SelectedMissionIdKey, value); }
    }

    public static int SelectedMapEntryId {
        get { return PlayerPrefs.GetInt (GameVariable.SelectedMapEntryIdKey, -1); }
        set { PlayerPrefs.SetInt (GameVariable.SelectedMapEntryIdKey, value); }
    }

    #endregion

    #region Temp saved

    public static int? EntryJustUnlockedMissionId { get; private set; } = null;

    #endregion

    #region Init

    static UserManager () {
        LoadUserProgress ();
    }

    private static void LoadUserProgress () {
        Log.PrintDebug ("Load User Progress", LogType.GameFlow);
        LoadMissionProgressList ();
        LoadEnabledCommandList ();
    }

    #endregion

    #region getter

    /// <summary>
    /// Get the MissionProgress of the corresponding missionId.<br />
    /// Notes :<br />
    /// 1. The returned value is referenced in UserManager class. Do not amend it outside the UserManager class. Clone and use it if needed.<br />
    /// 2. The returned value is ensured not be null
    /// </summary>
    public static MissionProgress GetMissionProgress (int missionId) {
        // Remarks : The static constructor ensure MissionProgressDict != null
        if (MissionProgressDict.ContainsKey (missionId)) {
            return MissionProgressDict[missionId];
        } else {
            var missionProgress = new MissionProgress ();
            MissionProgressDict.Add (missionId, missionProgress);
            return missionProgress;
        }
    }

    public static CharEnum.BodyPart GetObtainedBodyParts () {
        var obtainedBodyPart = CharEnum.BodyPart.None;
        if (EnabledCommandList.Contains(CharEnum.Command.Hit)) {
            obtainedBodyPart = obtainedBodyPart | CharEnum.BodyPart.Arms;
        }

        if (EnabledCommandList.Contains (CharEnum.Command.Jump)) {
            obtainedBodyPart = obtainedBodyPart | CharEnum.BodyPart.Legs;
        }

        if (EnabledCommandList.Contains (CharEnum.Command.Dash)) {
            obtainedBodyPart = obtainedBodyPart | CharEnum.BodyPart.Thrusters;
        }

        if (EnabledCommandList.Contains (CharEnum.Command.Arrow)) {
            obtainedBodyPart = obtainedBodyPart | CharEnum.BodyPart.Arrow;
        }

        return obtainedBodyPart;
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
        MissionProgressDict = new Dictionary<int, MissionProgress> ();

        var json = PlayerPrefs.GetString (GameVariable.AllMissionProgressKey, null);

        if (!string.IsNullOrEmpty (json)) {
            var allMissionProgress = new AllMissionProgress ();
            allMissionProgress = JsonUtility.FromJson<AllMissionProgress> (json);
            MissionProgressDict = allMissionProgress.ConvertToDict ();
        }

        SetFirstMissionUnlocked ();
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
        EnabledCommandList = new List<CharEnum.Command> ();

        var str = PlayerPrefs.GetString (GameVariable.EnabledCommandListKey, null);

        if (!string.IsNullOrEmpty (str)) {
            var array = str.Split (new string[] { FrameworkVariable.DefaultDelimiter }, System.StringSplitOptions.None);
            foreach (var commandIntStr in array) {
                EnabledCommandList.Add ((CharEnum.Command)int.Parse (commandIntStr));
            }
        }
    }

    #endregion

    #region Logic

    private static void SetFirstMissionUnlocked () {
        var firstMissionId = MissionManager.FirstMissionId;
        var progress = GetMissionProgress (firstMissionId);

        if (!progress.isUnlocked) {
            var mission = MissionManager.GetMission (firstMissionId);

            if (mission == null) {
                Log.PrintError ("Cannot get mission with mission id : " + firstMissionId, LogType.GameFlow);
                return;
            }

            if (mission.mapEntries == null || mission.mapEntries.Count <= 0) {
                Log.PrintError ("No map entries for mission with mission id : " + firstMissionId, LogType.GameFlow);
                return;
            }

            progress.AddUnlockedMapEntry (mission.mapEntries[0].id);
            SaveMissionProgressList ();
        }
    }

    public static void ClearMission (int missionId, int? toEntryId = null) {
        var clearMissionProgress = GetMissionProgress (missionId);
        clearMissionProgress.isCleared = true;

        if (toEntryId != null) {
            var toEntryIdInt = (int)toEntryId;
            var nextMission = MissionManager.GetMissionByMapEntry (toEntryIdInt);

            if (nextMission == null) {
                Log.PrintError ("Cannot get mission that contain map entry with id : " + toEntryId, LogType.GameFlow);
            } else {
                var nextMissionProgress = GetMissionProgress (nextMission.id);
                if (nextMissionProgress.AddUnlockedMapEntry (toEntryIdInt)) {
                    EntryJustUnlockedMissionId = nextMission.id;
                } else {
                    EntryJustUnlockedMissionId = null;
                }
            }
        }

        SaveMissionProgressList ();
    }

    public static void ClearEntryJustUnlockedMissionId () {
        EntryJustUnlockedMissionId = null;
    }

    public static void CollectedCollectable (int missionId, Collectable.Type collectable) {
        var missionProgress = GetMissionProgress (missionId);
        missionProgress.AddCollectedCollectable (collectable);

        SaveMissionProgressList ();
    }

    public static void EnableCommand (CharEnum.Command command) {
        if (EnabledCommandList == null) {
            EnabledCommandList = new List<CharEnum.Command> ();
        }

        if (!EnabledCommandList.Contains (command)) {
            EnabledCommandList.Add (command);
        }

        SaveEnabledCommandList ();
    }

    /// <summary>
    /// For development use only
    /// </summary>
    private static void ClearAllCollectedCollectables (int missionId) {
        var missionProgress = GetMissionProgress (missionId);
        missionProgress.ClearAllCollectedCollectables ();

        SaveMissionProgressList ();
    }

    /// <summary>
    /// For development use only. You would need to call ClearAllCollectedCollectables in order to get the corresponding collectable again.
    /// </summary>
    private static void ClearAllEnabledCommand () {
        EnabledCommandList.Clear ();

        SaveEnabledCommandList ();
    }
    #endregion
}
