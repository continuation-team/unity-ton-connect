using UnityEngine;
using TMPro;
using UnitonConnect.Core.Utils.View;

namespace UnitonConnect.Core.Demo
{
    public sealed class TestWalletAddressBarView : MonoBehaviour
    {
        [SerializeField, Space] private TextMeshProUGUI _addressBar;

        public string ShortAddress { get; private set; }
        public string FullAddress { get; private set; }

        public void Set(string address)
        {
            FullAddress = address;
            ShortAddress = WalletVisualUtils.ProcessWalletAddress(FullAddress, 6);

            _addressBar.text = ShortAddress;
        }
    }
}