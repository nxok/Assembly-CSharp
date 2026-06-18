using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionRemoveEntities : BaseAction
	{
		public override BaseAction.ActionCompleteStates OnPerformAction()
		{
			if (this.targetGroup != "")
			{
				List<Entity> entityGroup = base.Owner.GetEntityGroup(this.targetGroup);
				if (entityGroup != null)
				{
					GameEventManager gameEventManager = GameEventManager.Current;
					for (int i = 0; i < entityGroup.Count; i++)
					{
						this.HandleRemoveData(gameEventManager, entityGroup[i]);
						GameManager.Instance.StartCoroutine(this.removeLater(entityGroup[i]));
					}
				}
				return BaseAction.ActionCompleteStates.Complete;
			}
			if (base.Owner.Target != null)
			{
				this.HandleRemoveData(GameEventManager.Current, base.Owner.Target);
				GameManager.Instance.StartCoroutine(this.removeLater(base.Owner.Target));
			}
			return BaseAction.ActionCompleteStates.Complete;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual void HandleRemoveData(GameEventManager gm, Entity ent)
		{
			gm.RemoveSpawnedEntry(ent);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public IEnumerator removeLater(Entity e)
		{
			yield return new WaitForSeconds(0.25f);
			EntityVehicle entityVehicle = e as EntityVehicle;
			if (entityVehicle != null)
			{
				entityVehicle.Kill();
			}
			if (e != null)
			{
				GameManager.Instance.World.RemoveEntity(e.entityId, EnumRemoveEntityReason.Killed);
			}
			yield break;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(ActionRemoveEntities.PropTargetGroup, ref this.targetGroup);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionRemoveEntities
			{
				targetGroup = this.targetGroup
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string targetGroup = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropTargetGroup = "target_group";
	}
}
