using System;
using Epic.OnlineServices;

namespace Platform.EOS
{
	[PublicizedFrom(EAccessModifier.Internal)]
	public struct EOSSanction
	{
		public readonly string ReferenceId { get; }

		public EOSSanction(DateTime? expiryDate, Utf8String referenceId)
		{
			this.ReferenceId = referenceId;
			this.expiry = expiryDate.GetValueOrDefault();
		}

		public DateTime expiry;
	}
}
