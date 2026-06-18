using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionExplodeTarget : ActionBaseTargetAction
	{
		public override BaseAction.ActionCompleteStates PerformTargetAction(Entity target)
		{
			EntityAlive entityAlive = target as EntityAlive;
			EntityAlive entityAlive2 = base.Owner.Target as EntityAlive;
			if (entityAlive != null)
			{
				ExplosionData explosionData = new ExplosionData
				{
					BlastPower = GameEventManager.GetIntValue(entityAlive2, this.blastPowerText, 75),
					BlockDamage = GameEventManager.GetFloatValue(entityAlive2, this.blockDamageText, 1f),
					BlockRadius = GameEventManager.GetFloatValue(entityAlive2, this.blockRadiusText, 4f),
					BlockTags = this.blockTags,
					EntityDamage = GameEventManager.GetFloatValue(entityAlive2, this.entityDamageText, 5000f),
					EntityRadius = GameEventManager.GetIntValue(entityAlive2, this.entityRadiusText, 3),
					ParticleIndex = this.particleIndex,
					IgnoreHeatMap = this.ignoreHeatMap
				};
				GameManager.Instance.ExplosionServer(entityAlive.position, entityAlive.GetBlockPosition(), entityAlive.qrotation, explosionData, -1, 0.1f, false, null);
			}
			return BaseAction.ActionCompleteStates.Complete;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(ActionExplodeTarget.PropBlastPower, ref this.blastPowerText);
			properties.ParseString(ActionExplodeTarget.PropBlockDamage, ref this.blockDamageText);
			properties.ParseString(ActionExplodeTarget.PropBlockRadius, ref this.blockRadiusText);
			properties.ParseString(ActionExplodeTarget.PropEntityDamage, ref this.entityDamageText);
			properties.ParseString(ActionExplodeTarget.PropEntityRadius, ref this.entityRadiusText);
			properties.ParseString(ActionExplodeTarget.PropBlockTags, ref this.blockTags);
			properties.ParseInt(ActionExplodeTarget.PropParticleIndex, ref this.particleIndex);
			properties.ParseBool(ActionExplodeTarget.PropIgnoreHeatMap, ref this.ignoreHeatMap);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionExplodeTarget
			{
				targetGroup = this.targetGroup,
				blastPowerText = this.blastPowerText,
				blockDamageText = this.blockDamageText,
				blockRadiusText = this.blockRadiusText,
				entityDamageText = this.entityDamageText,
				entityRadiusText = this.entityRadiusText,
				particleIndex = this.particleIndex,
				blockTags = this.blockTags
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string blastPowerText;

		[PublicizedFrom(EAccessModifier.Protected)]
		public string blockDamageText;

		[PublicizedFrom(EAccessModifier.Protected)]
		public string blockRadiusText;

		[PublicizedFrom(EAccessModifier.Protected)]
		public string entityDamageText;

		[PublicizedFrom(EAccessModifier.Protected)]
		public string entityRadiusText;

		[PublicizedFrom(EAccessModifier.Protected)]
		public int particleIndex = 13;

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool ignoreHeatMap = true;

		[PublicizedFrom(EAccessModifier.Protected)]
		public string blockTags = "";

		public static string PropBlastPower = "blast_power";

		public static string PropBlockDamage = "block_damage";

		public static string PropBlockRadius = "block_radius";

		public static string PropBlockTags = "block_tags";

		public static string PropEntityDamage = "entity_damage";

		public static string PropEntityRadius = "entity_radius";

		public static string PropParticleIndex = "particle_index";

		public static string PropIgnoreHeatMap = "ignore_heatmap";
	}
}
