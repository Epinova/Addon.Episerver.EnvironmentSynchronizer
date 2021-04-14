using Addon.Episerver.EnvironmentSynchronizer.Configuration;
using EPiServer.DataAbstraction;
using EPiServer.Logging;
using EPiServer.Scheduler;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Addon.Episerver.EnvironmentSynchronizer.Synchronizers.ScheduledJobs
{
	[ServiceConfiguration(typeof(IEnvironmentSynchronizer))]
	public class ScheduledJobSynchronizer : IEnvironmentSynchronizer
    {
		private static readonly ILogger Logger = LogManager.GetLogger();
		private readonly IScheduledJobRepository _scheduledJobRepository;
        private readonly IScheduledJobExecutor _scheduledJobExecutor;
        private readonly IConfigurationReader _configurationReader;
		private StringBuilder resultLog = new StringBuilder();

		public ScheduledJobSynchronizer(
			IScheduledJobRepository scheduledJobRepository,
            IScheduledJobExecutor scheduledJobExecutor,
            IConfigurationReader configurationReader)
        {
            Logger.Information("ScheduledJobSynchronizer initialized.");
			_scheduledJobRepository = scheduledJobRepository;
            _scheduledJobExecutor = scheduledJobExecutor;
            _configurationReader = configurationReader;
        }

		public string Synchronize(string environmentName)
        {
	        Logger.Information("ScheduledJobSynchronizer starting synchronization.");
            var syncConfiguration = _configurationReader.ReadConfiguration();

            if(syncConfiguration.ScheduledJobs == null)
            {
	            resultLog.AppendLine("No ScheduleJob config found.<br />");
                return resultLog.ToString();
            }

			UpdateScheduledJobs(syncConfiguration.ScheduledJobs);

            AutoRunScheduledJobs(syncConfiguration.ScheduledJobs);

			return resultLog.ToString();
        }

        private void UpdateScheduledJobs(List<ScheduledJobDefinition> scheduledJobConfiguration)
		{
			var existingScheduledJobs = _scheduledJobRepository.List().ToList();

			foreach (var job in scheduledJobConfiguration)
			{
				if (!string.IsNullOrEmpty(job.Id) && job.Id == "*")
                {
                    UpdateAllScheduleJobSettings(existingScheduledJobs, job);
                }
                else
                {
                    UpdateScheduleJobSettings(existingScheduledJobs, job);
                }
            }
		}

        private void UpdateAllScheduleJobSettings(List<ScheduledJob> existingScheduledJobs, ScheduledJobDefinition job)
        {
            foreach (var existingScheduledJob in existingScheduledJobs)
            {
                if (existingScheduledJob.IsEnabled == job.IsEnabled)
                {
                    continue;
                }

                existingScheduledJob.IsEnabled = job.IsEnabled;
                Logger.Debug($"Set {existingScheduledJob.Name} ({existingScheduledJob.ID}) to IsEnabled={job.IsEnabled}. After * (wildcard) spec.");
                resultLog.AppendLine($"Set {existingScheduledJob.Name} ({existingScheduledJob.ID}) to IsEnabled={job.IsEnabled}. After * (wildcard) spec.<br />");
                _scheduledJobRepository.Save(existingScheduledJob);
            }
        }

        private (ScheduledJob scheduledJob, string message) FindScheduledJob(List<ScheduledJob> existingScheduledJobs, ScheduledJobDefinition job)
        {
            ScheduledJob existingJob = null;
            var extraInfoMessage = string.Empty;

            try
            {
                if (!string.IsNullOrEmpty(job.Id))
                {
                    existingJob = existingScheduledJobs.FirstOrDefault(x => x.ID == Guid.Parse(job.Id));
                    extraInfoMessage = $"Id = {job.Id}";
                }
                else if (!string.IsNullOrEmpty(job.Name))
                {
                    existingJob = existingScheduledJobs.FirstOrDefault(x => x.Name == job.Name || x.AssemblyName == job.Name);
                    extraInfoMessage = $"Name/AssemblyName = {job.Name}";
                }
            }
            catch (Exception ex)
            {
                extraInfoMessage = $"id=\"{job.Id}\" name=\"{job.Name}\"";
                Logger.Error($"Error when try to loaf schedulejob id=\"{job.Id}\" name=\"{job.Name}\".", ex);
            }


            return (existingJob, extraInfoMessage);
        }

        private void UpdateScheduleJobSettings(List<ScheduledJob> existingScheduledJobs, ScheduledJobDefinition job)
        {
            var jobDetails = FindScheduledJob(existingScheduledJobs, job);

            ScheduledJob existingJob = jobDetails.scheduledJob;
            string extraInfoMessage = jobDetails.message;

            if (existingJob != null)
            {
                existingJob.IsEnabled = job.IsEnabled;
                Logger.Debug($"Set {existingJob.Name} ({existingJob.ID}) to IsEnabled={job.IsEnabled}.");
                resultLog.AppendLine($"Set {existingJob.Name} ({existingJob.ID}) to IsEnabled={job.IsEnabled}.<br />");
                _scheduledJobRepository.Save(existingJob);
            }
            else
            {
                Logger.Warning($"Could not find scheduled job with {extraInfoMessage}");
                resultLog.AppendLine($"Could not find scheduled job with {extraInfoMessage}<br />");
            }
        }

        private void AutoRunScheduledJobs(List<ScheduledJobDefinition> scheduledJobConfiguration)
        {
            var existingScheduledJobs = _scheduledJobRepository.List().ToList();
            var autoRunJobs = scheduledJobConfiguration.Where(j => j.AutoRun).ToList();

            foreach (var job in autoRunJobs)
            {
                AutoRunScheduleJob(existingScheduledJobs, job);
            }
        }

        private void AutoRunScheduleJob(List<ScheduledJob> existingScheduledJobs, ScheduledJobDefinition job)
        {
            var jobDetails = FindScheduledJob(existingScheduledJobs, job);

            ScheduledJob existingJob = jobDetails.scheduledJob;
            string extraInfoMessage = jobDetails.message;

            if (existingJob != null)
            {
                this._scheduledJobExecutor.StartAsync(existingJob,
                    new JobExecutionOptions
                    {
                        RunSynchronously = true,
                        Trigger = ScheduledJobTrigger.User
                    });

                Logger.Debug($"Ran {existingJob.Name} ({existingJob.ID}).");
                resultLog.AppendLine($"Ran {existingJob.Name} ({existingJob.ID}).<br />");
                
            }
            else
            {
                Logger.Warning($"Could not find scheduled job with {extraInfoMessage}");
                resultLog.AppendLine($"Could not find scheduled job with {extraInfoMessage}<br />");
            }
        }
    }
}
