using UnityEngine;
using UnitonConnect.Core.Demo;

namespace UnitonConnect.Core.Data
{
    public sealed class TestCloseTransactionPanelButton : TestBaseButton
    {
        [SerializeField, Space] private TestSendTransactionPanel _panel;

        public sealed override void OnClick()
        {
            _panel.Close();
        }
    }
}