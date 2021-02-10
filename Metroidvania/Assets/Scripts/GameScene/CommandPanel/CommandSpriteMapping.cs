using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

public static class CommandSpriteMapping {
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

    public static string GetCommandSpriteResourcesName (CharEnum.Command command, CharEnum.InputSituation situation) {
        switch (command) {
            case CharEnum.Command.Hit:
                switch (situation) {
                    case CharEnum.InputSituation.GroundTap:
                    case CharEnum.InputSituation.AirTap:
                        return ResourcesFolderName + SpriteName_Hit_Normal;
                    case CharEnum.InputSituation.GroundHold:
                        return ResourcesFolderName + SpriteName_Hit_Charged;
                    case CharEnum.InputSituation.AirHold:
                        return ResourcesFolderName + SpriteName_Hit_AirCharging;
                    case CharEnum.InputSituation.GroundRelease:
                        return ResourcesFolderName + SpriteName_Hit_Finishing;
                    case CharEnum.InputSituation.AirRelease:
                        return ResourcesFolderName + SpriteName_Hit_DropHit;
                }
                break;

            case CharEnum.Command.Jump:
                switch (situation) {
                    case CharEnum.InputSituation.GroundTap:
                    case CharEnum.InputSituation.AirTap:
                        return ResourcesFolderName + SpriteName_Jump_Normal;
                    case CharEnum.InputSituation.GroundHold:
                    case CharEnum.InputSituation.AirHold:
                        return ResourcesFolderName + SpriteName_Jump_Charging;
                    case CharEnum.InputSituation.GroundRelease:
                    case CharEnum.InputSituation.AirRelease:
                        return ResourcesFolderName + SpriteName_Jump_Charged;
                }
                break;
                
            case CharEnum.Command.Dash:
                return ResourcesFolderName + SpriteName_Dash;

            case CharEnum.Command.Arrow:
                switch (situation) {
                    case CharEnum.InputSituation.GroundTap:
                    case CharEnum.InputSituation.AirTap:
                        return ResourcesFolderName + SpriteName_Arrow_Target;
                    case CharEnum.InputSituation.GroundHold:
                    case CharEnum.InputSituation.AirHold:
                        return ResourcesFolderName + SpriteName_Arrow_Straight;
                    case CharEnum.InputSituation.GroundRelease:
                    case CharEnum.InputSituation.AirRelease:
                        return ResourcesFolderName + SpriteName_Arrow_Triple;
                }
                break;

            case CharEnum.Command.Turn:
                return ResourcesFolderName + SpriteName_Turn;

        }

        Log.PrintError ("Have not implemented command sprite for command : " + command + " and input situation : " + situation, LogType.Asset | LogType.Char);
        return null;
    }
}
