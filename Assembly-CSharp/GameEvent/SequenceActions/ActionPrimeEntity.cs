using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionPrimeEntity : ActionBaseTargetAction
	{
		public override BaseAction.ActionCompleteStates PerformTargetAction(Entity target)
		{
			EntityZombieCop entityZombieCop = target as EntityZombieCop;
			if (entityZombieCop != null)
			{
				entityZombieCop.HandlePrimingDetonator(GameEventManager.GetFloatValue(base.Owner.Target as EntityAlive, this.overrideTimeText, -1f));
			}
			return BaseAction.ActionCompleteStates.Complete;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(ActionPrimeEntity.PropOverrideTime, ref this.overrideTimeText);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionPrimeEntity
			{
				targetGroup = this.targetGroup,
				overrideTimeText = this.overrideTimeText
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string overrideTimeText;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropOverrideTime = "override_time";
	}
}
