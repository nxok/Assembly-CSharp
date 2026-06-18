using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements
{
	[Preserve]
	public class RequirementHasSpawnedEntities : BaseRequirement
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public override void OnInit()
		{
		}

		public override bool CanPerform(Entity target)
		{
			FastTags<TagGroup.Global> fastTags = FastTags<TagGroup.Global>.Parse(this.tag);
			for (int i = 0; i < GameEventManager.Current.spawnEntries.Count; i++)
			{
				GameEventManager.SpawnEntry spawnEntry = GameEventManager.Current.spawnEntries[i];
				if (spawnEntry.SpawnedEntity.HasAnyTags(fastTags) && (!this.targetOnly || spawnEntry.SpawnedEntity.spawnById == target.entityId))
				{
					return true;
				}
			}
			return false;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			if (properties.Values.ContainsKey(RequirementHasSpawnedEntities.PropTag))
			{
				this.tag = properties.Values[RequirementHasSpawnedEntities.PropTag];
			}
			if (properties.Values.ContainsKey(RequirementHasSpawnedEntities.PropTargetOnly))
			{
				this.targetOnly = StringParsers.ParseBool(properties.Values[RequirementHasSpawnedEntities.PropTargetOnly], 0, -1, true);
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseRequirement CloneChildSettings()
		{
			return new RequirementHasSpawnedEntities
			{
				tag = this.tag,
				targetOnly = this.targetOnly
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string tag;

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool targetOnly;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropTag = "entity_tags";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropTargetOnly = "target_only";
	}
}
