using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using NodeGraph.View;

namespace RefactorGraph.Nodes.Other
{
    public class BusView : NodeView
    {
        #region Fields
        public RoutedEventHandler nodeAdded = (sender,  args) => { };
        public RoutedEventHandler nodeRemoved = (sender, args) => { };
        #endregion

        #region Methods
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var grid = Template.FindName("NodeFlowContent", this) as Grid;

            var buttonGrid = new Grid();
            buttonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(25) });
            buttonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(25) });
            grid.Children.Add(buttonGrid);
            Grid.SetColumn(buttonGrid, 1);

            var addImage = new Image();
            addImage.Source = new BitmapImage(new Uri("pack://application:,,,/RefactorGraph;component/Resources/Add.png", UriKind.RelativeOrAbsolute));
            var addNode = new Button
            {
                Content = addImage,
                Style = (Style)FindResource(ToolBar.ButtonStyleKey)
            };
            addNode.Click += nodeAdded;
            buttonGrid.Children.Add(addNode);

            var removeImage = new Image();
            removeImage.Source = new BitmapImage(new Uri("pack://application:,,,/RefactorGraph;component/Resources/Remove.png", UriKind.RelativeOrAbsolute));
            var removeNode = new Button
            {
                Content = removeImage,
                Style = (Style)FindResource(ToolBar.ButtonStyleKey)
            };
            removeNode.Click += nodeRemoved;
            buttonGrid.Children.Add(removeNode);
            Grid.SetColumn(removeNode, 1);
        }
        #endregion
    }
}