using System;

namespace SharpEXR.AttributeTypes
{
	public struct TimeCode
	{
		public TimeCode(uint timeAndFlags, uint userData)
		{
			this.TimeAndFlags = timeAndFlags;
			this.UserData = userData;
		}

		public readonly uint TimeAndFlags;

		public readonly uint UserData;
	}
}
