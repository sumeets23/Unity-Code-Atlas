using System;
using UnityEngine;

namespace ScriptIntelligence.Editor.Models
{
    [Serializable]
    public sealed class RuntimeNodeActivity
    {
        [SerializeField] private string typeName;
        [SerializeField] private int executionCount;
        [SerializeField] private double lastExecutedAt;
        [SerializeField] private double frequencyPerSecond;

        public string TypeName => typeName;
        public int ExecutionCount => executionCount;
        public double LastExecutedAt => lastExecutedAt;
        public double FrequencyPerSecond => frequencyPerSecond;

        public RuntimeNodeActivity(string typeName, int executionCount, double lastExecutedAt, double frequencyPerSecond)
        {
            this.typeName = typeName;
            this.executionCount = executionCount;
            this.lastExecutedAt = lastExecutedAt;
            this.frequencyPerSecond = frequencyPerSecond;
        }
    }
}
