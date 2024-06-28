using UnityEngine;
using UnitonConnect.Runtime.Data;

namespace UnitonConnect.Editor.Common
{
    public sealed class ProjectStorageConsts
    {
        public const string WALLET_PROVIDER_CONFIG_NAME = "WalletConfig";
        public const string WALLETS_PROVIDERS_STORAGE_NAME = "Wallets Storage";

        public const string CREATE_PATH_WALLET_PROVIDER = "Uniton Connect/Wallet";
        public const string CREATE_PATH_WALLETS_PROVIDERS_STORAGE = "Uniton Connect/Wallets Storage";

        public const string EDITOR_FILE_NAME = "dAppsData.asset";
        public const string EDITOR_STORAGE = "Assets/Ton Connect/Editor/Internal Resources";

        public const string RUNTIME_FILE_NAME_WITOUT_FORMAT = "dAppsRuntimeData";
        public const string RUNTIME_FILE_NAME = RUNTIME_FILE_NAME_WITOUT_FORMAT + ".asset";

        public const string RUNTIME_STORAGE = "Assets/Resources/Ton Connect";
        public const string RUNTIME_FOLDER_IN_RESOURCES = "Ton Connect";

        public const string APP_ICON_FILE_NAME = "icon.png";
        public const string APP_DATA_FILE_NAME = "dAppData.json";

        public const string START_TEST_URL = "https://mrveit.github.io/TGMiniApps-Saunposium/";
        public const string START_TEST_NAME = "TEST PROJECT";

        public const string START_TON_WALLETS_LINK = "https://raw.githubusercontent.com/ton-blockchain/wallets-list/main/wallets-v2.json";

        public static string GetTestAppManifest()
        {
            return GetAppManifest(START_TEST_URL, APP_DATA_FILE_NAME);
        }

        public static string GetAppManifest(string url, string manifestFileName)
        {
            return $"{url}/{manifestFileName}";
        }

        public static DAppConfig GetRuntimeAppStorage()
        {
            return Resources.Load<DAppConfig>($"{RUNTIME_STORAGE}/{RUNTIME_FILE_NAME_WITOUT_FORMAT}");
        }
    }
}