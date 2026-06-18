using System;

namespace Platform.MultiPlatform
{
	public class RichPresence : IRichPresence
	{
		public void Init(IPlatform _owner)
		{
		}

		public void UpdateRichPresence(IRichPresence.PresenceStates _state)
		{
			IRichPresence richPresence = PlatformManager.NativePlatform.RichPresence;
			if (richPresence != null)
			{
				richPresence.UpdateRichPresence(_state);
			}
			IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
			if (crossplatformPlatform == null)
			{
				return;
			}
			IRichPresence richPresence2 = crossplatformPlatform.RichPresence;
			if (richPresence2 == null)
			{
				return;
			}
			richPresence2.UpdateRichPresence(_state);
		}
	}
}
