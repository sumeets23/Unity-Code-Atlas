namespace ScriptIntelligence.Editor.Analysis
{
    public sealed class ParsedField
    {
        public string DeclaringType { get; }
        public string FieldType { get; }
        public string FieldName { get; }
        public string Visibility { get; }
        public string AssetPath { get; }
        public int Line { get; }
        public bool Serialized { get; }

        public ParsedField(string declaringType, string fieldType, string fieldName, string visibility, string assetPath, int line, bool serialized)
        {
            DeclaringType = declaringType;
            FieldType = fieldType;
            FieldName = fieldName;
            Visibility = visibility;
            AssetPath = assetPath;
            Line = line;
            Serialized = serialized;
        }
    }
}
