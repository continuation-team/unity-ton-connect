using UnityEngine;
using UnityEditor;
using UnitonConnect.Runtime.Data;
using UnitonConnect.Editor.Common;
using UnitonConnect.Editor.Utils;

namespace UnitonConnect.Editor.SetupWindow.Data
{
    public sealed class DAppSetupData : ScriptableObject
    {
        [field: SerializeField, Space] public DAppData Data { get; set; }

        private const string FILE_NAME = ProjectStorageConsts.EDITOR_FILE_NAME;
        private const string FOLDER_PATH = ProjectStorageConsts.EDITOR_STORAGE;
        private const string FULL_PATH = FOLDER_PATH + "/" + FILE_NAME;

        private static DAppSetupData _instance;

        public static DAppSetupData Instance
        {
            get
            {
                if (_instance)
                {
                    return _instance;
                }

                _instance = StorageUtils.Target<DAppSetupData>.LoadAssetAtPath(FOLDER_PATH, FULL_PATH);

                if (_instance)
                {
                    return _instance;
                }

                _instance = StorageUtils.Target<DAppSetupData>.CreateInstance();

                StorageUtils.CreateAsset(_instance, FULL_PATH);

                return _instance;
            }
        }

        public string ProjectLink
        {
            get => Data.ProjectLink;
            set => Data.ProjectLink = value;
        }

        public string Name
        {
            get => Data.Name;
            set => Data.Name = value;
        }

        public Texture2D Icon
        {
            get => Data.Icon;
            set => Data.Icon = value;
        }

        public static void SaveAsync()
        {
            StorageUtils.SaveAsync(_instance);
        }
    }
}