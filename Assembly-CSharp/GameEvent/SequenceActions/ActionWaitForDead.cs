using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionWaitForDead : BaseAction
	{
		public override BaseAction.ActionCompleteStates OnPerformAction()
		{
			if (this.entityList == null)
			{
				this.entityList = new List<EntityAlive>();
				List<Entity> entityGroup = base.Owner.GetEntityGroup(this.targetGroup);
				if (entityGroup == null)
				{
					Debug.LogWarning("ActionWaitForDead: Target Group " + this.targetGroup + " Does not exist!");
					return BaseAction.ActionCompleteStates.InCompleteRefund;
				}
				for (int i = 0; i < entityGroup.Count; i++)
				{
					EntityAlive entityAlive = entityGroup[i] as EntityAlive;
					if (entityAlive != null)
					{
						this.entityList.Add(entityAlive);
					}
				}
			}
			else
			{
				this.checkTime -= Time.deltaTime;
				if (this.checkTime <= 0f)
				{
					if (base.Owner.HasDespawn)
					{
						this.PhaseOnComplete = this.phaseOnDespawn;
						return BaseAction.ActionCompleteStates.Complete;
					}
					bool flag = false;
					for (int j = this.entityList.Count - 1; j >= 0; j--)
					{
						EntityAlive entityAlive2 = this.entityList[j];
						if (entityAlive2 != null)
						{
							if (entityAlive2.IsAlive())
							{
								flag = true;
							}
							else
							{
								this.entityList.RemoveAt(j);
							}
						}
					}
					if (!flag)
					{
						return BaseAction.ActionCompleteStates.Complete;
					}
					this.checkTime = 1f;
					return BaseAction.ActionCompleteStates.InComplete;
				}
			}
			return BaseAction.ActionCompleteStates.InComplete;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void OnReset()
		{
			this.entityList = null;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			if (properties.Values.ContainsKey(ActionWaitForDead.PropTargetGroup))
			{
				this.targetGroup = properties.Values[ActionWaitForDead.PropTargetGroup];
			}
			properties.ParseInt(ActionWaitForDead.PropPhaseOnDespawn, ref this.phaseOnDespawn);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionWaitForDead
			{
				targetGroup = this.targetGroup,
				phaseOnDespawn = this.phaseOnDespawn
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string targetGroup = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public int phaseOnDespawn = -1;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropTargetGroup = "target_group";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropPhaseOnDespawn = "phase_on_despawn";

		[PublicizedFrom(EAccessModifier.Private)]
		public List<EntityAlive> entityList;

		[PublicizedFrom(EAccessModifier.Private)]
		public float checkTime = 1f;
	}
}
