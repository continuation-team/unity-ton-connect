using UnityEngine;
using UnityEditor;
using UnitonConnect.Runtime.Data;
using UnitonConnect.Editor.Common;
using UnitonConnect.Editor.SetupWindow.Data;
using UnitonConnect.Editor.Utils;
using UnitonConnect.Core.Utils.Debugging;

namespace UnitonConnect.Editor.SetupWindow
{
    public sealed class DAppSetupWindow : EditorWindow
    {
        private Texture2D _selectedIcon;

        private void OnEnable()
        {
            var data = DAppConfig.Instance.Data;

            if (data.Icon != null)
            {
                _selectedIcon = DAppSetupData.Instance.Data.Icon;
            }
        }

        private void OnDestroy()
        {
            var runtimeData = DAppConfig.Instance;

            UpdateRuntimeStorage();

            DAppSetupData.SaveAsync();
            AssetDatabase.SaveAssets();
        }

        public static void ShowWindow()
        {
            var config = (DAppSetupWindow)GetWindow(typeof(DAppSetupWindow));
            config.titleContent = new GUIContent("dApp Config");
            config.minSize = new Vector2(500, 250);
            config.maxSize = new Vector2(500, 250);

            config.Show();
        }

        private void OnGUI()
        {
            LaberHeaderField("dApp Config");

            GUILayout.BeginHorizontal("box",
                GUILayout.Width(500), GUILayout.Height(250));

            EditorGUILayout.BeginVertical();

            GUILayout.Space(5);

            EditorGUILayout.LabelField("Icon:", GUILayout.Width(50));

            EditorGUILayout.BeginVertical();

            EditorGUI.BeginChangeCheck();

            _selectedIcon = EditorGUILayout.ObjectField(_selectedIcon, typeof(Texture2D),
                false, GUILayout.Width(150), GUILayout.Height(150)) as Texture2D;

            if (_selectedIcon != null && GUILayout.Button("Save Icon", GUILayout.Width(150)))
            {
                if (EditorGUI.EndChangeCheck())
                {
                    DAppSetupData.Instance.Data.Icon = _selectedIcon;
                }

                var filePath = AssetDatabase.GetAssetPath(_selectedIcon);

                StorageUtils.EditCompressionProperties(filePath);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();

            GUILayout.Space(20);

            EditorGUILayout.LabelField("Url:", GUILayout.Width(150));
            DAppSetupData.Instance.ProjectLink = GUILayout.TextField(
                DAppSetupData.Instance.ProjectLink, GUILayout.Width(300));

            GUILayout.Space(5);

            EditorGUILayout.LabelField("Name:", GUILayout.Width(150));
            DAppSetupData.Instance.Name = GUILayout.TextField(
                DAppSetupData.Instance.Name, GUILayout.Width(300));

            EditorGUILayout.EndVertical();

            GUILayout.Space(5);

            GUILayout.EndHorizontal();
        }

        private static void LaberHeaderField(string headerName)
        {
            EditorGUILayout.LabelField(headerName, new GUIStyle(EditorStyles.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,

                normal = new GUIStyleState()
                {
                    textColor = new Color(0.47f, 0.9f, 0.9f)
                },

                alignment = TextAnchor.MiddleCenter

            }, GUILayout.Height(20));

            HorizontalLine(new GUIStyle()
            {
                normal = new GUIStyleState()
                {
                    background = EditorGUIUtility.whiteTexture
                },
                margin = new RectOffset(0, 0, 10, 5),
                fixedHeight = 2
            });
        }

        private static string TextRow(string fieldTitle, string text, GUILayoutOption laberWidth,
            GUILayoutOption textFieldWidthOption = null)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent(fieldTitle), laberWidth);

            text = textFieldWidthOption == null
                ? GUILayout.TextField(text) :
                GUILayout.TextField(text, textFieldWidthOption);

            GUILayout.EndHorizontal();

            return text;
        }

        private static void HorizontalLine(GUIStyle lineStyle)
        {
            var color = GUI.color;

            GUI.color = Color.grey;
            GUILayout.Box(GUIContent.none, lineStyle);
            GUI.color = color;
        }

        private void UpdateRuntimeStorage()
        {
            var runtimeStorage = Resources.Load<DAppConfig>(
                $"{ProjectStorageConsts.RUNTIME_FOLDER_IN_RESOURCES}/" +
                $"{ProjectStorageConsts.RUNTIME_FILE_NAME_WITOUT_FORMAT}");

            runtimeStorage.Data.ProjectLink = DAppSetupData.Instance.ProjectLink;
            runtimeStorage.Data.Name = DAppSetupData.Instance.Name;
            runtimeStorage.Data.Icon = DAppSetupData.Instance.Icon;

            DAppConfig.SaveAsync();

            UnitonConnectLogger.Log("The dApp data storage has been successfully updated!");
        }
    }
}