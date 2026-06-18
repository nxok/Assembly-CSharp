using System;

namespace Twitch
{
	public class BaseTwitchVoteRequirement
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual void OnInit()
		{
		}

		public void Init()
		{
			this.OnInit();
		}

		public virtual bool CanPerform(EntityPlayer player)
		{
			return true;
		}

		public virtual void ParseProperties(DynamicProperties properties)
		{
			if (properties.Values.ContainsKey(BaseTwitchVoteRequirement.PropInvert))
			{
				this.Invert = StringParsers.ParseBool(properties.Values[BaseTwitchVoteRequirement.PropInvert], 0, -1, true);
			}
		}

		public TwitchVote Owner;

		public bool Invert;

		public static string PropInvert = "invert";
	}
}
