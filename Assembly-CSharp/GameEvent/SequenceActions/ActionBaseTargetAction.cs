using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionBaseTargetAction : BaseAction
	{
		public override BaseAction.ActionCompleteStates OnPerformAction()
		{
			if (this.targetGroup != "")
			{
				if (this.targetList == null)
				{
					this.targetList = new List<Entity>();
					List<Entity> entityGroup = base.Owner.GetEntityGroup(this.targetGroup);
					if (entityGroup != null)
					{
						this.targetList.AddRange(entityGroup);
						this.index = 0;
						this.StartTargetAction();
					}
				}
				else
				{
					if (this.targetList.Count <= this.index)
					{
						return BaseAction.ActionCompleteStates.Complete;
					}
					Entity entity = this.targetList[this.index];
					if ((entity is EntityAlive && entity.IsDead()) || entity.IsDespawned)
					{
						this.index++;
						if (this.index >= this.targetList.Count)
						{
							this.EndTargetAction();
							return BaseAction.ActionCompleteStates.Complete;
						}
					}
					else
					{
						BaseAction.ActionCompleteStates actionCompleteStates = this.PerformTargetAction(entity);
						if (actionCompleteStates == BaseAction.ActionCompleteStates.Complete)
						{
							this.index++;
						}
						else if (actionCompleteStates == BaseAction.ActionCompleteStates.InCompleteRefund)
						{
							return BaseAction.ActionCompleteStates.InCompleteRefund;
						}
						if (this.index >= this.targetList.Count)
						{
							this.EndTargetAction();
							return BaseAction.ActionCompleteStates.Complete;
						}
					}
				}
				return BaseAction.ActionCompleteStates.InComplete;
			}
			this.StartTargetAction();
			BaseAction.ActionCompleteStates actionCompleteStates2 = this.PerformTargetAction(base.Owner.Target);
			if (actionCompleteStates2 == BaseAction.ActionCompleteStates.Complete)
			{
				this.EndTargetAction();
				return BaseAction.ActionCompleteStates.Complete;
			}
			if (actionCompleteStates2 == BaseAction.ActionCompleteStates.InCompleteRefund)
			{
				return BaseAction.ActionCompleteStates.InCompleteRefund;
			}
			return BaseAction.ActionCompleteStates.InComplete;
		}

		public virtual void StartTargetAction()
		{
		}

		public virtual void EndTargetAction()
		{
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void OnReset()
		{
			base.OnReset();
			this.targetList = null;
			this.index = 0;
		}

		public virtual BaseAction.ActionCompleteStates PerformTargetAction(Entity target)
		{
			return BaseAction.ActionCompleteStates.Complete;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(ActionBaseTargetAction.PropTargetGroup, ref this.targetGroup);
		}

		public override BaseAction Clone()
		{
			ActionBaseTargetAction actionBaseTargetAction = (ActionBaseTargetAction)base.Clone();
			actionBaseTargetAction.targetGroup = this.targetGroup;
			return actionBaseTargetAction;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionBaseTargetAction
			{
				targetGroup = this.targetGroup
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string targetGroup = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropTargetGroup = "target_group";

		[PublicizedFrom(EAccessModifier.Private)]
		public List<Entity> targetList;

		[PublicizedFrom(EAccessModifier.Private)]
		public int index;
	}
}
