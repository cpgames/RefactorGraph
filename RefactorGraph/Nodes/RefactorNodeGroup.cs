using System.Collections.Generic;
using System.Windows.Media;

namespace RefactorGraph.Nodes
{
    public enum RefactorNodeGroup
    {
        CollectionOperations,
        DTE,
        LogicOperations,
        Other,
        PartitionOperations,
        StringOperations,
        Variables
    }

    public class NodeColors
    {
        #region Fields
        public static Dictionary<RefactorNodeGroup, SolidColorBrush> brushes = new Dictionary<RefactorNodeGroup, SolidColorBrush>
        {
            { RefactorNodeGroup.CollectionOperations, Brushes.DarkBlue },
            { RefactorNodeGroup.DTE, new SolidColorBrush(Color.FromRgb(20, 20, 20)) },
            { RefactorNodeGroup.LogicOperations, Brushes.DarkCyan },
            { RefactorNodeGroup.Other, Brushes.DarkMagenta },
            { RefactorNodeGroup.PartitionOperations, Brushes.DarkSlateBlue },
            { RefactorNodeGroup.StringOperations, Brushes.DarkGreen },
            { RefactorNodeGroup.Variables, Brushes.DarkRed }
        };
        #endregion
    }
}