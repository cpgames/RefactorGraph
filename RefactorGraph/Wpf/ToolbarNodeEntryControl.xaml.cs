using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
            set
            {
                _nodeEntry.nodeName = value;
                RaisePropertyChanged("NodeName");
            }
        }

        public NodeEntryModel NodeEntry
        {
            get => _nodeEntry;
            set
            {
                _nodeEntry = value;
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
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

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