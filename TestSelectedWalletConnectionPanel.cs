using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TonSdk.Connect;
using UnitonConnect.Core.Data;
using UnitonConnect.Core.Utils.View;
using UnitonConnect.Core.Utils.Debugging;

namespace UnitonConnect.Core.Demo
{
    public sealed class TestSelectedWalletConnectionPanel : TestBasePanel
    {
        [SerializeField, Space] private RawImage _qrCodeImage;
        [SerializeField, Space] private TestOpenDeepLinkWalletConnectionButton _deepLinkButton;

        private TestUIManager TestUI => TestUIManager.Instance;

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

        private async Task LoadConnectWalletContent()
        {
            _connectionUrl = await TestUI.GenerateURLForQRCodeConnection(_currentConfig);

            UnitonConnectLogger.Log($"Generated connect link {_connectionUrl} " +
                $"for wallet: {_currentConfig.Name}");

            _qrCodeForConnect = TestUI.GenerateQRCodeFromConnectURL(_connectionUrl);
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
            }

            await LoadConnectWalletContent();

            _deepLinkButton.SetListener(Connect);
            _qrCodeImage.texture = _qrCodeForConnect;
        }

        private async void Connect()
        {
            if (TestUIManager.Instance.HasHttpBridge(_currentConfig))
            {
                TestUI.OpenDeepLinkFromHttpBridgeWallet(_connectionUrl);
            }
            else if (TestUIManager.Instance.HasJSBridge(_currentConfig) &&
                UnitonConnectSDK.Instance.IsUseWebWallets)
            {
                await TestUI.ConnectWebWalletWithDeepLinkAsync(_currentConfig);
            }
        }
    }
}