using System;

namespace Platform
{
	public interface IRichPresence
	{
		void Init(IPlatform _owner);

		void UpdateRichPresence(IRichPresence.PresenceStates _state);

		public enum PresenceStates
		{
			Menu,
			Loading,
			Connecting,
			InGame
		}
	}
}
