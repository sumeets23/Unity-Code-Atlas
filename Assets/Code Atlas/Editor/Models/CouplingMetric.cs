using System;
using UnityEngine;

namespace ScriptIntelligence.Editor.Models
{
    [Serializable]
    public sealed class CouplingMetric
    {
        [SerializeField] private string typeName;
        [SerializeField] private int fanIn;
        [SerializeField] private int fanOut;
        [SerializeField] private int dependencyCount;
        [SerializeField] private int referenceCount;
        [SerializeField] private AnalysisSeverity severity;
        [SerializeField] private string classification;

        public string TypeName => typeName;
        public int FanIn => fanIn;
        public int FanOut => fanOut;
        public int DependencyCount => dependencyCount;
        public int ReferenceCount => referenceCount;
        public AnalysisSeverity Severity => severity;
        public string Classification => classification;

        public CouplingMetric(string typeName, int fanIn, int fanOut, int dependencyCount, int referenceCount, AnalysisSeverity severity, string classification)
        {
            this.typeName = typeName;
            this.fanIn = fanIn;
            this.fanOut = fanOut;
            this.dependencyCount = dependencyCount;
            this.referenceCount = referenceCount;
            this.severity = severity;
            this.classification = classification;
        }
    }
}
