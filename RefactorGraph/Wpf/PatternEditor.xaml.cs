using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using RefactorGraphdCore.Data;

namespace RefactorGraph
{
    public partial class PatternEditor : UserControl, INotifyPropertyChanged
    {
        #region Fields
        public static readonly DependencyProperty PatternProperty =
            DependencyProperty.Register(
                "Pattern",
                typeof(Pattern),
                typeof(PatternEditor));

        private Pattern _pattern = new Pattern();
        #endregion

        #region Properties
        public Pattern Pattern
        {
            get => _pattern;
            set
            {
                _pattern = value;
                RaisePropertyChanged("PatternName");
                RaisePropertyChanged("PatternContent");
            }
        }

        public string PatternName
        {
            get => _pattern.name;
            set
            {
                _pattern.name = value;
                RaisePropertyChanged("PatternName");
            }
        }

        public string PatternContent
        {
            get => _pattern.content;
            set
            {
                _pattern.content = value;
                RaisePropertyChanged("PatternContent");
            }
        }
        #endregion

        #region Constructors
        public PatternEditor()
        {
            InitializeComponent();
            this.DataContext = this;
        }
        #endregion

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Methods
        private void RaisePropertyChanged(string propertyName)
        {
            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}