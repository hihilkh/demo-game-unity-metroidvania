public class WaitSubEvent : SubEventBase {
    public override MissionEventEnum.SubEventType SubEventType => MissionEventEnum.SubEventType.Wait;

    public float WaitTime { get; }

    public WaitSubEvent (float waitTime) : base () {
        WaitTime = waitTime;
    }
}
