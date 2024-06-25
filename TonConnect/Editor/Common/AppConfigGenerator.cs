using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using Newtonsoft.Json;
using TonConnect.Runtime.Data;
using TonConnect.Editor.Common;

namespace TonConnect.Editor.PostProccess
{
    public sealed class AppConfigGenerator : IPostprocessBuildWithReport
    {
        private readonly string _runtimeStoragePath = ProjectConsts.RUNTIME_FOLDER_IN_RESOURCES +
            "/" + ProjectConsts.RUNTIME_FILE_NAME_WITOUT_FORMAT;

        private string _iconPath;

        public int callbackOrder { get { return 0; } }

        public void OnPostprocessBuild(BuildReport report)
        {
            var runtimeData = Resources.Load<DAppConfig>(_runtimeStoragePath);

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
                Debug.Log($"Selected icon path: {iconPath}");

                _iconPath = Path.Combine(buildPath, ProjectConsts.APP_ICON_FILE_NAME);

                byte[] textureBytes = iconTexture.EncodeToPNG();

                File.WriteAllBytes(_iconPath, textureBytes);

                Debug.Log($"dApp icon created by path: {_iconPath}");

                return;
            }

            Debug.LogWarning("dApp icon is not assigned in Runtime Storage.");
        }

        private void SaveAppData(DAppConfig appConfig, string buildPath)
        {
            var data = new RuntimeDAppData()
            {
                ProjectLink = appConfig.Data.ProjectLink,
                Name = appConfig.Data.Name,
                Icon = $"{appConfig.Data.ProjectLink}{ProjectConsts.APP_ICON_FILE_NAME}"
            };

            string json = JsonConvert.SerializeObject(data);
            string jsonPath = Path.Combine(buildPath, ProjectConsts.APP_DATA_FILE_NAME);

            File.WriteAllText(jsonPath, json);

            Debug.Log($"dApp data created by path: {jsonPath}");
        }
    }
}