using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionReplaceBuff : ActionBaseTargetAction
	{
		public override BaseAction.ActionCompleteStates PerformTargetAction(Entity target)
		{
			EntityAlive entityAlive = target as EntityAlive;
			if (entityAlive != null && entityAlive.Buffs.HasBuff(this.replaceBuff))
			{
				entityAlive.Buffs.RemoveBuff(this.replaceBuff, -1, true);
				entityAlive.Buffs.AddBuff(this.replaceWithBuff, -1, true, false, -1f);
			}
			return BaseAction.ActionCompleteStates.Complete;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(ActionReplaceBuff.PropReplaceBuffName, ref this.replaceBuff);
			properties.ParseString(ActionReplaceBuff.PropReplaceWithBuffName, ref this.replaceWithBuff);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionReplaceBuff
			{
				replaceBuff = this.replaceBuff,
				replaceWithBuff = this.replaceWithBuff,
				targetGroup = this.targetGroup
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string replaceBuff = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public string replaceWithBuff = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropReplaceBuffName = "replace_buff";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropReplaceWithBuffName = "replace_with_buff";
	}
}
