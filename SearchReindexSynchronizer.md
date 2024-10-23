# Autorun search and navigation indexing job
Environment synchronizer is unable to trigger this job using `"AutoRun: true"`. This is because the job requires access to `HttpContext`, and the `IScheduledJobExecutor` runs jobs as background tasks, where `HttpContext` does not exist.
This can be solved by modifying the job via a custom synchronizer.

[<= Back](../README.md)

This extension method allows the job to be picked up by the scheduler and run later, removing the need to use `IScheduledJobExecutor`.

```csharp
using EPiServer.DataAbstraction;
using System;

namespace YourSite.Infrastructure.Extensions;

public static class ScheduledJobExtensions
{
    public static void ScheduleRunNow(
      this ScheduledJob job, IScheduledJobRepository scheduledJobRepository)
    {
        job.IntervalType = ScheduledIntervalType.None;
        job.IntervalLength = 0;
        job.IsEnabled = true;
        job.NextExecution = DateTime.Now.AddSeconds(10);

        scheduledJobRepository.Save(job);
    }
}
```
Now we can create a custom handler which schedules the job using our new extension method:
```csharp
using Addon.Episerver.EnvironmentSynchronizer;
using YourSite.Infrastructure.Extensions;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;
using System;

namespace YourSite.Infrastructure.Envrionments;

[ServiceConfiguration(typeof(IEnvironmentSynchronizer))]
public class SearchReindexEnvironmentSynchronizer : IEnvironmentSynchronizer
{
    private readonly IScheduledJobRepository _scheduledJobRepository;
    private readonly Guid _searchReindexJobId = new Guid("8eb257f9-ff22-40ec-9958-c1c5ba8c2a53");

    public SearchReindexEnvironmentSynchronizer(IScheduledJobRepository scheduledJobRepository) => _scheduledJobRepository = scheduledJobRepository;

    public string Synchronize(string environmentName)
    {
        if (EnvironmentHelper.IsPreProductionEnvironment(environmentName) || EnvironmentHelper.IsIntegrationEnvironment(environmentName))
        {
            var searchReindexJob = _scheduledJobRepository.Get(_searchReindexJobId);

            if (searchReindexJob is not null)
            {
                searchReindexJob.ScheduleRunNow(_scheduledJobRepository);

                return $"Scheduled search reindexing job.";
            }
        }

        return string.Empty;
    }
}
```
Now you can be sure that the search index is updated every time the database is copied to your test environments from production.