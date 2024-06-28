using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using TonSdk.Connect;
using UnitonConnect.Core.Data;

namespace UnitonConnect.Core.Demo
{
    public class TonConnectHandler : MonoBehaviour
    {
        [Header("Plugin Settings")]
        [Tooltip("Toggle if you want to use injected/web wallets. \nOnly works in WebGL builds!")]
        public bool UseWebWallets = false;
        [Tooltip("Toggle if you want to restore saved connection from the storage. (recommended)")]
        public bool RestoreConnectionOnAwake = true;

        [Space(4)]

        [Header("TonConnect Settings")]
        [Tooltip("Url to the manifest with the Dapp metadata that will be displayed in the user's wallet.")]
        public string ManifestURL = "";
        [Tooltip("Redefine wallets list source URL.Must be a link to a json file with following structure - https://github.com/ton-connect/wallets-list (optional)")]
        public string WalletsListSource = "";
        [Tooltip("Wallets list cache time to live in milliseconds. (optional)")]
        public int WalletsListCacheTTL = 0;

        [HideInInspector] public delegate void OnProviderStatusChange(Wallet wallet);
        [HideInInspector] public static event OnProviderStatusChange OnProviderStatusChanged;

        [HideInInspector] public delegate void OnProviderStatusChangeError(string error);
        [HideInInspector] public static event OnProviderStatusChangeError OnProviderStatusChangedError;

        // main tonconnect instance, use it to work with tonconnect
        public TonConnect tonConnect { get; private set; }

        private void Start()
        {
            CheckHandlerSettings();
            CreateTonConnectInstance();
        }

        public async void CreateTonConnectInstance()
        {
            // Here we create tonconnect instance

            // Tonconnect options overrided by user data
            TonConnectOptions options = new()
            {
                ManifestUrl = ManifestURL,
                WalletsListSource = WalletsListSource,
                WalletsListCacheTTLMs = 0
            };

            // Unity cant work with Isolated Storage in web builds, IOS and Android
            // So we use PlayerPrefs, PlayerPrefs is isolated and also works in this platforms
            RemoteStorage remoteStorage = new(new(PlayerPrefs.GetString), new(PlayerPrefs.SetString), new(PlayerPrefs.DeleteKey), new(PlayerPrefs.HasKey));

            // Additional connect options used to set custom SSE listener
            // cause, Unity should work with requests in IEnumerable class
            AdditionalConnectOptions additionalConnectOptions = new()
            {
                listenEventsFunction = new ListenEventsFunction(ListenEvents),
                sendGatewayMessage = new SendGatewayMessage(SendRequest)
            };

            // Tonconnect instance
            tonConnect = new TonConnect(options, remoteStorage, additionalConnectOptions);

            // Subscribing to Status Change Callbacks
            tonConnect.OnStatusChange(OnStatusChange, OnStatusChangeError);

            // Restore connection, if needed
            if (RestoreConnectionOnAwake)
            {
                bool result = await tonConnect.RestoreConnection();
                Debug.Log($"Connection restored: {result}");
            }
            else
            {
                remoteStorage.RemoveItem(RemoteStorage.KEY_CONNECTION);
                remoteStorage.RemoveItem(RemoteStorage.KEY_LAST_EVENT_ID);
            }
        }

        private void CheckHandlerSettings()
        {
            // Here we check if the settings are valid

            // UseWebWallets must be true, only in WebGL
            // ManifestURL must not be empty
#if !UNITY_WEBGL || UNITY_EDITOR
            if (UseWebWallets)
            {
                UseWebWallets = false;
                Debug.LogWarning("The 'UseWebWallets' property has been automatically disabled due to platform incompatibility. It should be used specifically in WebGL builds.");
            }
#endif
            if (ManifestURL.Length == 0) throw new ArgumentNullException("'ManifestUrl' field cannot be empty. Please provide a valid URL in the 'ManifestUrl' field.");
        }

        #region Status change callbacks
        private void OnStatusChange(Wallet wallet) => OnProviderStatusChanged?.Invoke(wallet);
        private void OnStatusChangeError(string error) => OnProviderStatusChangedError?.Invoke(error);
        #endregion

        #region Override actions

