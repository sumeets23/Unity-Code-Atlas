using System;
using UnityEngine;

namespace ScriptIntelligence.Editor.Models
{
    [Serializable]
    public sealed class ScriptIssue
    {
        [SerializeField] private string ruleId;
        [SerializeField] private string title;
        [SerializeField] private string description;
        [SerializeField] private string assetPath;
        [SerializeField] private int line;
        [SerializeField] private AnalysisSeverity severity;

        public string RuleId => ruleId;
        public string Title => title;
        public string Description => description;
        public string AssetPath => assetPath;
        public int Line => line;
        public AnalysisSeverity Severity => severity;

        public ScriptIssue(string ruleId, string title, string description, string assetPath, int line, AnalysisSeverity severity)
        {
            this.ruleId = ruleId;
            this.title = title;
            this.description = description;
            this.assetPath = assetPath;
            this.line = line;
            this.severity = severity;
        }
    }
}
