using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;
using System.Threading;
using System.Windows.Media;
using EnvDTE80;
using Karpach.DebugAttachManager.Models;
using Karpach.DebugAttachManager.Properties;

namespace Karpach.DebugAttachManager
{
    /// <summary>
    /// Interaction logic for DebugOptionsControl.xaml
    /// </summary>
    public partial class DebugOptionsControl
    {
        #region Properties
        
        public KeyValuePair<string,string>[] DebugModes => _debugModes.Value;        

        #endregion      

        #region Constructors

        public DebugOptionsControl()
        {
            InitializeComponent();            
            _processes = Process.GetProcesses().Select(p => new ProcessExt(p)).ToList();
            _debugModes = new Lazy<KeyValuePair<string, string>[]>(() => GetDebugModes().ToArray());            
            lstSearchProcesses.ItemsSource = _processes;                        
        }

        #endregion

        #region Public Methods

        public bool AttachToProcesses()
        {
            return Attach(this, null);
        }

        #endregion

        #region UI Hanlders

        private void LstSearchProcessesMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lstAttachProcesses.Items.OfType<ProcessToBeAttached>().All(
                p => p.Process.Hash != ((ProcessExt)lstSearchProcesses.SelectedItem).Hash || p.DebugMode != null))
            {
                var selectedProcess = (ProcessExt)lstSearchProcesses.SelectedItem;
                var p = new ProcessToBeAttached {Process = selectedProcess, Checked = true };
                lstAttachProcesses.Items.Add(p);
                SaveProcessHash(p);
            }
        }

        private IEnumerable<KeyValuePair<string,string>> GetDebugModes()
        {
            Transport transport = ((Debugger2)DebugAttachManagerPackage.DTE.Debugger).Transports.Item("Default");
            yield return new KeyValuePair<string, string>("Auto", null);
            foreach (Engine engine in transport.Engines)
            {
                yield return new KeyValuePair<string,string>(engine.Name, $"{engine.ID}");
            }            
        }                

        private void BtnRefreshClick(object sender, RoutedEventArgs e)
        {            
            FilterRefresh();                       
        }               

        private void TxtFilterTextChanged(object sender, TextChangedEventArgs e)
        {
            lstSearchProcesses.ItemsSource =
                _processes.Where(
                    p => p.ProcessName.ToLower().Contains(txtFilter.Text.ToLower()) 
                         || p.Title.ToLower().Contains(txtFilter.Text.ToLower()));
        }

        private void BtnAttachClick(object sender, RoutedEventArgs e)
        {
            Attach(sender,e);            
        }

        private bool Attach(object sender, RoutedEventArgs e)
        {
            EnvDTE.Processes processes = ((Debugger2)DebugAttachManagerPackage.DTE.Debugger).LocalProcesses;
            var selectedProcesses = lstAttachProcesses.Items.OfType<ProcessToBeAttached>().Where(p => p.Checked).ToList();
            if (selectedProcesses.Count == 0)
            {
                if (!IsLoaded)
                {
                    DebugOptionsToolWindowLoaded(sender, e);
                    selectedProcesses = lstAttachProcesses.Items.OfType<ProcessToBeAttached>().Where(p => p.Checked).ToList();
                }
                if (selectedProcesses.Count == 0)
                {
                    MessageBox.Show("You didn't select any processes for attachment.", "Debug Attach History Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
            bool found = false;
            foreach (Process2 process in processes)
            {
                ProcessExt pp;
                try
                {
                    pp = new ProcessExt(Process.GetProcessById(process.ProcessID));
                }
                catch (ArgumentException)
                {                    
                    continue;
                }
                var selectedProcess = selectedProcesses.FirstOrDefault(p => pp.Hash == p.Process.Hash);
                if (!string.IsNullOrEmpty(selectedProcess?.DebugMode))
                {
                    foreach (Engine engine in ((Debugger2)DebugAttachManagerPackage.DTE.Debugger).Transports.Item("Default").Engines)
                    {
                        if (string.Equals(engine.ID, selectedProcess.DebugMode))
                        {
                            process.Attach2(new[] { engine });
                        }
                    }                    
                    found = true;
                }
                else if (selectedProcess != null)
                {
                    process.Attach();
                    found = true;
                }
            }
            if (!found)
            {
                MessageBox.Show("Selected processes are not running. Try to run your application first.", "Debug Attach History Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        private void DebugOptionsToolWindowLoaded(object sender, RoutedEventArgs e)
        {            
            if (lstAttachProcesses.Items.Count==0)
            {
                foreach (var pHash in Settings.Default.Processes.Keys)
                {
                    int hash = pHash;
                    var processes = _processes.Where(pp => pp.Hash == hash).ToList();
                    if (processes.Count > 0)
                    {
                        foreach (var p in processes)
                        {
                            lstAttachProcesses.Items.Add(new ProcessToBeAttached { Process = p, Checked = IsChecked(p.Hash), DebugMode = Settings.Default.Processes[pHash].DebugMode });
                        }   
                    }
                    else
                    {
                        lstAttachProcesses.Items.Add(new ProcessToBeAttached {
                            Process = new ProcessExt(Settings.Default.Processes[pHash].ProcessName, Settings.Default.Processes[pHash].Title), 
                            Checked = IsChecked(hash),
                            DebugMode = Settings.Default.Processes[pHash].DebugMode
                        });
                    }
                }   
            }            
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            var p = (ProcessToBeAttached)((Button)sender).DataContext;
            DeleteProcessHash(p.Process.Hash);
            lstAttachProcesses.Items.Remove(p);
        }

        private void CheckBoxChecked(object sender, RoutedEventArgs e)
        {
            var p = (ProcessToBeAttached)((CheckBox)sender).DataContext;
            p.Checked = true;
            SaveProcessHash(p);
        }

        private void CheckBoxUnchecked(object sender, RoutedEventArgs e)
        {
            var p = (ProcessToBeAttached)((CheckBox)sender).DataContext;
            p.Checked = false;
            SaveProcessHash(p);
        }

        private void DebugModeChanged(object sender, SelectionChangedEventArgs e)
        {
            var p = (ProcessToBeAttached)((ComboBox)sender).DataContext;            
            SaveProcessHash(p);
        }

        private void Filter_MenuItem_Click(Object sender, RoutedEventArgs e)
        {
            var menu = e.OriginalSource as MenuItem;
            if (menu != null && string.Equals(menu.Name, "FilterOne"))
            {
                FilterDevIIS = !FilterDevIIS;
                if (FilterDevIIS)
                {                    
                    FilterIIS = false;                    
                }                
            }
            if (menu != null && string.Equals(menu.Name, "FilterTwo"))
            {
                FilterIIS = !FilterIIS;
                if (FilterDevIIS)
                {
                    FilterDevIIS = false;                    
                }                
            }            
            Debug.WriteLine(menu.Name);
            FilterRefresh();
        }

        private void FilterRefresh()
        {
            if (FilterDevIIS)
            {                
                FilterTwo.Background = Brushes.Transparent;
                FilterOne.SetResourceReference(MenuItem.BackgroundProperty, Colors.ToolbarHoverBackground);
                _processes = Process.GetProcesses().Where(p => p.ProcessName.Contains("WebDev") || string.Equals(p.ProcessName, "iisexpress")).Select(p => new ProcessExt(p)).ToList();
                lstSearchProcesses.ItemsSource = _processes;
            }
            else
            {
                FilterOne.Background = Brushes.Transparent;
            }            
            if (FilterIIS)
            {                
                FilterOne.Background = Brushes.Transparent;
                FilterTwo.SetResourceReference(MenuItem.BackgroundProperty, Colors.ToolbarHoverBackground);
                _processes = Process.GetProcesses().Where(p => p.ProcessName.Contains("w3wp")).Select(p => new ProcessExt(p)).ToList();
                lstSearchProcesses.ItemsSource = _processes;
            }
            else
            {
                FilterTwo.Background = Brushes.Transparent;
            }
            if (!FilterIIS && !FilterDevIIS)
            {
                if (string.IsNullOrEmpty(txtFilter.Text))
                {
                    _processes = Process.GetProcesses().Select(p => new ProcessExt(p)).ToList();
                    lstSearchProcesses.ItemsSource = _processes;
                }
                else
                {
                    TxtFilterTextChanged(null, null);
                }                
            }
            Filter.Tag = FilterIIS || FilterDevIIS;
        }

        private void FilterReset(Object sender, RoutedEventArgs e)
        {
            var menu = e.OriginalSource as MenuItem;
            if (menu != null)
            {
                foreach (MenuItem item in menu.Items)
                {
                    item.Background = Brushes.Transparent;
                }
                FilterIIS = false;
                FilterDevIIS = false;
                FilterRefresh();
            }            
        }

        #endregion

        #region Helper methods

        private static bool IsChecked(int processHash)
        {
            return Settings.Default.Processes.ContainsKey(processHash) && Settings.Default.Processes[processHash].Selected;
        }

        private static void SaveProcessHash(ProcessToBeAttached process)
        {
            if (Settings.Default.Processes.ContainsKey(process.Process.Hash))
            {
                Settings.Default.Processes[process.Process.Hash].Selected = process.Checked;
                Settings.Default.Processes[process.Process.Hash].DebugMode = process.DebugMode;
            }
            else
            {
                Settings.Default.Processes.Add(process.Process.Hash, new StoredProcessInfo
                                                                         {
                                                                             Title = process.Process.Title,
                                                                             ProcessName = process.Process.ProcessName,
                                                                             Selected = process.Checked,
                                                                             DebugMode = process.DebugMode
                                                                         });
            }
            Settings.Default.Save();
        }

        private static void DeleteProcessHash(int processHash)
        {
            if (Settings.Default.Processes.ContainsKey(processHash))
            {
                Settings.Default.Processes.Remove(processHash);
                Settings.Default.Save();
            }
        }      

        #endregion

        #region Private Variables

        private List<ProcessExt> _processes;
        private readonly Lazy<KeyValuePair<string,string>[]> _debugModes;
        protected bool FilterDevIIS;
        protected bool FilterIIS;

        #endregion
    }
}