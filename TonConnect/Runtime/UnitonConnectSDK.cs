using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using TonSdk.Connect;
using UnitonConnect.Core.Data;
using UnitonConnect.Core.Common;
using UnitonConnect.Runtime.Data;
using UnitonConnect.Editor.Common;
using UnitonConnect.Core.Utils.Debugging;
using System;
using System.Collections.Generic;

namespace UnitonConnect.Core
{
    [SelectionBase]
    [DisallowMultipleComponent]
    [HelpURL("https://github.com/MrVeit/Uniton-Connect")]
    public sealed class UnitonConnectSDK : MonoBehaviour, IUnitonConnectSDKCallbacks
    {
        private static readonly object _lock = new();
        private static UnitonConnectSDK _instance;

        public static UnitonConnectSDK Instance
        {
            get
            {
                if (_instance)
                {
                    return _instance;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = FindObjectOfType<UnitonConnectSDK>();
                    }
                }

                return _instance;
            }
        }

        [Header("SDK Settings")]
        [Tooltip("Enable if you want to test the SDK without having to upload data about your dApp")]
        [SerializeField, Space] private bool _isTestMode;
        [Tooltip("Enable if you want to activate SDK logging for detailed analysis before releasing a dApp")]
        [SerializeField] private bool _isDebugMode;
        [Tooltip("Turn it off if you want to do your own cdk initialization in your scripts")]
        [SerializeField, Space] private bool _initializeOnAwake;
        [Tooltip("Enable if you want to restore a saved connection from storage (recommended)")]
        [SerializeField] private bool _restoreConnectionOnAwake;
        [Space]
        [Header("TonConnect Settings")]
        [SerializeField, Space] private WalletsListData _walletsListConfig;
        [Tooltip("Disable if you want to use injected/web wallets. It only works in WebGL builds!")]
        [SerializeField, Space] private bool _useWebWallets;

        private TonConnect _tonConnect;
        private DAppConfig _appConfig;

        private Wallet _wallet;

        public bool IsTestMode => _isTestMode;
        public bool IsDebugMode => _isDebugMode;

        public event IUnitonConnectSDKCallbacks.OnProviderStatusChange OnProviderStatusChanged;
        public event IUnitonConnectSDKCallbacks.OnProviderStatusFail OnProviderStatusFailed;

        private void Awake()
        {
            CreateInstance();

            if (!_initializeOnAwake)
            {
                return;
            }

            Initialize();
        }

        public async void Initialize()
        {
            var dAppManifestLink = string.Empty;
            var dAppConfig = ProjectStorageConsts.GetRuntimeAppStorage();

            dAppManifestLink = GetAppManifestLink(dAppConfig);

            if (string.IsNullOrEmpty(dAppManifestLink))
            {
                UnitonConnectLogger.LogError("Failed to initialize Uniton Connect SDK due" +
                    " to missing configuration of your dApp. \r\nIf you want to test the operation of" +
                    " the SDK without integrating your project, activate test mode.");

                return;
            }

            ConfigureWallet();

            var options = GetOptions(dAppManifestLink);
            var remoteStorage = GetRemoteStorage();
            var connectOptions = GetAdditionalConnectOptions();

            _tonConnect = GetTonConnectInstance(options, remoteStorage, connectOptions);
            _tonConnect.OnStatusChange(OnProviderStatusChange, OnProviderStatusFail);

            await RestoreConnectionAsync(remoteStorage);
        }

        private void CreateInstance()
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = this;

                    DontDestroyOnLoad(gameObject);

                    return;
                }

                UnitonConnectLogger.LogError($"Another instance is detected on the scene, running delete...");

                Destroy(gameObject);
            }
        }

        private async Task RestoreConnectionAsync(RemoteStorage storage)
        {
            if (!_restoreConnectionOnAwake)
            {
                storage.RemoveItem(RemoteStorage.KEY_CONNECTION);
                storage.RemoveItem(RemoteStorage.KEY_LAST_EVENT_ID);

                return;
            }

            bool isSuccess = await _tonConnect.RestoreConnection();

            UnitonConnectLogger.Log($"Connection restored with status: {isSuccess}");
        }

        private void ActivateInitializationSDKListener(CancellationToken token,
            string url, ProviderMessageHandler successProvider, ProviderErrorHandler errorProvider)
        {

        }

        private void ActivateGatewaySender(string bridgeUrl, string postPath,
            string sessionId, string receiver, int timeToLive, string topic, byte[] message)
        {

        }

        private void LoadWalletsConfigs(string url, Action<List<WalletConfig>> callback)
        {

        }

        private void ConfigureWallet()
        {
            if (!IsSupportedWebWallets())
            {
                if (!_useWebWallets)
                {
                    return;
                }

                _useWebWallets = false;

                UnitonConnectLogger.LogWarning("The 'UseWebWallets' property was automatically disabled" +
                    " due to platform incompatibility. It should only be used in WebGL builds.");
            }
        }

        private TonConnect GetTonConnectInstance(TonConnectOptions options,
            RemoteStorage storage, AdditionalConnectOptions connectOptions)
        {
            return new TonConnect(options, storage, connectOptions);
        }

        private TonConnectOptions GetOptions(string manifestLink)
        {
            TonConnectOptions options = new()
            {
                ManifestUrl = manifestLink,
                WalletsListSource = _walletsListConfig.SourceLink,
                WalletsListCacheTTLMs = _walletsListConfig.CachedTimeToLive
            };

            return options;
        }

        private RemoteStorage GetRemoteStorage()
        {
            return new RemoteStorage(new(PlayerPrefs.GetString), new(PlayerPrefs.SetString),
                new(PlayerPrefs.DeleteKey), new(PlayerPrefs.HasKey));
        }

        private AdditionalConnectOptions GetAdditionalConnectOptions()
        {
            AdditionalConnectOptions connectOptions = new()
            {
                listenEventsFunction = new ListenEventsFunction(ActivateInitializationSDKListener),
                sendGatewayMessage = new SendGatewayMessage(ActivateGatewaySender)
            };

            return connectOptions;
        }

        private string GetAppManifestLink(DAppConfig config)
        {
            var dAppManifestLink = ProjectStorageConsts.GetTestAppManifest();

            if (!_isTestMode && config == null)
            {
                UnitonConnectLogger.LogError("Failed to detect the configuration of your dApp" +
                    " to generate the manifest. It can be assigned via the `Uniton Connect -> dApp Config` configuration window");

                return string.Empty;
            }

            if (!_isTestMode)
            {
                dAppManifestLink = ProjectStorageConsts.GetAppManifest(config.Data.ProjectLink,
                    ProjectStorageConsts.APP_DATA_FILE_NAME);
            }

            return dAppManifestLink;
        }

        private bool IsSupportedWebWallets()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            return false;
#endif

            return true;
        }

        private void OnProviderStatusChange(Wallet wallet) => OnProviderStatusChanged?.Invoke(wallet);

        private void OnProviderStatusFail(string errorMessage) => OnProviderStatusFailed?.Invoke(errorMessage);
    }
}