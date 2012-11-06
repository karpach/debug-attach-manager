using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;

namespace Karpach.DebugAttachManager
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
                if (!DebugAttachManagerPackage.DTE.Globals.VariableExists["ProcessesForAttach"])
                {
                    DebugAttachManagerPackage.DTE.Globals["ProcessesForAttach"] = string.Empty;
                    DebugAttachManagerPackage.DTE.Globals.VariablePersists["ProcessesForAttach"] = true;
                }                
                return DebugAttachManagerPackage.DTE.Globals["ProcessesForAttach"] as string;
            } 
            set
            {
                DebugAttachManagerPackage.DTE.Globals["ProcessesForAttach"] = value;
            }
        }

        private static string SelectedProcessesForAttach
        {
            get
            {
                if (!DebugAttachManagerPackage.DTE.Globals.VariableExists["SelectedProcessesForAttach"])
                {
                    DebugAttachManagerPackage.DTE.Globals["SelectedProcessesForAttach"] = string.Empty;
                    DebugAttachManagerPackage.DTE.Globals.VariablePersists["SelectedProcessesForAttach"] = true;
                }
                return DebugAttachManagerPackage.DTE.Globals["SelectedProcessesForAttach"] as string;
            }
            set
            {
                DebugAttachManagerPackage.DTE.Globals["SelectedProcessesForAttach"] = value;
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

        public bool AttachToProcesses()
        {
            return Attach(this, null);
        }

        #endregion

        #region UI Hanlders

        private void LstSearchProcessesMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!lstAttachProcesses.Items.OfType<ProcessToBeAttached>().Where(p => p.Process == lstSearchProcesses.SelectedItem).Any())
            {
                var selectedProc = (ProcessExt)lstSearchProcesses.SelectedItem;
                lstAttachProcesses.Items.Add(new ProcessToBeAttached { Process = selectedProc, Checked = false });                
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
            Attach(sender,e);
        }

        private bool Attach(object sender, RoutedEventArgs e)
        {
            EnvDTE.Processes processes = DebugAttachManagerPackage.DTE.Debugger.LocalProcesses;
            var selectedProc = lstAttachProcesses.Items.OfType<ProcessToBeAttached>().Where(p => p.Checked).ToList();
            if (selectedProc.Count == 0)
            {
                if (!IsLoaded)
                {
                    MyToolWindowLoaded(sender, e);
                    selectedProc = lstAttachProcesses.Items.OfType<ProcessToBeAttached>().Where(p => p.Checked).ToList();
                }
                if (selectedProc.Count == 0)
                {
                    MessageBox.Show("You didn't select any processes for attachment.", "Debug Attach History Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
            bool found = false;
            foreach (EnvDTE.Process proc in processes)
            {
                ProcessExt pp = new ProcessExt(Process.GetProcessById(proc.ProcessID));
                if (selectedProc.Exists(p => pp.Hash == p.Process.Hash))
                {
                    proc.Attach();
                    found = true;
                }
            }
            if (!found)
            {
                MessageBox.Show("Selected processes are not running.", "Debug Attach History Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
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
                        lstAttachProcesses.Items.Add(new ProcessToBeAttached { Process = p, Checked = IsChecked(p.Hash.ToString()) });
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
            AddToSolutionGlobals(p.Process.Hash.ToString());
        }

        private void CheckBoxUnchecked(object sender, RoutedEventArgs e)
        {
            var p = (ProcessToBeAttached)((CheckBox)sender).DataContext;
            p.Checked = false;
            RemoveFromSolutionGlobals(p.Process.Hash.ToString());
        }

        #endregion

        #region Helper methods

        private static void AddToGlobals(string processHash)
        {            
            ProcessesForAttach = AddToCommaString(processHash, ProcessesForAttach);         
        }

        private static void RemoveFromGlobals(string processHash)
        {
            ProcessesForAttach = RemoveFromCommaString(processHash, ProcessesForAttach);         
        }

        private static void AddToSolutionGlobals(string processHash)
        {
            SelectedProcessesForAttach = AddToCommaString(processHash, SelectedProcessesForAttach);
        }

        private static void RemoveFromSolutionGlobals(string processHash)
        {
            SelectedProcessesForAttach = RemoveFromCommaString(processHash, SelectedProcessesForAttach);
        }

        private static bool IsChecked(string processHash)
        {
            foreach (var p in SelectedProcessesForAttach.Split(','))
            {
                if (p == processHash)
                {
                    return true;
                }
            }
            return false;
        }

        private static string AddToCommaString(string processHash, string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                s = processHash; 
            }
            else
            {
                s += string.Concat(",", processHash);   
            }
            return s;
        }

        private static string RemoveFromCommaString(string processHash, string s)
        {
            if (!string.IsNullOrEmpty(s))
            {
                StringBuilder sb = new StringBuilder();
                foreach (var p in s.Split(','))
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
                return sb.ToString();
            }
            return string.Empty;
        }      

        #endregion

        #region Private Variables

        private List<ProcessExt> _processes;

        #endregion        
    }
}