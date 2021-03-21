using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Linq;
using HihiFramework.Core;
using System.Text.RegularExpressions;
using HihiFramework.UI;

namespace HihiFramework.GameConfiguration {
    public class GameConfigSceneManager : MonoBehaviour {
        private List<GameConfigSet> allConfigSets;

        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject dropdownTemplate;
        [SerializeField] private GameObject inputFieldTemplate;
        [SerializeField] private GameObject verticalLayout;
        [SerializeField] private Text projectVersionText;
        [SerializeField] private Text frameworkVersionText;

        private readonly List<Dropdown> allDropdowns = new List<Dropdown> ();
        private readonly Dictionary<string, Dropdown> dropdownDict = new Dictionary<string, Dropdown> ();
        private readonly List<InputField> allInputField = new List<InputField> ();
        private readonly Dictionary<string, InputField> inputFieldDict = new Dictionary<string, InputField> ();

        private Dictionary<string, GameConfigFrameworkEnum.GameConfigType> propertyNameDict;

        private string configSetNameSearchKey;

        private const string GameVersionStringFormat = "Game Version : {0}";
        private const string FrameworkVersionStringFormat = "Framework Version : {0}";
        private const string StringConfigDisplayValue = "({0}) {1}";    // {0} : config set name, {1} : value

        private void Awake () {
            var isUseProductionConfig = FrameworkUtils.GetIsReleaseBuild () && !GameVariable.IsShowGameConfigSceneInReleaseBuild;
            Log.PrintDebug ("isUseProductionConfig :: " + isUseProductionConfig, LogTypes.General);

            if (isUseProductionConfig) {
                settingsPanel.SetActive (false);
            } else {
                settingsPanel.SetActive (true);
            }

            Action<bool> onInitGameSettingsFinished = (isSuccess) => {
                if (isUseProductionConfig) {
                    GoIntoGame (true);
                } else {
                    projectVersionText.text = FrameworkUtils.StringReplace (GameVersionStringFormat, Application.version);
                    frameworkVersionText.text = FrameworkUtils.StringReplace (FrameworkVersionStringFormat, FrameworkVariable.FrameworkVersion);

                    allConfigSets = GameConfig.AllGameConfigSetList;
                    GenerateDropdown ();
                    ShowInitialDropdownSelection ();

                    UIEventManager.AddEventHandler (BtnOnClickType.GameConfig_ClearPlayerPrefs, ClearPlayerPrefsBtnClickedHandler);
                    UIEventManager.AddEventHandler (BtnOnClickType.GameConfig_Confirm, ConfirmBtnClickedHandler);
                }
            };

            FrameworkUtils.InitGameSettings (onInitGameSettingsFinished);
        }

        private void OnDestroy () {
            UIEventManager.RemoveEventHandler (BtnOnClickType.GameConfig_ClearPlayerPrefs, ClearPlayerPrefsBtnClickedHandler);
            UIEventManager.RemoveEventHandler (BtnOnClickType.GameConfig_Confirm, ConfirmBtnClickedHandler);
        }

        private void GenerateDropdown () {
            if (allConfigSets == null || allConfigSets.Count == 0) {
                Log.PrintError ("No config sets are set. Cannot generate the config dropdown. Please check.", LogTypes.General);
                return;
            }

            var tempGameConfigSet = allConfigSets[0];
            propertyNameDict = tempGameConfigSet.GetGameConfigPropertyNameDict ();

            // order:
            // 1. GameConfigSetName
            // 2. String
            // 3. Enum
            // 4. Custom String
            foreach (var pair in propertyNameDict) {
                if (pair.Value == GameConfigFrameworkEnum.GameConfigType.GameConfigSetName) {
                    configSetNameSearchKey = pair.Key;
                    GenerateDropdown (pair.Key, pair.Value);
                    break;
                }
            }

            foreach (var pair in propertyNameDict) {
                if (pair.Value == GameConfigFrameworkEnum.GameConfigType.String) {
                    GenerateDropdown (pair.Key, pair.Value);
                }
            }

            foreach (var pair in propertyNameDict) {
                if (pair.Value == GameConfigFrameworkEnum.GameConfigType.Enum) {
                    GenerateDropdown (pair.Key, pair.Value);
                }
            }

            foreach (var pair in propertyNameDict) {
                if (pair.Value == GameConfigFrameworkEnum.GameConfigType.CustomString) {
                    GenerateInputField (pair.Key);
                }
            }
        }

