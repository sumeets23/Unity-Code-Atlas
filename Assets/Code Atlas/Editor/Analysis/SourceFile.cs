using System;

namespace ScriptIntelligence.Editor.Analysis
{
    public sealed class SourceFile
    {
        public string AssetPath { get; }
        public string Text { get; }
        public string[] Lines { get; }

        public SourceFile(string assetPath, string text)
        {
            AssetPath = assetPath;
            Text = text;
            Lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        }
    }
}
