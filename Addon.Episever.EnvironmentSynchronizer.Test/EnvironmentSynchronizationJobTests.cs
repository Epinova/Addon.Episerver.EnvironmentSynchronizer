using System;
using Addon.Episerver.EnvironmentSynchronizer.Jobs;
using EPiServer.Scheduler;
using Xunit;

namespace Addon.Episerver.EnvironmentSynchronizer.Test
{
    public class EnvironmentSynchronizationJobTests
    {
        [Fact]
        public void GetEnvironmentSynchronizationJobGuidAttribute()
        {
            var jobId =
                ((ScheduledJobAttribute)typeof(EnvironmentSynchronizationJob).GetCustomAttributes(
                    typeof(ScheduledJobAttribute), true)[0]).GetGUID();

            Assert.Equal(Guid.Parse("1eda8c91-a367-41df-adee-e6143b1e37c3"), jobId);
        }
    }
}
