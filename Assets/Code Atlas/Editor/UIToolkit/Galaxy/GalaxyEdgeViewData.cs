using System.Collections.Generic;
using ScriptIntelligence.Editor.Models;

namespace ScriptIntelligence.Editor.UIToolkit.Galaxy
{
    public sealed class GalaxyEdgeViewData
    {
        public string From { get; }
        public string To { get; }
        public ScriptRelationshipType RelationshipType { get; }
        public int Weight { get; }
        public IReadOnlyList<GalaxyMethodLinkViewData> MethodLinks { get; }

        public GalaxyEdgeViewData(string from, string to, ScriptRelationshipType relationshipType, int weight, IReadOnlyList<GalaxyMethodLinkViewData> methodLinks)
        {
            From = from;
            To = to;
            RelationshipType = relationshipType;
            Weight = weight;
            MethodLinks = methodLinks ?? new List<GalaxyMethodLinkViewData>();
        }
    }
}
