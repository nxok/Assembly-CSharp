using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionRemoveBuff : ActionBaseTargetAction
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public override void OnInit()
		{
			this.BuffList = this.buffName.Split(',', StringSplitOptions.None);
		}

		public override BaseAction.ActionCompleteStates PerformTargetAction(Entity target)
		{
			EntityAlive entityAlive = target as EntityAlive;
			if (entityAlive != null)
			{
				for (int i = 0; i < this.BuffList.Length; i++)
				{
					if (entityAlive.Buffs.HasBuff(this.BuffList[i]))
					{
						entityAlive.Buffs.RemoveBuff(this.BuffList[i], -1, true);
					}
				}
			}
			return BaseAction.ActionCompleteStates.Complete;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			if (properties.Values.ContainsKey(ActionRemoveBuff.PropBuffName))
			{
				this.buffName = properties.Values[ActionRemoveBuff.PropBuffName];
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionRemoveBuff
			{
				buffName = this.buffName,
				BuffList = this.BuffList,
				targetGroup = this.targetGroup
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string buffName = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public string[] BuffList;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropBuffName = "buff_name";
	}
}
