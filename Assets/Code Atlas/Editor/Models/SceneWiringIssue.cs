using System;
using UnityEngine;

namespace ScriptIntelligence.Editor.Models
{
    [Serializable]
    public sealed class SceneWiringIssue
    {
        [SerializeField] private string issueType;
        [SerializeField] private string scenePath;
        [SerializeField] private string objectPath;
        [SerializeField] private string componentType;
        [SerializeField] private string propertyPath;
        [SerializeField] private string description;
        [SerializeField] private string fixSuggestion;
        [SerializeField] private AnalysisSeverity severity;

        public string IssueType => issueType;
        public string ScenePath => scenePath;
        public string ObjectPath => objectPath;
        public string ComponentType => componentType;
        public string PropertyPath => propertyPath;
        public string Description => description;
        public string FixSuggestion => fixSuggestion;
        public AnalysisSeverity Severity => severity;

        public SceneWiringIssue(string issueType, string scenePath, string objectPath, string componentType, string propertyPath, string description, string fixSuggestion, AnalysisSeverity severity)
        {
            this.issueType = issueType;
            this.scenePath = scenePath;
            this.objectPath = objectPath;
            this.componentType = componentType;
            this.propertyPath = propertyPath;
            this.description = description;
            this.fixSuggestion = fixSuggestion;
            this.severity = severity;
        }
    }
}
