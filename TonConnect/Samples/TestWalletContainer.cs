using System.Collections.Generic;
using UnityEngine;

namespace UnitonConnect.Core.Demo
{
    public sealed class TestWalletContainer : MonoBehaviour
    {
        [field: SerializeField, Space] public List<TestWalletView> Wallets { get; private set; }

        public void Init()
        {
            
        }
    }
}