using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Karpach.DebugAttachManager
{
    public partial class SearchControl
    {

        public static readonly DependencyProperty HintTextProperty =
             DependencyProperty.Register("HintText", typeof(string),
             typeof(SearchControl), new FrameworkPropertyMetadata(string.Empty));

        public string HintText
        {
            get { return (string)GetValue(HintTextProperty); }
            set { SetValue(HintTextProperty, value); }
        }
        
        public static readonly DependencyProperty HintColorProperty =
             DependencyProperty.Register("HintColor", typeof(Brush),
             typeof(SearchControl), new FrameworkPropertyMetadata(Brushes.LightGray));

        public Brush HintColor
        {
            get { return (Brush)GetValue(HintColorProperty); }
            set { SetValue(HintColorProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
             DependencyProperty.Register("Text", typeof(string),
             typeof(SearchControl), new FrameworkPropertyMetadata(string.Empty));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public event TextChangedEventHandler TextChanged;

        public SearchControl()
        {
            InitializeComponent();
        }        

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            txtEntry.Text = string.Empty;
        }

        private void SearchTextChanged(object sender, TextChangedEventArgs e)
        {
            SetValue(TextProperty, txtEntry.Text);
            TextChanged?.Invoke(this, e);
        }        
    }
}