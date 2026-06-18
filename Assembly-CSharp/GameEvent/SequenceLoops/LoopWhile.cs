using System;
using GameEvent.SequenceActions;
using UnityEngine.Scripting;

namespace GameEvent.SequenceLoops
{
	[Preserve]
	public class LoopWhile : BaseLoop
	{
		public override bool UseRequirements
		{
			get
			{
				return false;
			}
		}

		public override BaseAction.ActionCompleteStates OnPerformAction()
		{
			if (!this.runLoop)
			{
				this.runLoop = this.CheckCondition();
			}
			if (this.runLoop)
			{
				if (base.HandleActions() == BaseAction.ActionCompleteStates.Complete)
				{
					this.CurrentPhase = 0;
					for (int i = 0; i < this.Actions.Count; i++)
					{
						this.Actions[i].Reset();
					}
					this.runLoop = false;
				}
				return BaseAction.ActionCompleteStates.InComplete;
			}
			return BaseAction.ActionCompleteStates.Complete;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool CheckCondition()
		{
			if (this.Requirements != null)
			{
				LoopWhile.ConditionTypes conditionType = this.ConditionType;
				if (conditionType == LoopWhile.ConditionTypes.Any)
				{
					bool flag = false;
					for (int i = 0; i < this.Requirements.Count; i++)
					{
						if (this.Requirements[i].CanPerform(base.Owner.Target))
						{
							flag = true;
							break;
						}
					}
					return flag;
				}
				if (conditionType == LoopWhile.ConditionTypes.All)
				{
					for (int j = 0; j < this.Requirements.Count; j++)
					{
						if (!this.Requirements[j].CanPerform(base.Owner.Target))
						{
							return false;
						}
					}
				}
			}
			return true;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseEnum<LoopWhile.ConditionTypes>(LoopWhile.PropConditionType, ref this.ConditionType);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new LoopWhile
			{
				ConditionType = this.ConditionType
			};
		}

		public LoopWhile.ConditionTypes ConditionType = LoopWhile.ConditionTypes.All;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropConditionType = "condition_type";

		[PublicizedFrom(EAccessModifier.Private)]
		public bool runLoop;

		public enum ConditionTypes
		{
			Any,
			All
		}
	}
}
