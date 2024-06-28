using System;
using UnityEngine;

namespace UnitonConnect.Runtime.Data
{
    [Serializable]
    public sealed class DAppData
    {
        [field: SerializeField, Space] public string ProjectLink { get; set; }
        [field: SerializeField] public string Name { get; set; }
        [field: SerializeField, Space] public Texture2D Icon { get; set; }
    }
}