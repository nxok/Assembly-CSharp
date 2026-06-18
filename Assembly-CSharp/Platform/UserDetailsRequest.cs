using System;

namespace Platform
{
	public class UserDetailsRequest
	{
		public UserDetailsRequest(PlatformUserIdentifierAbs id)
		{
			this.Id = id;
			this.NativePlatform = id.PlatformIdentifier;
		}

		public UserDetailsRequest(PlatformUserIdentifierAbs id, EPlatformIdentifier platform)
		{
			this.Id = id;
			this.NativePlatform = platform;
		}

		public readonly PlatformUserIdentifierAbs Id;

		public readonly EPlatformIdentifier NativePlatform;

		public PlatformUserDetails details;

		public bool IsSuccess;
	}
}
