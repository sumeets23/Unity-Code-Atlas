using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptIntelligence.Editor.Models
{
    [Serializable]
    public sealed class SceneHealthReport
    {
        [SerializeField] private int score;
        [SerializeField] private int sceneScriptCount;
        [SerializeField] private int sceneReferenceCount;
        [SerializeField] private List<string> findings = new List<string>();
        [SerializeField] private List<string> fixSuggestions = new List<string>();

        public int Score => score;
        public int SceneScriptCount => sceneScriptCount;
        public int SceneReferenceCount => sceneReferenceCount;
        public List<string> Findings => findings;
        public List<string> FixSuggestions => fixSuggestions;

        public SceneHealthReport()
        {
            score = 100;
        }

        public SceneHealthReport(int score, int sceneScriptCount, int sceneReferenceCount, IEnumerable<string> findings, IEnumerable<string> fixSuggestions)
        {
            this.score = Mathf.Clamp(score, 0, 100);
            this.sceneScriptCount = sceneScriptCount;
            this.sceneReferenceCount = sceneReferenceCount;
            this.findings.AddRange(findings);
            this.fixSuggestions.AddRange(fixSuggestions);
        }
    }
}
