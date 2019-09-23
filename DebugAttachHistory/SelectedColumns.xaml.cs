using System.Windows;
using Karpach.DebugAttachManager.Properties;

namespace Karpach.DebugAttachManager
{
    /// <summary>
    /// Interaction logic for SelectedColumns.xaml
    /// </summary>
    public partial class SelectedColumns : Window
    {
        public SelectedColumns()
        {
            InitializeComponent();
            SelectedColumn[] selectedColumns = 
            {
                new SelectedColumn
                {
                    Name = "PID",
                    IsChecked = false
                },
                new SelectedColumn
                {
                    Name = "Command Line",
                    IsChecked = false
                }
            };
            for (int i = 0; i < Constants.NUMBER_OF_OPTIONAL_COLUMNS; i++)
            {
                selectedColumns[i].IsChecked = Settings.Default.ProcessesColumns[i];
            }
            lstColumns.ItemsSource = selectedColumns;
        }

        private class SelectedColumn
        {
            public string Name { get; set; }
            public bool IsChecked { get; set; }
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            SelectedColumn[] selectedColumns = (SelectedColumn[]) lstColumns.ItemsSource;
            for (int i = 0; i < Constants.NUMBER_OF_OPTIONAL_COLUMNS; i++)
            {
                Settings.Default.ProcessesColumns[i] = selectedColumns[i].IsChecked;
            }
            Settings.Default.Save();
            DialogResult = true;
            Close();            
        }
    }
}
