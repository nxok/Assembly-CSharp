using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionAddSpawnedEntitiesToGroup : BaseAction
	{
		public override BaseAction.ActionCompleteStates OnPerformAction()
		{
			FastTags<TagGroup.Global> fastTags = FastTags<TagGroup.Global>.Parse(this.tag);
			List<Entity> list = new List<Entity>();
			for (int i = 0; i < GameEventManager.Current.spawnEntries.Count; i++)
			{
				GameEventManager.SpawnEntry spawnEntry = GameEventManager.Current.spawnEntries[i];
				if (spawnEntry.SpawnedEntity.HasAnyTags(fastTags) && (!this.targetOnly || spawnEntry.SpawnedEntity.spawnById == base.Owner.Target.entityId) && (this.excludeBuff == "" || !spawnEntry.SpawnedEntity.Buffs.HasBuff(this.excludeBuff)))
				{
					list.Add(spawnEntry.SpawnedEntity);
				}
			}
			base.Owner.AddEntitiesToGroup(this.groupName, list, false);
			return BaseAction.ActionCompleteStates.Complete;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(ActionAddSpawnedEntitiesToGroup.PropGroupName, ref this.groupName);
			properties.ParseString(ActionAddSpawnedEntitiesToGroup.PropTag, ref this.tag);
			properties.ParseString(ActionAddSpawnedEntitiesToGroup.PropExcludeBuff, ref this.excludeBuff);
			properties.ParseBool(ActionAddSpawnedEntitiesToGroup.PropTargetOnly, ref this.targetOnly);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionAddSpawnedEntitiesToGroup
			{
				tag = this.tag,
				groupName = this.groupName,
				targetOnly = this.targetOnly,
				excludeBuff = this.excludeBuff
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string groupName = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public string tag;

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool targetOnly;

		[PublicizedFrom(EAccessModifier.Protected)]
		public string excludeBuff = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropGroupName = "group_name";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropTag = "entity_tags";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropTargetOnly = "target_only";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropExcludeBuff = "exclude_buff";
	}
}
