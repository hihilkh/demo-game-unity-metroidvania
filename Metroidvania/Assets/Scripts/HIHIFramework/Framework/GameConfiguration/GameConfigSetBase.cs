using System.Collections.Generic;
using System.Text.RegularExpressions;
using HIHIFramework.Core;

namespace HIHIFramework.GameConfiguration {
    public abstract class GameConfigSetBase {
        // Remarks : the variable name "gameConfigSetName" must not be changed
        protected string gameConfigSetName;                        // Display name for the config set

        public Dictionary<string, GameConfigBaseEnum.GameConfigType> GetGameConfigFieldNameDict () {
            var gameConfigSearchKeys = new Dictionary<string, GameConfigBaseEnum.GameConfigType> ();

            var gameConfigFields = GetType ().GetFields (System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            foreach (var field in gameConfigFields) {
                if (field.Name == FrameworkVariable.GameConfigSetNameFieldName) {
                    gameConfigSearchKeys.Add (field.Name, GameConfigBaseEnum.GameConfigType.GameConfigSetName);
                } else if (field.FieldType == typeof (string)) {
                    if (field.Name.Contains (FrameworkVariable.GameConfigCustomStringSuffix)) {
                        gameConfigSearchKeys.Add (field.Name, GameConfigBaseEnum.GameConfigType.CustomString);
                    } else {
                        gameConfigSearchKeys.Add (field.Name, GameConfigBaseEnum.GameConfigType.String);
                    }
                } else if (field.FieldType.IsEnum) {
                    gameConfigSearchKeys.Add (field.Name, GameConfigBaseEnum.GameConfigType.Enum);
                }
            }
            return gameConfigSearchKeys;
        }

        public string GetFieldTypeName (string fieldName) {
            var fieldInfo = GetType ().GetField (fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            return fieldInfo.FieldType.FullName;
        }

        public object GetFieldValue (string fieldName) {
            var fieldInfo = GetType ().GetField (fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            return fieldInfo.GetValue (this);
        }

        public void SetFieldValue (string fieldName, object fieldValue) {
            var fieldInfo = GetType ().GetField (fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            fieldInfo.SetValue (this, fieldValue);
        }
    }
}