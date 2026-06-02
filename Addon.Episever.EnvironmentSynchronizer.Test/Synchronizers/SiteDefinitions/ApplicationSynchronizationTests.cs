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
		public void ReadConfiguration_MapsApplicationHosts()
		{
			var options = new EnvironmentSynchronizerOptions
			{
				SiteDefinitions = new List<SiteDefinitionOptions>
				{
					new SiteDefinitionOptions
					{
						Id = "customerx",
						Name = "Customer X",
						Hosts = new List<HostOptions>
						{
							new HostOptions
							{
								Name = "customer.example",
								Type = ApplicationHostType.Primary,
								UseSecureConnection = true,
								Language = "en"
							}
						}
					}
				}
			};

			var configuration = new ConfigurationReader(Options.Create(options)).ReadConfiguration();
			var application = configuration.SiteDefinitions.Single();

			application.Id.Should().Be("customerx");
			application.Name.Should().Be("Customer X");
			application.IsDefault.Should().BeNull();
			application.Hosts.Should().ContainSingle();
			application.Hosts.Single().Authority.Should().Be("customer.example");
			application.Hosts.Single().Type.Should().Be(ApplicationHostType.Primary);
			application.Hosts.Single().PreferredUrlScheme.Should().Be(UrlScheme.Https);
			application.Hosts.Single().Locale.Name.Should().Be("en");
		}

		[Fact]
		public void ReadConfiguration_DoesNotMapLocaleForApplicationHostTypesThatRejectLocale()
		{
			var options = new EnvironmentSynchronizerOptions
			{
				SiteDefinitions = new List<SiteDefinitionOptions>
				{
					new SiteDefinitionOptions
					{
						Id = "customerx",
						Name = "Customer X",
						Hosts = new List<HostOptions>
						{
							new HostOptions
							{
								Name = "media.customer.example",
								Type = ApplicationHostType.Media,
								Language = "en"
							},
							new HostOptions
							{
								Name = "edit.customer.example",
								Type = ApplicationHostType.Edit,
								Language = "en"
							},
							new HostOptions
							{
								Name = "preview.customer.example",
								Type = ApplicationHostType.Preview,
								Language = "en"
							}
						}
					}
				}
			};

			var configuration = new ConfigurationReader(Options.Create(options)).ReadConfiguration();
			var application = configuration.SiteDefinitions.Single();

			application.Hosts.Should().HaveCount(3);
			application.Hosts.Should().OnlyContain(host => host.Locale == null);
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
				Id = "customerx",
				Name = "Customer X",
				IsDefault = true,
				Hosts = new List<ApplicationHost>
				{
					new ApplicationHost("customer.example")
					{
						Type = ApplicationHostType.Primary,
						PreferredUrlScheme = UrlScheme.Https
					},
					new ApplicationHost("edit.customer.example")
					{
						Type = ApplicationHostType.Edit,
						PreferredUrlScheme = UrlScheme.Https
					},
					new ApplicationHost("preview.customer.example")
					{
						Type = ApplicationHostType.Preview,
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
				It.Is<Application>(application =>
					application is InProcessWebsite &&
					application.DisplayName == "Customer X" &&
					((IRoutableApplication)application).Hosts.Any(host => host.Authority == "customer.example") &&
					((IRoutableApplication)application).Hosts.Any(host => host.Authority == "edit.customer.example") &&
					!((IRoutableApplication)application).Hosts.Any(host => host.Authority == "preview.customer.example")),
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
				Id = "headless",
				Name = "Headless Website",
				Hosts = new List<ApplicationHost>
				{
					new ApplicationHost("headless.example")
					{
						Type = ApplicationHostType.Primary,
						PreferredUrlScheme = UrlScheme.Https
					},
					new ApplicationHost("preview.headless.example")
					{
						Type = ApplicationHostType.Preview,
						PreferredUrlScheme = UrlScheme.Https
					},
					new ApplicationHost("redirect.headless.example")
					{
						Type = ApplicationHostType.RedirectPermanent,
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
				It.Is<Application>(application =>
					application is Website &&
					application.DisplayName == "Headless Website" &&
					((Website)application).Hosts.Any(host => host.Authority == "headless.example") &&
					((Website)application).Hosts.Any(host => host.Authority == "preview.headless.example") &&
					!((Website)application).Hosts.Any(host => host.Authority == "redirect.headless.example")),
				It.IsAny<CancellationToken>()), Times.Once);
			repository.Verify(x => x.MakeDefaultAsync(It.IsAny<IRoutableApplication>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
		}

		[Fact]
		public void Synchronize_MatchesApplicationByDisplayNameWhenIdIsEmpty()
		{
			var repository = new Mock<IApplicationRepository>();
			var contentSecurityRepository = new Mock<IContentSecurityRepository>();
			var configurationReader = new Mock<IConfigurationReader>();
			var existingApplication = new InProcessWebsite("customerx", new ContentReference(9))
			{
				DisplayName = "Customer X"
			};
			var applicationDefinition = new EnvironmentSynchronizerSiteDefinition
			{
				Name = "Customer X",
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
			configurationReader.Setup(x => x.ReadConfiguration()).Returns(new SynchronizationData
			{
				SiteDefinitions = new List<EnvironmentSynchronizerSiteDefinition> { applicationDefinition }
			});

			var subject = new SiteDefinitionSynchronizer(repository.Object, contentSecurityRepository.Object, configurationReader.Object);

			subject.Synchronize("Test");

			repository.Verify(x => x.SaveAsync(
				It.Is<Application>(application =>
					application.Name == "customerx" &&
					application.DisplayName == "Customer X" &&
					((IRoutableApplication)application).Hosts.Single().Authority == "customer.example"),
				It.IsAny<CancellationToken>()), Times.Once);
		}

		[Fact]
		public void Synchronize_DoesNotUpdateDefaultStateWhenIsDefaultIsNull()
		{
			var repository = new Mock<IApplicationRepository>();
			var contentSecurityRepository = new Mock<IContentSecurityRepository>();
			var configurationReader = new Mock<IConfigurationReader>();
			var existingApplication = new InProcessWebsite("customerx", new ContentReference(9));
			var applicationDefinition = new EnvironmentSynchronizerSiteDefinition
			{
				Id = "customerx",
				Name = "Customer X",
				IsDefault = null,
				Hosts = new List<ApplicationHost>()
			};

			repository.Setup(x => x.List()).Returns(new Application[] { existingApplication });
			repository.Setup(x => x.SaveAsync(It.IsAny<Application>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
			configurationReader.Setup(x => x.ReadConfiguration()).Returns(new SynchronizationData
			{
				SiteDefinitions = new List<EnvironmentSynchronizerSiteDefinition> { applicationDefinition }
			});

			var subject = new SiteDefinitionSynchronizer(repository.Object, contentSecurityRepository.Object, configurationReader.Object);

			subject.Synchronize("Test");

			repository.Verify(x => x.SaveAsync(It.IsAny<Application>(), It.IsAny<CancellationToken>()), Times.Once);
			repository.Verify(x => x.MakeDefaultAsync(It.IsAny<IRoutableApplication>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
		}

		[Fact]
		public void Synchronize_DoesNotSaveWhenApplicationCannotBeFound()
		{
			var repository = new Mock<IApplicationRepository>();
			var contentSecurityRepository = new Mock<IContentSecurityRepository>();
			var configurationReader = new Mock<IConfigurationReader>();
			var applicationDefinition = new EnvironmentSynchronizerSiteDefinition
			{
				Id = "missing",
				Name = "Missing Application",
				Hosts = new List<ApplicationHost>()
			};

			repository.Setup(x => x.List()).Returns(Array.Empty<Application>());
			configurationReader.Setup(x => x.ReadConfiguration()).Returns(new SynchronizationData
			{
				SiteDefinitions = new List<EnvironmentSynchronizerSiteDefinition> { applicationDefinition }
			});

			var subject = new SiteDefinitionSynchronizer(repository.Object, contentSecurityRepository.Object, configurationReader.Object);

			var result = subject.Synchronize("Test");

			result.Should().Contain("Could not find application Missing Application.");
			repository.Verify(x => x.SaveAsync(It.IsAny<Application>(), It.IsAny<CancellationToken>()), Times.Never);
			repository.Verify(x => x.MakeDefaultAsync(It.IsAny<IRoutableApplication>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
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
			var definition = new EnvironmentSynchronizerSiteDefinition { Name = "Customer X", ForceLogin = true };

			subject.UpdateSitePermissions(application, definition);

			securityDescriptor.Entries.Should().BeEmpty();
			contentSecurityRepository.Verify(x => x.Save(startPage, securityDescriptor, SecuritySaveType.Replace), Times.Once);
		}

		[Fact]
		public void UpdateSitePermissions_AppliesSetAndRemoveRolesOnApplicationEntryPoint()
		{
			var startPage = new ContentReference(9);
			var securityDescriptor = new FakeContentSecurityDescriptor
			{
				IsInherited = true,
				Entries = new List<AccessControlEntry>
				{
					new AccessControlEntry("Everyone", AccessLevel.Read),
					new AccessControlEntry("CmsAdmins", AccessLevel.FullAccess),
					new AccessControlEntry("WebsiteEditors", AccessLevel.Read)
				}
			};
			var repository = new Mock<IApplicationRepository>();
			var contentSecurityRepository = new Mock<IContentSecurityRepository>();
			var configurationReader = new Mock<IConfigurationReader>();
			contentSecurityRepository.Setup(x => x.Get(startPage)).Returns(securityDescriptor);

			var subject = new SiteDefinitionSynchronizer(repository.Object, contentSecurityRepository.Object, configurationReader.Object);
			var application = new InProcessWebsite("customerx", startPage);
			var definition = new EnvironmentSynchronizerSiteDefinition
			{
				Name = "Customer X",
				ForceLogin = true,
				SetRoles = new[]
				{
					new SetRoleDefinition { Name = "CmsAdmins", Access = AccessLevel.Read }
				},
				RemoveRoles = new[]
				{
					new RemoveRoleDefinition { Name = "WebsiteEditors" }
				}
			};

			subject.UpdateSitePermissions(application, definition);

			securityDescriptor.IsInherited.Should().BeFalse();
			securityDescriptor.Entries.Should().ContainSingle(entry => entry.Name == "CmsAdmins" && entry.Access == AccessLevel.Read);
			securityDescriptor.Entries.Should().NotContain(entry => entry.Name == "Everyone");
			securityDescriptor.Entries.Should().NotContain(entry => entry.Name == "WebsiteEditors");
			contentSecurityRepository.Verify(x => x.Save(startPage, securityDescriptor, SecuritySaveType.Replace), Times.Once);
			contentSecurityRepository.Verify(x => x.Save(startPage, securityDescriptor, SecuritySaveType.ReplaceChildPermissions), Times.Once);
		}

		[Fact]
		public void UpdateSitePermissions_DoesNotLoadSecurityDescriptorWhenNoPermissionOptionsAreConfigured()
		{
			var startPage = new ContentReference(9);
			var repository = new Mock<IApplicationRepository>();
			var contentSecurityRepository = new Mock<IContentSecurityRepository>();
			var configurationReader = new Mock<IConfigurationReader>();

			var subject = new SiteDefinitionSynchronizer(repository.Object, contentSecurityRepository.Object, configurationReader.Object);
			var application = new InProcessWebsite("customerx", startPage);
			var definition = new EnvironmentSynchronizerSiteDefinition { Name = "Customer X" };

			subject.UpdateSitePermissions(application, definition);

			contentSecurityRepository.Verify(x => x.Get(It.IsAny<ContentReference>()), Times.Never);
			contentSecurityRepository.Verify(x => x.Save(It.IsAny<ContentReference>(), It.IsAny<IContentSecurityDescriptor>(), It.IsAny<SecuritySaveType>()), Times.Never);
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
			var definition = new EnvironmentSynchronizerSiteDefinition { Name = "Headless Website", ForceLogin = true };

			subject.UpdateSitePermissions(application, definition);

			securityDescriptor.Entries.Should().BeEmpty();
			contentSecurityRepository.Verify(x => x.Save(startPage, securityDescriptor, SecuritySaveType.Replace), Times.Once);
		}
	}
}
