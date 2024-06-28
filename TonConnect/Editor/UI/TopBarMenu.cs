using UnityEngine;
using UnityEditor;
using UnitonConnect.Editor.SetupWindow;

namespace UnitonConnect.Editor.Common
{
    public sealed class TopBarMenu : ScriptableObject
    {
        [MenuItem("Uniton Connect/dApp Config")]
        public static void OpenSettingsWindow()
        {
            DAppSetupWindow.ShowWindow();
        }
    }
}