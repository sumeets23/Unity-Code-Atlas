using System;
using UnityEngine;

namespace ScriptIntelligence.Editor.Models
{
    [Serializable]
    public sealed class ArchitectureAssistantRecommendation
    {
        [SerializeField] private string question;
        [SerializeField] private string answer;
        [SerializeField] private AnalysisSeverity severity;

        public string Question => question;
        public string Answer => answer;
        public AnalysisSeverity Severity => severity;

        public ArchitectureAssistantRecommendation(string question, string answer, AnalysisSeverity severity)
        {
            this.question = question;
            this.answer = answer;
            this.severity = severity;
        }
    }
}
