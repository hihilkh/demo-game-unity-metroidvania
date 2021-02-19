using System.Collections.Generic;

public class DialogSubEvent : SubEventBase {
    public override MissionEventEnum.SubEventType SubEventType => MissionEventEnum.SubEventType.Dialog;

    public class DialogDetails {
        public MissionEventEnum.Character LeftSide { get; }
        public MissionEventEnum.Expression LeftSideExpression { get; }
        public MissionEventEnum.Character RightSide { get; }
        public MissionEventEnum.Expression RightSideExpression { get; }

        public bool IsLeftSideTalking { get; }
        public string DialogLocalizationKeyBase { get; }

        public DialogDetails (MissionEventEnum.Character leftSide, MissionEventEnum.Expression leftSideExpression,string dialogLocalizationKeyBase) : this (leftSide, leftSideExpression, MissionEventEnum.Character.None, MissionEventEnum.Expression.Normal, true, dialogLocalizationKeyBase) {

        }

        public DialogDetails (MissionEventEnum.Character leftSide, MissionEventEnum.Expression leftSideExpression, MissionEventEnum.Character rightSide, MissionEventEnum.Expression rightSideExpression, bool isLeftSideTalking, string dialogLocalizationKeyBase) {
            LeftSide = leftSide;
            LeftSideExpression = leftSideExpression;
            RightSide = rightSide;
            RightSideExpression = rightSideExpression;
            IsLeftSideTalking = isLeftSideTalking;
            DialogLocalizationKeyBase = dialogLocalizationKeyBase;
        }
    }

    private List<DialogDetails> dialogDetailsList;

    public DialogSubEvent (params DialogDetails[] dialogDetailsArray) : base () {
        dialogDetailsList = new List<DialogDetails> (dialogDetailsArray);
    }

    public List<DialogDetails> GetDialogDetailsListClone () {
        return new List<DialogDetails> (dialogDetailsList);
    }
}