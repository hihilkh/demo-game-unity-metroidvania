using HihiFramework.GameConfiguration;

public partial class GameConfigSet : GameConfigSetBase {
    public GameConfigSet () {
        GameConfigSetName = null;
        BaseURL = null;
    }

    public GameConfigSet (string gameConfigSetName, string baseURL) {
        GameConfigSetName = gameConfigSetName;
        BaseURL = baseURL;
    }
}