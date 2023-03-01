using Xunit;
using System;
using FluentAssertions;
using System.Collections.Generic;
using System.Collections;
using EPiServer.Security;

namespace Addon.Episerver.EnvironmentSynchronizer.Configuration.Tests
{
	public class FlagsHelperTests
	{

		[Fact]
		public void AccessSringListToAccessLevel_Empty_Test()
		{
			// Arrange
			IList listOfValues = new List<string>();
			var accessLevel = AccessLevel.NoAccess;

			// Act
			foreach (string value in listOfValues)
			{
				var accessLevelValue = (AccessLevel)Enum.Parse(typeof(AccessLevel), value, true);
				if (!FlagsHelper.IsSet(accessLevel, accessLevelValue))
				{
					FlagsHelper.Set(ref accessLevel, accessLevelValue);
				}
			}

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

		[Fact]
		public void AccessStringListToAccessLevel_ReadCreate_Test()
		{
			// Arrange
			IList listOfValues = new List<string>();
			listOfValues.Add("Read");
			listOfValues.Add("Create");
			var accessLevel = AccessLevel.NoAccess;

			// Act
			foreach (string value in listOfValues)
			{
				var accessLevelValue = (AccessLevel)Enum.Parse(typeof(AccessLevel), value, true);
				if (!FlagsHelper.IsSet(accessLevel, accessLevelValue))
				{
					FlagsHelper.Set(ref accessLevel, accessLevelValue);
				}
			}

			// Assert
			((int)accessLevel).Should().Be(3);
			FlagsHelper.IsSet(accessLevel, AccessLevel.Read).Should().BeTrue();
			FlagsHelper.IsSet(accessLevel, AccessLevel.Create).Should().BeTrue();
			FlagsHelper.IsSet(accessLevel, AccessLevel.Edit).Should().BeFalse();
			FlagsHelper.IsSet(accessLevel, AccessLevel.Delete).Should().BeFalse();
			FlagsHelper.IsSet(accessLevel, AccessLevel.Publish).Should().BeFalse();
			FlagsHelper.IsSet(accessLevel, AccessLevel.Administer).Should().BeFalse();
			FlagsHelper.IsSet(accessLevel, AccessLevel.FullAccess).Should().BeTrue();
		}

		
	}

	
}