using System;
using UnityEngine;

namespace ScriptIntelligence.Editor.Models
{
    [Serializable]
    public sealed class ScriptMethodNode
    {
        [SerializeField] private string name;
        [SerializeField] private ScriptMemberVisibility visibility;
        [SerializeField] private int startLine;
        [SerializeField] private int endLine;
        [SerializeField] private bool unityMessage;
        [SerializeField] private string bodyText;

        public string Name => name;
        public ScriptMemberVisibility Visibility => visibility;
        public int StartLine => startLine;
        public int EndLine => endLine;
        public bool UnityMessage => unityMessage;
        public string BodyText => bodyText;

        public ScriptMethodNode(string name, ScriptMemberVisibility visibility, int startLine, int endLine, bool unityMessage, string bodyText = "")
        {
            this.name = name;
            this.visibility = visibility;
            this.startLine = startLine;
            this.endLine = endLine;
            this.unityMessage = unityMessage;
            this.bodyText = bodyText;
        }
    }
}
