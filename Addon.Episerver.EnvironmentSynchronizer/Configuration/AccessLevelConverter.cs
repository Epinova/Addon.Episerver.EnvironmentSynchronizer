using EPiServer.Logging;
using EPiServer.Security;
using System;
using System.Collections.Generic;

namespace Addon.Episerver.EnvironmentSynchronizer.Configuration
{
	public static class AccessLevelConverter
	{
		private static readonly ILogger Logger = LogManager.GetLogger();

		public static AccessLevel ConvertToAccessLevel(IEnumerable<string> values)
		{
			var accessLevel = AccessLevel.NoAccess;
			foreach (string value in values)
			{
				try
				{
					var accessLevelValue = (AccessLevel)Enum.Parse(typeof(AccessLevel), value, true);
					if (!FlagsHelper.IsSet(accessLevel, accessLevelValue))
					{
						FlagsHelper.Set(ref accessLevel, accessLevelValue);
					}
				}
				catch (Exception ex)
				{
					Logger.Error($"Tried to parse {value} to EPiServer.Security.AccessLevel enum but failed.", ex);
				}
			}

			return accessLevel;
		}
	}
}
