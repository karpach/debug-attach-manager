namespace Karpach.DebugAttachManager.Helpers
{
	public interface ISettingsProvider
	{
		string RemoteServer { get; set; }
		string RemotePort { get; set; }
		string RemoteUserName { get; set; }
		void Save();
	}
}