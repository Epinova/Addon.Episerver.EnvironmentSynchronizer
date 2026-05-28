using Xunit;
using System;
using Addon.Episerver.EnvironmentSynchronizer.Jobs;
#if NET10_0_OR_GREATER
using EPiServer.Scheduler;
#else
using EPiServer.PlugIn;
#endif

namespace Addon.Episerver.EnvironmentSynchronizer.Test
{
    public class EnvironmentSynchronizationJobTests
	{
        [Fact]
        public void GetEnvironmentSynchronizationJobGuidAttribute()
        {
#if NET10_0_OR_GREATER
	        var jobId =
		        ((ScheduledJobAttribute) typeof(EnvironmentSynchronizationJob).GetCustomAttributes(
			        typeof(ScheduledJobAttribute), true)[0]).GetGUID();

            Assert.Equal(Guid.Parse("1eda8c91-a367-41df-adee-e6143b1e37c3"), jobId);
#else
	        var jobId =
		        (string) ((ScheduledPlugInAttribute) typeof(EnvironmentSynchronizationJob).GetCustomAttributes(
			        typeof(ScheduledPlugInAttribute), true)[0]).GUID;

            Assert.Equal("1eda8c91-a367-41df-adee-e6143b1e37c3", jobId.ToString());
#endif
        }
	}
}
