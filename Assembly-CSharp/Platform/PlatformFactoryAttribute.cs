using System;

namespace Platform
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public class PlatformFactoryAttribute : Attribute
	{
		public PlatformFactoryAttribute(EPlatformIdentifier _targetPlatform)
		{
			this.TargetPlatform = _targetPlatform;
		}

		public readonly EPlatformIdentifier TargetPlatform;
	}
}
