using System.Windows;
using RefactorGraphdCore.Data;
using NodeGraph.Model;
using NodeGraph.View;

namespace RefactorGraph
{
    public class PatternPropertyPortView : NodePropertyPortView
    {
        #region Constructors
        public PatternPropertyPortView(bool isInput) : base(isInput) { }
        #endregion

        #region Methods
        protected override void CreatePropertyEditor()
        {
            var port = ViewModel.Model as NodePropertyPort;

            var patternEditor = new PatternEditor();
            patternEditor.Pattern = (Pattern)port.Value;
            patternEditor.SetBinding(PatternEditor.PatternProperty, CreateBinding(port, "Value", null));

            PropertyEditor = patternEditor;
            PropertyEditorVisibility = Visibility.Visible;
        }
        #endregion
    }
}