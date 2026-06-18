using System;

namespace Platform
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public class UserIdentifierFactoryAttribute : Attribute
	{
		public UserIdentifierFactoryAttribute(EPlatformIdentifier _targetPlatform)
		{
			this.TargetPlatform = _targetPlatform;
		}

		public readonly EPlatformIdentifier TargetPlatform;
	}
}
