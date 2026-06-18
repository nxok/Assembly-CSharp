using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements
{
	[Preserve]
	public class RequirementHasBuff : BaseRequirement
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public override void OnInit()
		{
			this.BuffList = this.BuffName.Split(',', StringSplitOptions.None);
		}

		public override bool CanPerform(Entity target)
		{
			EntityAlive entityAlive = target as EntityAlive;
			if (entityAlive != null)
			{
				for (int i = 0; i < this.BuffList.Length; i++)
				{
					if (!this.CheckBuff(entityAlive, this.BuffList[i]))
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool CheckBuff(EntityAlive player, string buffName)
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
			if (properties.Values.ContainsKey(RequirementHasBuff.PropBuffName))
			{
				this.BuffName = properties.Values[RequirementHasBuff.PropBuffName];
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseRequirement CloneChildSettings()
		{
			return new RequirementHasBuff
			{
				BuffName = this.BuffName,
				BuffList = this.BuffList,
				Invert = this.Invert
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string BuffName = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public string[] BuffList;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropBuffName = "buff_name";
	}
}
