using Addon.Episerver.EnvironmentSynchronizer.Configuration;
using Addon.Episerver.EnvironmentSynchronizer.Synchronizers.SiteDefinitions;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Security;
using EPiServer.Web;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Addon.Episever.EnvironmentSynchronizer.Test.Synchronizers.SiteDefinitions
{
	public class UpdateSitePermissions_RemoveRoles_Tests
	{
		[Fact]
		public void When_empty_list_of_removeroles_is_found_nothing_should_be_done()
		{
			// Arrange
			var startPageContentReference = new ContentReference(9);

			var contentSecurityDescriptor = new FakeContentSecurityDescriptor();

			var mockSiteDefinitionRepository = new Mock<ISiteDefinitionRepository>();
			var mockContentSecurityRepository = new Mock<IContentSecurityRepository>();
			mockContentSecurityRepository.Setup(x => x.Get(startPageContentReference)).Returns(contentSecurityDescriptor);

			var mockConfigurationReader = new Mock<IConfigurationReader>();

			var siteDefinitionSynchronizer = new SiteDefinitionSynchronizer(mockSiteDefinitionRepository.Object, mockContentSecurityRepository.Object, mockConfigurationReader.Object);

			var siteDefintion = new SiteDefinition();
			siteDefintion.StartPage = startPageContentReference;
			var environmentSynchronizerSiteDefinition = new EnvironmentSynchronizerSiteDefinition();
			environmentSynchronizerSiteDefinition.RemoveRoles = new List<RemoveRoleDefinition>();
			

			// Act
			Action action = () => siteDefinitionSynchronizer.UpdateSitePermissions(siteDefintion, environmentSynchronizerSiteDefinition);

			// Assert
			action.Should().NotThrow<Exception>();
			mockContentSecurityRepository.Verify(d => d.Save(startPageContentReference, contentSecurityDescriptor, SecuritySaveType.Replace), Times.Exactly(0));
		}

		[Fact]
		public void When_single_removeroles_on_a_empty_existing_accesscontrolentrylist_nothing_should_happen()
		{
			// Arrange
			var startPageContentReference = new ContentReference(9);

			var listAce = new List<AccessControlEntry>();

			var contentSecurityDescriptor = new FakeContentSecurityDescriptor();
			contentSecurityDescriptor.Entries = listAce;

			var mockSiteDefinitionRepository = new Mock<ISiteDefinitionRepository>();
			var mockContentSecurityRepository = new Mock<IContentSecurityRepository>();
			mockContentSecurityRepository.Setup(x => x.Get(startPageContentReference)).Returns(contentSecurityDescriptor);
			mockContentSecurityRepository.Setup(x => x.Save(startPageContentReference, contentSecurityDescriptor, SecuritySaveType.Replace)).Verifiable();

			var mockConfigurationReader = new Mock<IConfigurationReader>();

			var siteDefinitionSynchronizer = new SiteDefinitionSynchronizer(mockSiteDefinitionRepository.Object, mockContentSecurityRepository.Object, mockConfigurationReader.Object);

			var siteDefintion = new SiteDefinition();
			siteDefintion.StartPage = startPageContentReference;
			var environmentSynchronizerSiteDefinition = new EnvironmentSynchronizerSiteDefinition();
			environmentSynchronizerSiteDefinition.ForceLogin = true;
			environmentSynchronizerSiteDefinition.RemoveRoles = new List<RemoveRoleDefinition> {
				new RemoveRoleDefinition { Name = "CmsAdmin" }
			};

			// Act
			Action action = () => siteDefinitionSynchronizer.UpdateSitePermissions(siteDefintion, environmentSynchronizerSiteDefinition);

			// Assert
			action.Should().NotThrow<Exception>();
			mockContentSecurityRepository.Verify(d => d.Save(startPageContentReference, contentSecurityDescriptor, SecuritySaveType.Replace), Times.Exactly(1));

			contentSecurityDescriptor.Should().NotBeNull();
			contentSecurityDescriptor.Entries.Should().HaveCount(0);
		}

		[Fact]
		public void When_single_removeroles_on_a_existing_accesscontrolentrylist_save_empty_roles()
		{
			// Arrange
			var startPageContentReference = new ContentReference(9);

			var listAce = new List<AccessControlEntry>();
			listAce.Add(new AccessControlEntry("CmsAdmin", AccessLevel.Read | AccessLevel.Create | AccessLevel.Edit | AccessLevel.Delete | AccessLevel.Administer));

			var contentSecurityDescriptor = new FakeContentSecurityDescriptor();
			contentSecurityDescriptor.Entries = listAce;

			var mockSiteDefinitionRepository = new Mock<ISiteDefinitionRepository>();
			var mockContentSecurityRepository = new Mock<IContentSecurityRepository>();
			mockContentSecurityRepository.Setup(x => x.Get(startPageContentReference)).Returns(contentSecurityDescriptor);
			mockContentSecurityRepository.Setup(x => x.Save(startPageContentReference, contentSecurityDescriptor, SecuritySaveType.Replace)).Verifiable();

			var mockConfigurationReader = new Mock<IConfigurationReader>();

			var siteDefinitionSynchronizer = new SiteDefinitionSynchronizer(mockSiteDefinitionRepository.Object, mockContentSecurityRepository.Object, mockConfigurationReader.Object);

			var siteDefintion = new SiteDefinition();
			siteDefintion.StartPage = startPageContentReference;
			var environmentSynchronizerSiteDefinition = new EnvironmentSynchronizerSiteDefinition();
			environmentSynchronizerSiteDefinition.ForceLogin = true;
			environmentSynchronizerSiteDefinition.RemoveRoles = new List<RemoveRoleDefinition> {
				new RemoveRoleDefinition { Name = "CmsAdmin" }
			};

			// Act
			Action action = () => siteDefinitionSynchronizer.UpdateSitePermissions(siteDefintion, environmentSynchronizerSiteDefinition);

			// Assert
			action.Should().NotThrow<Exception>();
			mockContentSecurityRepository.Verify(d => d.Save(startPageContentReference, contentSecurityDescriptor, SecuritySaveType.Replace), Times.Exactly(1));

			contentSecurityDescriptor.Should().NotBeNull();
			contentSecurityDescriptor.Entries.Should().HaveCount(0);
		}

		[Fact]
		public void When_single_removeroles_on_many_existing_accesscontrolentrylist_save_as_one_less_role()
		{
			// Arrange
			var startPageContentReference = new ContentReference(9);

			var listAce = new List<AccessControlEntry>();
			listAce.Add(new AccessControlEntry("WebsiteAdmin", AccessLevel.Read | AccessLevel.Create | AccessLevel.Edit | AccessLevel.Delete | AccessLevel.Administer));
			listAce.Add(new AccessControlEntry("WebsiteEdit", AccessLevel.Read | AccessLevel.Create | AccessLevel.Edit | AccessLevel.Delete));
			listAce.Add(new AccessControlEntry("CmsAdmin", AccessLevel.Read | AccessLevel.Create | AccessLevel.Edit | AccessLevel.Delete | AccessLevel.Administer));
			listAce.Add(new AccessControlEntry("CmsEdit", AccessLevel.Read | AccessLevel.Create | AccessLevel.Edit | AccessLevel.Delete));

			var contentSecurityDescriptor = new FakeContentSecurityDescriptor();
			contentSecurityDescriptor.Entries = listAce;

			var mockSiteDefinitionRepository = new Mock<ISiteDefinitionRepository>();
			var mockContentSecurityRepository = new Mock<IContentSecurityRepository>();
			mockContentSecurityRepository.Setup(x => x.Get(startPageContentReference)).Returns(contentSecurityDescriptor);
			mockContentSecurityRepository.Setup(x => x.Save(startPageContentReference, contentSecurityDescriptor, SecuritySaveType.Replace)).Verifiable();

			var mockConfigurationReader = new Mock<IConfigurationReader>();

			var siteDefinitionSynchronizer = new SiteDefinitionSynchronizer(mockSiteDefinitionRepository.Object, mockContentSecurityRepository.Object, mockConfigurationReader.Object);

			var siteDefintion = new SiteDefinition();
			siteDefintion.StartPage = startPageContentReference;
			var environmentSynchronizerSiteDefinition = new EnvironmentSynchronizerSiteDefinition();
			environmentSynchronizerSiteDefinition.ForceLogin = true;
			environmentSynchronizerSiteDefinition.RemoveRoles = new List<RemoveRoleDefinition> {
				new RemoveRoleDefinition { Name = "CmsAdmin" }
			};

			// Act
			Action action = () => siteDefinitionSynchronizer.UpdateSitePermissions(siteDefintion, environmentSynchronizerSiteDefinition);

			// Assert
			action.Should().NotThrow<Exception>();
			mockContentSecurityRepository.Verify(d => d.Save(startPageContentReference, contentSecurityDescriptor, SecuritySaveType.Replace), Times.Exactly(1));

			contentSecurityDescriptor.Should().NotBeNull();
			contentSecurityDescriptor.Entries.Should().HaveCount(3);
			var descriptorList = contentSecurityDescriptor.Entries.ToList();
			descriptorList[0].Name.Should().NotBe("CmsAdmin");
			descriptorList[1].Name.Should().NotBe("CmsAdmin");
			descriptorList[2].Name.Should().NotBe("CmsAdmin");
		}

		[Fact]
		public void When_two_removeroles_on_many_existing_accesscontrolentrylist_save_two_less_roles()
		{
			// Arrange
			var startPageContentReference = new ContentReference(9);

			var listAce = new List<AccessControlEntry>();
			listAce.Add(new AccessControlEntry("WebsiteAdmin", AccessLevel.Read | AccessLevel.Create | AccessLevel.Edit | AccessLevel.Delete | AccessLevel.Administer));
			listAce.Add(new AccessControlEntry("WebsiteEdit", AccessLevel.Read | AccessLevel.Create | AccessLevel.Edit | AccessLevel.Delete));
			listAce.Add(new AccessControlEntry("CmsAdmin", AccessLevel.Read | AccessLevel.Create | AccessLevel.Edit | AccessLevel.Delete | AccessLevel.Administer));
			listAce.Add(new AccessControlEntry("CmsEdit", AccessLevel.Read | AccessLevel.Create | AccessLevel.Edit | AccessLevel.Delete));

			var contentSecurityDescriptor = new FakeContentSecurityDescriptor();
			contentSecurityDescriptor.Entries = listAce;

			var mockSiteDefinitionRepository = new Mock<ISiteDefinitionRepository>();
			var mockContentSecurityRepository = new Mock<IContentSecurityRepository>();
			mockContentSecurityRepository.Setup(x => x.Get(startPageContentReference)).Returns(contentSecurityDescriptor);
			mockContentSecurityRepository.Setup(x => x.Save(startPageContentReference, contentSecurityDescriptor, SecuritySaveType.Replace)).Verifiable();

			var mockConfigurationReader = new Mock<IConfigurationReader>();

			var siteDefinitionSynchronizer = new SiteDefinitionSynchronizer(mockSiteDefinitionRepository.Object, mockContentSecurityRepository.Object, mockConfigurationReader.Object);

			var siteDefintion = new SiteDefinition();
			siteDefintion.StartPage = startPageContentReference;
			var environmentSynchronizerSiteDefinition = new EnvironmentSynchronizerSiteDefinition();
			environmentSynchronizerSiteDefinition.ForceLogin = true;
			environmentSynchronizerSiteDefinition.RemoveRoles = new List<RemoveRoleDefinition> {
				new RemoveRoleDefinition { Name = "CmsAdmin" },
				new RemoveRoleDefinition { Name = "CmsEdit" }
			};

			// Act
			Action action = () => siteDefinitionSynchronizer.UpdateSitePermissions(siteDefintion, environmentSynchronizerSiteDefinition);

			// Assert
			action.Should().NotThrow<Exception>();
			mockContentSecurityRepository.Verify(d => d.Save(startPageContentReference, contentSecurityDescriptor, SecuritySaveType.Replace), Times.Exactly(1));

			contentSecurityDescriptor.Should().NotBeNull();
			contentSecurityDescriptor.Entries.Should().HaveCount(2);
			var descriptorList = contentSecurityDescriptor.Entries.ToList();
			descriptorList[0].Name.Should().NotBe("CmsAdmin");
			descriptorList[1].Name.Should().NotBe("CmsAdmin");
			descriptorList[0].Name.Should().NotBe("CmsEdit");
			descriptorList[1].Name.Should().NotBe("CmsEdit");
		}

		[Fact]
		public void When_forcelogin_two_setroles_two_removeroles_on_many_existing_accesscontrolentrylist_save_three_less_roles()
		{
			// Arrange
			var startPageContentReference = new ContentReference(9);

			var listAce = new List<AccessControlEntry>();
			listAce.Add(new AccessControlEntry("WebsiteAdmin", AccessLevel.Read | AccessLevel.Create | AccessLevel.Edit | AccessLevel.Delete | AccessLevel.Administer));
			listAce.Add(new AccessControlEntry("WebsiteEdit", AccessLevel.Read | AccessLevel.Create | AccessLevel.Edit | AccessLevel.Delete));
			listAce.Add(new AccessControlEntry("Everyone", AccessLevel.Read));
			listAce.Add(new AccessControlEntry("CmsAdmin", AccessLevel.Read | AccessLevel.Create | AccessLevel.Edit | AccessLevel.Delete | AccessLevel.Administer));
			listAce.Add(new AccessControlEntry("CmsEdit", AccessLevel.Read | AccessLevel.Create | AccessLevel.Edit | AccessLevel.Delete));

			var contentSecurityDescriptor = new FakeContentSecurityDescriptor();
			contentSecurityDescriptor.Entries = listAce;

			var mockSiteDefinitionRepository = new Mock<ISiteDefinitionRepository>();
			var mockContentSecurityRepository = new Mock<IContentSecurityRepository>();
			mockContentSecurityRepository.Setup(x => x.Get(startPageContentReference)).Returns(contentSecurityDescriptor);
			mockContentSecurityRepository.Setup(x => x.Save(startPageContentReference, contentSecurityDescriptor, SecuritySaveType.Replace)).Verifiable();

			var mockConfigurationReader = new Mock<IConfigurationReader>();

			var siteDefinitionSynchronizer = new SiteDefinitionSynchronizer(mockSiteDefinitionRepository.Object, mockContentSecurityRepository.Object, mockConfigurationReader.Object);

			var siteDefintion = new SiteDefinition();
			siteDefintion.StartPage = startPageContentReference;
			var environmentSynchronizerSiteDefinition = new EnvironmentSynchronizerSiteDefinition();
			environmentSynchronizerSiteDefinition.ForceLogin = true;
			environmentSynchronizerSiteDefinition.SetRoles = new List<SetRoleDefinition> {
				new SetRoleDefinition { Name = "CmsAdmin", Access = AccessLevel.Read },
				new SetRoleDefinition { Name = "CmsEdit", Access = AccessLevel.Read | AccessLevel.Create | AccessLevel.Edit | AccessLevel.Delete }
			};
			environmentSynchronizerSiteDefinition.RemoveRoles = new List<RemoveRoleDefinition> {
				new RemoveRoleDefinition { Name = "WebsiteAdmin" },
				new RemoveRoleDefinition { Name = "WebsiteEdit" }
			};

			// Act
			Action action = () => siteDefinitionSynchronizer.UpdateSitePermissions(siteDefintion, environmentSynchronizerSiteDefinition);

			// Assert
			action.Should().NotThrow<Exception>();
			mockContentSecurityRepository.Verify(d => d.Save(startPageContentReference, contentSecurityDescriptor, SecuritySaveType.Replace), Times.Exactly(1));

			contentSecurityDescriptor.Should().NotBeNull();
			contentSecurityDescriptor.Entries.Should().HaveCount(2);
			var descriptorList = contentSecurityDescriptor.Entries.ToList();
			descriptorList[0].Name.Should().NotBe("WebsiteAdmin");
			descriptorList[1].Name.Should().NotBe("WebsiteAdmin");
			descriptorList[0].Name.Should().NotBe("WebsiteEdit");
			descriptorList[1].Name.Should().NotBe("WebsiteEdit");
			descriptorList[0].Name.Should().NotBe("Everyone");
			descriptorList[1].Name.Should().NotBe("Everyone");

			descriptorList[0].Name.Should().Be("CmsAdmin");
			descriptorList[0].Access.Should().Be(AccessLevel.Read);
			descriptorList[1].Name.Should().Be("CmsEdit");
			descriptorList[1].Access.Should().Be(AccessLevel.Read | AccessLevel.Create | AccessLevel.Edit | AccessLevel.Delete);
		}

	}
}
