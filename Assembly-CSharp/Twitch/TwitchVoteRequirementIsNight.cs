using System;

namespace Twitch
{
	public class TwitchVoteRequirementIsNight : BaseTwitchVoteRequirement
	{
		public override bool CanPerform(EntityPlayer player)
		{
			if (!this.Invert)
			{
				return !GameManager.Instance.World.IsDaytime();
			}
			return GameManager.Instance.World.IsDaytime();
		}
	}
}
