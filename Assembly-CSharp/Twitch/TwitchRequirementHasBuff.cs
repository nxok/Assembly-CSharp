using System;
using UnityEngine.Scripting;

namespace Twitch
{
	[Preserve]
	public class TwitchRequirementHasBuff : BaseTwitchRequirement
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public override void OnInit()
		{
			this.BuffList = this.BuffName.Split(',', StringSplitOptions.None);
		}

		public override bool CanPerform(Entity entity)
		{
			EntityPlayer entityPlayer = entity as EntityPlayer;
			if (entityPlayer != null)
			{
				for (int i = 0; i < this.BuffList.Length; i++)
				{
					if (!this.CheckBuff(entityPlayer, this.BuffList[i]))
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool CheckBuff(EntityPlayer player, string buffName)
		{
			if (player.Buffs.HasBuff(buffName))
			{
				return !this.Invert;
			}
			return this.Invert;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(TwitchRequirementHasBuff.PropBuffName, ref this.BuffName);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string BuffName = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public string[] BuffList;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropBuffName = "buff_name";
	}
}
