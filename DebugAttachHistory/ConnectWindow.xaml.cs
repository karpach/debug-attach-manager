using System.Management;
using System.Windows;
using EnvDTE;
using EnvDTE80;
using Karpach.DebugAttachManager.Helpers;

namespace Karpach.DebugAttachManager
{
    /// <summary>
    /// Interaction logic for ConnectWindow.xaml
    /// </summary>
    public partial class ConnectWindow : System.Windows.Window
    {
	    private readonly ISettingsProvider _settingsProvider;
	    public string ServerName => txtServerName.Text;
	    public string UserName => txtUserName.Text;
	    public string Password => txtPassword.Text;
	    public bool SuccessWmiConnection { get; protected set; }

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

        public ConnectWindow(): this(new SettingsProvider())
        {
        }

        public ConnectWindow(ISettingsProvider settingsProvider)
        {
	        _settingsProvider = settingsProvider;
	        InitializeComponent();
	        txtServerName.Text = _settingsProvider.RemoteServer;
	        txtPortNumber.Text = _settingsProvider.RemotePort;
	        txtUserName.Text = _settingsProvider.RemoteUserName;
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

            ConnectionOptions options;
            if (string.IsNullOrEmpty(txtUserName.Text) && string.IsNullOrEmpty(txtPassword.Text))
            {
	            options = new ConnectionOptions
	            {
		            Impersonation = ImpersonationLevel.Default,
		            EnablePrivileges = true,
		            Authentication = AuthenticationLevel.PacketPrivacy
	            };
            }
            else
            {
	            options = new ConnectionOptions
	            {
		            Impersonation = ImpersonationLevel.Identify,
		            EnablePrivileges = true,
		            Authentication = AuthenticationLevel.PacketPrivacy,
		            Username = txtUserName.Text,
		            Password = txtPassword.Text
	            };
            }

            var scope = new ManagementScope($@"\\{txtServerName.Text}\root\cimv2", options);
            SuccessWmiConnection = true;
            try
            {
                scope.Connect();
            }
            catch
            {
                MessageBoxResult result = MessageBox.Show($"Unable to connect to {txtServerName.Text} WMI service. Please check permissions. You can use WBEMTest.exe to test your WMI access. If you are not in a Domain, UAC on remote machine will prevent remote access.", "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
                SuccessWmiConnection = false;
            }

            DialogResult = true;
            _settingsProvider.RemoteServer = txtServerName.Text;
            if (string.IsNullOrEmpty(txtPortNumber.Text))
            {
	            _settingsProvider.RemotePort = txtPortNumber.Text;
            }

            if (SuccessWmiConnection)
            {
	            if (string.IsNullOrEmpty(txtUserName.Text))
	            {
		            _settingsProvider.RemoteUserName = txtUserName.Text;
	            }
            }
            else
            {
	            txtUserName.Text = string.Empty;
	            txtPassword.Text = string.Empty;
            }
            _settingsProvider.Save();
            Close();
        }        
    }
}
