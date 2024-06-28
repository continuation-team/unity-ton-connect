using System.IO;
using UnityEngine;
using UnityEditor;
using UnitonConnect.Core.Utils.Debugging;

namespace UnitonConnect.Editor.Utils
{
    public sealed class StorageUtils
    {
#if UNITY_EDITOR
        public sealed class Target<T> where T : ScriptableObject
        {
            public static T CreateInstance()
            {
                var instance = ScriptableObject.CreateInstance<T>();

                return instance;
            }

            public static T LoadAssetAtPath(string folder, string path)
            {
                Directory.CreateDirectory(folder);

                return AssetDatabase.LoadAssetAtPath<T>(path);
            }
        }

        public static void CreateAsset(Object asset, string path)
        {
            AssetDatabase.CreateAsset(asset, path);
        }

        public static void EditCompressionProperties(string path)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

            var targetTextureFormat = TextureImporterFormat.RGBA32;
            var targetPlatform = "WebGL";

            if (importer != null)
            {
                var format = importer.GetAutomaticFormat(targetPlatform);

                if (importer.isReadable && format == targetTextureFormat)
                {
                    return;
                }

                importer.isReadable = true;

                if (format != targetTextureFormat)
                {
                    TextureImporterPlatformSettings platformSettings = new()
                    {
                        name = targetPlatform,
                        maxTextureSize = 512,
                        overridden = true,
                        format = targetTextureFormat
                    };

                    importer.SetPlatformTextureSettings(platformSettings);

                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

                    return;
                }

                importer.SaveAndReimport();

                return;
            }

            UnitonConnectLogger.LogWarning($"Failed to load Texture Importer for texture '{path}'.");
        }

        public static void SaveAsync<T>(T item) where T : ScriptableObject
        {
            EditorUtility.SetDirty(item);
        }
#endif
    }
}