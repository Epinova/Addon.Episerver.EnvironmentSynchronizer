using Microsoft.VisualStudio.TestTools.UnitTesting;
using Addon.Episerver.EnvironmentSynchronizer.Jobs;
using EPiServer.PlugIn;

namespace Addon.Episerver.EnvironmentSynchronizer.Test
{
    [TestClass]
    public class EnvironmentSynchronizationJobTests
	{
        [TestMethod]
        public void GetEnvironmentSynchronizationJobGuidAttribute()
        {
	        var jobId =
		        (string) ((ScheduledPlugInAttribute) typeof(EnvironmentSynchronizationJob).GetCustomAttributes(
			        typeof(ScheduledPlugInAttribute), true)[0]).GUID;

            Assert.AreEqual("1eda8c91-a367-41df-adee-e6143b1e37c3", jobId.ToString());
        }
	}
}
