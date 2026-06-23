using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptIntelligence.Editor.Models
{
    [Serializable]
    public sealed class TechnicalDebtReport
    {
        [SerializeField] private int debtScore;
        [SerializeField] private List<AnalysisCategoryScore> categories = new List<AnalysisCategoryScore>();
        [SerializeField] private List<string> recommendations = new List<string>();

        public int DebtScore => debtScore;
        public List<AnalysisCategoryScore> Categories => categories;
        public List<string> Recommendations => recommendations;

        public TechnicalDebtReport()
        {
        }

        public TechnicalDebtReport(int debtScore, IEnumerable<AnalysisCategoryScore> categories, IEnumerable<string> recommendations)
        {
            this.debtScore = Mathf.Clamp(debtScore, 0, 100);
            this.categories.AddRange(categories);
            this.recommendations.AddRange(recommendations);
        }
    }
}
