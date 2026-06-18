using System;
using System.Collections.Generic;
using GameEvent.SequenceActions;
using UnityEngine.Scripting;

namespace GameEvent.SequenceDecisions
{
	[Preserve]
	public class BaseDecision : BaseAction
	{
		public override bool UseRequirements
		{
			get
			{
				return false;
			}
		}

		public override void SetActionKeyData(int _actionIndex, BaseAction _parent, string prefix = "")
		{
			base.SetActionKeyData(_actionIndex, _parent, prefix);
			for (int i = 0; i < this.Actions.Count; i++)
			{
				this.Actions[i].SetActionKeyData(i, this, "");
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void OnInit()
		{
			base.OnInit();
			List<int> list = new List<int>();
			for (int i = 0; i < this.Actions.Count; i++)
			{
				if (!list.Contains(this.Actions[i].Phase))
				{
					list.Add(this.Actions[i].Phase);
				}
			}
			list.Sort();
			if (list.Count > 0)
			{
				this.phaseMax = list[list.Count - 1] + 1;
				return;
			}
			this.phaseMax = 0;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void OnReset()
		{
			base.OnReset();
			for (int i = 0; i < this.Actions.Count; i++)
			{
				this.Actions[i].Reset();
			}
			this.currentPhase = 0;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public BaseAction.ActionCompleteStates HandleActions()
		{
			bool flag = false;
			int num = this.currentPhase;
			for (int i = 0; i < this.Actions.Count; i++)
			{
				if (this.Actions[i].Phase == this.currentPhase && !this.Actions[i].IsComplete)
				{
					flag = true;
					this.Actions[i].Owner = base.Owner;
					BaseAction.ActionCompleteStates actionCompleteStates = this.Actions[i].PerformAction();
					if (actionCompleteStates == BaseAction.ActionCompleteStates.Complete || (actionCompleteStates == BaseAction.ActionCompleteStates.InCompleteRefund && this.Actions[i].IgnoreRefund))
					{
						this.Actions[i].IsComplete = true;
						if (this.Actions[i].PhaseOnComplete != -1)
						{
							num = this.Actions[i].PhaseOnComplete;
						}
					}
					else if (actionCompleteStates == BaseAction.ActionCompleteStates.RequirementsNotMet)
					{
						this.Actions[i].IsComplete = true;
						if (this.Actions[i].PhaseOnDenied != -1)
						{
							num = this.Actions[i].PhaseOnDenied;
						}
					}
					else if (base.Owner.AllowRefunds && actionCompleteStates == BaseAction.ActionCompleteStates.InCompleteRefund)
					{
						return BaseAction.ActionCompleteStates.InCompleteRefund;
					}
				}
			}
			if (!flag)
			{
				this.currentPhase++;
			}
			else if (this.currentPhase != num)
			{
				this.currentPhase = num;
				for (int j = 0; j < this.Actions.Count; j++)
				{
					if (this.Actions[j].Phase >= this.currentPhase)
					{
						this.Actions[j].Reset();
					}
				}
			}
			if (this.currentPhase >= this.phaseMax)
			{
				this.IsComplete = true;
				return BaseAction.ActionCompleteStates.Complete;
			}
			return BaseAction.ActionCompleteStates.InComplete;
		}

		public override void HandleTemplateInit(GameEventActionSequence seq)
		{
			base.HandleTemplateInit(seq);
			for (int i = 0; i < this.Actions.Count; i++)
			{
				seq.HandleVariablesForProperties(this.Actions[i].Properties);
				this.Actions[i].ParseProperties(this.Actions[i].Properties);
			}
		}

		public override BaseAction Clone()
		{
			BaseDecision baseDecision = (BaseDecision)base.Clone();
			for (int i = 0; i < this.Actions.Count; i++)
			{
				BaseAction baseAction = this.Actions[i].Clone();
				baseAction.Owner = base.Owner;
				baseDecision.Actions.Add(baseAction);
			}
			baseDecision.phaseMax = this.phaseMax;
			return baseDecision;
		}

		public List<BaseAction> Actions = new List<BaseAction>();

		[PublicizedFrom(EAccessModifier.Protected)]
		public int currentPhase;

		[PublicizedFrom(EAccessModifier.Protected)]
		public int phaseMax;
	}
}