        private string GetPropertyDisplayName (string propertyName) {
            return propertyName.Replace (FrameworkVariable.GameConfigCustomStringSuffix, "");
        }

        private void GenerateDropdown (string propertyName, GameConfigFrameworkEnum.GameConfigType gameConfigType) {
            var dropdownObject = Instantiate (dropdownTemplate);
            dropdownObject.transform.SetParent (verticalLayout.transform);
            dropdownObject.transform.localScale = dropdownTemplate.transform.localScale;
            var titleText = dropdownObject.transform.GetChild (0).GetComponent<Text> ();
            var dropdown = dropdownObject.transform.GetChild (1).GetComponent<Dropdown> ();

            titleText.text = GetPropertyDisplayName (propertyName);

            var dropdownOptions = new List<Dropdown.OptionData> ();

            switch (gameConfigType) {
                case GameConfigFrameworkEnum.GameConfigType.GameConfigSetName:
                    for (var i = 0; i < allConfigSets.Count; i++) {
                        var displayString = new Dropdown.OptionData (allConfigSets[i].GetPropertyValue (propertyName).ToString ());
                        dropdownOptions.Add (displayString);
                    }

                    dropdown.onValueChanged.AddListener (delegate {
                        ConfigSetDropdownValueSelectedHandler (dropdown);
                    });
                    break;
                case GameConfigFrameworkEnum.GameConfigType.String:
                    for (var i = 0; i < allConfigSets.Count; i++) {
                        var configSetName = allConfigSets[i].GetPropertyValue (configSetNameSearchKey).ToString ();
                        var displayString = new Dropdown.OptionData (FrameworkUtils.StringReplace (StringConfigDisplayValue, configSetName, allConfigSets[i].GetPropertyValue (propertyName).ToString ()));
                        dropdownOptions.Add (displayString);
                    }
                    break;
                case GameConfigFrameworkEnum.GameConfigType.Enum:
                    var propertyTypeName = allConfigSets[0].GetPropertyTypeName (propertyName);
                    var enumType = Type.GetType (propertyTypeName);
                    foreach (var enumValue in Enum.GetValues (enumType)) {
                        var displayString = new Dropdown.OptionData (enumValue.ToString ());
                        dropdownOptions.Add (displayString);
                    }
                    break;

            }

            dropdown.options = dropdownOptions;
            dropdownObject.SetActive (true);
            allDropdowns.Add (dropdown);
            dropdownDict.Add (propertyName, dropdown);
        }

        private void GenerateInputField (string propertyName) {
            var inputFieldObject = Instantiate (inputFieldTemplate);
            inputFieldObject.transform.SetParent (verticalLayout.transform);
            inputFieldObject.transform.localScale = dropdownTemplate.transform.localScale;
            var titleText = inputFieldObject.transform.GetChild (0).GetComponent<Text> ();
            var inputField = inputFieldObject.transform.GetChild (1).GetComponent<InputField> ();

            titleText.text = GetPropertyDisplayName (propertyName);

            inputFieldObject.SetActive (true);
            allInputField.Add (inputField);
            inputFieldDict.Add (propertyName, inputField);
        }

        private void ShowInitialDropdownSelection () {
            try {
                var gameConfigSelectionStringLastTime = PlayerPrefs.GetString (FrameworkVariable.GameConfigLastTimeKey, "");
                if (gameConfigSelectionStringLastTime != "") {
                    var selectionStringList = gameConfigSelectionStringLastTime.Split (new string[] { FrameworkVariable.DefaultDelimiter }, StringSplitOptions.None);

                    var index = 0;
                    for (var i = 0; i < allDropdowns.Count; i++) {
                        allDropdowns[i].value = int.Parse (selectionStringList[index]);
                        index++;
                    }
                    for (var j = 0; j < allInputField.Count; j++) {
                        allInputField[j].text = selectionStringList[index];
                        index++;
                    }
                } else {
                    ShowFirstGameConfigSet ();
                }
            } catch (Exception) {
                // if there is any error, set all the dropdown to show the first option
                ShowFirstGameConfigSet ();
            }
        }

