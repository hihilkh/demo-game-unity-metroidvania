public class MapSwitchSubEvent : SubEventBase {
    public override MissionEventEnum.SubEventType SubEventType => MissionEventEnum.SubEventType.MapSwitch;

    public int MapSwitchId { get; }

    public MapSwitchSubEvent (int mapSwitchId) : base () {
        MapSwitchId = mapSwitchId;
    }
}