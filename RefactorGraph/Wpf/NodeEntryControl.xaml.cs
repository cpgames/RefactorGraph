using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RefactorGraph
{
    public partial class NodeEntryControl : UserControl
    {
        #region Fields
        private RefactorNodeType _nodeType;
        #endregion

        #region Properties
        public RefactorNodeType NodeType
        {
            get => _nodeType;
            set
            {
                _nodeType = value;
                RaisePropertyChanged("NodeType");
            }
        }
        #endregion

        #region Constructors
        public NodeEntryControl()
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
                DragDrop.DoDragDrop(this, NodeType, DragDropEffects.All);
            }
        }
        #endregion
    }
}