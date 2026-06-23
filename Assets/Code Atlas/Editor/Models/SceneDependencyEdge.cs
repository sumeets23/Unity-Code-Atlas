using System;
using UnityEngine;

namespace ScriptIntelligence.Editor.Models
{
    [Serializable]
    public sealed class SceneDependencyEdge
    {
        [SerializeField] private string fromType;
        [SerializeField] private string toType;
        [SerializeField] private string fieldPath;
        [SerializeField] private string fromObjectPath;
        [SerializeField] private string toObjectPath;
        [SerializeField] private string scenePath;

        public string FromType => fromType;
        public string ToType => toType;
        public string FieldPath => fieldPath;
        public string FromObjectPath => fromObjectPath;
        public string ToObjectPath => toObjectPath;
        public string ScenePath => scenePath;

        public SceneDependencyEdge(string fromType, string toType, string fieldPath, string fromObjectPath, string toObjectPath, string scenePath)
        {
            this.fromType = fromType;
            this.toType = toType;
            this.fieldPath = fieldPath;
            this.fromObjectPath = fromObjectPath;
            this.toObjectPath = toObjectPath;
            this.scenePath = scenePath;
        }
    }
}
