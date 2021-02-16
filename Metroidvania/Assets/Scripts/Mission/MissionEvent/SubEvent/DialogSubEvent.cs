using System.Collections.Generic;

public class DialogSubEvent : SubEventBase {
    public override MissionEventEnum.SubEventType SubEventType => MissionEventEnum.SubEventType.Dialog;

    public MissionEventEnum.Character Speaker { get; }
    public MissionEventEnum.Expression SpeakerExpression { get; }
    public MissionEventEnum.Character Listener { get; }
    public MissionEventEnum.Expression ListenerExpression { get; }

    public string DialogLocalizationKeyBase { get; }

    public DialogSubEvent (MissionEventEnum.Character speaker, MissionEventEnum.Expression speakerExpression, string dialogLocalizationKeyBase) : this (speaker, speakerExpression, MissionEventEnum.Character.None, MissionEventEnum.Expression.Normal, dialogLocalizationKeyBase) {
    }

    public DialogSubEvent (MissionEventEnum.Character speaker, MissionEventEnum.Expression speakerExpression, MissionEventEnum.Character listener, MissionEventEnum.Expression listenerExpression, string dialogLocalizationKeyBase) : base () {
        Speaker = speaker;
        SpeakerExpression = speakerExpression;
        Listener = listener;
        ListenerExpression = listenerExpression;

        DialogLocalizationKeyBase = dialogLocalizationKeyBase;
    }
}