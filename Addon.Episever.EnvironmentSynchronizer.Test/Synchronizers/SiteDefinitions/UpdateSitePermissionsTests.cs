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
using Xunit;

namespace Addon.Episever.EnvironmentSynchronizer.Test.Synchronizers.SiteDefinitions
{
	public class UpdateSitePermissionsTests
	{
		[Fact]
		public void When_no_startpage_exists_on_sitedefinition_nothing_should_be_done()
		{
			// Arrange
			var mockSiteDefinitionRepository = new Mock<ISiteDefinitionRepository>();
			var mockContentSecurityRepository = new Mock<IContentSecurityRepository>();
			var mockConfigurationReader = new Mock<IConfigurationReader>();

			var siteDefinitionSynchronizer = new SiteDefinitionSynchronizer(mockSiteDefinitionRepository.Object, mockContentSecurityRepository.Object, mockConfigurationReader.Object);

			var siteDefintion = new SiteDefinition();
			var environmentSynchronizerSiteDefinition = new EnvironmentSynchronizerSiteDefinition(); 

			// Act
			Action action = () => siteDefinitionSynchronizer.UpdateSitePermissions(siteDefintion, environmentSynchronizerSiteDefinition);

			// Assert
			action.Should().NotThrow<Exception>();
		}

		[Fact]
		public void When_no_contentsecuritydescriptor_for_startpage_is_found_nothing_should_be_done()
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
			Action action = () => siteDefinitionSynchronizer.UpdateSitePermissions(siteDefintion, environmentSynchronizerSiteDefinition);

			// Assert
			action.Should().NotThrow<Exception>();
		}

		[Fact]
		public void When_noforcelogin_nosetroles_noremoveroles_nothing_should_be_done()
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
			Action action = () => siteDefinitionSynchronizer.UpdateSitePermissions(siteDefintion, environmentSynchronizerSiteDefinition);

			// Assert
			action.Should().NotThrow<Exception>();
			mockContentSecurityRepository.Verify(d => d.Save(startPageContentReference, contentSecurityDescriptor, SecuritySaveType.Replace), Times.Exactly(0));

			contentSecurityDescriptor.Should().NotBeNull();
			contentSecurityDescriptor.Entries.Should().HaveCount(0);
		}
	}
}
