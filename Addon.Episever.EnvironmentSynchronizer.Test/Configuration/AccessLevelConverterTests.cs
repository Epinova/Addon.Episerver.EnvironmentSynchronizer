using Xunit;
using FluentAssertions;
using System.Collections.Generic;
using EPiServer.Security;


namespace Addon.Episerver.EnvironmentSynchronizer.Configuration.Tests
{
	public class AccessLevelConverterTests
	{
		[Fact()]
		public void ConvertToAccessLevel_Empty_Test()
		{
			// Arrange
			var listOfValues = new List<string>();

			// Act
			var accessLevel = AccessLevelConverter.ConvertToAccessLevel(listOfValues);

			// Assert
			((int)accessLevel).Should().Be(0);
			FlagsHelper.IsSet(accessLevel, AccessLevel.Read).Should().BeFalse();
			FlagsHelper.IsSet(accessLevel, AccessLevel.Create).Should().BeFalse();
			FlagsHelper.IsSet(accessLevel, AccessLevel.Edit).Should().BeFalse();
			FlagsHelper.IsSet(accessLevel, AccessLevel.Delete).Should().BeFalse();
			FlagsHelper.IsSet(accessLevel, AccessLevel.Publish).Should().BeFalse();
			FlagsHelper.IsSet(accessLevel, AccessLevel.Administer).Should().BeFalse();
			FlagsHelper.IsSet(accessLevel, AccessLevel.FullAccess).Should().BeFalse();
		}

		[Fact()]
		public void ConvertToAccessLevel_Read_Test()
		{
			// Arrange
			var listOfValues = new List<string>();
			listOfValues.Add("Read");

			// Act
			var accessLevel = AccessLevelConverter.ConvertToAccessLevel(listOfValues);

			// Assert
			((int)accessLevel).Should().Be(1);
			FlagsHelper.IsSet(accessLevel, AccessLevel.Read).Should().BeTrue();
			FlagsHelper.IsSet(accessLevel, AccessLevel.Create).Should().BeFalse();
			FlagsHelper.IsSet(accessLevel, AccessLevel.Edit).Should().BeFalse();
			FlagsHelper.IsSet(accessLevel, AccessLevel.Delete).Should().BeFalse();
			FlagsHelper.IsSet(accessLevel, AccessLevel.Publish).Should().BeFalse();
			FlagsHelper.IsSet(accessLevel, AccessLevel.Administer).Should().BeFalse();
			FlagsHelper.IsSet(accessLevel, AccessLevel.FullAccess).Should().BeTrue();
		}

		[Fact()]
		public void ConvertToAccessLevel_ReadPublish_Test()
		{
			// Arrange
			var listOfValues = new List<string>();
			listOfValues.Add("Read");
			listOfValues.Add("Publish");

			// Act
			var accessLevel = AccessLevelConverter.ConvertToAccessLevel(listOfValues);

			// Assert
			((int)accessLevel).Should().Be(17);
			FlagsHelper.IsSet(accessLevel, AccessLevel.Read).Should().BeTrue();
			FlagsHelper.IsSet(accessLevel, AccessLevel.Create).Should().BeFalse();
			FlagsHelper.IsSet(accessLevel, AccessLevel.Edit).Should().BeFalse();
			FlagsHelper.IsSet(accessLevel, AccessLevel.Delete).Should().BeFalse();
			FlagsHelper.IsSet(accessLevel, AccessLevel.Publish).Should().BeTrue();
			FlagsHelper.IsSet(accessLevel, AccessLevel.Administer).Should().BeFalse();
			FlagsHelper.IsSet(accessLevel, AccessLevel.FullAccess).Should().BeTrue();
		}

		[Fact()]
		public void ConvertToAccessLevel_FullAccess_Test()
		{
			// Arrange
			var listOfValues = new List<string>();
			listOfValues.Add("FullAccess");

			// Act
			var accessLevel = AccessLevelConverter.ConvertToAccessLevel(listOfValues);

			// Assert
			((int)accessLevel).Should().Be(63);
			FlagsHelper.IsSet(accessLevel, AccessLevel.Read).Should().BeTrue();
			FlagsHelper.IsSet(accessLevel, AccessLevel.Create).Should().BeTrue();
			FlagsHelper.IsSet(accessLevel, AccessLevel.Edit).Should().BeTrue();
			FlagsHelper.IsSet(accessLevel, AccessLevel.Delete).Should().BeTrue();
			FlagsHelper.IsSet(accessLevel, AccessLevel.Publish).Should().BeTrue();
			FlagsHelper.IsSet(accessLevel, AccessLevel.Administer).Should().BeTrue();
			FlagsHelper.IsSet(accessLevel, AccessLevel.FullAccess).Should().BeTrue();
		}

		[Fact()]
		public void ConvertToAccessLevel_FullAccessPlusErroneous_Test()
		{
			// Arrange
			var listOfValues = new List<string>();
			listOfValues.Add("FullAccess");
			listOfValues.Add("Erroneous");

			// Act
			var accessLevel = AccessLevelConverter.ConvertToAccessLevel(listOfValues);

			// Assert
			((int)accessLevel).Should().Be(63);
			FlagsHelper.IsSet(accessLevel, AccessLevel.Read).Should().BeTrue();
			FlagsHelper.IsSet(accessLevel, AccessLevel.Create).Should().BeTrue();
			FlagsHelper.IsSet(accessLevel, AccessLevel.Edit).Should().BeTrue();
			FlagsHelper.IsSet(accessLevel, AccessLevel.Delete).Should().BeTrue();
			FlagsHelper.IsSet(accessLevel, AccessLevel.Publish).Should().BeTrue();
			FlagsHelper.IsSet(accessLevel, AccessLevel.Administer).Should().BeTrue();
			FlagsHelper.IsSet(accessLevel, AccessLevel.FullAccess).Should().BeTrue();
		}
	}


}