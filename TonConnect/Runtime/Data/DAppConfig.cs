using UnityEngine;
using UnityEditor;
using UnitonConnect.Editor.Common;
using UnitonConnect.Editor.Utils;

namespace UnitonConnect.Runtime.Data
{
    public sealed class DAppConfig : ScriptableObject
    {
        [field: SerializeField, Space] public DAppData Data { get; set; }

        private const string FILE_NAME = ProjectStorageConsts.RUNTIME_FILE_NAME;
        private const string FOLDER_PATH = ProjectStorageConsts.RUNTIME_STORAGE;
        private const string FULL_PATH = FOLDER_PATH + "/" + FILE_NAME;

#if UNITY_EDITOR
        private static DAppConfig _instance;

        public static DAppConfig Instance
        {
            get
            {
                if (_instance)
                {
                    return _instance;
                }

                _instance = StorageUtils.Target<DAppConfig>.LoadAssetAtPath(FOLDER_PATH, FULL_PATH);

                if (_instance)
                {
                    return _instance;
                }

                _instance = StorageUtils.Target<DAppConfig>.CreateInstance();

                StorageUtils.CreateAsset(_instance, FULL_PATH);

                return _instance;
            }
        }

        public static void SaveAsync()
        {
            StorageUtils.SaveAsync(_instance);
        }
#endif
    }
}