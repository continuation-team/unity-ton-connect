using System;
using UnityEngine;

namespace UnitonConnect.Core.Data
{
    [Serializable]
    public sealed class WalletsListData
    {
        [Tooltip("Redefine the URL of the wallet list source. The following format json reference is required: https://github.com/ton-connect/wallets-list (optional)")]
        public string SourceLink;
        [Tooltip("Cache lifetime in milliseconds (optional)")]
        public int CachedTimeToLive;
    }
}