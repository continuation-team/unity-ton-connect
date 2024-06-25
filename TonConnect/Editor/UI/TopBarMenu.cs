using UnityEngine;
using UnityEditor;
using TonConnect.Editor.SetupWindow;

namespace TonConnect.Editor.Common
{
    public sealed class TopBarMenu : ScriptableObject
    {
        [MenuItem("Ton Connect/dApps Setup")]
        public static void OpenSettingsWindow()
        {
            DAppSetupWindow.ShowWindow();
        }
    }
}