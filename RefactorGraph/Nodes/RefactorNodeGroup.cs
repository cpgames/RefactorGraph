using System.Collections.Generic;
using System.Windows.Media;

namespace RefactorGraph.Nodes
{
    public enum RefactorNodeGroup
    {
        CollectionOperations,
        LogicOperations,
        Other,
        PartitionOperations,
        StringOperations,
        Variables,
    }

    public class NodeColors
    {
        public static Dictionary<RefactorNodeGroup, SolidColorBrush> brushes = new Dictionary<RefactorNodeGroup, SolidColorBrush>
        {
            { RefactorNodeGroup.CollectionOperations, Brushes.DarkBlue },
            { RefactorNodeGroup.LogicOperations, Brushes.DarkCyan },
            { RefactorNodeGroup.Other, Brushes.DarkMagenta },
            { RefactorNodeGroup.PartitionOperations, Brushes.DarkSlateBlue },
            { RefactorNodeGroup.StringOperations, Brushes.DarkGreen },
            { RefactorNodeGroup.Variables, Brushes.DarkRed },
        };
    }
}