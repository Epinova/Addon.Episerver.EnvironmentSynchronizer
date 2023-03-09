using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EPiServer.Web;

namespace Addon.Episerver.EnvironmentSynchronizer.Configuration
{
    public class EnvironmentSynchronizerOptions
    {
        public const string EnvironmentSynchronizer = "EnvironmentSynchronizer";

        public bool RunAsInitializationModule { get; set; }

        public bool RunInitializationModuleEveryStartup {  get; set;}

		public IList<SiteDefinitionOptions> SiteDefinitions { get; set; }

        public IList<ScheduledJobOptions> ScheduledJobs { get; set; }

    }

    public class ScheduledJobOptions
    {
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public bool IsEnabled { get; set; }

        public bool AutoRun { get; set; }

        public bool IsRequired { get; set; }
    }

    public class SiteDefinitionOptions
    {
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string SiteUrl { get; set; }

		public bool ForceLogin { get; set; }

		[Required]
        public IList<HostOptions> Hosts { get; set; }

		public IList<SetRoleOptions> SetRoles { get; set; }

		public IList<string> RemoveRoles { get; set; }
	}

    public class HostOptions
    {
        public string Name { get; set; }

        public bool UseSecureConnection { get; set; }

        public HostDefinitionType Type { get; set; }

        public string Language { get; set; }
    }

    public class SetRoleOptions
    {
        public string Name { get; set; }

        public IList<string> Access { get; set; }
    }
}
