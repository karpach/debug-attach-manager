using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Interop;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using Karpach.DebugAttachManager.Helpers;
using Moq;

namespace Karpach.DebugAttachManager.Tests
{
	[TestClass]
	public class ConnectWindowTests
	{
		[TestMethod]
		public async Task ShowWindowDialog()
		{
			// Arrange
			string server = "develop";
			string port = "4021";
			string userName = "admin";
			Mock<ISettingsProvider> settingsProvider = new Mock<ISettingsProvider>();
			settingsProvider.Setup(x => x.RemoteServer).Returns(server);
			settingsProvider.Setup(x => x.RemotePort).Returns(port);
			settingsProvider.Setup(x => x.RemoteUserName).Returns(userName);
			var dlg = new ConnectWindow(settingsProvider.Object);

			// Act
			dlg.Show();
			IntPtr windowHandle = new WindowInteropHelper(dlg).Handle;
			using (var automation = new UIA3Automation())
			{
				var window = automation.FromHandle(windowHandle).AsWindow();
				Button cancelButton = window.FindFirstDescendant(cf => cf.ByName("CancelButton"))?.AsButton();

				// Assert
				TextBox txtServerName = window.FindFirstDescendant(cf => cf.ByAutomationId("txtServerName"))?.AsTextBox();
				TextBox txtPortNumber = window.FindFirstDescendant(cf => cf.ByAutomationId("txtPortNumber"))?.AsTextBox();
				TextBox txtUserName = window.FindFirstDescendant(cf => cf.ByAutomationId("txtUserName"))?.AsTextBox();
				Assert.AreEqual(server, txtServerName?.Text);
				Assert.AreEqual(port, txtPortNumber?.Text);
				Assert.AreEqual(userName, txtUserName?.Text);
#if DEBUG
				await Task.Delay(5000).ConfigureAwait(false);
#endif
				cancelButton?.Invoke();
			}
		}
	}
}