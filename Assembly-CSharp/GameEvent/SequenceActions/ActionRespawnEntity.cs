using System;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionRespawnEntity : BaseAction
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public override void OnInit()
		{
			base.OnInit();
			this.AddToGroups = this.addToGroup.Split(',', StringSplitOptions.None);
		}

		public override BaseAction.ActionCompleteStates OnPerformAction()
		{
			if (this.oldEntityClass == -1 && base.Owner.Target != null && !(base.Owner.Target is EntityPlayer))
			{
				Entity target = base.Owner.Target;
				this.oldEntityClass = target.entityClass;
				this.oldEntityID = target.entityId;
				this.oldPosition = target.position;
				this.oldRotation = target.rotation;
			}
			if (this.delay > 0f)
			{
				this.delay -= Time.deltaTime;
				return BaseAction.ActionCompleteStates.InComplete;
			}
			World world = GameManager.Instance.World;
			GameEventActionSequence gameEventActionSequence = ((base.Owner.OwnerSequence == null) ? base.Owner : base.Owner.OwnerSequence);
			Entity entity = EntityFactory.CreateEntity(this.oldEntityClass, this.oldPosition, this.oldRotation, gameEventActionSequence.Target.entityId, gameEventActionSequence.ExtraData);
			if (entity == null)
			{
				return BaseAction.ActionCompleteStates.Complete;
			}
			entity.SetSpawnerSource(EnumSpawnerSource.Dynamic);
			world.SpawnEntityInWorld(entity);
			world.RemoveEntity(this.oldEntityID, EnumRemoveEntityReason.Killed);
			base.Owner.Target = entity;
			EntityAlive entityAlive = entity as EntityAlive;
			EntityAlive entityAlive2 = gameEventActionSequence.Target as EntityAlive;
			if (entityAlive2 != null)
			{
				GameEventManager.Current.RegisterSpawnedEntity(entityAlive, entityAlive2, gameEventActionSequence.Requester, gameEventActionSequence, true);
				entityAlive.SetAttackTarget(entityAlive2, 12000);
			}
			if (base.Owner.Requester != null)
			{
				if (gameEventActionSequence.Requester is EntityPlayerLocal)
				{
					GameEventManager.Current.HandleGameEntitySpawned(gameEventActionSequence.Name, entityAlive.entityId, gameEventActionSequence.Tag);
				}
				else
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup(gameEventActionSequence.Name, gameEventActionSequence.Target.entityId, gameEventActionSequence.ExtraData, gameEventActionSequence.Tag, NetPackageGameEventResponse.ResponseTypes.TwitchSetOwner, entityAlive.entityId, -1, false, ""), false, gameEventActionSequence.Requester.entityId, -1, -1, null, 192, false);
				}
			}
			if (entity != null && this.AddToGroups != null)
			{
				for (int i = 0; i < this.AddToGroups.Length; i++)
				{
					if (this.AddToGroups[i] != "")
					{
						base.Owner.AddEntityToGroup(this.AddToGroups[i], entity);
					}
				}
			}
			if (this.respawnSound != "")
			{
				Manager.BroadcastPlayByLocalPlayer(this.oldPosition, this.respawnSound);
			}
			return BaseAction.ActionCompleteStates.Complete;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(ActionRespawnEntity.PropTargetGroup, ref this.targetGroup);
			properties.ParseString(ActionRespawnEntity.PropRespawnSound, ref this.respawnSound);
			properties.ParseString(ActionRespawnEntity.PropAddToGroup, ref this.addToGroup);
			properties.ParseFloat(ActionRespawnEntity.PropDelay, ref this.delay);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionRespawnEntity
			{
				targetGroup = this.targetGroup,
				addToGroup = this.addToGroup,
				AddToGroups = this.AddToGroups,
				respawnSound = this.respawnSound,
				delay = this.delay
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string targetGroup = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public string addToGroup = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public string[] AddToGroups;

		[PublicizedFrom(EAccessModifier.Protected)]
		public string respawnSound = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropTargetGroup = "target_group";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropAddToGroup = "add_to_group";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropRespawnSound = "respawn_sound";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropDelay = "delay";

		[PublicizedFrom(EAccessModifier.Private)]
		public int oldEntityClass = -1;

		[PublicizedFrom(EAccessModifier.Private)]
		public int oldEntityID = -1;

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 oldPosition;

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 oldRotation;

		[PublicizedFrom(EAccessModifier.Private)]
		public float delay = 3f;
	}
}
