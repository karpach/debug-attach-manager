using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Karpach.DebugAttachManager
{
    /// <summary>
    /// Interaction logic for ToggleImageButton.xaml
    /// </summary>
    public partial class ToggleImageButton : UserControl
    {
        public ToggleImageButton()
        {
            InitializeComponent();
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
          DependencyProperty.Register("Text", typeof(string), typeof(ToggleImageButton), new UIPropertyMetadata(""));

        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }        

        public static readonly DependencyProperty ImageProperty =
           DependencyProperty.Register("Image", typeof(ImageSource), typeof(ToggleImageButton), new UIPropertyMetadata(null));
        
        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }

        public static readonly DependencyProperty IsCheckedProperty =
          DependencyProperty.Register("IsChecked", typeof(bool), typeof(ToggleImageButton), new UIPropertyMetadata(false));
        
        public delegate void CheckedHandler(object sender, RoutedEventArgs e);
        public event CheckedHandler Checked;

        public void ToggleButtonChecked(object sender, RoutedEventArgs e)
        {
            var toggleButton = sender as ToggleButton;
            if (toggleButton != null)
            {
                var grid = toggleButton.Parent as Grid;
                if (grid != null)
                {
                    var toggleImageButton = grid.Parent as ToggleImageButton;
                    if (toggleImageButton != null)
                    {
                        toggleImageButton.Checked(sender, e);
                    }
                }
            }
            e.Handled = false;
        }

        public delegate void UnCheckedHandler(object sender, RoutedEventArgs e);
        public event UnCheckedHandler UnChecked;

        public void ToggleButtonUnChecked(object sender, RoutedEventArgs e)
        {
            var toggleButton = sender as ToggleButton;
            if (toggleButton != null)
            {
                var grid = toggleButton.Parent as Grid;
                if (grid != null)
                {
                    var toggleImageButton = grid.Parent as ToggleImageButton;
                    if (toggleImageButton != null)
                    {
                        toggleImageButton.UnChecked(sender, e);
                    }
                }
            }
            e.Handled = false;
        }
    }
}
