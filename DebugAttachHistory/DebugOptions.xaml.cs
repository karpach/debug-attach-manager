using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows.Data;
using System.Windows.Media;
using EnvDTE80;
using Karpach.DebugAttachManager.Models;
using Karpach.DebugAttachManager.Properties;
using Button = System.Windows.Controls.Button;
using CheckBox = System.Windows.Controls.CheckBox;
using ComboBox = System.Windows.Controls.ComboBox;
using MenuItem = System.Windows.Controls.MenuItem;
using MessageBox = System.Windows.MessageBox;

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
            _processes = GetProcesses(_remoteServer, _remoteServerPort, _remoteUserName, _remotePassword, _useWmi).ToList();
            _debugModes = new Lazy<KeyValuePair<string, string>[]>(() => GetDebugModes().ToArray());
            InitializeColumns();
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

        private void BtnConnectClick(object sender, RoutedEventArgs e)
        {
            var dlg = new ConnectWindow();
            bool? result = dlg.ShowDialog();
            if (result.HasValue && result.Value)
            {
                _remoteServer = dlg.ServerName;
                _remoteServerPort = dlg.PortNumber;
                _remoteUserName = dlg.UserName;
                _remotePassword = dlg.Password;
                _useWmi = dlg.SuccessWmiConnection;
                FilterRefresh();
            }
            else
            {
                btnConnect.IsChecked = false;
            }
        }

        private void BtnConnectClear(object sender, RoutedEventArgs e)
        {
            _remoteServer = null;
            _remoteServerPort = null;
            FilterRefresh();
        }

        private void BtnRefreshClick(object sender, RoutedEventArgs e)
        {            
            FilterRefresh();                       
        }               

        private void Search(object sender, TextChangedEventArgs e)
        {
            bool searchPid = Settings.Default.ProcessesColumns[0];
            bool searchCommandLine = Settings.Default.ProcessesColumns[1];
            lstSearchProcesses.ItemsSource =
                _processes.Where(
                    p => p.ProcessName.ToLower().Contains(txtFilter.Text.ToLower()) 
						|| searchPid && p.ProcessId.HasValue && string.Equals(p.ProcessId.Value.ToString(),txtFilter.Text, StringComparison.InvariantCultureIgnoreCase)
                         || searchCommandLine && p.CommandLine.ToLower().Contains(txtFilter.Text.ToLower())
                         || p.Title != null && p.Title.StartsWith(ProcessExt.TitlePrefix) && p.Title.ToLower().Contains(txtFilter.Text.ToLower()));
        }

        private void BtnAttachClick(object sender, RoutedEventArgs e)
        {
            Attach(sender,e);            
        }

        private bool Attach(object sender, RoutedEventArgs e)
        {
            if (lstAttachProcesses.Items.OfType<ProcessToBeAttached>().Count(p => p.Checked) == 0)
            {
                bool skipCheck = IsLoaded;
                if (!IsLoaded)
                {
                    DebugOptionsToolWindowLoaded(sender, e);                    
                }
                if (skipCheck || lstAttachProcesses.Items.OfType<ProcessToBeAttached>().Count(p => p.Checked) == 0)
                {
                    MessageBox.Show("You didn't select any processes for attachment.", "Debug Attach History Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
            var selectedProcesses = lstAttachProcesses.Items.OfType<ProcessToBeAttached>()
                .Where(p => p.Checked)
                .GroupBy(p => new
                {
                    p.Process.ServerName,
                    p.Process.PortNumber,
                    p.Process.UserName,
                    p.Process.Password,
                    p.Process.UseWmi
                })
                .ToDictionary(x=>x.Key, x=>x.ToList());
            foreach (var key in selectedProcesses.Keys)
            {
                EnvDTE.Processes processes = GetDebugProcesses(key.ServerName, key.PortNumber);
                bool found = false;
                
                foreach (Process2 process in processes)
                {
                    ProcessExt pp = new ProcessExt(process.Name, process.ProcessID, key.ServerName, key.PortNumber, key.UserName, key.Password, key.UseWmi);                    
                    var selectedProcess = selectedProcesses[key].FirstOrDefault(p => pp.Hash == p.Process.Hash);
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
                            lstAttachProcesses.Items.Add(new ProcessToBeAttached
                            {
                                Process = p,
                                Checked = IsChecked(p.Hash),
                                DebugMode = Settings.Default.Processes[pHash].DebugMode                                
                            });
                        }   
                    }
                    else
                    {
                        lstAttachProcesses.Items.Add(new ProcessToBeAttached {
                            Process = new ProcessExt(Settings.Default.Processes[pHash].ProcessName, 
                                                     Settings.Default.Processes[pHash].Title, 
                                                     Settings.Default.Processes[pHash].RemoteServerName,
                                                     Settings.Default.Processes[pHash].RemotePortNumber), 
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
                _processes = GetProcesses(_remoteServer, _remoteServerPort, _remoteUserName, _remotePassword, _useWmi).Where(p => p.ProcessName.Contains("WebDev") || string.Equals(p.ProcessName, "iisexpress.exe")).ToList();
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
                _processes = GetProcesses(_remoteServer, _remoteServerPort, _remoteUserName, _remotePassword, _useWmi).Where(p => p.ProcessName.Contains("w3wp")).ToList();
                lstSearchProcesses.ItemsSource = _processes;
            }
            else
            {
                FilterTwo.Background = Brushes.Transparent;
            }
            if (!FilterIIS && !FilterDevIIS)
            {
                _processes = GetProcesses(_remoteServer, _remoteServerPort, _remoteUserName, _remotePassword, _useWmi).ToList();
                if (string.IsNullOrEmpty(txtFilter.Text))
                {                    
                    lstSearchProcesses.ItemsSource = _processes;
                }
                else
                {
                    Search(null, null);
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

        private void LstSearchProcessesHeaderClick(Object sender, RoutedEventArgs e)
        {
            var dlg = new SelectedColumns();
            dlg.ShowDialog();
            if (dlg.DialogResult ?? false)
            {
                InitializeColumns();
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
                                                                             DebugMode = process.DebugMode,
                                                                             RemoteServerName = process.Process.ServerName,
                                                                             RemotePortNumber = process.Process.PortNumber
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

        private static IEnumerable<ProcessExt> GetProcesses(string remoteServer, long? remoteServerPort, string remoteUserName, string remotePassword, bool useWmi)
        {
            var result = new List<ProcessExt>();
            if (string.IsNullOrEmpty(remoteServer))
            {                
                foreach (EnvDTE.Process p in ((Debugger2)DebugAttachManagerPackage.DTE.Debugger).LocalProcesses)
                {
                    result.Add(new ProcessExt(p.Name, p.ProcessID, remoteServer, remoteServerPort, remoteUserName, remotePassword, useWmi));
                }
            }
            else
            {
                Debugger2 db = (Debugger2)DebugAttachManagerPackage.DTE.Debugger;
                Transport trans = db.Transports.Item("Default");

                foreach (EnvDTE.Process p in db.GetProcesses(trans, remoteServerPort == null ? remoteServer : $"{remoteServer}:{remoteServerPort}"))
                {
                    result.Add(new ProcessExt(p.Name, p.ProcessID, remoteServer, remoteServerPort, remoteUserName, remotePassword, useWmi));
                }
            }            
            return result;
        }

        private static EnvDTE.Processes GetDebugProcesses(string remoteServer, long? remoteServerPort)
        {
            if (string.IsNullOrEmpty(remoteServer))
            {
                return ((Debugger2)DebugAttachManagerPackage.DTE.Debugger).LocalProcesses;
            }
            Debugger2 db = (Debugger2)DebugAttachManagerPackage.DTE.Debugger;
            Transport trans = db.Transports.Item("Default");
            
            return db.GetProcesses(trans, remoteServerPort == null ? remoteServer : $"{remoteServer}:{remoteServerPort}");
        }

        private void InitializeColumns()
        {
            while (lstSearchProcesses.Columns.Count > 1)
            {
                lstSearchProcesses.Columns.RemoveAt(lstSearchProcesses.Columns.Count - 1);
            }
            if (Settings.Default.ProcessesColumns[0])
            {
                lstSearchProcesses.Columns.Add(new DataGridTextColumn
                {
                    Header = "PID",
                    Binding = new Binding("ProcessId"),
                    Width = new DataGridLength(30, DataGridLengthUnitType.Star)
                });
            }

            if (Settings.Default.ProcessesColumns[1])
            {
                lstSearchProcesses.Columns.Add(new DataGridTextColumn
                {
                    Header = "Command Line",
                    Binding = new Binding("CommandLine"),
                    Width = new DataGridLength(60, DataGridLengthUnitType.Star)
                });
            }
        }

        #endregion

        #region Private Variables

        private List<ProcessExt> _processes;
        private readonly Lazy<KeyValuePair<string,string>[]> _debugModes;
        protected bool FilterDevIIS;
        protected bool FilterIIS;
        private string _remoteServer = null;
        private int? _remoteServerPort = null;
        private string _remoteUserName = null;
        private string _remotePassword = null;
        private bool _useWmi = true;

        #endregion
    }
}