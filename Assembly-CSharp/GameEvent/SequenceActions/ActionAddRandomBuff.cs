using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionAddRandomBuff : ActionBaseTargetAction
	{
		public override BaseAction.ActionCompleteStates PerformTargetAction(Entity target)
		{
			if (this.removesBuffs == null)
			{
				this.removesBuffs = this.removesBuff.Split(',', StringSplitOptions.None);
			}
			if (this.buffNames == null)
			{
				this.buffNames = this.addsBuff.Split(',', StringSplitOptions.None);
			}
			EntityAlive entityAlive = target as EntityAlive;
			if (entityAlive != null)
			{
				bool flag = false;
				for (int i = 0; i < this.removesBuffs.Length; i++)
				{
					if (entityAlive.Buffs.HasBuff(this.removesBuffs[i]))
					{
						entityAlive.Buffs.RemoveBuff(this.removesBuffs[i], -1, true);
						flag = true;
					}
				}
				if (!flag)
				{
					string text = this.buffNames[target.rand.RandomRange(this.buffNames.Length)];
					entityAlive.Buffs.AddBuff(text, -1, true, false, -1f);
				}
			}
			return BaseAction.ActionCompleteStates.Complete;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(ActionAddRandomBuff.PropBuffName, ref this.addsBuff);
			properties.ParseString(ActionAddRandomBuff.PropRemovesBuff, ref this.removesBuff);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionAddRandomBuff
			{
				addsBuff = this.addsBuff,
				removesBuff = this.removesBuff,
				targetGroup = this.targetGroup
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string addsBuff = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public string[] buffNames;

		[PublicizedFrom(EAccessModifier.Protected)]
		public string removesBuff = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public string[] removesBuffs;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropBuffName = "buff_names";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropRemovesBuff = "removes_buff";
	}
}
