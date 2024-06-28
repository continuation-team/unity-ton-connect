using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using Newtonsoft.Json;
using UnitonConnect.Runtime.Data;
using UnitonConnect.Editor.Common;
using UnitonConnect.Core.Utils.Debugging;

namespace UnitonConnect.Editor.PostProccess
{
    public sealed class UnitonConnectAppConfigGenerator : IPostprocessBuildWithReport
    {
        private string _iconPath;

        public int callbackOrder { get { return 0; } }

        public void OnPostprocessBuild(BuildReport report)
        {
            var runtimeData = ProjectStorageConsts.GetRuntimeAppStorage();

            if (runtimeData != null)
            {
                string buildDirectory = Path.GetDirectoryName(report.summary.outputPath);
                string buildFolderName = Path.GetFileNameWithoutExtension(report.summary.outputPath);
                string buildFolderPath = Path.Combine(buildDirectory, buildFolderName);

                SaveIcon(runtimeData, buildFolderPath);
                SaveAppData(runtimeData, buildFolderPath);
            }
        }

        private void SaveIcon(DAppConfig appConfig, string buildPath)
        {
            var iconTexture = appConfig.Data.Icon;
            var iconPath = AssetDatabase.GetAssetPath(appConfig.Data.Icon);

            if (appConfig.Data.Icon != null)
            {
                _iconPath = Path.Combine(buildPath, ProjectStorageConsts.APP_ICON_FILE_NAME);

                byte[] textureBytes = iconTexture.EncodeToPNG();

                File.WriteAllBytes(_iconPath, textureBytes);

                UnitonConnectLogger.Log($"dApp icon created by path: {_iconPath}");

                return;
            }

            UnitonConnectLogger.LogWarning("dApp icon is not assigned in Runtime Storage.");
        }

        private void SaveAppData(DAppConfig appConfig, string buildPath)
        {
            var data = new RuntimeDAppData()
            {
                ProjectLink = appConfig.Data.ProjectLink,
                Name = appConfig.Data.Name,
                Icon = $"{appConfig.Data.ProjectLink}{ProjectStorageConsts.APP_ICON_FILE_NAME}"
            };

            string json = JsonConvert.SerializeObject(data);
            string jsonPath = Path.Combine(buildPath, ProjectStorageConsts.APP_DATA_FILE_NAME);

            File.WriteAllText(jsonPath, json);

            UnitonConnectLogger.Log($"dApp data created by path: {jsonPath}");
        }
    }
}