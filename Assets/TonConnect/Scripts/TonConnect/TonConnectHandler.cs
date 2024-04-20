using UnityEngine;
using TonSdk.Connect;
using System;
using UnityEngine.Networking;
using System.Collections;
using System.Threading;
using Newtonsoft.Json;
using System.Collections.Generic;

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
    public TonConnect tonConnect {get; private set;}

    /// <summary>
    /// Get wallets list from url and call callback method with result of the request
    /// </summary>
    /// <param name="url">Source url of the wallets list</param>
    /// <param name="callback">Callback method which will be called after request is completed</param>
    public void GetWalletConfigs(string url, Action<List<WalletConfig>> callback)
    {
        StartCoroutine(LoadWallets(url, callback));
    }

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
        if(RestoreConnectionOnAwake)
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
        if(UseWebWallets)
        {
            UseWebWallets = false;
            Debug.LogWarning("The 'UseWebWallets' property has been automatically disabled due to platform incompatibility. It should be used specifically in WebGL builds.");
        }
#endif
        if(ManifestURL.Length == 0) throw new ArgumentNullException("'ManifestUrl' field cannot be empty. Please provide a valid URL in the 'ManifestUrl' field.");
    }

    #region Status change callbacks
    private void OnStatusChange(Wallet wallet) => OnProviderStatusChanged?.Invoke(wallet);
    private void OnStatusChangeError(string error) => OnProviderStatusChangedError?.Invoke(error);
    #endregion

    #region Override actions

    private IEnumerator SendPostRequest(string bridgeUrl, string postPath, string sessionId, string receiver, int ttl, string topic, byte[] message)
    {
        string url = $"{bridgeUrl}/{postPath}?client_id={sessionId}&to={receiver}&ttl={ttl}&topic={topic}";

        UnityWebRequest request = new(url, "POST")
        {
            uploadHandler = new UploadHandlerRaw(message)
        };
        //request.SetRequestHeader("mode", "no-cors");
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

    private void SendRequest(string bridgeUrl, string postPath, string sessionId, string receiver, int ttl, string topic, byte[] message)
    {
        StartCoroutine(SendPostRequest(bridgeUrl, postPath, sessionId, receiver, ttl, topic, message));
    }

    private void ListenEvents(CancellationToken cancellationToken, string url, ProviderMessageHandler handler, ProviderErrorHandler errorHandler)
    {
        StartCoroutine(ListenForEvents(cancellationToken, url, handler, errorHandler));
    }

    private IEnumerator ListenForEvents(CancellationToken cancellationToken, string url, ProviderMessageHandler handler, ProviderErrorHandler errorHandler)
    {
        UnityWebRequest request = new(url)
        {
            method = UnityWebRequest.kHttpVerbGET
        };
        request.SetRequestHeader("Accept", "text/event-stream");

        DownloadHandlerBuffer handlerBuff = new();
        request.downloadHandler = handlerBuff;

        AsyncOperation operation = request.SendWebRequest();

        int currentPosition = 0; 

        while (!cancellationToken.IsCancellationRequested && !operation.isDone)
        {
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
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
                    // Debug.Log(line);
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
    #endregion

    #region Coroutines and Tasks

    public IEnumerator LoadWallets(string url, Action<List<WalletConfig>> callback)
    {
        // Here we load wallets list, from the web.
        // Provide callback function to work with result.

        List<WalletConfig> wallets = new();
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("HTTP Error: " + www.error);
        }
        else
        {
            List<Dictionary<string, object>> walletsList = null;
            string response = www.downloadHandler.text;
            walletsList = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(response);
            for(int i = 0; i < walletsList.Count; i++)
            {
                if (walletsList[i] == null)
                {
                    Debug.Log("Not supported wallet: is not a dictionary -> " + walletsList[i]);
                    continue;
                }

                if (!walletsList[i].ContainsKey("name") || !walletsList[i].ContainsKey("image") || !walletsList[i].ContainsKey("about_url") || !walletsList[i].ContainsKey("bridge"))
                {
                    Debug.Log("Not supported wallet. Config -> " + walletsList[i]);
                    continue;
                }

                List<Dictionary<string, object>> bridges = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(walletsList[i]["bridge"].ToString());
                if (bridges == null || bridges.Count == 0)
                {
                    Debug.Log("Not supported wallet: bridges is not a list or len is equal 0, config -> " + walletsList[i]);
                    continue;
                }

                WalletConfig walletConfig = new WalletConfig()
                {
                    Name = walletsList[i]["name"].ToString(),
                    Image = walletsList[i]["image"].ToString(),
                    AboutUrl = walletsList[i]["about_url"].ToString(),
                    AppName = walletsList[i]["app_name"].ToString()
                };

                foreach (Dictionary<string, object> bridge in bridges)
                {
                    if (bridge.TryGetValue("type", out object value) && value.ToString() == "sse")
                    {
                        if (!bridge.ContainsKey("url"))
                        {
                            Debug.Log("Not supported wallet: bridge url not found, config -> " + walletsList[i]);
                            continue;
                        }

                        walletConfig.BridgeUrl = bridge["url"].ToString();
                        if (walletsList[i].TryGetValue("universal_url", out object urlUni)) walletConfig.UniversalUrl = urlUni.ToString();
                        if(walletConfig.JsBridgeKey != null) walletConfig.JsBridgeKey = null;
                        wallets.Add(walletConfig);
                    }
                    else if(value.ToString() == "js")
                    {
                        if(!bridge.ContainsKey("key"))
                        {
                            Debug.Log("Not supported wallet: bridge key not found, config -> " + walletsList[i]);
                            continue;
                        }
                        walletConfig.JsBridgeKey = bridge["key"].ToString();
                        if(walletConfig.BridgeUrl != null) walletConfig.BridgeUrl = null;
                        wallets.Add(walletConfig);
                    }
                }

                if (walletConfig.BridgeUrl == null && walletConfig.JsBridgeKey == null) continue;
            }
        }

        callback(wallets);
    }

    #endregion
}
