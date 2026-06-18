using System;

namespace Platform.XBL
{
	public class XuidResolveRequest
	{
		public XuidResolveRequest(PlatformUserIdentifierAbs id)
		{
			this.Id = id;
		}

		public readonly PlatformUserIdentifierAbs Id;

		public bool IsSuccess;

		public ulong Xuid;
	}
}
