using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Karpach.DebugAttachManager.Tests
{
    [TestClass]
    public class IISExpressRunnerTests
    {
        [TestMethod]
        public void TestGetPath()
        {
            var iisExpressRunner = new IISExpressRunner(string.Empty);
            Assert.IsNotNull(iisExpressRunner.GetPath());
        }
    }
}
