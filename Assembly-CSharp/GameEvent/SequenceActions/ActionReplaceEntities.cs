using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionReplaceEntities : ActionBaseTargetAction
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public override void OnInit()
		{
			base.OnInit();
			string[] array = this.entityNames.Split(',', StringSplitOptions.None);
			for (int i = 0; i < array.Length; i++)
			{
				foreach (KeyValuePair<int, EntityClass> keyValuePair in EntityClass.list.Dict)
				{
					if (keyValuePair.Value.entityClassName == array[i])
					{
						this.entityIDs.Add(keyValuePair.Key);
						if (this.entityIDs.Count == array.Length)
						{
							break;
						}
					}
				}
			}
			if (this.singleChoice && this.selectedEntityIndex == -1)
			{
				this.selectedEntityIndex = Random.Range(0, this.entityIDs.Count);
			}
		}

		public override void StartTargetAction()
		{
			this.newList = new List<Entity>();
		}

		public override void EndTargetAction()
		{
			if (this.targetGroup != "")
			{
				base.Owner.AddEntitiesToGroup(this.targetGroup, this.newList, false);
			}
		}

		public override BaseAction.ActionCompleteStates PerformTargetAction(Entity target)
		{
			World world = GameManager.Instance.World;
			if (target != null && !(target is EntityPlayer))
			{
				int num = ((this.selectedEntityIndex == -1) ? Random.Range(0, this.entityIDs.Count) : this.selectedEntityIndex);
				Entity entity = EntityFactory.CreateEntity(this.entityIDs[num], target.position, target.rotation, (base.Owner.Target != null) ? base.Owner.Target.entityId : (-1), base.Owner.ExtraData);
				entity.SetSpawnerSource(EnumSpawnerSource.Dynamic);
				world.SpawnEntityInWorld(entity);
				this.newList.Add(entity);
				if (this.attackTarget)
				{
					EntityAlive entityAlive = entity as EntityAlive;
					if (entityAlive != null)
					{
						EntityAlive entityAlive2 = base.Owner.Target as EntityAlive;
						if (entityAlive2 != null)
						{
							GameEventManager.Current.RegisterSpawnedEntity(entityAlive, entityAlive2, base.Owner.Requester, base.Owner, true);
							entityAlive.SetAttackTarget(entityAlive2, 12000);
							entityAlive.aiManager.SetTargetOnlyPlayers(100f);
							if (base.Owner.Requester != null)
							{
								GameEventActionSequence gameEventActionSequence = ((base.Owner.OwnerSequence == null) ? base.Owner : base.Owner.OwnerSequence);
								if (base.Owner.Requester is EntityPlayerLocal)
								{
									GameEventManager.Current.HandleGameEntitySpawned(gameEventActionSequence.Name, entity.entityId, gameEventActionSequence.Tag);
								}
								else
								{
									SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup(gameEventActionSequence.Name, gameEventActionSequence.Target.entityId, gameEventActionSequence.ExtraData, gameEventActionSequence.Tag, NetPackageGameEventResponse.ResponseTypes.EntitySpawned, entity.entityId, -1, false, ""), false, gameEventActionSequence.Requester.entityId, -1, -1, null, 192, false);
								}
							}
						}
					}
				}
				this.HandleRemoveData(target);
				GameManager.Instance.StartCoroutine(this.removeLater(target));
			}
			return BaseAction.ActionCompleteStates.Complete;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual void HandleRemoveData(Entity ent)
		{
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public IEnumerator removeLater(Entity e)
		{
			yield return new WaitForSeconds(0.25f);
			if (e is EntityVehicle)
			{
				(e as EntityVehicle).Kill();
			}
			GameManager.Instance.World.RemoveEntity(e.entityId, EnumRemoveEntityReason.Killed);
			yield break;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			if (properties.Values.ContainsKey(ActionReplaceEntities.PropEntityNames))
			{
				this.entityNames = properties.Values[ActionReplaceEntities.PropEntityNames];
			}
			if (properties.Values.ContainsKey(ActionReplaceEntities.PropSingleChoice))
			{
				this.singleChoice = StringParsers.ParseBool(properties.Values[ActionReplaceEntities.PropSingleChoice], 0, -1, true);
			}
			properties.ParseBool(ActionReplaceEntities.PropAttackTarget, ref this.attackTarget);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionReplaceEntities
			{
				entityNames = this.entityNames,
				entityIDs = this.entityIDs,
				singleChoice = this.singleChoice,
				targetGroup = this.targetGroup,
				selectedEntityIndex = this.selectedEntityIndex,
				attackTarget = this.attackTarget
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string entityNames = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool singleChoice;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropEntityNames = "entity_names";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropSingleChoice = "single_choice";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropAttackTarget = "attack_target";

		[PublicizedFrom(EAccessModifier.Protected)]
		public List<int> entityIDs = new List<int>();

		[PublicizedFrom(EAccessModifier.Protected)]
		public int selectedEntityIndex = -1;

		[PublicizedFrom(EAccessModifier.Protected)]
		public List<Entity> newList;

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool attackTarget = true;
	}
}
