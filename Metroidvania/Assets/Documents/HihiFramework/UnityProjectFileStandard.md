# Unity Project File Standard

- All folder and file names should be in <b>Pascal case</b>.
- All GameObject names inside a scene should be in <b>Pascal case</b>.
- Underscores(`_`) are <b>only</b> used in the case that connect to the suffix that distinguish similar objects with different properties (e.g. `RectBtn_Blue`, `RectBtn_Red`). In this case, the suffix after underscore should be in <b>Pascal case</b>.
- The first-layer folders inside `Assets` folder should be the base folder of a particular type of assets and should be in plural form (e.g. `Animations`, `Scenes`).
- All imported assets (except HihiFramework) should be placed under `Assets/ThirdParties` folder, except those must be directly under `Assets` folder.
- Folders that named `HihiFramework` is preserved for HihiFramework, which should be shared with different Unity project. You should avoid amending anything inside `HihiFramework`, except the scripts inside `Assets/Scripts/HihiFramework/Project` folder, which is designed to be amended by projects.