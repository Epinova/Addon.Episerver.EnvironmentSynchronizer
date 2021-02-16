using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using EPiServer.Data;
using EPiServer.Data.Dynamic;

namespace Addon.Episerver.EnvironmentSynchronizer.DynamicData
{
	[DebuggerDisplay("{Environment} {TimeStamp} {Environment}")]
	[Serializable]
	[EPiServerDataStore(AutomaticallyRemapStore = true)]
	public class EnvironmentSynchronizationStamp : ISerializable, IDynamicData
	{
		public Identity Id { get; set; }

		public DateTime TimeStamp { get; set; }

		public string Environment { get; set; }

		public virtual void GetObjectData(SerializationInfo info, StreamingContext ctxt)
		{
			info.AddValue("TimeStamp", TimeStamp);
			info.AddValue("Environment", Environment);
		}
	}
}
