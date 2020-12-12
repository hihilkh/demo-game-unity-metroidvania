using HIHIFramework.GameConfiguration;

public partial class GameConfigSet : GameConfigSetBase {
    public string baseURL { get; private set; }
    public GameConfigEnum.AnalyticsType analyticsType { get; private set; }

    public GameConfigSet () {
        this.gameConfigSetName = null;
        this.baseURL = null;
        this.analyticsType = GameConfigEnum.AnalyticsType.None;
    }

    public GameConfigSet (string gameConfigSetName, string baseURL, GameConfigEnum.AnalyticsType analyticsType) {
        this.gameConfigSetName = gameConfigSetName;
        this.baseURL = baseURL;
        this.analyticsType = analyticsType;
    }
}