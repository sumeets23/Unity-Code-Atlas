using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptIntelligence.Editor.Models
{
    [Serializable]
    public sealed class ArchitectureAssistantResult
    {
        [SerializeField] private List<ArchitectureAssistantRecommendation> recommendations = new List<ArchitectureAssistantRecommendation>();

        public List<ArchitectureAssistantRecommendation> Recommendations => recommendations;

        public ArchitectureAssistantResult()
        {
        }

        public ArchitectureAssistantResult(IEnumerable<ArchitectureAssistantRecommendation> recommendations)
        {
            this.recommendations.AddRange(recommendations);
        }
    }
}
