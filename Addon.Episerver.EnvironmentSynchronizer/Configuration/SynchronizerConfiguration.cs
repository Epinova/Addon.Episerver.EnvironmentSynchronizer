﻿using EPiServer.ServiceLocation;
using System.Configuration;

namespace Addon.Episerver.EnvironmentSynchronizer.Configuration
{
	[ServiceConfiguration(typeof(ISynchronizerConfiguration))]
	public class SynchronizerConfiguration : ISynchronizerConfiguration
	{
		public SynchronizerSection Settings => GetSection<SynchronizerSection>("env.synchronizer");

		protected static T GetSection<T>(string path) => (T)ConfigurationManager.GetSection(path);
	}
}
