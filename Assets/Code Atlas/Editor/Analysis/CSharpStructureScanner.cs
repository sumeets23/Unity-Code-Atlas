using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ScriptIntelligence.Editor.Analysis
{
    public sealed class CSharpStructureScanner
    {
        private static readonly Regex ClassRegex = new Regex(@"\b(?<abstract>abstract\s+)?(?<kind>class|interface)\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)(?:\s*:\s*(?<bases>[^{]+))?", RegexOptions.Compiled);
        private static readonly Regex MethodRegex = new Regex(@"^\s*(?:\[[^\]]+\]\s*)*(?:(?<visibility>public|private|protected|internal)\s+)?(?:static\s+|virtual\s+|override\s+|sealed\s+|async\s+)*[A-Za-z_][A-Za-z0-9_<>,\.\[\]]*\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*\(", RegexOptions.Compiled);
        private static readonly Regex FieldRegex = new Regex(@"^\s*(?:\[[^\]]+\]\s*)*(?:(?<visibility>public|private|protected|internal)\s+)?(?:readonly\s+|static\s+|const\s+)*\s*(?<type>[A-Za-z_][A-Za-z0-9_<>,\.\[\]]*)\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*(?:=|;)", RegexOptions.Compiled);

        public List<ParsedClass> FindClasses(SourceFile sourceFile)
        {
            var classes = new List<ParsedClass>();
            for (var i = 0; i < sourceFile.Lines.Length; i++)
            {
                var classMatch = ClassRegex.Match(sourceFile.Lines[i]);
                if (classMatch.Success)
                {
                    classes.Add(new ParsedClass(
                        classMatch.Groups["name"].Value,
                        sourceFile.AssetPath,
                        i + 1,
                        IsMonoBehaviourBaseList(classMatch.Groups["bases"].Value),
                        IsScriptableObjectBaseList(classMatch.Groups["bases"].Value),
                        classMatch.Groups["kind"].Value == "interface",
                        classMatch.Groups["abstract"].Success,
                        ParseBaseTypes(classMatch.Groups["bases"].Value)));
                }
            }

            return classes;
        }

        public List<ParsedMethod> FindMethods(SourceFile sourceFile)
        {
            var methods = new List<ParsedMethod>();
            var currentClass = string.Empty;

            for (var i = 0; i < sourceFile.Lines.Length; i++)
            {
                var line = sourceFile.Lines[i];
                var classMatch = ClassRegex.Match(line);
                if (classMatch.Success)
                {
                    currentClass = classMatch.Groups["name"].Value;
                }

                var methodMatch = MethodRegex.Match(line);
                if (!methodMatch.Success || string.IsNullOrEmpty(currentClass))
                {
                    continue;
                }

                if (!TryReadBlock(sourceFile.Lines, i, out var body, out var endLine))
                {
                    continue;
                }

                methods.Add(new ParsedMethod(
                    currentClass,
                    methodMatch.Groups["name"].Value,
                    NormalizeVisibility(methodMatch.Groups["visibility"].Value),
                    body,
                    i + 1,
                    endLine + 1));
                i = endLine;
            }

            return methods;
        }

        public List<ParsedField> FindFields(SourceFile sourceFile)
        {
            var fields = new List<ParsedField>();
            var currentClass = string.Empty;
            var pendingSerializeFieldAttribute = false;

            for (var i = 0; i < sourceFile.Lines.Length; i++)
            {
                var line = sourceFile.Lines[i];
                var classMatch = ClassRegex.Match(line);
                if (classMatch.Success)
                {
                    currentClass = classMatch.Groups["name"].Value;
                    continue;
                }

                if (string.IsNullOrEmpty(currentClass))
                {
                    continue;
                }

                var methodMatch = MethodRegex.Match(line);
                if (methodMatch.Success && TryReadBlock(sourceFile.Lines, i, out _, out var methodEndLine))
                {
                    i = methodEndLine;
                    pendingSerializeFieldAttribute = false;
                    continue;
                }

                if (line.Trim().StartsWith("[SerializeField]"))
                {
                    pendingSerializeFieldAttribute = true;
                }

                if (line.Contains("(") || line.TrimStart().StartsWith("using "))
                {
                    continue;
                }

                var fieldMatch = FieldRegex.Match(line);
                if (!fieldMatch.Success)
                {
                    if (!line.TrimStart().StartsWith("["))
                    {
                        pendingSerializeFieldAttribute = false;
                    }

                    continue;
                }

                var visibility = NormalizeVisibility(fieldMatch.Groups["visibility"].Value);
                var serialized = pendingSerializeFieldAttribute || line.Contains("[SerializeField]") || visibility == "public";
                fields.Add(new ParsedField(
                    currentClass,
                    CleanTypeName(fieldMatch.Groups["type"].Value),
                    fieldMatch.Groups["name"].Value,
                    visibility,
                    sourceFile.AssetPath,
                    i + 1,
                    serialized));
                pendingSerializeFieldAttribute = false;
            }

            return fields;
        }

        private static bool TryReadBlock(string[] lines, int startLine, out string body, out int endLine)
        {
            var braceDepth = 0;
            var foundOpen = false;
            var collected = new List<string>();

            for (var i = startLine; i < lines.Length; i++)
            {
                var line = lines[i];
                collected.Add(line);

                for (var c = 0; c < line.Length; c++)
                {
                    if (line[c] == '{')
                    {
                        foundOpen = true;
                        braceDepth++;
                    }
                    else if (line[c] == '}')
                    {
                        braceDepth--;
                        if (foundOpen && braceDepth <= 0)
                        {
                            body = string.Join("\n", collected);
                            endLine = i;
                            return true;
                        }
                    }
                }
            }

            body = string.Empty;
            endLine = startLine;
            return false;
        }

        private static string CleanTypeName(string value)
        {
            return value.Replace("[]", string.Empty).Trim();
        }

        private static string NormalizeVisibility(string value)
        {
            return string.IsNullOrEmpty(value) ? "private" : value;
        }

        private static bool IsMonoBehaviourBaseList(string baseList)
        {
            if (string.IsNullOrEmpty(baseList))
            {
                return false;
            }

            var normalized = baseList.Replace("UnityEngine.", string.Empty);
            return normalized.Contains("MonoBehaviour");
        }

        private static bool IsScriptableObjectBaseList(string baseList)
        {
            if (string.IsNullOrEmpty(baseList))
            {
                return false;
            }

            var normalized = baseList.Replace("UnityEngine.", string.Empty);
            return normalized.Contains("ScriptableObject");
        }

        private static IReadOnlyList<string> ParseBaseTypes(string baseList)
        {
            var types = new List<string>();
            if (string.IsNullOrEmpty(baseList))
            {
                return types;
            }

            foreach (var item in baseList.Split(','))
            {
                var clean = item.Trim()
                    .Replace("UnityEngine.", string.Empty)
                    .Replace("{", string.Empty)
                    .Trim();

                if (!string.IsNullOrEmpty(clean))
                {
                    types.Add(clean);
                }
            }

            return types;
        }
    }
}
