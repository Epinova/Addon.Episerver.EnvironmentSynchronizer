using Xunit;
using System;
using Microsoft.Extensions.Configuration;
using FluentAssertions;
using System.IO;

namespace Addon.Episerver.EnvironmentSynchronizer.Configuration.Tests
{
	public class ConfigurationReaderTests
	{
		[Fact]
		public void ReadConfiguration_Null_Settings_Test()
		{
			// Arrange

			// Act
			var config = new ConfigurationBuilder()
				 .AddEnvironmentVariables()
				 .Build();

			// Assert
			config.Should().NotBeNull();
		}

		[Fact()]
		public void ReadConfiguration_SiteDefinition_All_Settings_Test()
		{
			// Arrange
			var configName = "all-settings.json";

			// Act
			var options = GetConfiguration(configName);

			//// Assert
			options.Should().NotBeNull();

			// Site Definitions
			options.SiteDefinitions.Should().NotBeNull();
			options.SiteDefinitions.Should().HaveCount(1);
			options.SiteDefinitions[0].Id.Should().Be("6AAEAF2F-20F9-41EB-8260-D0BBA76DB141");
			options.SiteDefinitions[0].Name.Should().Be("CustomerX");
			options.SiteDefinitions[0].SiteUrl.Should().Be("https://custxmstr972znb5prep.azurewebsites.net/");
			options.SiteDefinitions[0].Hosts.Should().HaveCount(2);

			options.SiteDefinitions[0].Hosts[0].Name.Should().Be("*");
			options.SiteDefinitions[0].Hosts[0].Language.Should().Be("en");
			options.SiteDefinitions[0].Hosts[0].UseSecureConnection.Should().BeFalse();
			options.SiteDefinitions[0].ForceLogin.Should().BeTrue();

			options.SiteDefinitions[0].SetRoles.Should().HaveCount(2);
			options.SiteDefinitions[0].SetRoles[0].Name.Should().Be("WebAdmins");
			options.SiteDefinitions[0].SetRoles[0].Access.Should().HaveCount(1);
			options.SiteDefinitions[0].SetRoles[0].Access[0].Should().Be("FullAccess");

			options.SiteDefinitions[0].SetRoles[1].Name.Should().Be("WebEditors");
			options.SiteDefinitions[0].SetRoles[1].Access.Should().HaveCount(3);
			options.SiteDefinitions[0].SetRoles[1].Access[0].Should().Be("Read");
			options.SiteDefinitions[0].SetRoles[1].Access[1].Should().Be("Create");
			options.SiteDefinitions[0].SetRoles[1].Access[2].Should().Be("Change");

			options.SiteDefinitions[0].RemoveRoles.Should().HaveCount(2);
			options.SiteDefinitions[0].RemoveRoles[0].Should().Be("Everyone");
			options.SiteDefinitions[0].RemoveRoles[1].Should().Be("TESTER");

			// Scheduled jobs
			options.ScheduledJobs.Should().NotBeNull();
			options.ScheduledJobs.Should().HaveCount(2);

			options.ScheduledJobs[0].Id.Should().Be("*");
			options.ScheduledJobs[0].Name.Should().Be("*");
			options.ScheduledJobs[0].IsEnabled.Should().BeFalse();
			options.ScheduledJobs[0].AutoRun.Should().BeFalse();

			options.ScheduledJobs[1].Id.Should().BeNull();
			options.ScheduledJobs[1].Name.Should().Be("YourScheduledJob");
			options.ScheduledJobs[1].IsEnabled.Should().BeTrue();
			options.ScheduledJobs[1].AutoRun.Should().BeTrue();
		}

		[Fact()]
		public void ReadConfiguration_SiteDefinition_No_Settings_Test()
		{
			// Arrange
			var configName = "no-settings.json";


			// Act
			var options = GetConfiguration(configName);

			// Assert
			options.Should().NotBeNull();

			options.RunAsInitializationModule.Should().BeFalse();
			options.RunInitializationModuleEveryStartup.Should().BeFalse();

			options.SiteDefinitions.Should().BeNull();
			options.ScheduledJobs.Should().BeNull();
		}

		[Fact()]
		public void ReadConfiguration_SiteDefinition_Test1_Settings_Test()
		{
			// Arrange
			var configName = "test1-settings.json";


			// Act
			var options = GetConfiguration(configName);

			// Assert
			options.Should().NotBeNull();

			options.RunAsInitializationModule.Should().BeFalse();
			options.RunInitializationModuleEveryStartup.Should().BeFalse();

			options.ScheduledJobs.Should().NotBeNull();
			options.ScheduledJobs.Should().HaveCount(2);

			options.ScheduledJobs[0].Id.Should().Be("8bd1ac63-9ed3-42e1-9b63-76498ab5ac94");
			options.ScheduledJobs[0].Name.Should().Be("Optimizely Notifications");
			options.ScheduledJobs[0].IsEnabled.Should().BeFalse();
			options.ScheduledJobs[0].AutoRun.Should().BeFalse();
		}

		public static EnvironmentSynchronizerOptions GetConfiguration(string name)
		{
			string[] paths = { Environment.CurrentDirectory, "test-configs" };
			string fullPath = Path.Combine(paths);
			var configuration = new EnvironmentSynchronizerOptions();

			var configurationBuilder = new ConfigurationBuilder();
			configurationBuilder
				.SetBasePath(fullPath)
				.AddJsonFile(name, optional: true)
				.Build()
				.GetSection(EnvironmentSynchronizerOptions.EnvironmentSynchronizer)
				.Bind(configuration);



			//var envSyncOptions = configurationBuilder.GetSection(EnvironmentSynchronizerOptions.EnvironmentSynchronizer).Get<EnvironmentSynchronizerOptions>();
			//services.AddSingleton<EnvironmentSynchronizerOptions>(envSyncOptions);

			return configuration;
		}

		
	}

	
}