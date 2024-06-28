using System;
using UnityEngine;

namespace UnitonConnect.Core.Utils.Debugging
{
    public sealed class UnitonConnectLogger
    {
        private static bool IsEnabled => UnitonConnectSDK.Instance != null && UnitonConnectSDK.Instance.IsDebugMode;

        public const string PREFIX = "[Uniton Connect] ";

        public static void Log(object message)
        {
            if (IsEnabled)
            {
                Debug.Log(PREFIX + message);
            }
        }

        public static void LogWarning(object message)
        {
            if (IsEnabled)
            {
                Debug.LogWarning(PREFIX + message);
            }
        }

        public static void LogError(object message)
        {
            if (IsEnabled)
            {
                Debug.LogError(PREFIX + message);
            }
        }

        public static void LogException(Exception exception)
        {
            if (IsEnabled)
            {
                Debug.LogException(exception);
            }
        }
    }
}