using System;

namespace Platform
{
	public class MappedAccountReverseRequest
	{
		public MappedAccountReverseRequest(EPlatformIdentifier _platform, string _id)
		{
			this.Platform = _platform;
			this.Id = _id;
		}

		public readonly EPlatformIdentifier Platform;

		public readonly string Id;

		public MappedAccountQueryResult Result;

		public PlatformUserIdentifierAbs PlatformId;
	}
}
