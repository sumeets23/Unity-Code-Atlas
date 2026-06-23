using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptIntelligence.Editor.Models
{
    [Serializable]
    public sealed class GraphExportSnapshot
    {
        [SerializeField] private List<GraphExportNode> nodes = new List<GraphExportNode>();
        [SerializeField] private List<GraphExportEdge> edges = new List<GraphExportEdge>();

        public List<GraphExportNode> Nodes => nodes;
        public List<GraphExportEdge> Edges => edges;

        public void Clear()
        {
            nodes.Clear();
            edges.Clear();
        }
    }
}
