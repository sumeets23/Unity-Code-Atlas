using System;
using UnityEngine;

namespace ScriptIntelligence.Editor.Models
{
    [Serializable]
    public sealed class ScriptFieldNode
    {
        [SerializeField] private string name;
        [SerializeField] private string typeName;
        [SerializeField] private ScriptMemberVisibility visibility;
        [SerializeField] private int line;
        [SerializeField] private bool serialized;
        [SerializeField] private bool projectScriptReference;

        public string Name => name;
        public string TypeName => typeName;
        public ScriptMemberVisibility Visibility => visibility;
        public int Line => line;
        public bool Serialized => serialized;
        public bool ProjectScriptReference => projectScriptReference;

        public ScriptFieldNode(string name, string typeName, ScriptMemberVisibility visibility, int line, bool serialized, bool projectScriptReference)
        {
            this.name = name;
            this.typeName = typeName;
            this.visibility = visibility;
            this.line = line;
            this.serialized = serialized;
            this.projectScriptReference = projectScriptReference;
        }
    }
}
