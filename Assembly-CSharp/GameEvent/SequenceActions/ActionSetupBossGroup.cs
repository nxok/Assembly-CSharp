using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionSetupBossGroup : BaseAction
	{
		public override BaseAction.ActionCompleteStates OnPerformAction()
		{
			List<Entity> entityGroup = base.Owner.GetEntityGroup(this.bossGroupName);
			List<Entity> entityGroup2 = base.Owner.GetEntityGroup(this.minionGroupName);
			List<EntityAlive> list = new List<EntityAlive>();
			EntityAlive entityAlive = entityGroup[0] as EntityAlive;
			for (int i = 0; i < entityGroup2.Count; i++)
			{
				EntityAlive entityAlive2 = entityGroup2[i] as EntityAlive;
				if (entityAlive2 != null)
				{
					list.Add(entityAlive2);
				}
			}
			EntityPlayer entityPlayer = base.Owner.Target as EntityPlayer;
			if (entityPlayer != null)
			{
				base.Owner.CurrentBossGroupID = GameEventManager.Current.SetupBossGroup(entityPlayer, entityAlive, list, this.bossGroupType, this.bossIcon1);
			}
			return BaseAction.ActionCompleteStates.Complete;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(ActionSetupBossGroup.PropMinionGroupName, ref this.minionGroupName);
			properties.ParseString(ActionSetupBossGroup.PropBossGroupName, ref this.bossGroupName);
			properties.ParseString(ActionSetupBossGroup.PropBossIcon1, ref this.bossIcon1);
			properties.ParseEnum<BossGroup.BossGroupTypes>(ActionSetupBossGroup.PropGroupType, ref this.bossGroupType);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionSetupBossGroup
			{
				minionGroupName = this.minionGroupName,
				bossGroupName = this.bossGroupName,
				bossGroupType = this.bossGroupType,
				bossIcon1 = this.bossIcon1
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string minionGroupName = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public string bossGroupName = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public string bossIcon1 = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public BossGroup.BossGroupTypes bossGroupType;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropMinionGroupName = "minion_group_name";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropBossGroupName = "boss_group_name";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropBossIcon1 = "boss_icon1";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropGroupType = "group_type";
	}
}
