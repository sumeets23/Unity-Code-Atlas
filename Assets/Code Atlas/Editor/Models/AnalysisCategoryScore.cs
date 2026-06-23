using System;
using UnityEngine;

namespace ScriptIntelligence.Editor.Models
{
    [Serializable]
    public sealed class AnalysisCategoryScore
    {
        [SerializeField] private string category;
        [SerializeField] private int score;
        [SerializeField] private string summary;

        public string Category => category;
        public int Score => score;
        public string Summary => summary;

        public AnalysisCategoryScore(string category, int score, string summary)
        {
            this.category = category;
            this.score = Mathf.Clamp(score, 0, 100);
            this.summary = summary;
        }
    }
}
