using System.Collections.Generic;
using UnityEngine;
using UnitonConnect.Editor.Common;

namespace UnitonConnect.Core.Data
{
    [CreateAssetMenu(fileName = ProjectStorageConsts.WALLETS_PROVIDERS_STORAGE_NAME, 
        menuName = ProjectStorageConsts.CREATE_PATH_WALLETS_PROVIDERS_STORAGE)]
    public sealed class WalletsProvidersData : ScriptableObject
    {
        [field: SerializeField, Space] public List<WalletProviderConfig> Config { get; private set; }
    }
}