#if NET10_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Addon.Episerver.EnvironmentSynchronizer.Configuration;
using Addon.Episerver.EnvironmentSynchronizer.Models;
using Addon.Episerver.EnvironmentSynchronizer.Synchronizers.SiteDefinitions;
using EPiServer.Applications;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Security;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Addon.Episever.EnvironmentSynchronizer.Test.Synchronizers.SiteDefinitions
{
	public class ApplicationSynchronizationTests
	{
		[Fact]
		public void ReadConfiguration_MapsWildcardSiteHostToDefaultApplicationAndPrimaryHost()
		{
			var options = new EnvironmentSynchronizerOptions
			{
				SiteDefinitions = new List<SiteDefinitionOptions>
				{
					new SiteDefinitionOptions
					{
						Name = "customerx",
						SiteUrl = "https://customer.example/",
						Hosts = new List<HostOptions> { new HostOptions { Name = "*" } }
					}
				}
			};

			var configuration = new ConfigurationReader(Options.Create(options)).ReadConfiguration();
			var application = configuration.SiteDefinitions.Single();

			application.IsDefault.Should().BeTrue();
			application.Hosts.Should().ContainSingle();
			application.Hosts.Single().Authority.Should().Be("customer.example");
			application.Hosts.Single().Type.Should().Be(ApplicationHostType.Primary);
			application.Hosts.Single().PreferredUrlScheme.Should().Be(UrlScheme.Https);
		}

		[Fact]
		public void Synchronize_SavesMatchingApplicationAndUpdatesDefaultState()
		{
			var repository = new Mock<IApplicationRepository>();
			var contentSecurityRepository = new Mock<IContentSecurityRepository>();
			var configurationReader = new Mock<IConfigurationReader>();
			var existingApplication = new InProcessWebsite("customerx", new ContentReference(9));
			var applicationDefinition = new EnvironmentSynchronizerSiteDefinition
			{
				Name = "customerx",
				SiteUrl = new Uri("https://customer.example/"),
				IsDefault = true,
				Hosts = new List<ApplicationHost>
				{
					new ApplicationHost("customer.example")
					{
						Type = ApplicationHostType.Primary,
						PreferredUrlScheme = UrlScheme.Https
					}
				}
			};

			repository.Setup(x => x.List()).Returns(new Application[] { existingApplication });
			repository.Setup(x => x.SaveAsync(It.IsAny<Application>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
			repository.Setup(x => x.MakeDefaultAsync(It.IsAny<IRoutableApplication>(), true, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
			configurationReader.Setup(x => x.ReadConfiguration()).Returns(new SynchronizationData
			{
				SiteDefinitions = new List<EnvironmentSynchronizerSiteDefinition> { applicationDefinition }
			});

			var subject = new SiteDefinitionSynchronizer(repository.Object, contentSecurityRepository.Object, configurationReader.Object);

			subject.Synchronize("Test");

			repository.Verify(x => x.SaveAsync(
				It.Is<Application>(application => application is InProcessWebsite && ((IRoutableApplication)application).Hosts.Any(host => host.Authority == "customer.example")),
				It.IsAny<CancellationToken>()), Times.Once);
			repository.Verify(x => x.MakeDefaultAsync(It.IsAny<IRoutableApplication>(), true, It.IsAny<CancellationToken>()), Times.Once);
		}

		[Fact]
		public void Synchronize_SavesMatchingHeadlessWebsiteWithoutChangingItsType()
		{
			var repository = new Mock<IApplicationRepository>();
			var contentSecurityRepository = new Mock<IContentSecurityRepository>();
			var configurationReader = new Mock<IConfigurationReader>();
			var existingApplication = new Website("headless", new ContentReference(10));
			var applicationDefinition = new EnvironmentSynchronizerSiteDefinition
			{
				Name = "headless",
				SiteUrl = new Uri("https://headless.example/"),
				Hosts = new List<ApplicationHost>
				{
					new ApplicationHost("headless.example")
					{
						Type = ApplicationHostType.Primary,
						PreferredUrlScheme = UrlScheme.Https
					}
				}
			};

			repository.Setup(x => x.List()).Returns(new Application[] { existingApplication });
			repository.Setup(x => x.SaveAsync(It.IsAny<Application>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
			configurationReader.Setup(x => x.ReadConfiguration()).Returns(new SynchronizationData
			{
				SiteDefinitions = new List<EnvironmentSynchronizerSiteDefinition> { applicationDefinition }
			});

			var subject = new SiteDefinitionSynchronizer(repository.Object, contentSecurityRepository.Object, configurationReader.Object);

			subject.Synchronize("Test");

			repository.Verify(x => x.SaveAsync(
				It.Is<Application>(application => application is Website && ((Website)application).Hosts.Any(host => host.Authority == "headless.example")),
				It.IsAny<CancellationToken>()), Times.Once);
		}

		[Fact]
		public void UpdateSitePermissions_UsesApplicationEntryPoint()
		{
			var startPage = new ContentReference(9);
			var securityDescriptor = new FakeContentSecurityDescriptor
			{
				Entries = new List<AccessControlEntry> { new AccessControlEntry("Everyone", AccessLevel.Read) }
			};
			var repository = new Mock<IApplicationRepository>();
			var contentSecurityRepository = new Mock<IContentSecurityRepository>();
			var configurationReader = new Mock<IConfigurationReader>();
			contentSecurityRepository.Setup(x => x.Get(startPage)).Returns(securityDescriptor);

			var subject = new SiteDefinitionSynchronizer(repository.Object, contentSecurityRepository.Object, configurationReader.Object);
			var application = new InProcessWebsite("customerx", startPage);
			var definition = new EnvironmentSynchronizerSiteDefinition { Name = "customerx", ForceLogin = true };

			subject.UpdateSitePermissions(application, definition);

			securityDescriptor.Entries.Should().BeEmpty();
			contentSecurityRepository.Verify(x => x.Save(startPage, securityDescriptor, SecuritySaveType.Replace), Times.Once);
		}

		[Fact]
		public void UpdateSitePermissions_UsesHeadlessWebsiteEntryPoint()
		{
			var startPage = new ContentReference(10);
			var securityDescriptor = new FakeContentSecurityDescriptor
			{
				Entries = new List<AccessControlEntry> { new AccessControlEntry("Everyone", AccessLevel.Read) }
			};
			var repository = new Mock<IApplicationRepository>();
			var contentSecurityRepository = new Mock<IContentSecurityRepository>();
			var configurationReader = new Mock<IConfigurationReader>();
			contentSecurityRepository.Setup(x => x.Get(startPage)).Returns(securityDescriptor);

			var subject = new SiteDefinitionSynchronizer(repository.Object, contentSecurityRepository.Object, configurationReader.Object);
			var application = new Website("headless", startPage);
			var definition = new EnvironmentSynchronizerSiteDefinition { Name = "headless", ForceLogin = true };

			subject.UpdateSitePermissions(application, definition);

			securityDescriptor.Entries.Should().BeEmpty();
			contentSecurityRepository.Verify(x => x.Save(startPage, securityDescriptor, SecuritySaveType.Replace), Times.Once);
		}
	}
}
#endif
