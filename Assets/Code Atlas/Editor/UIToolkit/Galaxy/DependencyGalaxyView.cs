using System;
using System.Collections.Generic;
using System.Linq;
using ScriptIntelligence.Editor.Models;
using ScriptIntelligence.Editor.UIToolkit.Theme;
using ScriptIntelligence.Editor.Utilities;
using UnityEngine;
using UnityEngine.UIElements;

namespace ScriptIntelligence.Editor.UIToolkit.Galaxy
{
    public sealed class DependencyGalaxyView : VisualElement
    {
        private readonly GalaxyLayoutEngine layoutEngine = new GalaxyLayoutEngine();
        private readonly Dictionary<string, VisualElement> nodeElements = new Dictionary<string, VisualElement>();
        private readonly Dictionary<string, VisualElement> methodDotElements = new Dictionary<string, VisualElement>();
        private readonly List<GalaxyNodeViewData> nodes = new List<GalaxyNodeViewData>();
        private readonly List<GalaxyEdgeViewData> edges = new List<GalaxyEdgeViewData>();
        private VisualElement methodPreview;
        private Label methodPreviewTitle;
        private Label methodPreviewMeta;
        private Label methodPreviewEvidence;
        private string selectedNode;
        private string hoveredMethodDotKey;
        private Vector2 lastLayoutSize = Vector2.negativeInfinity;
        private Vector2 pan;
        private Vector2 lastPointerPosition;
        private bool dragging;
        private float zoom = 1f;
        private bool layoutDirty = true;

        public event Action<string> NodeSelected;

        public DependencyGalaxyView()
        {
            AddToClassList("galaxy-view");
            generateVisualContent += DrawGalaxy;
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            RegisterCallback<WheelEvent>(OnWheel);
            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
            BuildMethodPreview();
        }

        public void SetData(ScriptIntelligenceReport report, string selected)
        {
            selectedNode = selected;
            nodes.Clear();
            edges.Clear();

            foreach (var graphNode in report.ScriptGraphNodes)
            {
                nodes.Add(new GalaxyNodeViewData(
                    graphNode.ClassName,
                    graphNode.ClassName,
                    graphNode.AssetPath,
                    graphNode.Line,
                    graphNode.NodeType,
                    AnalysisSeverity.Info,
                    string.Empty,
                    graphNode.Methods.Count,
                    graphNode.Fields.Count,
                    report.ScriptRelationshipEdges.Count(edge => edge.FromType == graphNode.ClassName || edge.ToType == graphNode.ClassName),
                    0));
            }

            foreach (var group in report.ScriptRelationshipEdges.GroupBy(edge => edge.FromType + "|" + edge.ToType))
            {
                var first = group.First();
                var methodLinks = group
                    .Select(MethodLink)
                    .Where(link => link != null && !string.IsNullOrEmpty(link.Label))
                    .GroupBy(link => link.Label + "|" + link.AssetPath)
                    .Select(MergeMethodLinks)
                    .Where(link => link != null)
                    .OrderBy(link => link.Label)
                    .ToList();
                edges.Add(new GalaxyEdgeViewData(first.FromType, first.ToType, first.RelationshipType, group.Count(), methodLinks));
            }

            MarkLayoutDirty();
            SyncNodeElements();
        }

        public void SelectNode(string nodeId)
        {
            selectedNode = nodeId;
            MarkLayoutDirty();
            SyncNodeElements();
        }

        private void MarkLayoutDirty()
        {
            layoutDirty = true;
            MarkDirtyRepaint();
            schedule.Execute(SyncNodeElements).StartingIn(0);
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            var newSize = evt.newRect.size;
            var oldSize = evt.oldRect.size;
            if (Mathf.Approximately(newSize.x, oldSize.x) && Mathf.Approximately(newSize.y, oldSize.y))
            {
                return;
            }

            if (Mathf.Approximately(newSize.x, lastLayoutSize.x) && Mathf.Approximately(newSize.y, lastLayoutSize.y))
            {
                return;
            }

            lastLayoutSize = newSize;
            MarkLayoutDirty();
        }

