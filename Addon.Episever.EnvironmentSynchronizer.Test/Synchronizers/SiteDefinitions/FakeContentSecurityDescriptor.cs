using EPiServer.Core;
using EPiServer.Security;
using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace Addon.Episever.EnvironmentSynchronizer.Test.Synchronizers.SiteDefinitions
{
	public class FakeContentSecurityDescriptor : IContentSecurityDescriptor
	{
		private List<AccessControlEntry> _ace = new List<AccessControlEntry>();

		public ContentReference ContentLink { get; set; }
		public bool IsInherited { get; set; }
		public string Creator { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public IEnumerable<AccessControlEntry> Entries { get { return _ace; } set { _ace = (List<AccessControlEntry>)value; } }

		public bool IsReadOnly => throw new NotImplementedException();

		public void AddEntry(AccessControlEntry accessControlEntry)
		{
			_ace.Add(accessControlEntry);
		}

		public void Clear()
		{
			_ace.Clear();
		}

		public object CreateWritableClone()
		{
			return this;
		}

		public AccessLevel GetAccessLevel(IPrincipal principal)
		{
			throw new NotImplementedException();
		}

		public bool HasAccess(IPrincipal principal, AccessLevel access)
		{
			throw new NotImplementedException();
		}

		public void MakeReadOnly()
		{
			throw new NotImplementedException();
		}

		public void RemoveEntry(AccessControlEntry accessControlEntry)
		{
			throw new NotImplementedException();
		}
	}
}
