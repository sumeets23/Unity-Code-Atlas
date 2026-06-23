using System.Collections.Generic;
using ScriptIntelligence.Editor.Models;

namespace ScriptIntelligence.Editor.Analysis
{
    public sealed class ScriptIntelligenceAnalysisContext
    {
        public IReadOnlyList<SourceFile> SourceFiles { get; }
        public IReadOnlyList<ParsedClass> Classes { get; }
        public IReadOnlyList<ParsedMethod> Methods { get; }
        public IReadOnlyList<ParsedField> Fields { get; }

        public ScriptIntelligenceAnalysisContext(
            IReadOnlyList<SourceFile> sourceFiles,
            IReadOnlyList<ParsedClass> classes,
            IReadOnlyList<ParsedMethod> methods,
            IReadOnlyList<ParsedField> fields)
        {
            SourceFiles = sourceFiles;
            Classes = classes;
            Methods = methods;
            Fields = fields;
        }
    }
}
