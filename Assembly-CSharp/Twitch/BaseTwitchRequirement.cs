using System;
using UnityEngine.Scripting;

namespace Twitch
{
	[Preserve]
	public class BaseTwitchRequirement
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual void OnInit()
		{
		}

		public void Init()
		{
			this.OnInit();
		}

		public virtual bool CanPerform(Entity player)
		{
			return true;
		}

		public virtual void ParseProperties(DynamicProperties properties)
		{
			this.Properties = properties;
			properties.ParseBool(BaseTwitchRequirement.PropInvert, ref this.Invert);
			properties.ParseBool(BaseTwitchRequirement.PropHidesAction, ref this.HideAction);
		}

		public BaseTwitchRequirement.OwnerTypes OwnerType;

		public TwitchAction OwnerAction;

		public TwitchVote OwnerVote;

		public BaseTwitchEventEntry OwnerEvent;

		public DynamicProperties Properties;

		public bool HideAction;

		public bool Invert;

		public static string PropInvert = "invert";

		public static string PropHidesAction = "hide_action";

		public enum OwnerTypes
		{
			Action,
			Vote,
			Event
		}
	}
}
