using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Karpach.DebugAttachManager.Controls
{
    /// <summary>
    /// Interaction logic for ToggleImageButton.xaml
    /// </summary>
    public partial class ImageButton : UserControl
    {
        public ImageButton()
        {
            InitializeComponent();
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
          DependencyProperty.Register("Text", typeof(string), typeof(ImageButton), new UIPropertyMetadata(""));

        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }        

        public static readonly DependencyProperty ImageProperty =
           DependencyProperty.Register("Image", typeof(ImageSource), typeof(ImageButton), new UIPropertyMetadata(null));
                
        
        public delegate void ClickHandler(object sender, RoutedEventArgs e);
        public event ClickHandler Click;

        public void ButtonClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                var grid = button.Parent as Grid;
                if (grid != null)
                {
                    var imageButton = grid.Parent as ImageButton;
                    if (imageButton != null)
                    {
                        imageButton.Click(sender, e);
                    }
                }
            }
            e.Handled = false;
        }        
    }
}
