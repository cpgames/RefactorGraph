using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using NodeGraph.View;

namespace RefactorGraph.Nodes
{
    public class DynamicNodeView : NodeView
    {
        #region Fields
        public RoutedEventHandler portAdded = (sender,  args) => { };
        public RoutedEventHandler portRemoved = (sender, args) => { };
        #endregion
        
        #region Methods
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var grid = Template.FindName("NodeFlowContent", this) as Grid;

            var buttonGrid = new Grid();
            buttonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20) });
            buttonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20) });
            grid.Children.Add(buttonGrid);
            Grid.SetColumn(buttonGrid, 1);

            var addImage = new Image();
            addImage.Source = new BitmapImage(new Uri("pack://application:,,,/RefactorGraph;component/Resources/Plus.png", UriKind.RelativeOrAbsolute));
            var addPortButton = new Button
            {
                Content = addImage,
                Style = (Style)FindResource(ToolBar.ButtonStyleKey)
            };
            addPortButton.Click += portAdded;
            buttonGrid.Children.Add(addPortButton);

            var removeImage = new Image();
            removeImage.Source = new BitmapImage(new Uri("pack://application:,,,/RefactorGraph;component/Resources/Minus.png", UriKind.RelativeOrAbsolute));
            var removePortButton = new Button
            {
                Content = removeImage,
                Style = (Style)FindResource(ToolBar.ButtonStyleKey)
            };
            removePortButton.Click += portRemoved;
            buttonGrid.Children.Add(removePortButton);
            Grid.SetColumn(removePortButton, 1);
        }
        #endregion
    }
}