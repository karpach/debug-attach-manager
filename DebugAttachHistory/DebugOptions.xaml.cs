using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;

namespace comScoreInc.DebugAttachHistory
{
    /// <summary>
    /// Interaction logic for DebugOptionsControl.xaml
    /// </summary>
    public partial class DebugOptionsControl
    {
        #region Public Properties

        private static string ProcessesForAttach
        {
            get
            {                  
                if (!DebugAttachHistoryPackage.DTE.Globals.VariableExists["ProcessesForAttach"])
                {
                    DebugAttachHistoryPackage.DTE.Globals["ProcessesForAttach"] = string.Empty;
                    DebugAttachHistoryPackage.DTE.Globals.VariablePersists["ProcessesForAttach"] = true;
                }                
                return DebugAttachHistoryPackage.DTE.Globals["ProcessesForAttach"] as string;
            } 
            set
            {
                DebugAttachHistoryPackage.DTE.Globals["ProcessesForAttach"] = value;
            }
        }

        #endregion        

        #region Constructors

        public DebugOptionsControl()
        {
            InitializeComponent();            
            _processes = Process.GetProcesses().Select(p => new ProcessExt(p)).ToList();
            lstSearchProcesses.ItemsSource = _processes;            
            rbnAll.Checked+=RbnAllChecked;
        }

        #endregion

        #region Public Methods

        public void AttachToProcesses()
        {
            BtnAttachClick(this, null);
        }

        #endregion

        #region UI Hanlders

        private void LstSearchProcessesMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!lstAttachProcesses.Items.OfType<ProcessToBeAttached>().Where(p => p.Process == lstSearchProcesses.SelectedItem).Any())
            {
                var selectedProc = (ProcessExt)lstSearchProcesses.SelectedItem;
                lstAttachProcesses.Items.Add(new ProcessToBeAttached { Process = selectedProc,Checked=false});                
                AddToGlobals(selectedProc.Hash.ToString());   
            }
        }        

        private void RbnDevChecked(object sender, RoutedEventArgs e)
        {
            _processes = Process.GetProcesses().Where(p => p.ProcessName.Contains("WebDev")).Select(p=>new ProcessExt(p)).ToList();
            lstSearchProcesses.ItemsSource = _processes;
        }

        private void RbnIisChecked(object sender, RoutedEventArgs e)
        {
            _processes = Process.GetProcesses().Where(p => p.ProcessName.Contains("w3wp")).Select(p => new ProcessExt(p)).ToList();
            lstSearchProcesses.ItemsSource = _processes;
        }       

        private void RbnAllChecked(object sender, RoutedEventArgs e)
        {
            _processes = Process.GetProcesses().Select(p => new ProcessExt(p)).ToList();
            lstSearchProcesses.ItemsSource = _processes;
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
            EnvDTE.Processes processes = DebugAttachHistoryPackage.DTE.Debugger.LocalProcesses;
            var selectedProc = lstAttachProcesses.Items.OfType<ProcessToBeAttached>().Where(p=>p.Checked).ToList();
            if (selectedProc.Count == 0)
            {
                MessageBox.Show("You didn't select any processes for attachment.", "Debug Attach History Error",MessageBoxButton.OK,MessageBoxImage.Error);
                return;
            }
            bool found = false;
            foreach (EnvDTE.Process proc in processes)
            {                
                ProcessExt pp = new ProcessExt(Process.GetProcessById(proc.ProcessID));
                if (selectedProc.Exists(p=> pp.Hash == p.Process.Hash))                
                {
                    proc.Attach();
                    found = true;
                }
            }
            if (!found)
            {
                MessageBox.Show("Selected processes are not running.", "Debug Attach History Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }               

        private void MyToolWindowLoaded(object sender, RoutedEventArgs e)
        {            
            if (!string.IsNullOrEmpty(ProcessesForAttach) && lstAttachProcesses.Items.Count==0)
            {
                foreach (var pHash in ProcessesForAttach.Split(','))
                {
                    string hash = pHash;
                    var procss = _processes.Where(pp => string.Compare(pp.Hash.ToString(),hash,true) == 0).ToList();
                    foreach (var p in procss)
                    {
                        lstAttachProcesses.Items.Add(new ProcessToBeAttached {Process = p, Checked = false});
                    }
                }   
            }            
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            var p = (ProcessToBeAttached)((Button)sender).DataContext;
            RemoveFromGlobals(p.Process.Hash.ToString());
            lstAttachProcesses.Items.Remove(p);
        }

        private void CheckBoxChecked(object sender, RoutedEventArgs e)
        {
            var p = (ProcessToBeAttached)((CheckBox)sender).DataContext;
            p.Checked = true;
        }

        private void CheckBoxUnchecked(object sender, RoutedEventArgs e)
        {
            var p = (ProcessToBeAttached)((CheckBox)sender).DataContext;
            p.Checked = false;
        }

        #endregion

        #region Helper methods

        private static void AddToGlobals(string processHash)
        {
            if (string.IsNullOrEmpty(ProcessesForAttach))
            {
                ProcessesForAttach = processHash; 
            }
            else
            {
                ProcessesForAttach += string.Concat(",", processHash);   
            }            
        }

        private static void RemoveFromGlobals(string processHash)
        {
            if (!string.IsNullOrEmpty(ProcessesForAttach))
            {
                StringBuilder sb = new StringBuilder();
                foreach (var p in ProcessesForAttach.Split(','))
                {
                    if (p!=processHash)
                    {
                        sb.AppendFormat("{0},", p);
                    }
                }
                if (sb.Length>0)
                {
                    sb.Remove(sb.Length - 1, 1);   
                }                
                ProcessesForAttach = sb.ToString();
            }
        }

        #endregion

        #region Private Variables

        private List<ProcessExt> _processes;

        #endregion
    }
}