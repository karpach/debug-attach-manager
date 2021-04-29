using Karpach.DebugAttachManager.Properties;

namespace Karpach.DebugAttachManager.Helpers
{
	public class SettingsProvider : ISettingsProvider
	{
		public string RemoteServer
		{
			get => Settings.Default.RemoteServer;
			set => Settings.Default.RemoteServer = value;
		}

		public string RemotePort
		{
			get => Settings.Default.RemotePort;
			set => Settings.Default.RemotePort = value;
		}

		public string RemoteUserName
		{
			get => Settings.Default.RemoteUserName;
			set => Settings.Default.RemoteUserName = value;
		}

		public void Save()
		{
			Settings.Default.Save();
		}
	}
}