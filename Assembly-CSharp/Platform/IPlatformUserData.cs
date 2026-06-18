using System;
using System.Collections.Generic;

namespace Platform
{
	public interface IPlatformUserData : IPlatformUser
	{
		IReadOnlyDictionary<EBlockType, IPlatformUserBlockedData> Blocked { get; }

		void MarkBlockedStateChanged();

		string Name { get; }

		void RequestUserDetailsUpdate();
	}
}
