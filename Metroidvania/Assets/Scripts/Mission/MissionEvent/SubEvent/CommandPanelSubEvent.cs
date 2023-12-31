﻿using System.Collections.Generic;

public class CommandPanelSubEvent : SubEventBase {
    public override MissionEventEnum.SubEventType SubEventType => MissionEventEnum.SubEventType.CommandPanel;

    public CharEnum.Command Command { get; }
    public CharEnum.InputSituation InputSituation { get; }

    public string BeforeSetCommandLocalizationKeyBase { get; }
    public string AfterSetCommandLocalizationKeyBase { get; }

    public CommandPanelSubEvent (CharEnum.Command command, CharEnum.InputSituation inputSituation, string beforeSetCommandLocalizationKeyBase, string afterSetCommandLocalizationKeyBase) : base () {
        Command = command;
        InputSituation = inputSituation;
        BeforeSetCommandLocalizationKeyBase = beforeSetCommandLocalizationKeyBase;
        AfterSetCommandLocalizationKeyBase = afterSetCommandLocalizationKeyBase;
    }
}