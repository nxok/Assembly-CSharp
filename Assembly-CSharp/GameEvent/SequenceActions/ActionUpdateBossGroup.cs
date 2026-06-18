using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionUpdateBossGroup : BaseAction
	{
		public override BaseAction.ActionCompleteStates OnPerformAction()
		{
			GameEventManager.Current.UpdateBossGroupType(base.Owner.CurrentBossGroupID, this.bossGroupType);
			return BaseAction.ActionCompleteStates.Complete;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseEnum<BossGroup.BossGroupTypes>(ActionUpdateBossGroup.PropGroupType, ref this.bossGroupType);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionUpdateBossGroup
			{
				bossGroupType = this.bossGroupType
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public BossGroup.BossGroupTypes bossGroupType;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropGroupType = "group_type";
	}
}
