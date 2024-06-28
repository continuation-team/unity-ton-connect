using UnityEngine;
using UnitonConnect.Core.Demo;

namespace UnitonConnect.Core.Data
{
    public sealed class TestCloseChooseWalletButton : TestBaseButton
    {
        [SerializeField, Space] private TestChooseWalletPanel _panel;

        public sealed override void OnClick()
        {
            UnitonConnectSDK.Instance.PauseConnection();

            _panel.Close();
        }
    }
}
