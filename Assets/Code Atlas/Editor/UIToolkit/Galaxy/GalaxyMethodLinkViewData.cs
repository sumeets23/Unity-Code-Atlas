namespace ScriptIntelligence.Editor.UIToolkit.Galaxy
{
    public sealed class GalaxyMethodLinkViewData
    {
        public string Label { get; }
        public string Evidence { get; }
        public string AssetPath { get; }
        public int Line { get; }

        public GalaxyMethodLinkViewData(string label, string evidence, string assetPath, int line)
        {
            Label = label;
            Evidence = evidence;
            AssetPath = assetPath;
            Line = line;
        }
    }
}
