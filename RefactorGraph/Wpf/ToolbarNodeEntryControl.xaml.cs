using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RefactorGraph.Nodes;

namespace RefactorGraph
{
    public partial class ToolbarNodeEntryControl : UserControl
    {
        #region Fields
        private NodeEntryModel _nodeEntry;
        #endregion

        #region Properties
        public string NodeName
        {
            get => _nodeEntry.nodeName;
            set => _nodeEntry.nodeName = value;
        }

        public NodeEntryModel NodeEntry
        {
            get => _nodeEntry;
            set
            {
                _nodeEntry = value;
                PointColor.Color = NodeColors.brushes[_nodeEntry.nodeGroup].Color;
                NodeName = value.nodeName;
            }
        }
        #endregion

        #region Constructors
        public ToolbarNodeEntryControl()
        {
            InitializeComponent();
        }
        #endregion

        #region Methods
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (MouseButtonState.Pressed == e.LeftButton)
            {
                DragDrop.DoDragDrop(this, new DataObject("NodeEntry", _nodeEntry, true), DragDropEffects.All);
            }
        }
        #endregion
    }
}