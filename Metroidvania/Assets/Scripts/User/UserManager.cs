using System.Collections.Generic;
using HihiFramework.Core;
using UnityEngine;

public static class UserManager {

    public static Dictionary<int, MissionProgress> MissionProgressDict { get; private set; }
    public static List<CharEnum.Command> EnabledCommandList { get; private set; }
    public static Dictionary<CharEnum.InputSituation, CharEnum.Command> CommandSettingsCache { get; private set; }

    #region PlayerPrefs

    public static int SelectedMissionId {
        get { return PlayerPrefs.GetInt (GameVariable.SelectedMissionIdKey, -1); }
        set { PlayerPrefs.SetInt (GameVariable.SelectedMissionIdKey, value); }
    }

    public static int SelectedEntryId {
        get { return PlayerPrefs.GetInt (GameVariable.SelectedEntryIdKey, -1); }
        set { PlayerPrefs.SetInt (GameVariable.SelectedEntryIdKey, value); }
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
        Log.PrintDebug ("Load User Progress", LogTypes.GameFlow | LogTypes.UserData);
        LoadMissionProgressList ();
        LoadEnabledCommandList ();
        LoadCommandSettingsCache ();
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

    public static CharEnum.BodyParts GetObtainedBodyParts () {
        var obtainedBodyParts = CharEnum.BodyParts.None;
        if (EnabledCommandList.Contains(CharEnum.Command.Hit)) {
            obtainedBodyParts = obtainedBodyParts | CharEnum.BodyParts.Arms;
        }

        if (EnabledCommandList.Contains (CharEnum.Command.Jump)) {
            obtainedBodyParts = obtainedBodyParts | CharEnum.BodyParts.Legs;
        }

        if (EnabledCommandList.Contains (CharEnum.Command.Dash)) {
            obtainedBodyParts = obtainedBodyParts | CharEnum.BodyParts.Thrusters;
        }

        if (EnabledCommandList.Contains (CharEnum.Command.Arrow)) {
            obtainedBodyParts = obtainedBodyParts | CharEnum.BodyParts.Arrow;
        }

        return obtainedBodyParts;
    }

    public static List<Collectable.Type> GetAllCollectedCollectable () {
        var list = new List<Collectable.Type> ();

        foreach (var pair in MissionProgressDict) {
            var collectedList = pair.Value.collectedCollectables;
            if (collectedList != null && collectedList.Count > 0) {
                list.AddRange (collectedList);
            }
            
        }

        return list;
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

    private static void SaveCommandSettingsCache () {
        var str = "";
        if (CommandSettingsCache != null && CommandSettingsCache.Count > 0) {
            var tempList = new List<int> ();
            foreach (var pair in CommandSettingsCache) {
                tempList.Add ((int)pair.Key);
                tempList.Add ((int)pair.Value);
            }

            str = string.Join (FrameworkVariable.DefaultDelimiter, tempList);
        }

        PlayerPrefs.SetString (GameVariable.CommandSettingsCacheKey, str);
    }

    private static void LoadCommandSettingsCache () {
        CommandSettingsCache = new Dictionary<CharEnum.InputSituation, CharEnum.Command> ();

        var str = PlayerPrefs.GetString (GameVariable.CommandSettingsCacheKey, null);

        if (!string.IsNullOrEmpty (str)) {
            var array = str.Split (new string[] { FrameworkVariable.DefaultDelimiter }, System.StringSplitOptions.None);
            if (array.Length % 2 != 0) {
                Log.PrintWarning ("CommandSettingsCache is somehow with a wrong format. Save an empty cache to overwrite.", LogTypes.GameFlow | LogTypes.UserData);
                SaveCommandSettingsCache ();
                return;
            }

            var count = array.Length / 2;
            for (var i = 0; i < count; i++) {
                CommandSettingsCache.Add ((CharEnum.InputSituation)int.Parse (array[i]), (CharEnum.Command)int.Parse (array[i + 1]));
            }
        }
    }
    
    #endregion

    #region Logic

    private static void SetFirstMissionUnlocked () {
        var firstMissionId = MissionManager.FirstMissionId;
        var progress = GetMissionProgress (firstMissionId);

        if (!progress.IsUnlocked) {
            var mission = MissionManager.GetMission (firstMissionId);

            if (mission == null) {
                Log.PrintError ("Cannot get mission with mission id : " + firstMissionId, LogTypes.GameFlow | LogTypes.UserData);
                return;
            }

            if (mission.Entries == null || mission.Entries.Count <= 0) {
                Log.PrintError ("No map entries for mission with mission id : " + firstMissionId, LogTypes.GameFlow | LogTypes.UserData);
                return;
            }

            progress.AddUnlockedEntry (mission.Entries[0].Id);
            SaveMissionProgressList ();
        }
    }

    public static void ClearMission (int missionId, int? toEntryId = null) {
        var clearMissionProgress = GetMissionProgress (missionId);
        clearMissionProgress.isCleared = true;

        if (toEntryId != null) {
            var toEntryIdInt = (int)toEntryId;
            var nextMission = MissionManager.GetMissionByEntry (toEntryIdInt);

            if (nextMission == null) {
                Log.PrintError ("Cannot get mission that contain map entry with id : " + toEntryId, LogTypes.GameFlow | LogTypes.UserData);
            } else {
                var nextMissionProgress = GetMissionProgress (nextMission.Id);
                if (nextMissionProgress.AddUnlockedEntry (toEntryIdInt)) {
                    EntryJustUnlockedMissionId = nextMission.Id;
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

    public static void CollectCollectable (int missionId, Collectable.Type collectable) {
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

    public static void SetCommandSettingsCache (Dictionary<CharEnum.InputSituation, CharEnum.Command> settings) {
        if (settings == null) {
            CommandSettingsCache = new Dictionary<CharEnum.InputSituation, CharEnum.Command> ();
        } else {
            CommandSettingsCache = settings;
        }

        SaveCommandSettingsCache ();
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
