using UnityEngine;
using TMPro;

namespace UnitonConnect.Core.Demo
{
    public sealed class TestSendTransactionPanel : TestBasePanel
    {
        [SerializeField, Space] private TextMeshProUGUI _amountBar;
        [SerializeField] private TestWalletAddressBarView _targetWalletAddress;

        private const string CREATOR_WALLET_ADDRESS =
            "UQDPwEk-cnQXEfFaaNVXywpbKACUMwVRupkgWjhr_f4Ursw6";

        private const float START_TON_AMOUNT = 0.01f;

        public void Init()
        {
            SetAmountBar(START_TON_AMOUNT);
            SetTargetAddress(CREATOR_WALLET_ADDRESS);
        }

        private void SetAmountBar(float amount)
        {
            _amountBar.text = $"{amount} Ton";
        }

        private void SetTargetAddress(string address)
        {
            _targetWalletAddress.Set(address);
        }
    }
}