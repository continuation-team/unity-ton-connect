using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using TonSdk.Connect;
using Newtonsoft.Json;
using UnitonConnect.Core.Data;
using UnitonConnect.Core.Data.Common;
using UnitonConnect.Core.Common;
using UnitonConnect.Core.Utils;
using UnitonConnect.Core.Utils.Debugging;
using UnitonConnect.Editor.Common;

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

        [Header("SDK Settings"), Space]
        [Tooltip("Enable if you want to test the SDK without having to upload data about your dApp")]
        [SerializeField, Space] private bool _testMode;
        [Tooltip("Enable if you want to activate SDK logging for detailed analysis before releasing a dApp")]
        [SerializeField] private bool _debugMode;
        [Tooltip("Turn it off if you want to do your own cdk initialization in your scripts")]
        [SerializeField, Space] private bool _initializeOnAwake;
        [Tooltip("Enable if you want to restore a saved connection from storage (recommended)")]
        [SerializeField] private bool _restoreConnectionOnAwake;
        [Header("TonConnect Settings"), Space]
        [SerializeField, Space] private WalletsListData _walletsListConfig;
        [Tooltip("Disable if you want to use injected/web wallets. It only works in WebGL builds!")]
        [SerializeField, Space] private bool _useWebWallets;
        [Tooltip("Enable if you want to instantly receive wallet icons in your interface without waiting for them to be downloaded from the server")]
        [SerializeField] private bool _useCachedWalletsIcons;
        [Tooltip("Configuration of supported wallets for your dApp. You can change their order and number, and override the way their configurations are loaded by hosting it yourself")]
        [SerializeField, Space] private WalletsProvidersData _supportedWallets;

        private TonConnect _tonConnect;

        public List<WalletProviderConfig> SupportedWallets => _supportedWallets.Config;

        public bool IsTestMode => _testMode;
        public bool IsDebugMode => _debugMode;
        public bool IsWalletConnected => _tonConnect.IsConnected;

        public bool IsUseWebWallets => _useWebWallets;
        public bool IsUseCachedWalletsIcons => _useCachedWalletsIcons;

        /// <summary>
        /// Callback, in case of successful initialization of sdk and loading of wallet configurations for further connection
        /// </summary>
        public event IUnitonConnectSDKCallbacks.OnWalletConnectionFinish OnWalletConnectionFinished;

        /// <summary>
        /// Callback for error handling, in case of unsuccessful loading of wallet configurations
        /// </summary>
        public event IUnitonConnectSDKCallbacks.OnWalletConnectionFail OnWalletConnectionFailed;

        private void Awake()
        {
            CreateInstance();

            if (!_initializeOnAwake)
            {
                return;
            }

            Initialize();
        }
        /// <summary>
        /// Initialization of the Uniton Connect sdk if you want to do it manually.
        /// </summary>
        public async void Initialize()
        {
            var dAppManifestLink = string.Empty;
            var dAppConfig = ProjectStorageConsts.GetRuntimeAppStorage();

            dAppManifestLink = WebRequestUtils.GetAppManifestLink(_testMode, dAppConfig);

            if (string.IsNullOrEmpty(dAppManifestLink))
            {
                UnitonConnectLogger.LogError("Failed to initialize Uniton Connect SDK due" +
                    " to missing configuration of your dApp. \r\nIf you want to test the operation of" +
                    " the SDK without integrating your project, activate test mode.");

                return;
            }

            ConfigureWalletsConfig();

            var options = GetOptions(dAppManifestLink);
            var remoteStorage = GetRemoteStorage();
            var connectOptions = GetAdditionalConnectOptions();

            _tonConnect = GetTonConnectInstance(options, remoteStorage, connectOptions);
            _tonConnect.OnStatusChange(OnWalletConnectionFinish, OnWalletConnectionFail);

            await RestoreConnectionAsync(remoteStorage);

            UnitonConnectLogger.Log("Success init");
        }

        /// <summary>
        /// Start downloading wallet configurations by the specified link to the json file
        /// </summary>
        /// <param name="supportedWalletsUrl">Link to a list of wallets to get their configuration, example: https://raw.githubusercontent.com/ton-blockchain/wallets-list/main/wallets-v2.json</param>
        /// <param name="walletsClaimed">Callback to retrieve successfully downloaded wallet configurations</param>
        public void GetWalletsConfigs(string supportedWalletsUrl,
            Action<List<WalletConfig>> walletsClaimed)
        {
            StartCoroutine(LoadWallets(supportedWalletsUrl, walletsClaimed));
        }

        /// <summary>
        /// Get a link to connect to the wallet via the specified config
        /// </summary>
        public async Task<string> ConnectWallet(WalletConfig wallet)
        {
            var connectUrl = string.Empty;

            try
            {
                connectUrl = await _tonConnect.Connect(wallet);
            }
            catch (WalletAlreadyConnectedError error)
            {
                UnitonConnectLogger.LogError($"Error: {error.Message}");
            }
            catch (Exception exceoption)
            {
                UnitonConnectLogger.LogError($"Failed to connect to the wallet due to " +
                    $"the following reason: {exceoption.Message}");
            }

            return connectUrl;
        }

        /// <summary>
        /// Unlinking of previously connected wallet
        /// </summary>
        public async Task DisconnectWallet()
        {
            try
            {
                PauseConnection();

                await _tonConnect.Disconnect();
            }
            catch (TonConnectError error)
            {
                UnitonConnectLogger.LogError($"Error: {error.Message}");
            }
            catch (Exception exception)
            {
                UnitonConnectLogger.LogError($"The previously connected wallet could not be " +
                    $"disconnected due to the following reason: {exception.Message}");
            }
        }

        public void PauseConnection()
        {
            _tonConnect.PauseConnection();
        }

        public void UnPauseConnection()
        {
            _tonConnect.UnPauseConnection();
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

        private IEnumerator LoadWallets(string supportedWalletsUrl, 
            Action<List<WalletConfig>> walletsClaimed)
        {
            UnityWebRequest request = UnityWebRequest.Get(supportedWalletsUrl);

            yield return request.SendWebRequest();

            if (request.result != WebRequestUtils.SUCCESS)
            {
                UnitonConnectLogger.LogError($"HTTP Error with message: {request.error}");

                yield break;
            }
            else
            {
                var responseResult = request.downloadHandler.text;
                var walletsList = JsonConvert.DeserializeObject<List<WalletProviderData>>(responseResult);

                UnitonConnectLogger.Log($"Wallet list config after load: {JsonConvert.SerializeObject(walletsList)}");

                ParseWalletsConfigs(ref walletsList, walletsClaimed);
            }

            request.Dispose();
        }

        private IEnumerator ActivateInitializationSDKListenerRoutine(EventListenerArgumentsData listenerData)
        {
            var url = listenerData.Url;

            var acceptHeader = WebRequestUtils.HEADER_ACCEPT;
            var textEventStreamValue = WebRequestUtils.HEADER_VALUE_TEXT_EVENT_STREAM;

            UnityWebRequest request = new(listenerData.Url, UnityWebRequest.kHttpVerbGET) { };

            WebRequestUtils.SetRequestHeader(request, acceptHeader, textEventStreamValue);

            DownloadHandlerBuffer handlerBuff = new();
            request.downloadHandler = handlerBuff;

            AsyncOperation operation = request.SendWebRequest();

            int currentPosition = 0;

            while (!listenerData.Token.IsCancellationRequested && !operation.isDone)
            {
                if (request.result == WebRequestUtils.CONNECTION_ERROR ||
                        request.result == WebRequestUtils.PROTOCOL_ERROR)
                {
                    listenerData.ErrorProvider(
                        new Exception($"SSE request error: {request.error}"));

                    UnitonConnectLogger.LogError($"Failed to activate event listener with error: {request.error}");

                    break;
                }

                string result = request.downloadHandler.text.Substring(currentPosition);
                string[] lines = result.Split('\n');

                foreach (string line in lines)
                {
                    if (!string.IsNullOrEmpty(line))
                    {
                        listenerData.Provider(line);
                    }
                }

                currentPosition += result.Length;

                yield return null;
            }
        }

        private IEnumerator ActivateGatewaySenderRoutine(GatewayMessageData gatewayMessage)
        {
            var url = WebRequestUtils.GetGatewaySenderLink(gatewayMessage);

            var contentTypeHeader = WebRequestUtils.HEADER_CONTENT_TYPE;
            var textPlainValue = WebRequestUtils.HEADER_VALUE_TEXT_PLAIN;

            UnityWebRequest request = new(url, UnityWebRequest.kHttpVerbPOST)
            {
                uploadHandler = new UploadHandlerRaw(gatewayMessage.Message)
            };

            WebRequestUtils.SetRequestHeader(request, contentTypeHeader, textPlainValue);

            UnitonConnectLogger.Log($"Sending POST request to URL: {url}");

            yield return request.SendWebRequest();

            if (request.result != WebRequestUtils.SUCCESS)
            {
                UnitonConnectLogger.LogError($"Failed to send Gateway message with error: {request.error}");
            }
            else
            {
                UnitonConnectLogger.Log("Gateway message successfully sended");
            }
        }

        private void ActivateInitializationSDKListener(CancellationToken token,
            string url, ProviderMessageHandler initializationSuccess, ProviderErrorHandler initializationFailed)
        {
            var listenerData = new EventListenerArgumentsData()
            {
                Token = token,
                Url = url,
                Provider = initializationSuccess,
                ErrorProvider = initializationFailed
            };

            StartCoroutine(ActivateInitializationSDKListenerRoutine(listenerData));
        }

        private void ActivateGatewaySender(string bridgeUrl, string postPath,
            string sessionId, string receiver, int timeToLive, string topic, byte[] message)
        {
            var gatewayMessage = new GatewayMessageData()
            {
                BridgeUrl = bridgeUrl,
                PostPath = postPath,
                SessionId = sessionId,
                Receiver = receiver,
                TimeToLive = timeToLive,
                Topic = topic,
                Message = message
            };

            StartCoroutine(ActivateGatewaySenderRoutine(gatewayMessage));
        }

        private void ConfigureWalletsConfig()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            if (!_useWebWallets)
            {
                return;
            }

            _useWebWallets = false;

            UnitonConnectLogger.LogWarning("The 'Use Web Wallets' property was automatically disabled" +
                " due to platform incompatibility. It should only be used in WebGL builds.");
#endif
        }

        private void ParseWalletsConfigs(ref List<WalletProviderData> walletsList, 
            Action<List<WalletConfig>> walletsClaimed)
        {
            var loadedWallets = new List<WalletConfig>();

            foreach (var wallet in walletsList)
            {
                WalletConfig walletConfig = new()
                {
                    Name = wallet.Name,
                    Image = wallet.Image,
                    AboutUrl = wallet.AboutUrl,
                    AppName = wallet.AppName
                };

                foreach (var bridge in wallet.Bridge)
                {
                    if (bridge.Type == WalletConfigComponents.SSE)
                    {
                        walletConfig.BridgeUrl = bridge.Url;
                        walletConfig.UniversalUrl = wallet.UniversalUrl;
                        walletConfig.JsBridgeKey = null;

                        loadedWallets.Add(walletConfig);
                    }
                    else if (bridge.Type == WalletConfigComponents.JAVA_SCRIPT)
                    {
                        walletConfig.JsBridgeKey = bridge.Key;
                        walletConfig.BridgeUrl = null;

                        loadedWallets.Add(walletConfig);
                    }
                }
            }

            walletsClaimed(loadedWallets);
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

        private void OnWalletConnectionFinish(Wallet wallet) => OnWalletConnectionFinished?.Invoke(wallet);

        private void OnWalletConnectionFail(string errorMessage) => OnWalletConnectionFailed?.Invoke(errorMessage);

        public void OnInjectedWalletMessageReceived(string message) => _tonConnect.ParseInjectedProviderMessage(message);
    }
}