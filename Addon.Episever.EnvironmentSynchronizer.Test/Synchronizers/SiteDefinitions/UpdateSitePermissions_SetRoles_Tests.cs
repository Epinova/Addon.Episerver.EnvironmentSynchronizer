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
	public class UpdateSitePermissions_SetRoles_Tests
	{
		[Fact]
		public void When_empty_list_of_setroles_is_found_nothing_should_be_done()
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
			environmentSynchronizerSiteDefinition.SetRoles = new List<SetRoleDefinition>();
			

			// Act
			Action action = () => siteDefinitionSynchronizer.UpdateSitePermissions(siteDefintion, environmentSynchronizerSiteDefinition);

			// Assert
			action.Should().NotThrow<Exception>();
			mockContentSecurityRepository.Verify(d => d.Save(startPageContentReference, contentSecurityDescriptor, SecuritySaveType.Replace), Times.Exactly(0));
		}

		[Fact]
		public void When_single_setroles_on_a_empty_existing_accesscontrolentrylist_save_as_new_role()
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
			environmentSynchronizerSiteDefinition.SetRoles = new List<SetRoleDefinition> {
				new SetRoleDefinition { Name = "CmsAdmin", Access = AccessLevel.Read }
			};

			// Act
			Action action = () => siteDefinitionSynchronizer.UpdateSitePermissions(siteDefintion, environmentSynchronizerSiteDefinition);

			// Assert
			action.Should().NotThrow<Exception>();
			mockContentSecurityRepository.Verify(d => d.Save(startPageContentReference, contentSecurityDescriptor, SecuritySaveType.Replace), Times.Exactly(1));

			contentSecurityDescriptor.Should().NotBeNull();
			contentSecurityDescriptor.Entries.Should().HaveCount(1);
			var descriptorList = contentSecurityDescriptor.Entries.ToList();
			descriptorList[0].Name.Should().Be("CmsAdmin");
			descriptorList[0].Access.Should().Be(AccessLevel.Read);
		}

		[Fact]
		public void When_single_setroles_on_a_existing_accesscontrolentrylist_save_as_update_role()
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
			environmentSynchronizerSiteDefinition.SetRoles = new List<SetRoleDefinition> {
				new SetRoleDefinition { Name = "CmsAdmin", Access = AccessLevel.Read }
			};

			// Act
			Action action = () => siteDefinitionSynchronizer.UpdateSitePermissions(siteDefintion, environmentSynchronizerSiteDefinition);

			// Assert
			action.Should().NotThrow<Exception>();
			mockContentSecurityRepository.Verify(d => d.Save(startPageContentReference, contentSecurityDescriptor, SecuritySaveType.Replace), Times.Exactly(1));

			contentSecurityDescriptor.Should().NotBeNull();
			contentSecurityDescriptor.Entries.Should().HaveCount(1);
			var descriptorList = contentSecurityDescriptor.Entries.ToList();
			descriptorList[0].Name.Should().Be("CmsAdmin");
			descriptorList[0].Access.Should().Be(AccessLevel.Read);
		}

		[Fact]
		public void When_single_setroles_on_many_existing_accesscontrolentrylist_save_as_update_role()
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
			environmentSynchronizerSiteDefinition.SetRoles = new List<SetRoleDefinition> {
				new SetRoleDefinition { Name = "CmsAdmin", Access = AccessLevel.Read }
			};

			// Act
			Action action = () => siteDefinitionSynchronizer.UpdateSitePermissions(siteDefintion, environmentSynchronizerSiteDefinition);

			// Assert
			action.Should().NotThrow<Exception>();
			mockContentSecurityRepository.Verify(d => d.Save(startPageContentReference, contentSecurityDescriptor, SecuritySaveType.Replace), Times.Exactly(1));

			contentSecurityDescriptor.Should().NotBeNull();
			contentSecurityDescriptor.Entries.Should().HaveCount(4);
			var descriptorList = contentSecurityDescriptor.Entries.ToList();
			descriptorList[3].Name.Should().Be("CmsAdmin");
			descriptorList[3].Access.Should().Be(AccessLevel.Read);
		}

		[Fact]
		public void When_two_setroles_on_many_existing_accesscontrolentrylist_save_as_update_role()
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
			environmentSynchronizerSiteDefinition.SetRoles = new List<SetRoleDefinition> {
				new SetRoleDefinition { Name = "CmsAdmin", Access = AccessLevel.Read },
				new SetRoleDefinition { Name = "CmsEdit", Access = AccessLevel.Read | AccessLevel.Create | AccessLevel.Edit | AccessLevel.Delete }
			};

			// Act
			Action action = () => siteDefinitionSynchronizer.UpdateSitePermissions(siteDefintion, environmentSynchronizerSiteDefinition);

			// Assert
			action.Should().NotThrow<Exception>();
			mockContentSecurityRepository.Verify(d => d.Save(startPageContentReference, contentSecurityDescriptor, SecuritySaveType.Replace), Times.Exactly(1));

			contentSecurityDescriptor.Should().NotBeNull();
			contentSecurityDescriptor.Entries.Should().HaveCount(4);
			var descriptorList = contentSecurityDescriptor.Entries.ToList();
			descriptorList[2].Name.Should().Be("CmsAdmin");
			descriptorList[2].Access.Should().Be(AccessLevel.Read);
			descriptorList[3].Name.Should().Be("CmsEdit");
			descriptorList[3].Access.Should().Be(AccessLevel.Read | AccessLevel.Create | AccessLevel.Edit | AccessLevel.Delete);
		}

	}
}
