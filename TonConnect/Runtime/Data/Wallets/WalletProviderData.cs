using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace UnitonConnect.Core.Data
{
    [Serializable]
    public sealed class WalletProviderData
    {
        [JsonProperty("app_name")]
        public string AppName { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("about_url")]
        public string AboutUrl { get; set; }

        [JsonProperty("universal_url")]
        public string UniversalUrl { get; set; }

        [JsonProperty("bridge")]
        public List<WalletBridgeData> Bridge { get; set; }

        [JsonProperty("platforms")]
        public List<string> Platforms { get; set; }

        [JsonProperty("tondns")]
        public string TonDNS { get; set; }

        [JsonProperty("deepLink")]
        public string DeepLink { get; set; }
    }
}