using UnityEngine;
using UnitonConnect.Editor.Common;

namespace UnitonConnect.Core.Data
{
    [CreateAssetMenu(fileName = ProjectStorageConsts.WALLET_PROVIDER_CONFIG_NAME,
        menuName = ProjectStorageConsts.CREATE_PATH_WALLET_PROVIDER)]
    public sealed class WalletProviderConfig : ScriptableObject
    {
        [field: SerializeField, Space] public WalletViewData Data { get; private set; }
    }
}