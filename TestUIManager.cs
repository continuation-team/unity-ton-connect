using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using TMPro;
using TonSdk.Core;
using TonSdk.Connect;
using UnitonConnect.Core.Data;
using UnitonConnect.Core.Data.Common;
using UnitonConnect.Core.Utils.View;
using UnitonConnect.Core.Utils.Debugging;
using UnitonConnect.Editor.Common;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace UnitonConnect.Core.Demo
{
    public sealed class TestUIManager : MonoBehaviour
    {
        private static readonly object _lock = new();

        private static TestUIManager _instance;

        public static TestUIManager Instance
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
                        _instance = FindObjectOfType<TestUIManager>();
                    }
                }

                return _instance;
            }
        }

        [SerializeField, Space] private UnitonConnectSDK _unitonSDK;
        [SerializeField, Space] private WalletsProvidersData _walletsStorage;
        [SerializeField, Space] private TextMeshProUGUI _debugMessage;
        [SerializeField] private TextMeshProUGUI _shortWalletAddress;
        [SerializeField, Space] private Button _connectButton;
        [SerializeField] private Button _disconnectButton;
        [SerializeField, Space] private TestChooseWalletPanel _chooseWalletPanel;
        [SerializeField] private TestSelectedWalletConnectionPanel _connectPanel;
        [SerializeField, Space] private TestWalletView _walletViewPrefab;
        [SerializeField] private Transform _walletsParent;
        [SerializeField, Space] private List<TestWalletView> _activeWallets;

        public List<WalletConfig> LoadedWallets { get; set; }

        private string _connectUrl;

        private void Awake()
        {
            CreateInstance();

            _unitonSDK.OnWalletConnectionFinished += OnWalletConnectionFinished;
            _unitonSDK.OnWalletConnectionFailed += OnWalletConnectionFailed;
        }

        private void OnDestroy()
        {
            _unitonSDK.OnWalletConnectionFinished -= OnWalletConnectionFinished;
            _unitonSDK.OnWalletConnectionFailed -= OnWalletConnectionFailed;
        }

        private void Start()
        {
            if (!_unitonSDK.IsWalletConnected)
            {
                _disconnectButton.interactable = false;
            }

            Invoke(nameof(Initialize), 1f);
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

        public async void DisconnnectWallet()
        {
            await _unitonSDK.DisconnectWallet();
        }

        private void Initialize()
        {
            if (_unitonSDK.IsWalletConnected)
            {
                return;
            }

            _unitonSDK.GetWalletsConfigs(ProjectStorageConsts.
                START_TON_WALLETS_LINK, WalletsConfigsClaimed);
        }

        private async void WalletsConfigsClaimed(List<WalletConfig> wallets)
        {
            List<WalletConfig> httpBridgeWallets = GetHttpBridgeWallets(wallets);
            List<WalletConfig> jsBridgeWallets = GetJavaScriptBridgeWallets(wallets);

            List<WalletConfig> uniqueWallets = null;
            List<WalletConfig> walletsConfigs = null;

            if (UnitonConnectSDK.Instance.IsUseWebWallets)
            {
                foreach (var wallet in httpBridgeWallets)
                {
                    jsBridgeWallets.Add(wallet);
                }

                UnitonConnectLogger.Log($"Created wallet list: {JsonConvert.SerializeObject(jsBridgeWallets)}");
            }

            if (UnitonConnectSDK.Instance.IsUseWebWallets)
            {
                uniqueWallets = jsBridgeWallets.GroupBy(w => w.Name).Select(g => g.First()).ToList();
            }
            else
            {
                uniqueWallets = httpBridgeWallets.GroupBy(w => w.Name).Select(g => g.First()).ToList();
            }
                
            walletsConfigs = uniqueWallets.Take(uniqueWallets.Capacity).ToList();

            UnitonConnectLogger.Log($"Created {walletsConfigs.Capacity} wallets");

            LoadedWallets = walletsConfigs;

            UnitonConnectLogger.Log(JsonConvert.SerializeObject(walletsConfigs));

            var walletsViewList = new List<WalletViewData>();

            foreach (var wallet in LoadedWallets)
            {
                var walletView = await GetWalletViewComponents(wallet);

                walletsViewList.Add(walletView);
            }

            foreach (var walletView in walletsViewList)
            {
                var name = walletView.Name;
                var icon = walletView.Icon;

                var walletViewData = Instantiate(_walletViewPrefab, _walletsParent);

                walletViewData.SetView(name, icon, _connectPanel);

                _activeWallets.Add(walletViewData);
            }
        }

        /// <summary>
        /// Receiving QR Code on the specified link for further connection
        /// </summary>
        /// <param name="config">Wallet configuration for connection</param>
        /// <param name="qrCodeImageClaimed">Callback for QR code retrieval</param>
        public async Task GenerateQRCodeConnectionFromWalletConfigAsync(WalletConfig config)
        {
            var connectUrl = await GetConnectUrlAsync(config);
            var qrCode = WalletVisualUtils.GetQRCodeFromUrl(connectUrl);

            return;
        }

        public Texture2D GenerateQRCodeFromConnectURL(string connectUrl)
        {
            var qrCode = WalletVisualUtils.GetQRCodeFromUrl(connectUrl);

            return qrCode;
        }

        public async Task<string> GenerateURLForQRCodeConnection(WalletConfig wallet)
        {
            var connectUrl = await GetConnectUrlAsync(wallet);

            return connectUrl;
        }

        /// <summary>
        /// Download the wallet icon from the server using the specified link.
        /// </summary>
        /// <param name="config">Referencing a wallet icon from a previously retrieved wallet configuration via the `OnWalletConnectionFinished` event.</param>
        public async Task ConnectWebWalletWithDeepLinkAsync(WalletConfig config)
        {
            await _unitonSDK.ConnectWallet(config);
        }

        /// <summary>
        /// Get a list of htttp bridge wallets for further generation of QR code to connect to them
        /// </summary>
        /// <param name="loadedWallets">Previously received wallet configuration via the `OnWalletConnectionFinished` event.</param>
        public List<WalletConfig> GetHttpBridgeWallets(List<WalletConfig> loadedWallets)
        {
            List<WalletConfig> wallets = new();

            var httpBridgeWallets = loadedWallets.Where(wallet => wallet.BridgeUrl != null);

            foreach (var wallet in httpBridgeWallets)
            {
                wallets.Add(wallet);
            }

            return wallets;
        }

        /// <summary>
        /// Get a list of javascript bridge wallets to connect to via DeepLink on WebGL Mobile/Desktop
        /// </summary>
        /// <param name="loadedWallets">Previously received wallet configuration via the `OnWalletConnectionFinished` event.</param>
        public List<WalletConfig> GetJavaScriptBridgeWallets(List<WalletConfig> loadedWallets)
        {
            List<WalletConfig> wallets = new();

            if (_unitonSDK.IsUseWebWallets)
            {
                var jsBridgeWallets = loadedWallets.Where(
                    wallet => wallet.JsBridgeKey != null &&
                    InjectedProvider.IsWalletInjected(wallet.JsBridgeKey));

                foreach (var wallet in jsBridgeWallets)
                {
                    wallets.Add(wallet);
                }

                return wallets;
            }

            return null;
        }

        public void OpenDeepLinkFromHttpBridgeWallet(string connectUrl)
        {
            string escapedUrl = Uri.EscapeUriString(connectUrl);

            Application.OpenURL(escapedUrl);
        }

        private async Task<WalletViewData> GetWalletViewComponents(
            WalletConfig config)
        {
            string name = config.Name;
            Texture2D icon;

            if (_unitonSDK.IsUseCachedWalletsIcons)
            {
                icon = await WalletVisualUtils.GetWalletIconFromLocalStorage(
                    config, _walletsStorage.Config);
            }
            else
            {
                icon = await GetWalletIconFromServerAsync(config);
            }

            return WalletVisualUtils.GetNewWalletView(name, icon);
        }

        private async Task<Texture2D> GetWalletIconFromServerAsync(WalletConfig config)
        {
            Texture2D icon = await WalletVisualUtils.GetWalletIconFromServerAsync(config.Image);

            return icon;
        }

        private async Task<string> GetConnectUrlAsync(WalletConfig config)
        {
            string connectUrl = await _unitonSDK.ConnectWallet(config);

            _connectUrl = connectUrl;

            return connectUrl;
        }

        public bool HasHttpBridge(WalletConfig config)
        {
            return !string.IsNullOrEmpty(config.BridgeUrl);
        }

        public bool HasJSBridge(WalletConfig config)
        {
            return !string.IsNullOrEmpty(config.JsBridgeKey);
        }

        public bool HasMultipleBridgeTypes(string targetWalletName,
            List<WalletConfig> wallets)
        {
            var targetWallets = wallets.Where(w => w.Name == targetWalletName).ToList();

            foreach (var wallet in targetWallets)
            {
                bool hasHttpBridge = HasHttpBridge(wallet);
                bool hasJavaScriptBridge = HasJSBridge(wallet);

                if (hasHttpBridge == hasJavaScriptBridge)
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        public WalletConfig GetTargetWalletConfigWithoutSecondBridge(string bridgeType,
            string walletName, List<WalletConfig> wallets)
        {
            WalletConfig config;

            var httpBridge = WalletConfigComponents.SSE;
            var jsBridge = WalletConfigComponents.JAVA_SCRIPT;

            var targetWallets = wallets.Where(w => w.Name == walletName).ToList();

            foreach (var wallet in targetWallets)
            {
                bool hasHttpBridge = HasHttpBridge(wallet);
                bool hasJavaScriptBridge = HasJSBridge(wallet);

                if (bridgeType == httpBridge &&
                    hasHttpBridge && !hasJavaScriptBridge)
                {
                    return wallet;
                }

                if (bridgeType == jsBridge &&
                    hasJavaScriptBridge && !hasHttpBridge)
                {
                    return wallet;
                }

                if (hasHttpBridge == hasJavaScriptBridge)
                {
                    continue;
                }
            }

            return config;
        }

        public WalletConfig GetTargetWalletConfig(string bridgeType, 
            string walletName, List<WalletConfig> wallets)
        {
            var wallet = wallets.FirstOrDefault(wallet => wallet.Name == walletName &&
                ((bridgeType == WalletConfigComponents.SSE && wallet.BridgeUrl != null) ||
                (bridgeType == WalletConfigComponents.JAVA_SCRIPT && wallet.JsBridgeKey != null)));

            return wallet;
        }

        private void OnWalletConnectionFinished(Wallet wallet)
        {
            if (UnitonConnectSDK.Instance.IsWalletConnected)
            {
                var successConnectMessage = $"Wallet is connected, full account address: {wallet.Account.Address}, \n" +
                $"Platform: {wallet.Device.Platform}, " +
                $"Name: {wallet.Device.AppName}, " +
                $"Version: {wallet.Device.AppVersion}";

                var shortWalletAddress = WalletVisualUtils.ProcessWalletAddress(
                    wallet.Account.Address.ToString(AddressType.Base64));

                _debugMessage.text = successConnectMessage;
                _shortWalletAddress.text = shortWalletAddress;

                UnitonConnectLogger.Log($"Connected wallet short address: {shortWalletAddress}");

                _connectButton.interactable = false;
                _disconnectButton.interactable = true;

                _chooseWalletPanel.Close();

                return;
            }
            else
            {
                _connectButton.interactable = true;
                _disconnectButton.interactable = false;

                _debugMessage.text = string.Empty;
                _shortWalletAddress.text = string.Empty;

                UnitonConnectLogger.LogWarning($"Connect status: {UnitonConnectSDK.Instance.IsWalletConnected}");
            }
        }

        private void OnWalletConnectionFailed(string message)
        {
            UnitonConnectLogger.LogError($"Failed to connect " +
                $"the wallet due to the following reason: {message}");
        }
    }
}