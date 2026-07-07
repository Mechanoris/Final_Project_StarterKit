using System.Reflection;
using UnityEngine;

namespace SHDH2367.Toolkit.Runtime
{
    static class RuntimeGameStateBridge
    {
        static MonoBehaviour cachedGameManager;
        static PropertyInfo cachedIsGameOverProperty;

        public static bool IsGameOver()
        {
            if (!TryResolveGameManager())
                return false;

            if (cachedIsGameOverProperty == null || cachedIsGameOverProperty.PropertyType != typeof(bool))
                return false;

            object value = cachedIsGameOverProperty.GetValue(cachedGameManager);
            return value is bool isGameOver && isGameOver;
        }

        static bool TryResolveGameManager()
        {
            if (cachedGameManager != null)
                return true;

            var behaviours = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            for (int i = 0; i < behaviours.Length; i++)
            {
                var behaviour = behaviours[i];
                if (behaviour == null)
                    continue;

                var type = behaviour.GetType();
                if (type.Name != "GameManager")
                    continue;

                cachedGameManager = behaviour;
                cachedIsGameOverProperty = type.GetProperty("IsGameOver", BindingFlags.Instance | BindingFlags.Public);
                return true;
            }

            return false;
        }
    }
}
