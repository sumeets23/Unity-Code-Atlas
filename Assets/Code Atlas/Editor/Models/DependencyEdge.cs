using System;
using UnityEngine;

namespace ScriptIntelligence.Editor.Models
{
    [Serializable]
    public sealed class DependencyEdge
    {
        [SerializeField] private string fromType;
        [SerializeField] private string toType;
        [SerializeField] private string fieldName;
        [SerializeField] private string assetPath;
        [SerializeField] private int line;

        public string FromType => fromType;
        public string ToType => toType;
        public string FieldName => fieldName;
        public string AssetPath => assetPath;
        public int Line => line;

        public DependencyEdge(string fromType, string toType, string fieldName, string assetPath, int line)
        {
            this.fromType = fromType;
            this.toType = toType;
            this.fieldName = fieldName;
            this.assetPath = assetPath;
            this.line = line;
        }
    }
}
