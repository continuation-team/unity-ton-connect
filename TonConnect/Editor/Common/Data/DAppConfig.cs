using UnityEngine;
using UnityEditor;
using TonConnect.Editor.Common;
using TonConnect.Editor.Utils;

namespace TonConnect.Runtime.Data
{
    public sealed class DAppConfig : ScriptableObject
    {
        [field: SerializeField, Space] public DAppData Data { get; set; }

        private const string FILE_NAME = ProjectConsts.RUNTIME_FILE_NAME;
        private const string FOLDER_PATH = ProjectConsts.RUNTIME_STORAGE;
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

                _instance = WindowUtils.Target<DAppConfig>.LoadAssetAtPath(FOLDER_PATH, FULL_PATH);

                if (_instance)
                {
                    return _instance;
                }

                _instance = WindowUtils.Target<DAppConfig>.CreateInstance();

                WindowUtils.CreateAsset(_instance, FULL_PATH);

                return _instance;
            }
        }

        public static void SaveAsync()
        {
            EditorUtility.SetDirty(_instance);
        }
#endif
    }
}