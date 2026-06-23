using System;
using System.Collections.Generic;
using System.Linq;
using ScriptIntelligence.Editor.Models;
using UnityEngine.UIElements;

namespace ScriptIntelligence.Editor.UIToolkit.Views
{
    public sealed class ProjectExplorerView
    {
        private readonly VisualElement root;
        private readonly ScrollView list;
        private ScriptIntelligenceReport report = new ScriptIntelligenceReport();
        private string selectedScript;
        private string query = string.Empty;

        public event Action<string> ScriptSelected;

        public ProjectExplorerView(VisualElement root)
        {
            this.root = root;
            root.Clear();
            root.Add(Label("Scene Scripts", "panel-title"));
            root.Add(Label("Scripts attached in the open scene.", "panel-subtitle"));

            list = new ScrollView();
            list.style.flexGrow = 1f;
            root.Add(list);
        }

        public void SetSearch(string value)
        {
            query = value ?? string.Empty;
            Refresh();
        }

        public void Bind(ScriptIntelligenceReport value, string selected)
        {
            report = value ?? new ScriptIntelligenceReport();
            selectedScript = selected;
            Refresh();
        }

        private void Refresh()
        {
            list.Clear();
            foreach (var node in BuildNodes())
            {
                if (!Matches(node.ClassName))
                {
                    continue;
                }

                var incoming = report.ScriptRelationshipEdges.Count(edge => edge.ToType == node.ClassName);
                var outgoing = report.ScriptRelationshipEdges.Count(edge => edge.FromType == node.ClassName);
                var card = new Button(() => ScriptSelected?.Invoke(node.ClassName));
                card.AddToClassList("script-card");
                card.EnableInClassList("selected", selectedScript == node.ClassName);
                card.Add(Label(node.ClassName, "script-title"));
                card.Add(Label(NodeTypeLabel(node.NodeType), "meta-text"));
                card.Add(Label("In " + incoming + "  /  Out " + outgoing, "meta-text"));
                list.Add(card);
            }
        }

        private IEnumerable<ScriptGraphNode> BuildNodes()
        {
            return report.ScriptGraphNodes
                .OrderBy(node => SortOrder(node.NodeType))
                .ThenBy(node => node.ClassName);
        }

        private bool Matches(string scriptName)
        {
            return string.IsNullOrEmpty(query) || scriptName.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static int SortOrder(ScriptNodeType nodeType)
        {
            switch (nodeType)
            {
                case ScriptNodeType.MonoBehaviour:
                    return 0;
                case ScriptNodeType.ScriptableObject:
                    return 1;
                case ScriptNodeType.Manager:
                case ScriptNodeType.Singleton:
                    return 2;
                case ScriptNodeType.Interface:
                case ScriptNodeType.AbstractClass:
                    return 3;
                default:
                    return 4;
            }
        }

        private static string NodeTypeLabel(ScriptNodeType nodeType)
        {
            switch (nodeType)
            {
                case ScriptNodeType.ScriptableObject:
                    return "ScriptableObject";
                case ScriptNodeType.Interface:
                    return "Interface";
                case ScriptNodeType.AbstractClass:
                    return "Abstract class";
                case ScriptNodeType.Manager:
                    return "Manager";
                case ScriptNodeType.Singleton:
                    return "Singleton";
                case ScriptNodeType.MonoBehaviour:
                    return "MonoBehaviour";
                default:
                    return "Class";
            }
        }

        private static Label Label(string text, string className)
        {
            var label = new Label(text);
            label.AddToClassList(className);
            return label;
        }
    }
}
