using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionAddXPDeficit : ActionBaseClientAction
	{
		public override void OnClientPerform(Entity target)
		{
			EntityPlayer entityPlayer = target as EntityPlayer;
			if (entityPlayer != null)
			{
				this.xpAmount = GameEventManager.GetIntValue(entityPlayer, this.xpAmountText, 0);
				if (this.xpAmount == 0)
				{
					entityPlayer.Progression.AddXPDeficit();
					return;
				}
				entityPlayer.Progression.ExpDeficit += this.xpAmount;
			}
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(ActionAddXPDeficit.PropXPAmount, ref this.xpAmountText);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionAddXPDeficit
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
