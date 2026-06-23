using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptIntelligence.Editor.Models
{
    [Serializable]
    public sealed class DependencyCycle
    {
        [SerializeField] private List<string> typePath;

        public IReadOnlyList<string> TypePath => typePath;

        public DependencyCycle(IEnumerable<string> typePath)
        {
            this.typePath = new List<string>(typePath);
        }
    }
}
