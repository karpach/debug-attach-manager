using System;
using System.Management;
using System.Windows;
using EnvDTE80;

namespace Karpach.DebugAttachManager
{
    /// <summary>
    /// Interaction logic for ConnectWindow.xaml
    /// </summary>
    public partial class ConnectWindow : Window
    {
        public string ServerName => txtServerName.Text;

        private int _portNumber;        

        public int? PortNumber
        {
            get
            {
                if (string.IsNullOrEmpty(txtPortNumber.Text))
                {
                    return null;
                }
                if (!int.TryParse(txtPortNumber.Text, out _portNumber))
                {
                    _portNumber = -1;
                }
                return _portNumber;
            }            
        }

        public ConnectWindow()
        {
            InitializeComponent();
        }

        private void ConnectOnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtServerName.Text))
            {
                MessageBox.Show("Server name is required.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (PortNumber == -1)
            {
                MessageBox.Show("Invalid port number.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Debugger2 db = (Debugger2)DebugAttachManagerPackage.DTE.Debugger;
            Transport trans = db.Transports.Item("Default");

            try
            {
                db.GetProcesses(trans, PortNumber == null ? txtServerName.Text : $"{txtServerName.Text}:{_portNumber}");
            }
            catch
            {
                MessageBox.Show($"Unable to connect to {txtServerName.Text}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ConnectionOptions options = new ConnectionOptions
            {
                Impersonation = ImpersonationLevel.Default,
                EnablePrivileges = true,
                Authentication = AuthenticationLevel.PacketPrivacy                
            };            

            var scope = new ManagementScope($@"\\{txtServerName.Text}\root\cimv2", options);
            try
            {
                scope.Connect();
            }
            catch
            {
                MessageBoxResult result = MessageBox.Show($"Unable to connect to {txtServerName.Text} WMI service. Please check permissions.", "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            DialogResult = true;
            Close();
        }        
    }
}