        private void EnsureLayout()
        {
            if (!layoutDirty)
            {
                return;
            }

            layoutEngine.Layout(nodes, edges, contentRect, selectedNode);
            layoutDirty = false;
        }

        private void SyncNodeElements()
        {
            EnsureLayout();
            var active = new HashSet<string>();
            var connected = BuildConnectedSet();

            foreach (var node in nodes)
            {
                active.Add(node.Id);
                if (!nodeElements.TryGetValue(node.Id, out var element))
                {
                    element = CreateNodeElement(node);
                    nodeElements.Add(node.Id, element);
                    Add(element);
                }

                var selected = node.Id == selectedNode;
                element.EnableInClassList("selected", selected);
                element.EnableInClassList("faded", !selected && connected.Count > 0 && !connected.Contains(node.Id));
                element.style.left = pan.x + node.Position.x * zoom - 95f;
                element.style.top = pan.y + node.Position.y * zoom - 38f;
                element.style.width = Mathf.Clamp(176f + node.Dependencies * 2f, 176f, 232f);
                element.style.borderLeftColor = selected ? StudioThemeManager.Healthy : StudioThemeManager.Isolated;

                var title = element.Q<Label>("node-title");
                var meta = element.Q<Label>("node-meta");
                var footer = element.Q<Label>("node-footer-text");
                title.text = node.Title;
                meta.text = NodeTypeLabel(node.NodeType);
                footer.text = node.Dependencies + " connections  /  " + node.Methods + " methods";
            }

            foreach (var stale in nodeElements.Keys.Where(key => !active.Contains(key)).ToList())
            {
                nodeElements[stale].RemoveFromHierarchy();
                nodeElements.Remove(stale);
            }

            SyncMethodDots(connected);
            MarkDirtyRepaint();
        }

        private void SyncMethodDots(HashSet<string> connected)
        {
            var active = new HashSet<string>();
            var byId = nodes.ToDictionary(node => node.Id);

            foreach (var edge in edges)
            {
                if (edge.MethodLinks.Count == 0 || !byId.TryGetValue(edge.From, out var from) || !byId.TryGetValue(edge.To, out var to))
                {
                    continue;
                }

                var isConnected = connected.Count == 0 || connected.Contains(edge.From) && connected.Contains(edge.To);
                var start = pan + from.Position * zoom;
                var end = pan + to.Position * zoom;
                var delta = end - start;
                var c1 = start + new Vector2(delta.x * 0.42f, 0f);
                var c2 = end - new Vector2(delta.x * 0.42f, 0f);
                var count = Mathf.Min(edge.MethodLinks.Count, 12);

                for (var i = 0; i < count; i++)
                {
                    var methodLink = edge.MethodLinks[i];
                    var key = edge.From + "|" + edge.To + "|" + i + "|" + methodLink.Label + "|" + methodLink.AssetPath;
                    active.Add(key);
                    if (!methodDotElements.TryGetValue(key, out var dot))
                    {
                        dot = new VisualElement();
                        dot.AddToClassList("method-link-dot");
                        var capturedKey = key;
                        dot.RegisterCallback<PointerEnterEvent>(_ => ShowMethodPreview(capturedKey, dot, edge, methodLink));
                        dot.RegisterCallback<PointerLeaveEvent>(_ => HideMethodPreview(capturedKey));
                        dot.RegisterCallback<PointerDownEvent>(evt =>
                        {
                            evt.StopPropagation();
                            OpenMethod(methodLink);
                        });
                        methodDotElements.Add(key, dot);
                        Add(dot);
                    }

                    var t = (i + 1f) / (count + 1f);
                    var position = BezierPoint(start, c1, c2, end, t);
                    var size = Mathf.Clamp(7f * zoom, 5f, 11f);
                    dot.style.left = position.x - size * 0.5f;
                    dot.style.top = position.y - size * 0.5f;
                    dot.style.width = size;
                    dot.style.height = size;
                    dot.tooltip = "Open " + methodLink.Label;
                    dot.EnableInClassList("faded", !isConnected);
                }
            }

            foreach (var stale in methodDotElements.Keys.Where(key => !active.Contains(key)).ToList())
            {
                methodDotElements[stale].RemoveFromHierarchy();
                methodDotElements.Remove(stale);
            }

            foreach (var nodeElement in nodeElements.Values)
            {
                nodeElement.BringToFront();
            }

            methodPreview.BringToFront();
        }

