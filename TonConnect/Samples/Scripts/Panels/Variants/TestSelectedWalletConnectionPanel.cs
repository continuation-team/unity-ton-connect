using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TonSdk.Connect;
using UnitonConnect.Core.Data;
using UnitonConnect.Core.Utils;
using UnitonConnect.Core.Utils.View;
using UnitonConnect.Core.Utils.Debugging;
using System;

namespace UnitonConnect.Core.Demo
{
    public sealed class TestSelectedWalletConnectionPanel : TestBasePanel
    {
        [SerializeField, Space] private RawImage _qrCodeImage;
        [SerializeField, Space] private TestOpenDeepLinkWalletConnectionButton _deepLinkButton;

        private TestWalletInterfaceAdapter TestUI => TestWalletInterfaceAdapter.Instance;

        private WalletConfig _currentConfig;

        private Texture2D _qrCodeForConnect;

        private string _connectionUrl;

        private void OnEnable()
        {
            UnitonConnectSDK.Instance.OnWalletConnectionFinished += WalletConnectionFinished;
        }

        private void OnDisable()
        {
            UnitonConnectSDK.Instance.OnWalletConnectionFinished -= WalletConnectionFinished;

            _deepLinkButton.RemoveListeners();
        }

        private async void LoadConnectWalletContent()
        {
            _connectionUrl = await UnitonConnectSDK.Instance.GenerateConnectURL(_currentConfig);

            UnitonConnectLogger.Log($"Generated connect link {_connectionUrl} " +
                $"for wallet: {_currentConfig.Name}");

            _qrCodeForConnect = WalletVisualUtils.GetQRCodeFromUrl(_connectionUrl);

            _deepLinkButton.SetListener(Connect);

            _qrCodeImage.texture = _qrCodeForConnect;
        }

        private void WalletConnectionFinished(Wallet wallet)
        {
            Close();
        }

        public async void SetOptions(WalletConfig connectionConfig)
        {
            _currentConfig = connectionConfig;

            if (UnitonConnectSDK.Instance.IsWalletConnected)
            {
                Debug.LogWarning($"The wallet named {connectionConfig.Name} is already connected, the process of disconnecting it from the session begins");

                await UnitonConnectSDK.Instance.DisconnectWallet();

                return;
            }

            LoadConnectWalletContent();
        }

        private void Connect()
        {
            if (WalletConnectUtils.HasHttpBridge(_currentConfig))
            {
                UnitonConnectSDK.Instance.ConnectHttpBridgeWalletViaDeepLink(
                    _currentConfig, _connectionUrl);
            }
            else if (WalletConnectUtils.HasJSBridge(_currentConfig) &&
                UnitonConnectSDK.Instance.IsUseWebWallets)
            {
                UnitonConnectSDK.Instance.ConnectJavaScriptBridgeWalletViaDeeplink(_currentConfig);
            }
        }
    }
}