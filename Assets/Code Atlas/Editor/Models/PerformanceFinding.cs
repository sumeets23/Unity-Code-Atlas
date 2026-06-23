using System;
using UnityEngine;

namespace ScriptIntelligence.Editor.Models
{
    [Serializable]
    public sealed class PerformanceFinding
    {
        [SerializeField] private string category;
        [SerializeField] private string title;
        [SerializeField] private string recommendation;
        [SerializeField] private string platformProfile;
        [SerializeField] private AnalysisSeverity severity;

        public string Category => category;
        public string Title => title;
        public string Recommendation => recommendation;
        public string PlatformProfile => platformProfile;
        public AnalysisSeverity Severity => severity;

        public PerformanceFinding(string category, string title, string recommendation, string platformProfile, AnalysisSeverity severity)
        {
            this.category = category;
            this.title = title;
            this.recommendation = recommendation;
            this.platformProfile = platformProfile;
            this.severity = severity;
        }
    }
}
