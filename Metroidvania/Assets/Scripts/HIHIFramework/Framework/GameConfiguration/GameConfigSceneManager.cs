using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Linq;
using HIHIFramework.Core;
using System.Text.RegularExpressions;

namespace HIHIFramework.GameConfiguration {
    public class GameConfigSceneManager : MonoBehaviour {

        private GameConfig gameConfig = new GameConfig();
        private List<GameConfigSet> allConfigSets;

        public GameObject settingsPanel;
        public GameObject dropdownTemplate;
        public GameObject inputFieldTemplate;
        public GameObject verticalLayout;
        public Text projectVersionText;
        public Text frameworkVersionText;
        public Button clearPlayerPrefsBtn;
        public Button confirmBtn;

        private List<Dropdown> allDropdowns;
        private Dictionary<string, Dropdown> dropdownDictionary;
        private List<InputField> allInputField;
        private Dictionary<string, InputField> inputFieldDictionary;

        private Dictionary<string, GameConfigBaseEnum.GameConfigType> fieldNameDictionary;

        private string configSetNameSearchKey;

        private const string GameVersionStringFormat = "Game Version : {0}";
        private const string FrameworkVersionStringFormat = "Framework Version : {0}";
        private const string StringConfigDisplayValue = "({0}) {1}";    // {0} : config set name, {1} : value

        void Awake () {
            GameUtils.InitGameSettings ();
            var isUseProductionConfig = GameUtils.GetIsReleaseBuild() && !GameVariable.IsShowGameConfigSceneInReleaseBuild;
            Log.PrintDebug ("isUseProductionConfig :: " + isUseProductionConfig);

            if (isUseProductionConfig) {
                settingsPanel.SetActive (false);
                GoIntoGame (true);
            } else {
                settingsPanel.SetActive (true);
                projectVersionText.text = GameUtils.StringReplace(GameVersionStringFormat, Application.version);
                frameworkVersionText.text = GameUtils.StringReplace (FrameworkVersionStringFormat, FrameworkVariable.FrameworkVersion);

                allDropdowns = new List<Dropdown> ();
                dropdownDictionary = new Dictionary<string, Dropdown> ();
                allInputField = new List<InputField> ();
                inputFieldDictionary = new Dictionary<string, InputField> ();
                allConfigSets = gameConfig.allGameConfigSetList;
                GenerateDropdown ();
                ShowInitialDropdownSelection ();

                clearPlayerPrefsBtn.onClick.AddListener (OnClearPlayerPrefsButtonClick);
                confirmBtn.onClick.AddListener (OnConfirmButtonClick);
            }
        }

        private void GenerateDropdown () {
            if (allConfigSets == null || allConfigSets.Count == 0) {
                Log.PrintError ("No config sets are set. Cannot generate the config dropdown. Please check.");
                return;
            }

            var tempGameConfigSet = allConfigSets[0];
            fieldNameDictionary = tempGameConfigSet.GetGameConfigFieldNameDict ();

            // order:
            // 1. GameConfigSetName
            // 2. String
            // 3. Enum
            // 4. Custom String
            foreach (var pair in fieldNameDictionary) {
                if (pair.Value == GameConfigBaseEnum.GameConfigType.GameConfigSetName) {
                    configSetNameSearchKey = pair.Key;
                    GenerateDropdown (pair.Key, pair.Value);
                    break;
                }
            }

            foreach (var pair in fieldNameDictionary) {
                if (pair.Value == GameConfigBaseEnum.GameConfigType.String) {
                    GenerateDropdown (pair.Key, pair.Value);
                }
            }

            foreach (var pair in fieldNameDictionary) {
                if (pair.Value == GameConfigBaseEnum.GameConfigType.Enum) {
                    GenerateDropdown (pair.Key, pair.Value);
                }
            }

            foreach (var pair in fieldNameDictionary) {
                if (pair.Value == GameConfigBaseEnum.GameConfigType.CustomString) {
                    GenerateInputField (pair.Key);
                }
            }
        }

        private string GetFieldDisplayName (string fieldName) {
            var displayName = fieldName.Replace (FrameworkVariable.GameConfigCustomStringSuffix, "");

            // Remarks : For non public field of GameConfigSet derived class, the field name would be something like "<baseURL>k__BackingField"
            var matches = Regex.Matches (displayName, "<.*?>");
            if (matches.Count > 0) {
                displayName = matches[0].ToString ();
                return displayName.Substring (1, displayName.Length - 2);
            } else {
                return fieldName;
            }
        }

        private void GenerateDropdown (string fieldName, GameConfigBaseEnum.GameConfigType gameConfigType) {
            var dropdownObject = Instantiate (dropdownTemplate);
            dropdownObject.transform.SetParent (verticalLayout.transform);
            dropdownObject.transform.localScale = dropdownTemplate.transform.localScale;
            var titleText = dropdownObject.transform.GetChild (0).GetComponent<Text> ();
            var dropdown = dropdownObject.transform.GetChild (1).GetComponent<Dropdown> ();

            titleText.text = GetFieldDisplayName (fieldName);

            var dropdownOptions = new List<Dropdown.OptionData> ();

            switch (gameConfigType) {
                case GameConfigBaseEnum.GameConfigType.GameConfigSetName:
                    for (var i = 0; i < allConfigSets.Count; i++) {
                        var displayString = new Dropdown.OptionData (allConfigSets[i].GetFieldValue (fieldName).ToString ());
                        dropdownOptions.Add (displayString);
                    }

                    dropdown.onValueChanged.AddListener (delegate {
                        OnConfigSetDropdownValueSelect (dropdown);
                    });
                    break;
                case GameConfigBaseEnum.GameConfigType.String:
                    for (var i = 0; i < allConfigSets.Count; i++) {
                        var configSetName = allConfigSets[i].GetFieldValue (configSetNameSearchKey).ToString ();
                        var displayString = new Dropdown.OptionData (GameUtils.StringReplace (StringConfigDisplayValue, configSetName, allConfigSets[i].GetFieldValue (fieldName).ToString ()));
                        dropdownOptions.Add (displayString);
                    }
                    break;
                case GameConfigBaseEnum.GameConfigType.Enum:
                    var fieldTypeName = allConfigSets[0].GetFieldTypeName (fieldName);
                    var enumType = Type.GetType (fieldTypeName);
                    foreach (var enumValue in Enum.GetValues (enumType)) {
                        var displayString = new Dropdown.OptionData (enumValue.ToString ());
                        dropdownOptions.Add (displayString);
                    }
                    break;

            }

            dropdown.options = dropdownOptions;
            dropdownObject.SetActive (true);
            allDropdowns.Add (dropdown);
            dropdownDictionary.Add (fieldName, dropdown);
        }

