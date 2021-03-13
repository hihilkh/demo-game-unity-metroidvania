using System.Collections.Generic;
using HihiFramework.Core;

namespace HihiFramework.GameConfiguration {
    public abstract class GameConfigSetBase {
        // Remarks : the variable name "gameConfigSetName" must not be changed
        protected string GameConfigSetName { get; set; }                        // Display name for the config set
        public string BaseURL { get; protected set; }                                  // The base URL (mainly for API)

        public Dictionary<string, GameConfigFrameworkEnum.GameConfigType> GetGameConfigPropertyNameDict () {
            var gameConfigSearchKeys = new Dictionary<string, GameConfigFrameworkEnum.GameConfigType> ();

            var gameConfigProperties = GetType ().GetProperties (System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            foreach (var property in gameConfigProperties) {
                if (property.Name == FrameworkVariable.GameConfigSetNameFieldName) {
                    gameConfigSearchKeys.Add (property.Name, GameConfigFrameworkEnum.GameConfigType.GameConfigSetName);
                } else if (property.PropertyType == typeof (string)) {
                    if (property.Name.Contains (FrameworkVariable.GameConfigCustomStringSuffix)) {
                        gameConfigSearchKeys.Add (property.Name, GameConfigFrameworkEnum.GameConfigType.CustomString);
                    } else {
                        gameConfigSearchKeys.Add (property.Name, GameConfigFrameworkEnum.GameConfigType.String);
                    }
                } else if (property.PropertyType.IsEnum) {
                    gameConfigSearchKeys.Add (property.Name, GameConfigFrameworkEnum.GameConfigType.Enum);
                }
            }
            return gameConfigSearchKeys;
        }

        public string GetPropertyTypeName (string propertyName) {
            var propertyInfo = GetType ().GetProperty (propertyName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            return propertyInfo.PropertyType.FullName;
        }

        public object GetPropertyValue (string propertyName) {
            var propertyInfo = GetType ().GetProperty (propertyName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            return propertyInfo.GetValue (this);
        }

        public void SetPropertyValue (string propertyName, object propertyValue) {
            var propertyInfo = GetType ().GetProperty (propertyName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            propertyInfo.SetValue (this, propertyValue);
        }
    }
}