using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptIntelligence.Editor.Models
{
    [Serializable]
    public sealed class ScriptGraphNode
    {
        [SerializeField] private string className;
        [SerializeField] private string assetPath;
        [SerializeField] private int line;
        [SerializeField] private ScriptNodeType nodeType;
        [SerializeField] private List<ScriptMethodNode> methods = new List<ScriptMethodNode>();
        [SerializeField] private List<ScriptFieldNode> fields = new List<ScriptFieldNode>();

        public string ClassName => className;
        public string AssetPath => assetPath;
        public int Line => line;
        public ScriptNodeType NodeType => nodeType;
        public List<ScriptMethodNode> Methods => methods;
        public List<ScriptFieldNode> Fields => fields;

        public ScriptGraphNode(string className, string assetPath, int line)
            : this(className, assetPath, line, ScriptNodeType.MonoBehaviour)
        {
        }

        public ScriptGraphNode(string className, string assetPath, int line, ScriptNodeType nodeType)
        {
            this.className = className;
            this.assetPath = assetPath;
            this.line = line;
            this.nodeType = nodeType;
        }
    }
}