        private IEnumerator SendPostRequest(string bridgeUrl, string postPath,
            string sessionId, string receiver, int ttl, string topic, byte[] message)
        {
            string url = $"{bridgeUrl}/{postPath}?client_id={sessionId}&to={receiver}&ttl={ttl}&topic={topic}";

            UnityWebRequest request = new(url, "POST")
            {
                uploadHandler = new UploadHandlerRaw(message)
            };

            request.SetRequestHeader("Content-Type", "text/plain");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error while sending request: " + request.error);
            }
            else
            {
                Debug.Log("Request sucessfully sent.");
            }
        }

        private void SendRequest(string bridgeUrl, string postPath, string sessionId,
            string receiver, int ttl, string topic, byte[] message)
        {
            StartCoroutine(SendPostRequest(bridgeUrl, postPath, sessionId, receiver, ttl, topic, message));
        }

        private void ListenEvents(CancellationToken cancellationToken, string url,
            ProviderMessageHandler handler, ProviderErrorHandler errorHandler)
        {
            StartCoroutine(ListenForEvents(cancellationToken, url, handler, errorHandler));
        }

        private IEnumerator ListenForEvents(CancellationToken cancellationToken, string url,
            ProviderMessageHandler handler, ProviderErrorHandler errorHandler)
        {
            UnityWebRequest request = new(url, "GET")
            {
            };
            request.SetRequestHeader("Accept", "text/event-stream");

            DownloadHandlerBuffer handlerBuff = new();
            request.downloadHandler = handlerBuff;

            AsyncOperation operation = request.SendWebRequest();

            int currentPosition = 0;

            while (!cancellationToken.IsCancellationRequested && !operation.isDone)
            {
                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {
                    errorHandler(new Exception("SSE request error: " + request.error));
                    Debug.Log("Err");
                    break;
                }

                string text = handlerBuff.text.Substring(currentPosition);

                string[] lines = text.Split('\n');

                foreach (string line in lines)
                {
                    if (!string.IsNullOrEmpty(line))
                    {
                        Debug.Log(line);

                        handler(line);
                    }
                }

                currentPosition += text.Length;

                yield return null;
            }
        }

        /// <summary>
        /// Hadnle injected wallet message from js side. Dont use it directly
        /// </summary>
        public void OnInjectedWalletMessageReceived(string message)
        {
            tonConnect.ParseInjectedProviderMessage(message);
        }

        /// <summary>
        /// Get wallets list from url and call callback method with result of the request
        /// </summary>
        /// <param name="url">Source url of the wallets list</param>
        /// <param name="callback">Callback method which will be called after request is completed</param>
        public void GetWalletConfigs(string url, Action<List<WalletConfig>> callback)
        {
            StartCoroutine(BestLoadWallets(url, callback));
        }

        #endregion

        #region Coroutines and Tasks

        public IEnumerator BestLoadWallets(string url, Action<List<WalletConfig>> callback)
        {
            List<WalletConfig> wallets = new List<WalletConfig>();
            UnityWebRequest www = UnityWebRequest.Get(url);

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("HTTP Error: " + www.error);
            }
            else
            {
                Debug.Log(www.downloadHandler.text);

                var walletsList = JsonConvert.DeserializeObject<
                    List<WalletProviderData>>(www.downloadHandler.text);

                foreach (var walletData in walletsList)
                {
                    WalletConfig walletConfig = new WalletConfig()
                    {
                        Name = walletData.Name,
                        Image = walletData.Image,
                        AboutUrl = walletData.AboutUrl,
                        AppName = walletData.AppName
                    };

                    foreach (var bridge in walletData.Bridge)
                    {
                        if (bridge.Type == "sse")
                        {
                            walletConfig.BridgeUrl = bridge.Url;
                            walletConfig.UniversalUrl = walletData.UniversalUrl;
                            walletConfig.JsBridgeKey = null;
                            wallets.Add(walletConfig);
                        }
                        else if (bridge.Type == "js")
                        {
                            walletConfig.JsBridgeKey = bridge.Key;
                            walletConfig.BridgeUrl = null;
                            wallets.Add(walletConfig);
                        }
                    }
                }

                Debug.Log($"Test list data: {JsonConvert.SerializeObject(wallets)}");
            }

            callback(wallets);
        }
        #endregion
    }
}