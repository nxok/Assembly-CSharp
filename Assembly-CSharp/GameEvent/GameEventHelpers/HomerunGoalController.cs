using System;
using System.Collections.Generic;
using Audio;
using UnityEngine;

namespace GameEvent.GameEventHelpers
{
	public class HomerunGoalController : MonoBehaviour
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public void Update()
		{
			if (GameManager.Instance.World == null)
			{
				this.ReadyForDelete = true;
				return;
			}
			switch (this.direction)
			{
			case HomerunGoalController.Direction.YPositive:
				this.position = this.StartPosition + Vector3.up * Mathf.PingPong(Time.time, 2f) * 2f;
				base.transform.position = this.position - Origin.position;
				break;
			case HomerunGoalController.Direction.XPositive:
				this.position = this.StartPosition + Vector3.right * Mathf.PingPong(Time.time, 2f) * 2f;
				base.transform.position = this.position - Origin.position;
				break;
			case HomerunGoalController.Direction.XNegative:
				this.position = this.StartPosition + Vector3.left * Mathf.PingPong(Time.time, 2f) * 2f;
				base.transform.position = this.position - Origin.position;
				break;
			case HomerunGoalController.Direction.ZPositive:
				this.position = this.StartPosition + Vector3.forward * Mathf.PingPong(Time.time, 2f) * 2f;
				base.transform.position = this.position - Origin.position;
				break;
			case HomerunGoalController.Direction.ZNegative:
				this.position = this.StartPosition + Vector3.back * Mathf.PingPong(Time.time, 2f) * 2f;
				base.transform.position = this.position - Origin.position;
				break;
			}
			if (Vector3.Distance(this.position, this.Owner.Player.position) > 50f)
			{
				this.ReadyForDelete = true;
				return;
			}
			List<Entity> entitiesInBounds = GameManager.Instance.World.GetEntitiesInBounds(null, new Bounds(this.position, Vector3.one * this.Size));
			for (int i = 0; i < entitiesInBounds.Count; i++)
			{
				EntityAlive entityAlive = entitiesInBounds[i] as EntityAlive;
				if (entityAlive != null && entityAlive != null && entityAlive.IsAlive() && !(entityAlive is EntityPlayer) && entityAlive.emodel != null && entityAlive.emodel.transform != null && entityAlive.emodel.IsRagdollActive)
				{
					World world = GameManager.Instance.World;
					float lightBrightness = world.GetLightBrightness(entityAlive.GetBlockPosition());
					world.GetGameManager().SpawnParticleEffectServer(new ParticleEffect("twitch_fireworks", entityAlive.position, lightBrightness, Color.white, null, null, false, 1f, ""), entityAlive.entityId, false, true);
					Manager.BroadcastPlayByLocalPlayer(entityAlive.position, "twitch_celebrate");
					entityAlive.DamageEntity(new DamageSource(EnumDamageSource.Internal, EnumDamageTypes.Suicide), 99999, false, 1f);
					GameManager.Instance.World.RemoveEntity(entityAlive.entityId, EnumRemoveEntityReason.Killed);
					if (!this.ReadyForDelete)
					{
						this.Owner.Score += this.ScoreAdded;
						this.ReadyForDelete = true;
					}
				}
			}
		}

		public HomerunData Owner;

		public Vector3 position;

		public bool ReadyForDelete;

		public int ScoreAdded = 1;

		public float Size = 2f;

		public Vector3 StartPosition;

		public HomerunGoalController.Direction direction;

		public enum Direction
		{
			YPositive,
			XPositive,
			XNegative,
			ZPositive,
			ZNegative,
			Max
		}
	}
}
