using ScriptIntelligence.Editor.Models;
using UnityEngine;

namespace ScriptIntelligence.Editor.UIToolkit.Galaxy
{
    public sealed class GalaxyNodeViewData
    {
        public string Id { get; }
        public string Title { get; }
        public string AssetPath { get; }
        public int Line { get; }
        public ScriptNodeType NodeType { get; }
        public AnalysisSeverity Severity { get; }
        public string Health { get; }
        public int Methods { get; }
        public int Fields { get; }
        public int Dependencies { get; }
        public int Issues { get; }
        public Vector2 Position { get; set; }

        public GalaxyNodeViewData(string id, string title, string assetPath, int line, ScriptNodeType nodeType, AnalysisSeverity severity, string health, int methods, int fields, int dependencies, int issues)
        {
            Id = id;
            Title = title;
            AssetPath = assetPath;
            Line = line;
            NodeType = nodeType;
            Severity = severity;
            Health = health;
            Methods = methods;
            Fields = fields;
            Dependencies = dependencies;
            Issues = issues;
        }
    }
}
