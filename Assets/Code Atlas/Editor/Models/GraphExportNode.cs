using System;
using UnityEngine;

namespace ScriptIntelligence.Editor.Models
{
    [Serializable]
    public sealed class GraphExportNode
    {
        [SerializeField] private string id;
        [SerializeField] private string label;
        [SerializeField] private ScriptNodeType nodeType;
        [SerializeField] private string health;
        [SerializeField] private int complexity;
        [SerializeField] private int coupling;
        [SerializeField] private int issues;
        [SerializeField] private string assetPath;

        public string Id => id;
        public string Label => label;
        public ScriptNodeType NodeType => nodeType;
        public string Health => health;
        public int Complexity => complexity;
        public int Coupling => coupling;
        public int Issues => issues;
        public string AssetPath => assetPath;

        public GraphExportNode(string id, string label, ScriptNodeType nodeType, string health, int complexity, int coupling, int issues, string assetPath)
        {
            this.id = id;
            this.label = label;
            this.nodeType = nodeType;
            this.health = health;
            this.complexity = complexity;
            this.coupling = coupling;
            this.issues = issues;
            this.assetPath = assetPath;
        }
    }
}
