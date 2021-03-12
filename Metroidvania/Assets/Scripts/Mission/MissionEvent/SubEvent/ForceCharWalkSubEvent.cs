public class ForceCharWalkSubEvent : SubEventBase {
    public override MissionEventEnum.SubEventType SubEventType => MissionEventEnum.SubEventType.ForceCharWalk;

    public LifeEnum.HorizontalDirection Direction { get; }

    public ForceCharWalkSubEvent (LifeEnum.HorizontalDirection direction) : base () {
        Direction = direction;
    }
}