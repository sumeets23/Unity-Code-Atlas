using System;
using UnityEngine;

namespace ScriptIntelligence.Editor.Models
{
    [Serializable]
    public sealed class GraphExportEdge
    {
        [SerializeField] private string from;
        [SerializeField] private string to;
        [SerializeField] private ScriptRelationshipType relationshipType;
        [SerializeField] private int weight;
        [SerializeField] private AnalysisSeverity severity;

        public string From => from;
        public string To => to;
        public ScriptRelationshipType RelationshipType => relationshipType;
        public int Weight => weight;
        public AnalysisSeverity Severity => severity;

        public GraphExportEdge(string from, string to, ScriptRelationshipType relationshipType, int weight, AnalysisSeverity severity)
        {
            this.from = from;
            this.to = to;
            this.relationshipType = relationshipType;
            this.weight = weight;
            this.severity = severity;
        }
    }
}
