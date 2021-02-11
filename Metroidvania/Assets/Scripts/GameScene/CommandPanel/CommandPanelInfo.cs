using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

public static class CommandPanelInfo {
    private const string ResourcesFolderName = "Commands/";

    private const string SpriteName_Hit_Normal = "Command_Hit_Normal";
    private const string SpriteName_Hit_Charged = "Command_Hit_Charged";
    private const string SpriteName_Hit_Finishing = "Command_Hit_Finishing";
    private const string SpriteName_Hit_AirCharging = "Command_Hit_AirCharging";
    private const string SpriteName_Hit_DropHit = "Command_Hit_DropHit";

    private const string SpriteName_Jump_Normal = "Command_Jump_Normal";
    private const string SpriteName_Jump_Charging = "Command_Jump_Charging";
    private const string SpriteName_Jump_Charged = "Command_Jump_Charged";

    private const string SpriteName_Dash = "Command_Dash";

    private const string SpriteName_Arrow_Target = "Command_Arrow_Target";
    private const string SpriteName_Arrow_Straight = "Command_Arrow_Straight";
    private const string SpriteName_Arrow_Triple = "Command_Arrow_Triple";

    private const string SpriteName_Turn = "Command_Turn";

    #region Binded hold command

    public static bool CheckIsHoldBindedWithRelease (CharEnum.Command command, bool isInAir) {
        switch (command) {
            case CharEnum.Command.Hit:
                if (isInAir) {
                    return true;
                }
                break;
            case CharEnum.Command.Jump:
                return true;
        }

        return false;
    }

    #endregion

    #region Disallow input situation

    public static List<CharEnum.InputSituation> GetDisallowInputSituationList (CharEnum.Command command) {
        var result = new List<CharEnum.InputSituation> ();

        switch (command) {
            case CharEnum.Command.Turn:
                result.Add (CharEnum.InputSituation.GroundHold);
                result.Add (CharEnum.InputSituation.AirHold);
                break;
        }

        return result;
    }

    #endregion

    #region Sprite resources name

    public static string GetTapCommandSpriteResourcesName (CharEnum.Command command, bool isInAir) {
        switch (command) {
            case CharEnum.Command.Hit:
                return ResourcesFolderName + SpriteName_Hit_Normal;
            case CharEnum.Command.Jump:
                return ResourcesFolderName + SpriteName_Jump_Normal;
            case CharEnum.Command.Dash:
                return ResourcesFolderName + SpriteName_Dash;
            case CharEnum.Command.Arrow:
                return ResourcesFolderName + SpriteName_Arrow_Target;
            case CharEnum.Command.Turn:
                return ResourcesFolderName + SpriteName_Turn;
        }

        Log.PrintError ("Have not implemented tap command sprite for command : " + command + " , isInAir : " + isInAir, LogType.Asset | LogType.Char);
        return null;
    }

    public static string GetHoldCommandSpriteResourcesName (CharEnum.Command command, bool isInAir) {
        switch (command) {
            case CharEnum.Command.Hit:
                if (isInAir) {
                    return ResourcesFolderName + SpriteName_Hit_AirCharging;
                } else {
                    return ResourcesFolderName + SpriteName_Hit_Charged;
                }
            case CharEnum.Command.Jump:
                return ResourcesFolderName + SpriteName_Jump_Charging;
            case CharEnum.Command.Dash:
                return ResourcesFolderName + SpriteName_Dash;
            case CharEnum.Command.Arrow:
                return ResourcesFolderName + SpriteName_Arrow_Straight;
            case CharEnum.Command.Turn:
                Log.PrintWarning ("Actually should not have Turn command for hold. Please check.", LogType.Char);
                return ResourcesFolderName + SpriteName_Turn;
        }

        Log.PrintError ("Have not implemented hold command sprite for command : " + command + " , isInAir : " + isInAir, LogType.Asset | LogType.Char);
        return null;
    }

    public static string GetReleaseCommandSpriteResourcesName (CharEnum.Command command, bool isInAir, bool isSameWithHoldCommand) {
        switch (command) {
            case CharEnum.Command.Hit:
                if (isInAir) {
                    if (isSameWithHoldCommand) {
                        return ResourcesFolderName + SpriteName_Hit_DropHit;
                    } else {
                        return ResourcesFolderName + SpriteName_Hit_Finishing;
                    }
                } else {
                    return ResourcesFolderName + SpriteName_Hit_Finishing;
                }
            case CharEnum.Command.Jump:
                if (isSameWithHoldCommand) {
                    return ResourcesFolderName + SpriteName_Jump_Charged;
                } else {
                    return ResourcesFolderName + SpriteName_Jump_Normal;
                }
            case CharEnum.Command.Dash:
                return ResourcesFolderName + SpriteName_Dash;
            case CharEnum.Command.Arrow:
                return ResourcesFolderName + SpriteName_Arrow_Triple;
            case CharEnum.Command.Turn:
                return ResourcesFolderName + SpriteName_Turn;
        }

        Log.PrintError ("Have not implemented release command sprite for command : " + command + " , isInAir : " + isInAir + " , isSameWithHoldCommand : " + isSameWithHoldCommand, LogType.Asset | LogType.Char);
        return null;
    }

    #endregion
}