        private void BuildMethodPreview()
        {
            methodPreview = new VisualElement();
            methodPreview.AddToClassList("method-link-preview");
            methodPreview.pickingMode = PickingMode.Ignore;
            methodPreview.style.display = DisplayStyle.None;
            methodPreviewTitle = new Label();
            methodPreviewTitle.AddToClassList("method-link-preview-title");
            methodPreviewMeta = new Label();
            methodPreviewMeta.AddToClassList("method-link-preview-meta");
            methodPreviewEvidence = new Label();
            methodPreviewEvidence.AddToClassList("method-link-preview-evidence");
            methodPreview.Add(methodPreviewTitle);
            methodPreview.Add(methodPreviewMeta);
            methodPreview.Add(methodPreviewEvidence);
            Add(methodPreview);
        }

        private void ShowMethodPreview(string dotKey, VisualElement dot, GalaxyEdgeViewData edge, GalaxyMethodLinkViewData methodLink)
        {
            hoveredMethodDotKey = dotKey;
            methodPreviewTitle.text = methodLink.Label;
            methodPreviewMeta.text = edge.From + " -> " + edge.To + "  /  click to open";
            methodPreviewEvidence.text = string.IsNullOrEmpty(methodLink.Evidence) ? methodLink.AssetPath : methodLink.Evidence;
            methodPreview.style.left = Mathf.Min(dot.resolvedStyle.left + 14f, Mathf.Max(12f, contentRect.width - 330f));
            methodPreview.style.top = Mathf.Max(12f, dot.resolvedStyle.top - 54f);
            methodPreview.style.display = DisplayStyle.Flex;
            dot.AddToClassList("hovered");
        }

        private void HideMethodPreview(string dotKey)
        {
            if (hoveredMethodDotKey != dotKey)
            {
                return;
            }

            hoveredMethodDotKey = null;
            methodPreview.style.display = DisplayStyle.None;
            if (methodDotElements.TryGetValue(dotKey, out var dot))
            {
                dot.RemoveFromClassList("hovered");
            }
        }

        private static void OpenMethod(GalaxyMethodLinkViewData methodLink)
        {
            if (string.IsNullOrEmpty(methodLink.AssetPath))
            {
                return;
            }

            SourceNavigation.Open(methodLink.AssetPath, methodLink.Line);
        }

        private static void OpenScript(GalaxyNodeViewData node)
        {
            if (string.IsNullOrEmpty(node.AssetPath))
            {
                return;
            }

            SourceNavigation.Open(node.AssetPath, node.Line);
        }

        private VisualElement CreateNodeElement(GalaxyNodeViewData node)
        {
            var element = new VisualElement { name = node.Id };
            element.AddToClassList("galaxy-node");
            element.style.position = Position.Absolute;
            element.style.borderLeftWidth = 4f;
            element.RegisterCallback<PointerDownEvent>(evt =>
            {
                evt.StopPropagation();
                if (evt.clickCount >= 2)
                {
                    OpenScript(node);
                    return;
                }

                NodeSelected?.Invoke(node.Id);
            });

            var title = new Label(node.Title) { name = "node-title" };
            title.AddToClassList("node-title");
            element.Add(title);

            var meta = new Label { name = "node-meta" };
            meta.AddToClassList("node-meta");
            element.Add(meta);

            var footer = new Label { name = "node-footer-text" };
            footer.AddToClassList("node-meta");
            footer.style.marginTop = 8f;
            element.Add(footer);
            return element;
        }

