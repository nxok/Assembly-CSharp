using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionExplodePosition : BaseAction
	{
		public override BaseAction.ActionCompleteStates OnPerformAction()
		{
			if (base.Owner.TargetPosition.y == 0f)
			{
				return BaseAction.ActionCompleteStates.InCompleteRefund;
			}
			Vector3 targetPosition = base.Owner.TargetPosition;
			EntityAlive entityAlive = base.Owner.Target as EntityAlive;
			ExplosionData explosionData = new ExplosionData
			{
				BlastPower = GameEventManager.GetIntValue(entityAlive, this.blastPowerText, 75),
				BlockDamage = GameEventManager.GetFloatValue(entityAlive, this.blockDamageText, 1f),
				BlockRadius = GameEventManager.GetFloatValue(entityAlive, this.blockRadiusText, 4f),
				BlockTags = this.blockTags,
				EntityDamage = GameEventManager.GetFloatValue(entityAlive, this.entityDamageText, 5000f),
				EntityRadius = GameEventManager.GetIntValue(entityAlive, this.entityRadiusText, 3),
				ParticleIndex = this.particleIndex,
				IgnoreHeatMap = this.ignoreHeatMap
			};
			GameManager.Instance.ExplosionServer(targetPosition, World.worldToBlockPos(targetPosition), Quaternion.identity, explosionData, -1, 0.1f, false, null);
			return BaseAction.ActionCompleteStates.Complete;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(ActionExplodePosition.PropBlastPower, ref this.blastPowerText);
			properties.ParseString(ActionExplodePosition.PropBlockDamage, ref this.blockDamageText);
			properties.ParseString(ActionExplodePosition.PropBlockRadius, ref this.blockRadiusText);
			properties.ParseString(ActionExplodePosition.PropEntityDamage, ref this.entityDamageText);
			properties.ParseString(ActionExplodePosition.PropEntityRadius, ref this.entityRadiusText);
			properties.ParseString(ActionExplodePosition.PropBlockTags, ref this.blockTags);
			properties.ParseInt(ActionExplodePosition.PropParticleIndex, ref this.particleIndex);
			properties.ParseBool(ActionExplodePosition.PropIgnoreHeatMap, ref this.ignoreHeatMap);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionExplodePosition
			{
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
