[![en](https://img.shields.io/badge/lang-en-red.svg)](./SourceCodeGuide.md)
[![jp](https://img.shields.io/badge/lang-jp-green.svg)](./SourceCodeGuide.jp.md)

# More About Source Code

## Branches

* `master` : The main branch.
* `develop` : The branch for development. `feature` branches would be merged into `develop` branch.
* `feature` : `feature` branches are created to develop a new feature or do changes or bugs fix.
* `release` : The branches for release. Within the `release` branches, tags would be added with the name of release version number.

## Folder Structure

```
./
├ DesignAssets/				// The raw image and model assets I have made
├ Metroidvania/				// The Unity project
├ Tools/				// The tools I have made for convenience
    └ checksum/				// The tool to calculate the md5 checksum of a file
└ ...
```

## Unity Scenes

The Unity scenes are inside `Assets/Scenes` folder.

* `HihiFramework/GameConfig` : The entry point of the game. After all the necessary config is set up (e.g., framerate, audio and localization modules initialization), it will move to `Landing` scene.
* `HihiFramework/PrefabsUIEnvironment` : The UI Prefab environment setting when you enter the Prefab Mode of Unity Editor.
* `Landing` : The first scene of the game that a player would see. If you tap the screen, it will move to `MainMenu` scene. (For a new game, it will move to `Game` scene)
* `MainMenu` : The area selection scene. It will move to `Game` scene after an area is chosen.
* `Game` : The main game scene. It will load the JSON file that contain the map data of the area, and generate the map dynamically.
* `MapEditor` : The scene to create or modify the maps. After editing, you would need to press the GUI button `Export MapData` in order to export the corresponding JSON file for `Game` scene usage.
* `Sandbox` : The scene to test the character control and actions. You can do the testing by setting the `Dev Only` flag of `Assets/Scripts/Life/Char/Params_Char`.

## About HihiFramework

* The scripts inside `Assets/Scripts` folder are sorted by different modules. Within all those modules, there is a folder called `HihiFramework`.
* `HihiFramework` is a framework I have made in order to handle generic features of a game, such as assets handling, audio, logging, localization, etc.
* Within `HihiFramework`, the scripts are separated into `Framework` and `Project` folders.
	* `Framework` : The basic and generic codes which are independent to the features of developing game project. These codes should not be changed and can be used in different game projects directly.
	* `Project` : Based on the codes of `Framework` folder, the behaviour can be customized by the scripts in `Project` folder to fit the developing game project.

## Other References

* [Code Standard](./Metroidvania/Assets/Documents/HihiFramework/CodeStandard.md)
* [Unity Project File Standard](./Metroidvania/Assets/Documents/HihiFramework/UnityProjectFileStandard.md)