        private HashSet<string> BuildConnectedSet()
        {
            var connected = new HashSet<string>();
            if (string.IsNullOrEmpty(selectedNode))
            {
                return connected;
            }

            connected.Add(selectedNode);
            foreach (var edge in edges)
            {
                if (edge.From == selectedNode)
                {
                    connected.Add(edge.To);
                }

                if (edge.To == selectedNode)
                {
                    connected.Add(edge.From);
                }
            }

            return connected;
        }

        private void DrawGalaxy(MeshGenerationContext context)
        {
            EnsureLayout();
            var painter = context.painter2D;
            DrawEdges(painter);
        }

        private void DrawEdges(Painter2D painter)
        {
            var byId = nodes.ToDictionary(node => node.Id);
            var connected = BuildConnectedSet();
            foreach (var edge in edges)
            {
                if (!byId.TryGetValue(edge.From, out var from) || !byId.TryGetValue(edge.To, out var to))
                {
                    continue;
                }

                var isConnected = connected.Count == 0 || connected.Contains(edge.From) && connected.Contains(edge.To);
                var color = StudioThemeManager.Relationship(edge.RelationshipType);
                color.a = isConnected ? 0.82f : 0.12f;
                painter.strokeColor = color;
                painter.lineWidth = Mathf.Clamp(1.4f + edge.Weight * 0.45f, 1.4f, 5f);
                var start = pan + from.Position * zoom;
                var end = pan + to.Position * zoom;
                var delta = end - start;
                var c1 = start + new Vector2(delta.x * 0.42f, 0f);
                var c2 = end - new Vector2(delta.x * 0.42f, 0f);
                painter.BeginPath();
                painter.MoveTo(start);
                painter.BezierCurveTo(c1, c2, end);
                painter.Stroke();
            }
        }

        private static Vector2 BezierPoint(Vector2 start, Vector2 c1, Vector2 c2, Vector2 end, float t)
        {
            var oneMinusT = 1f - t;
            return oneMinusT * oneMinusT * oneMinusT * start +
                   3f * oneMinusT * oneMinusT * t * c1 +
                   3f * oneMinusT * t * t * c2 +
                   t * t * t * end;
        }

        private void OnWheel(WheelEvent evt)
        {
            var previous = zoom;
            zoom = Mathf.Clamp(zoom - evt.delta.y * 0.04f, 0.45f, 1.8f);
            var mouse = evt.localMousePosition;
            pan = mouse - (mouse - pan) * (zoom / Mathf.Max(0.01f, previous));
            SyncNodeElements();
            evt.StopPropagation();
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0 && evt.button != 2)
            {
                return;
            }

            dragging = true;
            lastPointerPosition = evt.position;
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!dragging)
            {
                return;
            }

            var current = (Vector2)evt.position;
            pan += current - lastPointerPosition;
            lastPointerPosition = current;
            SyncNodeElements();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            dragging = false;
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

        private static GalaxyMethodLinkViewData MethodLink(ScriptRelationshipEdge edge)
        {
            if (edge.RelationshipType != ScriptRelationshipType.MethodCall || string.IsNullOrEmpty(edge.MethodName))
            {
                return null;
            }

            var label = edge.MethodName + "()";
            return new GalaxyMethodLinkViewData(label, edge.Evidence, edge.AssetPath, edge.Line);
        }

        private static GalaxyMethodLinkViewData MergeMethodLinks(IGrouping<string, GalaxyMethodLinkViewData> links)
        {
            var ordered = links.OrderBy(link => link.Line <= 0 ? int.MaxValue : link.Line).ToList();
            if (ordered.Count == 0)
            {
                return null;
            }

            var primary = ordered[0];
            return new GalaxyMethodLinkViewData(primary.Label, primary.Evidence, primary.AssetPath, primary.Line);
        }
    }
}
