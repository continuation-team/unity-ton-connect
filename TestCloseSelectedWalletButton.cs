using UnityEngine;
using UnitonConnect.Core.Demo;

namespace UnitonConnect.Core.Data
{
    public sealed class TestCloseSelectedWalletButton : TestBaseButton
    {
        [SerializeField, Space] private TestSelectedWalletConnectionPanel _panel;

        public sealed override void OnClick()
        {
            UnitonConnectSDK.Instance.PauseConnection();

            _panel.Close();
        }
    }
}