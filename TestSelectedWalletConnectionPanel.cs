using UnityEngine;
using UnityEngine.UI;
using TonSdk.Connect;
using UnitonConnect.Core.Data;
using UnitonConnect.Core.Utils.View;

namespace UnitonConnect.Core.Demo
{
    public sealed class TestSelectedWalletConnectionPanel : TestBasePanel
    {
        [SerializeField, Space] private Image _qrCodeImage;
        [SerializeField, Space] private TestOpenDeepLinkWalletConnectionButton _deepLinkButton;

        private TestUIManager TestUI => TestUIManager.Instance;

        private WalletConfig _currentConfig;

        private Sprite _qrCodeForConnect;

        private string _connectionUrl;

        private async void OnEnable()
        {
            if (_currentConfig.Equals(null))
            {
                return;
            }

            await TestUI.ConnectWalletWithQRCodeAsync(_currentConfig,
                (qrCode, connectionUrl) =>
                {
                    _qrCodeForConnect = WalletVisualUtils.GetSpriteFromTexture(qrCode);
                    _connectionUrl = connectionUrl;
                });

            UnitonConnectSDK.Instance.OnWalletConnectionFinished += WalletConnectionFinished;
        }

        private void OnDisable()
        {
            _deepLinkButton.RemoveListeners();
        }

        private void WalletConnectionFinished(Wallet wallet)
        {
            Close();
        }

        public void SetOptions(WalletConfig connectionConfig)
        {
            _currentConfig = connectionConfig;

            _deepLinkButton.SetListener(Connect);

            _qrCodeImage.sprite = _qrCodeForConnect;
        }

        private async void Connect()
        {
            if (TestUIManager.Instance.HasHttpBridge(_currentConfig))
            {
                TestUI.OpenDeepLinkFromHttpBridgeWallet(_connectionUrl);
            }
            else if (UnitonConnectSDK.Instance.IsUseWebWallets)
            {
                await TestUI.ConnectWebWalletWithDeepLinkAsync(_currentConfig);
            }
        }
    }
}