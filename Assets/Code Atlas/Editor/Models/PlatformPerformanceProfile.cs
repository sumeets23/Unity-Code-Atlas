using System;
using UnityEngine;

namespace ScriptIntelligence.Editor.Models
{
    [Serializable]
    public sealed class PlatformPerformanceProfile
    {
        [SerializeField] private string name;
        [SerializeField] private int warningThreshold;
        [SerializeField] private int criticalThreshold;
        [SerializeField] private string guidance;

        public string Name => name;
        public int WarningThreshold => warningThreshold;
        public int CriticalThreshold => criticalThreshold;
        public string Guidance => guidance;

        public PlatformPerformanceProfile(string name, int warningThreshold, int criticalThreshold, string guidance)
        {
            this.name = name;
            this.warningThreshold = warningThreshold;
            this.criticalThreshold = criticalThreshold;
            this.guidance = guidance;
        }
    }
}
