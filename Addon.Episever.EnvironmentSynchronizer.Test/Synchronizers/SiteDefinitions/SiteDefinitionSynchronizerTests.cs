using Addon.Episerver.EnvironmentSynchronizer.Configuration;
using Addon.Episerver.EnvironmentSynchronizer.Synchronizers.SiteDefinitions;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Security;
using EPiServer.Web;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace Addon.Episever.EnvironmentSynchronizer.Test.Synchronizers.SiteDefinitions
{
	public class SiteDefinitionSynchronizerTests
	{
		[Fact]
		public void RemoveAccessForEveryoneRole_NullStartPage_Nothing()
		{
			// Arrange
			var mockSiteDefinitionRepository = new Mock<ISiteDefinitionRepository>();
			var mockContentSecurityRepository = new Mock<IContentSecurityRepository>();
			var mockConfigurationReader = new Mock<IConfigurationReader>();

			var siteDefinitionSynchronizer = new SiteDefinitionSynchronizer(mockSiteDefinitionRepository.Object, mockContentSecurityRepository.Object, mockConfigurationReader.Object);

			var siteDefintion = new SiteDefinition();
			var environmentSynchronizerSiteDefinition = new EnvironmentSynchronizerSiteDefinition();

			// Act
			Action action = () => siteDefinitionSynchronizer.RemoveAccessForEveryoneRole(siteDefintion, environmentSynchronizerSiteDefinition);

			// Assert
			action.Should().NotThrow<Exception>();
		}

		[Fact]
		public void RemoveAccessForEveryoneRole_NullSecirityDescriptor_Nothing()
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

			// Act
			Action action = () => siteDefinitionSynchronizer.RemoveAccessForEveryoneRole(siteDefintion, environmentSynchronizerSiteDefinition);

			// Assert
			action.Should().NotThrow<Exception>();
		}

		[Fact]
		public void RemoveAccessForEveryoneRole_EmptyAccessControlEntryList_Nothing()
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

			// Act
			Action action = () => siteDefinitionSynchronizer.RemoveAccessForEveryoneRole(siteDefintion, environmentSynchronizerSiteDefinition);

			// Assert
			action.Should().NotThrow<Exception>();
			mockContentSecurityRepository.Verify(d => d.Save(startPageContentReference, contentSecurityDescriptor, SecuritySaveType.Replace), Times.Exactly(1));

			contentSecurityDescriptor.Should().NotBeNull();
			contentSecurityDescriptor.Entries.Should().HaveCount(0);
		}

		[Fact]
		public void RemoveAccessForEveryoneRole_AccessControlEntryListOnlyEveryone_EmptyList()
		{
			// Arrange
			var startPageContentReference = new ContentReference(9);

			var listAce = new List<AccessControlEntry>();
			listAce.Add(new AccessControlEntry("Everyone", AccessLevel.Read));

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

			// Act
			Action action = () => siteDefinitionSynchronizer.RemoveAccessForEveryoneRole(siteDefintion, environmentSynchronizerSiteDefinition);

			// Assert
			action.Should().NotThrow<Exception>();
			mockContentSecurityRepository.Verify(d => d.Save(startPageContentReference, contentSecurityDescriptor, SecuritySaveType.Replace), Times.Exactly(1));

			contentSecurityDescriptor.Should().NotBeNull();
			contentSecurityDescriptor.Entries.Should().HaveCount(0);
		}

		[Fact]
		public void RemoveAccessForEveryoneRole_ACEListEveryoneAndOther_NoEveryone()
		{
			// Arrange
			var startPageContentReference = new ContentReference(9);

			var listAce = new List<AccessControlEntry>();
			listAce.Add(new AccessControlEntry("Everyone", AccessLevel.Read | AccessLevel.Create));
			listAce.Add(new AccessControlEntry("CmsAdmin", AccessLevel.Read | AccessLevel.Create | AccessLevel.Edit | AccessLevel.Delete | AccessLevel.Administer));
			listAce.Add(new AccessControlEntry("CmsEdit", AccessLevel.Read | AccessLevel.Create | AccessLevel.Edit | AccessLevel.Delete));

			var contentSecurityDescriptor = new FakeContentSecurityDescriptor();
			contentSecurityDescriptor.Entries = listAce;

			var mockSiteDefinitionRepository = new Mock<ISiteDefinitionRepository>();
			var mockContentSecurityRepository = new Mock<IContentSecurityRepository>();
			mockContentSecurityRepository.Setup(x => x.Get(startPageContentReference)).Returns(contentSecurityDescriptor);
			mockContentSecurityRepository.Setup(x => x.Save(startPageContentReference, contentSecurityDescriptor, SecuritySaveType.Replace)).Verifiable();
			mockContentSecurityRepository.Setup(x => x.Save(startPageContentReference, contentSecurityDescriptor, SecuritySaveType.ReplaceChildPermissions)).Verifiable();

			var mockConfigurationReader = new Mock<IConfigurationReader>();

			var siteDefinitionSynchronizer = new SiteDefinitionSynchronizer(mockSiteDefinitionRepository.Object, mockContentSecurityRepository.Object, mockConfigurationReader.Object);

			var siteDefintion = new SiteDefinition();
			siteDefintion.StartPage = startPageContentReference;
			var environmentSynchronizerSiteDefinition = new EnvironmentSynchronizerSiteDefinition();

			// Act
			Action action = () => siteDefinitionSynchronizer.RemoveAccessForEveryoneRole(siteDefintion, environmentSynchronizerSiteDefinition);

			// Assert
			action.Should().NotThrow<Exception>();
			mockContentSecurityRepository.Verify(d => d.Save(startPageContentReference, contentSecurityDescriptor, SecuritySaveType.Replace), Times.Exactly(1));
			mockContentSecurityRepository.Verify(d => d.Save(startPageContentReference, contentSecurityDescriptor, SecuritySaveType.ReplaceChildPermissions), Times.Exactly(1));

			contentSecurityDescriptor.Should().NotBeNull();
			contentSecurityDescriptor.Entries.Should().HaveCount(2);
			contentSecurityDescriptor.Entries.Should().NotContain(new AccessControlEntry("Everyone", AccessLevel.Read | AccessLevel.Create));
			contentSecurityDescriptor.Entries.Should().Contain(new AccessControlEntry("CmsAdmin", AccessLevel.Read | AccessLevel.Create | AccessLevel.Edit | AccessLevel.Delete | AccessLevel.Administer));
			contentSecurityDescriptor.Entries.Should().Contain(new AccessControlEntry("CmsEdit", AccessLevel.Read | AccessLevel.Create | AccessLevel.Edit | AccessLevel.Delete));
		}

		[Fact]
		public void RemoveAccessForEveryoneRole_ACEListEveryoneAndManyOther_NoEveryone()
		{
			// Arrange
			var startPageContentReference = new ContentReference(9);

			var listAce = new List<AccessControlEntry>();
			listAce.Add(new AccessControlEntry("WebsiteAdmin", AccessLevel.Read | AccessLevel.Create | AccessLevel.Edit | AccessLevel.Delete | AccessLevel.Administer));
			listAce.Add(new AccessControlEntry("WebsiteEdit", AccessLevel.Read | AccessLevel.Create | AccessLevel.Edit | AccessLevel.Delete));
			listAce.Add(new AccessControlEntry("Everyone", AccessLevel.Read | AccessLevel.Create));
			listAce.Add(new AccessControlEntry("CmsAdmin", AccessLevel.Read | AccessLevel.Create | AccessLevel.Edit | AccessLevel.Delete | AccessLevel.Administer));
			listAce.Add(new AccessControlEntry("CmsEdit", AccessLevel.Read | AccessLevel.Create | AccessLevel.Edit | AccessLevel.Delete));

			var contentSecurityDescriptor = new FakeContentSecurityDescriptor();
			contentSecurityDescriptor.Entries = listAce;

			var mockSiteDefinitionRepository = new Mock<ISiteDefinitionRepository>();
			var mockContentSecurityRepository = new Mock<IContentSecurityRepository>();
			mockContentSecurityRepository.Setup(x => x.Get(startPageContentReference)).Returns(contentSecurityDescriptor);
			mockContentSecurityRepository.Setup(x => x.Save(startPageContentReference, contentSecurityDescriptor, SecuritySaveType.Replace)).Verifiable();
			mockContentSecurityRepository.Setup(x => x.Save(startPageContentReference, contentSecurityDescriptor, SecuritySaveType.ReplaceChildPermissions)).Verifiable();

			var mockConfigurationReader = new Mock<IConfigurationReader>();

			var siteDefinitionSynchronizer = new SiteDefinitionSynchronizer(mockSiteDefinitionRepository.Object, mockContentSecurityRepository.Object, mockConfigurationReader.Object);

			var siteDefintion = new SiteDefinition();
			siteDefintion.StartPage = startPageContentReference;
			var environmentSynchronizerSiteDefinition = new EnvironmentSynchronizerSiteDefinition();

			// Act
			Action action = () => siteDefinitionSynchronizer.RemoveAccessForEveryoneRole(siteDefintion, environmentSynchronizerSiteDefinition);

			// Assert
			action.Should().NotThrow<Exception>();
			mockContentSecurityRepository.Verify(d => d.Save(startPageContentReference, contentSecurityDescriptor, SecuritySaveType.Replace), Times.Exactly(1));
			mockContentSecurityRepository.Verify(d => d.Save(startPageContentReference, contentSecurityDescriptor, SecuritySaveType.ReplaceChildPermissions), Times.Exactly(1));

			contentSecurityDescriptor.Should().NotBeNull();
			contentSecurityDescriptor.Entries.Should().HaveCount(4);
			contentSecurityDescriptor.Entries.Should().NotContain(new AccessControlEntry("Everyone", AccessLevel.Read | AccessLevel.Create));
			contentSecurityDescriptor.Entries.Should().Contain(new AccessControlEntry("WebsiteAdmin", AccessLevel.Read | AccessLevel.Create | AccessLevel.Edit | AccessLevel.Delete | AccessLevel.Administer));
			contentSecurityDescriptor.Entries.Should().Contain(new AccessControlEntry("WebsiteEdit", AccessLevel.Read | AccessLevel.Create | AccessLevel.Edit | AccessLevel.Delete));
			contentSecurityDescriptor.Entries.Should().Contain(new AccessControlEntry("CmsAdmin", AccessLevel.Read | AccessLevel.Create | AccessLevel.Edit | AccessLevel.Delete | AccessLevel.Administer));
			contentSecurityDescriptor.Entries.Should().Contain(new AccessControlEntry("CmsEdit", AccessLevel.Read | AccessLevel.Create | AccessLevel.Edit | AccessLevel.Delete));
		}

		[Fact]
		public void RemoveAccessForEveryoneRole_ACEListNoEveryoneAndManyOther_NoEveryone()
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
			mockContentSecurityRepository.Setup(x => x.Save(startPageContentReference, contentSecurityDescriptor, SecuritySaveType.ReplaceChildPermissions)).Verifiable();

			var mockConfigurationReader = new Mock<IConfigurationReader>();

			var siteDefinitionSynchronizer = new SiteDefinitionSynchronizer(mockSiteDefinitionRepository.Object, mockContentSecurityRepository.Object, mockConfigurationReader.Object);

			var siteDefintion = new SiteDefinition();
			siteDefintion.StartPage = startPageContentReference;
			var environmentSynchronizerSiteDefinition = new EnvironmentSynchronizerSiteDefinition();

			// Act
			Action action = () => siteDefinitionSynchronizer.RemoveAccessForEveryoneRole(siteDefintion, environmentSynchronizerSiteDefinition);

			// Assert
			action.Should().NotThrow<Exception>();
			mockContentSecurityRepository.Verify(d => d.Save(startPageContentReference, contentSecurityDescriptor, SecuritySaveType.Replace), Times.Exactly(1));
			mockContentSecurityRepository.Verify(d => d.Save(startPageContentReference, contentSecurityDescriptor, SecuritySaveType.ReplaceChildPermissions), Times.Exactly(1));

			contentSecurityDescriptor.Should().NotBeNull();
			contentSecurityDescriptor.Entries.Should().HaveCount(4);
			contentSecurityDescriptor.Entries.Should().Contain(new AccessControlEntry("WebsiteAdmin", AccessLevel.Read | AccessLevel.Create | AccessLevel.Edit | AccessLevel.Delete | AccessLevel.Administer));
			contentSecurityDescriptor.Entries.Should().Contain(new AccessControlEntry("WebsiteEdit", AccessLevel.Read | AccessLevel.Create | AccessLevel.Edit | AccessLevel.Delete));
			contentSecurityDescriptor.Entries.Should().Contain(new AccessControlEntry("CmsAdmin", AccessLevel.Read | AccessLevel.Create | AccessLevel.Edit | AccessLevel.Delete | AccessLevel.Administer));
			contentSecurityDescriptor.Entries.Should().Contain(new AccessControlEntry("CmsEdit", AccessLevel.Read | AccessLevel.Create | AccessLevel.Edit | AccessLevel.Delete));
		}
	}
}
