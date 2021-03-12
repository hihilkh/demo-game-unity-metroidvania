public class SpecialSceneEvent : MissionEventBase {
    public MissionEventEnum.SpecialSceneType SpecialSceneType { get; }

    public SpecialSceneEvent (MissionEventEnum.SpecialSceneType specialSceneType, params SubEventBase[] subEvents) : base (subEvents) {
        SpecialSceneType = specialSceneType;
    }
}