using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptIntelligence.Editor.Models
{
    [Serializable]
    public sealed class ArchitectureHealthReport
    {
        [SerializeField] private int overallScore;
        [SerializeField] private ArchitectureScoreGrade grade;
        [SerializeField] private List<AnalysisCategoryScore> categories = new List<AnalysisCategoryScore>();

        public int OverallScore => overallScore;
        public ArchitectureScoreGrade Grade => grade;
        public List<AnalysisCategoryScore> Categories => categories;

        public ArchitectureHealthReport()
        {
            overallScore = 100;
            grade = ArchitectureScoreGrade.APlus;
        }

        public ArchitectureHealthReport(int overallScore, ArchitectureScoreGrade grade, IEnumerable<AnalysisCategoryScore> categories)
        {
            this.overallScore = Mathf.Clamp(overallScore, 0, 100);
            this.grade = grade;
            this.categories.AddRange(categories);
        }
    }
}
