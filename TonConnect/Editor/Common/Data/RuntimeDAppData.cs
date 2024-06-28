using System;
using Newtonsoft.Json;

namespace UnitonConnect.Runtime.Data
{
    [Serializable]
    public sealed class RuntimeDAppData
    {
        [JsonProperty("url")]
        public string ProjectLink { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("iconUrl")]
        public string Icon { get; set; }
    }
}