using HihiFramework.GameConfiguration;

public partial class GameConfigSet : GameConfigSetBase {
    public string BaseURL { get; private set; }
    public GameConfigEnum.AnalyticsType AnalyticsType { get; private set; }

    public GameConfigSet () {
        GameConfigSetName = null;
        BaseURL = null;
        AnalyticsType = GameConfigEnum.AnalyticsType.None;
    }

    public GameConfigSet (string gameConfigSetName, string baseURL, GameConfigEnum.AnalyticsType analyticsType) {
        GameConfigSetName = gameConfigSetName;
        BaseURL = baseURL;
        AnalyticsType = analyticsType;
    }
}