using System;
using UnityEngine;

namespace ScriptIntelligence.Editor.Models
{
    [Serializable]
    public sealed class ProfilerMetric
    {
        [SerializeField] private string samplerName;
        [SerializeField] private double lastMilliseconds;
        [SerializeField] private double averageMilliseconds;

        public string SamplerName => samplerName;
        public double LastMilliseconds => lastMilliseconds;
        public double AverageMilliseconds => averageMilliseconds;

        public ProfilerMetric(string samplerName, double lastMilliseconds, double averageMilliseconds)
        {
            this.samplerName = samplerName;
            this.lastMilliseconds = lastMilliseconds;
            this.averageMilliseconds = averageMilliseconds;
        }
    }
}
