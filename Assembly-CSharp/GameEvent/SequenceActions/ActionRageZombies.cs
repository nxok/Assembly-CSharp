using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionRageZombies : ActionBaseTargetAction
	{
		public override BaseAction.ActionCompleteStates PerformTargetAction(Entity target)
		{
			EntityAlive entityAlive = target as EntityAlive;
			if (entityAlive != null)
			{
				if (entityAlive is EntityPlayer || entityAlive is EntityNPC)
				{
					return BaseAction.ActionCompleteStates.Complete;
				}
				EntityHuman entityHuman = entityAlive as EntityHuman;
				if (entityHuman != null)
				{
					entityHuman.ConditionalTriggerSleeperWakeUp();
					entityHuman.StartRage(GameEventManager.GetFloatValue(entityAlive, this.speedPercentText, 2f), GameEventManager.GetFloatValue(entityAlive, this.rageTimeText, 5f) + 1f);
				}
				EntityAlive entityAlive2 = base.Owner.Target as EntityAlive;
				if (entityAlive2 != null)
				{
					entityAlive.SetAttackTarget(entityAlive2, 12000);
				}
			}
			return BaseAction.ActionCompleteStates.Complete;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(ActionRageZombies.PropTime, ref this.rageTimeText);
			properties.ParseString(ActionRageZombies.PropSpeedPercent, ref this.speedPercentText);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionRageZombies
			{
				rageTimeText = this.rageTimeText,
				speedPercentText = this.speedPercentText,
				targetGroup = this.targetGroup
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string rageTimeText;

		[PublicizedFrom(EAccessModifier.Protected)]
		public string speedPercentText;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropTime = "time";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropSpeedPercent = "speed_percent";
	}
}
