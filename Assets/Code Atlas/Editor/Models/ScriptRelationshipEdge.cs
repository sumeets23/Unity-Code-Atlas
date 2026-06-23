using System;
using UnityEngine;

namespace ScriptIntelligence.Editor.Models
{
    [Serializable]
    public sealed class ScriptRelationshipEdge
    {
        [SerializeField] private string fromType;
        [SerializeField] private string toType;
        [SerializeField] private ScriptRelationshipType relationshipType;
        [SerializeField] private string memberName;
        [SerializeField] private string methodName;
        [SerializeField] private string assetPath;
        [SerializeField] private int line;
        [SerializeField] private string evidence;

        public string FromType => fromType;
        public string ToType => toType;
        public ScriptRelationshipType RelationshipType => relationshipType;
        public string MemberName => memberName;
        public string MethodName => methodName;
        public string AssetPath => assetPath;
        public int Line => line;
        public string Evidence => evidence;

        public ScriptRelationshipEdge(string fromType, string toType, ScriptRelationshipType relationshipType, string memberName, string methodName, string assetPath, int line, string evidence)
        {
            this.fromType = fromType;
            this.toType = toType;
            this.relationshipType = relationshipType;
            this.memberName = memberName;
            this.methodName = methodName;
            this.assetPath = assetPath;
            this.line = line;
            this.evidence = evidence;
        }
    }
}
