using System;
using UnityEngine.Scripting;

namespace Platform.PSN
{
	[Preserve]
	[UserIdentifierFactory(EPlatformIdentifier.PSN)]
	public class UserIdentifierFactory : AbsUserIdentifierFactory
	{
		public override PlatformUserIdentifierAbs FromId(string _idString)
		{
			Log.Out("[PSN] Creating PSN user identifier from: {0}", new object[] { _idString });
			ulong num;
			if (StringParsers.TryParseUInt64(_idString, out num))
			{
				return new UserIdentifierPSN(num);
			}
			Log.Warning("[PSN] Could not parse PSN user from " + _idString);
			return null;
		}
	}
}