        private void ShowFirstGameConfigSet () {
            SetConfigSet (0);
        }

        private void SetConfigSet (int configSetIndex) {
            var selectedConfigSet = allConfigSets[configSetIndex];

            foreach (var pair in dropdownDict) {
                var dropdown = pair.Value;
                var gameConfigType = propertyNameDict[pair.Key];
                switch (gameConfigType) {
                    case GameConfigFrameworkEnum.GameConfigType.String:
                        dropdown.value = configSetIndex;
                        break;
                    case GameConfigFrameworkEnum.GameConfigType.Enum:
                        for (var i = 0; i < dropdown.options.Count (); i++) {
                            if (dropdown.options[i].text == selectedConfigSet.GetPropertyValue (pair.Key).ToString ()) {
                                dropdown.value = i;
                                break;
                            }
                        }
                        break;
                }
            }

            foreach (var pair in inputFieldDict) {
                var inputField = pair.Value;
                var gameConfigType = propertyNameDict[pair.Key];
                switch (gameConfigType) {
                    case GameConfigFrameworkEnum.GameConfigType.CustomString:
                        inputField.text = selectedConfigSet.GetPropertyValue (pair.Key).ToString ();
                        break;
                }
            }
        }

        private void SaveCustomConfigForNextTime () {
            var tempList = new List<string> ();

            for (var i = 0; i < allDropdowns.Count; i++) {
                tempList.Add (allDropdowns[i].value.ToString ());
            }

            for (var j = 0; j < allInputField.Count; j++) {
                tempList.Add (allInputField[j].text);
            }

            var customConfigSelectionString = "";
            if (tempList.Count > 0) {
                customConfigSelectionString = string.Join (FrameworkVariable.DefaultDelimiter, tempList);
            }

            PlayerPrefs.SetString (FrameworkVariable.GameConfigLastTimeKey, customConfigSelectionString);
        }

        private void SetGameConfigWithCustomConfig () {
            var customConfigSet = GameConfig.GetEmptyGameConfigSet ();

            foreach (var pair in propertyNameDict) {
                switch (pair.Value) {
                    case GameConfigFrameworkEnum.GameConfigType.GameConfigSetName:
                    case GameConfigFrameworkEnum.GameConfigType.String:
                        var selectedValue = dropdownDict[pair.Key].value;
                        customConfigSet.SetPropertyValue (pair.Key, allConfigSets[selectedValue].GetPropertyValue (pair.Key));
                        break;
                    case GameConfigFrameworkEnum.GameConfigType.Enum:
                        var selectedValue_Enum = dropdownDict[pair.Key].value;
                        customConfigSet.SetPropertyValue (pair.Key, (object)selectedValue_Enum);
                        break;
                    case GameConfigFrameworkEnum.GameConfigType.CustomString:
                        var selectedValue_CustomString = inputFieldDict[pair.Key].text;
                        customConfigSet.SetPropertyValue (pair.Key, selectedValue_CustomString);
                        break;
                }
            }
            SetGameConfig (customConfigSet);
        }

        private void SetGameConfigWithProductionConfig () {
            var productionConfigSet = GameConfig.ReleaseBuildGameConfigSet;
            SetGameConfig (productionConfigSet);
        }

        private void SetGameConfig (GameConfigSet gameConfigSet) {
            GameConfig.SaveRuntimeGameConfig (gameConfigSet);
        }

        private void GoIntoGame (bool useProductionConfig) {
            if (useProductionConfig) {
                SetGameConfigWithProductionConfig ();
            } else {
                SaveCustomConfigForNextTime ();
                SetGameConfigWithCustomConfig ();
            }

            SceneManager.LoadScene (GameVariable.FirstSceneName);
        }

        #region event handler

        private void ConfigSetDropdownValueSelectedHandler (Dropdown target) {
            var selectedValue = target.value;
            SetConfigSet (selectedValue);
        }

        private void ConfirmBtnClickedHandler (HihiButton sender) {
            GoIntoGame (false);
        }

        private void ClearPlayerPrefsBtnClickedHandler (HihiButton sender) {
            PlayerPrefs.DeleteAll ();
            Log.PrintWarning ("PlayerPrefs have been cleared", LogTypes.General);
        }

        #endregion
    }
}