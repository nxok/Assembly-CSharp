using System;

namespace Platform
{
	public class MappedAccountRequest
	{
		public MappedAccountRequest(PlatformUserIdentifierAbs _id, EPlatformIdentifier _platform)
		{
			this.Id = _id;
			this.Platform = _platform;
		}

		public readonly PlatformUserIdentifierAbs Id;

		public readonly EPlatformIdentifier Platform;

		public string MappedAccountId;

		public string DisplayName;

		public MappedAccountQueryResult Result;
	}
}
