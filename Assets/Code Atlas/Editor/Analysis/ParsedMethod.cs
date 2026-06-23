namespace ScriptIntelligence.Editor.Analysis
{
    public sealed class ParsedMethod
    {
        public string ClassName { get; }
        public string MethodName { get; }
        public string Visibility { get; }
        public string Body { get; }
        public int StartLine { get; }
        public int EndLine { get; }

        public ParsedMethod(string className, string methodName, string visibility, string body, int startLine, int endLine)
        {
            ClassName = className;
            MethodName = methodName;
            Visibility = visibility;
            Body = body;
            StartLine = startLine;
            EndLine = endLine;
        }
    }
}
