using System.Collections.Generic;
using UnityEngine;

namespace ScriptIntelligence.Editor.UIToolkit.Galaxy
{
    public sealed class GalaxyLayoutEngine
    {
        public void Layout(IReadOnlyList<GalaxyNodeViewData> nodes, IReadOnlyList<GalaxyEdgeViewData> edges, Rect bounds, string selectedNode)
        {
            if (nodes.Count == 0)
            {
                return;
            }

            var center = new Vector2(bounds.width * 0.5f, bounds.height * 0.5f);
            var radius = Mathf.Min(bounds.width, bounds.height) * 0.31f;
            var selectedIndex = -1;
            for (var i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].Id == selectedNode)
                {
                    selectedIndex = i;
                    break;
                }
            }

            if (selectedIndex >= 0)
            {
                nodes[selectedIndex].Position = center;
            }

            var ringIndex = 0;
            for (var i = 0; i < nodes.Count; i++)
            {
                if (i == selectedIndex)
                {
                    continue;
                }

                var angle = Mathf.PI * 2f * ringIndex / Mathf.Max(1, nodes.Count - (selectedIndex >= 0 ? 1 : 0));
                var nodeRadius = radius * (0.72f + 0.18f * (ringIndex % 3));
                nodes[i].Position = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * nodeRadius;
                ringIndex++;
            }

            Relax(nodes, edges, bounds, selectedNode);
        }

        private static void Relax(IReadOnlyList<GalaxyNodeViewData> nodes, IReadOnlyList<GalaxyEdgeViewData> edges, Rect bounds, string selectedNode)
        {
            var velocities = new Dictionary<string, Vector2>();
            var byId = new Dictionary<string, GalaxyNodeViewData>();
            foreach (var node in nodes)
            {
                velocities[node.Id] = Vector2.zero;
                byId[node.Id] = node;
            }

            for (var iteration = 0; iteration < 42; iteration++)
            {
                foreach (var node in nodes)
                {
                    if (node.Id == selectedNode)
                    {
                        continue;
                    }

                    var force = Vector2.zero;
                    foreach (var other in nodes)
                    {
                        if (node == other)
                        {
                            continue;
                        }

                        var delta = node.Position - other.Position;
                        var distance = Mathf.Max(80f, delta.magnitude);
                        force += delta.normalized * (4200f / distance);
                    }

                    velocities[node.Id] = (velocities[node.Id] + force * 0.008f) * 0.72f;
                }

                foreach (var edge in edges)
                {
                    if (!byId.TryGetValue(edge.From, out var from) || !byId.TryGetValue(edge.To, out var to))
                    {
                        continue;
                    }

                    var delta = to.Position - from.Position;
                    var distance = Mathf.Max(1f, delta.magnitude);
                    var spring = delta.normalized * ((distance - 260f) * 0.012f);
                    if (from.Id != selectedNode)
                    {
                        velocities[from.Id] += spring;
                    }

                    if (to.Id != selectedNode)
                    {
                        velocities[to.Id] -= spring;
                    }
                }

                foreach (var node in nodes)
                {
                    if (node.Id == selectedNode)
                    {
                        node.Position = new Vector2(bounds.width * 0.5f, bounds.height * 0.5f);
                        continue;
                    }

                    node.Position += velocities[node.Id];
                    node.Position = new Vector2(
                        Mathf.Clamp(node.Position.x, 120f, Mathf.Max(120f, bounds.width - 120f)),
                        Mathf.Clamp(node.Position.y, 110f, Mathf.Max(110f, bounds.height - 110f)));
                }
            }
        }
    }
}
