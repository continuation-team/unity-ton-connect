using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TonSdk.Connect;
using UnitonConnect.Core.Utils.View;
using UnitonConnect.Core.Data.Common;
using UnitonConnect.Core.Utils;

namespace UnitonConnect.Core.Demo
{
    public sealed class TestWalletView : MonoBehaviour
    {
        [SerializeField, Space] private TextMeshProUGUI _header;
        [SerializeField] private Image _icon;
        [SerializeField] private Button _connectButton;

        private TestSelectedWalletConnectionPanel _connectPanel;

        private WalletConfig _javascriptConfig;
        private WalletConfig _httpConfig;

        private WalletConfig _targetConfig;

        private string _name;

        public void SetView(string appName, Texture2D icon,
            TestSelectedWalletConnectionPanel connectPanel)
        {
            var testUIManager = TestWalletInterfaceAdapter.Instance;

            _name = appName;

            _header.text = _name;
            _icon.sprite = WalletVisualUtils.GetSpriteFromTexture(icon);

            _connectPanel = connectPanel;

            _targetConfig = WalletConnectUtils.GetTargetWalletConfigWithoutSecondBridge(
                WalletConfigComponents.SSE, _name, testUIManager.LoadedWallets);

            StartConnect();
        }

        private void StartConnect()
        {
            var testUIManager = TestWalletInterfaceAdapter.Instance;
            var loadedWallets = testUIManager.LoadedWallets;

            _connectButton.onClick.AddListener(() =>
            {
                if (WalletConnectUtils.HasMultipleBridgeTypes(_name, loadedWallets))
                {
                    _javascriptConfig = WalletConnectUtils.GetTargetWalletConfigWithoutSecondBridge(
                        WalletConfigComponents.JAVA_SCRIPT, _name, loadedWallets);
                    _httpConfig = WalletConnectUtils.GetTargetWalletConfigWithoutSecondBridge(
                        WalletConfigComponents.SSE, _name, loadedWallets);
                }

                if (UnitonConnectSDK.Instance.IsUseWebWallets &&
                    WalletConnectUtils.HasJSBridge(_targetConfig))
                {
                    _targetConfig = _javascriptConfig;
                }
                else if (!UnitonConnectSDK.Instance.IsUseWebWallets &&
                    WalletConnectUtils.HasHttpBridge(_targetConfig))
                {
                    _targetConfig = _httpConfig;
                }

                _connectPanel.SetOptions(_targetConfig);
                _connectPanel.Open();
            });
        }
    }
}