        private void GenerateInputField (string fieldName) {
            var inputFieldObject = Instantiate (inputFieldTemplate);
            inputFieldObject.transform.SetParent (verticalLayout.transform);
            inputFieldObject.transform.localScale = dropdownTemplate.transform.localScale;
            var titleText = inputFieldObject.transform.GetChild (0).GetComponent<Text> ();
            var inputField = inputFieldObject.transform.GetChild (1).GetComponent<InputField> ();

            titleText.text = GetFieldDisplayName (fieldName);

            inputFieldObject.SetActive (true);
            allInputField.Add (inputField);
            inputFieldDictionary.Add (fieldName, inputField);
        }

        private void ShowInitialDropdownSelection () {
            try {
                var GameConfigSelectionStringLastTime = PlayerPrefs.GetString (FrameworkVariable.GameConfigLastTimeKey, "");
                if (GameConfigSelectionStringLastTime != "") {
                    var selectionStringList = GameConfigSelectionStringLastTime.Split ('|');

                    var index = 0;
                    for (var i = 0; i < allDropdowns.Count; i++) {
                        allDropdowns[i].value = Int32.Parse (selectionStringList[index]);
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

            foreach (var pair in dropdownDictionary) {
                var dropdown = pair.Value;
                var gameConfigType = fieldNameDictionary[pair.Key];
                switch (gameConfigType) {
                    case GameConfigBaseEnum.GameConfigType.String:
                        dropdown.value = configSetIndex;
                        break;
                    case GameConfigBaseEnum.GameConfigType.Enum:
                        for (var i = 0; i < dropdown.options.Count (); i++) {
                            if (dropdown.options[i].text == selectedConfigSet.GetFieldValue (pair.Key).ToString ()) {
                                dropdown.value = i;
                                break;
                            }
                        }
                        break;
                }
            }

            foreach (var pair in inputFieldDictionary) {
                var inputField = pair.Value;
                var gameConfigType = fieldNameDictionary[pair.Key];
                switch (gameConfigType) {
                    case GameConfigBaseEnum.GameConfigType.CustomString:
                        inputField.text = selectedConfigSet.GetFieldValue (pair.Key).ToString ();
                        break;
                }
            }
        }

        private void SaveCustomConfigForNextTime () {
            var customConfigSelectionString = "";

            for (var i = 0; i < allDropdowns.Count; i++) {
                customConfigSelectionString += allDropdowns[i].value.ToString () + "|";
            }

            for (var j = 0; j < allInputField.Count; j++) {
                customConfigSelectionString += allInputField[j].text + "|";
            }

            customConfigSelectionString.Substring (0, customConfigSelectionString.Length - 1);
            PlayerPrefs.SetString (FrameworkVariable.GameConfigLastTimeKey, customConfigSelectionString);
        }

        private void SetGameConfigWithCustomConfig () {
            var customConfigSet = gameConfig.GetEmptyGameConfigSet ();

            foreach (var pair in fieldNameDictionary) {
                switch (pair.Value) {
                    case GameConfigBaseEnum.GameConfigType.GameConfigSetName:
                    case GameConfigBaseEnum.GameConfigType.String:
                        var selectedValue = dropdownDictionary[pair.Key].value;
                        customConfigSet.SetFieldValue (pair.Key, allConfigSets[selectedValue].GetFieldValue (pair.Key));
                        break;
                    case GameConfigBaseEnum.GameConfigType.Enum:
                        var selectedValue_Enum = dropdownDictionary[pair.Key].value;
                        customConfigSet.SetFieldValue (pair.Key, (object)selectedValue_Enum);
                        break;
                    case GameConfigBaseEnum.GameConfigType.CustomString:
                        var selectedValue_CustomString = inputFieldDictionary[pair.Key].text;
                        customConfigSet.SetFieldValue (pair.Key, selectedValue_CustomString);
                        break;
                }
            }
            SetGameConfig (customConfigSet);
        }

        private void SetGameConfigWithProductionConfig () {
            var productionConfigSet = gameConfig.releaseBuildGameConfigSet;
            SetGameConfig (productionConfigSet);
        }

        private void SetGameConfig (GameConfigSet gameConfigSet) {
            gameConfig.SetRuntimeGameConfig (gameConfigSet);
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

        private void OnConfigSetDropdownValueSelect (Dropdown target) {
            var selectedValue = target.value;
            SetConfigSet (selectedValue);
        }

        private void OnConfirmButtonClick () {
            GoIntoGame (false);
        }

        private void OnClearPlayerPrefsButtonClick () {
            PlayerPrefs.DeleteAll ();
            Log.PrintWarning ("PlayerPrefs have been cleared");
        }

        #endregion
    }
}