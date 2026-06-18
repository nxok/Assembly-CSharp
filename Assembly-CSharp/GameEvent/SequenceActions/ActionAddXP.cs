using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionAddXP : ActionBaseClientAction
	{
		public override void OnClientPerform(Entity target)
		{
			EntityPlayer entityPlayer = target as EntityPlayer;
			if (entityPlayer != null)
			{
				this.xpAmount = GameEventManager.GetIntValue(entityPlayer, this.xpAmountText, 0);
				if (this.xpAmount < 0)
				{
					this.xpAmount = 0;
				}
				entityPlayer.Progression.AddLevelExp(this.xpAmount, "_xpOther", Progression.XPTypes.Other, true, true, -1, null);
			}
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(ActionAddXP.PropXPAmount, ref this.xpAmountText);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionAddXP
			{
				xpAmountText = this.xpAmountText
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string xpAmountText;

		[PublicizedFrom(EAccessModifier.Protected)]
		public int xpAmount;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropXPAmount = "xp_amount";
	}
}
