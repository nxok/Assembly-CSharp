using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace Challenges
{
	[Preserve]
	public class BaseRequirementObjectiveGroup
	{
		public int Count
		{
			get
			{
				if (this.PhaseList == null)
				{
					return 0;
				}
				return this.PhaseList[this.currentIndex].RequirementObjectiveList.Count;
			}
		}

		public virtual void CreateRequirements()
		{
		}

		public List<BaseChallengeObjective> CurrentObjectiveList
		{
			get
			{
				if (this.PhaseList == null)
				{
					return null;
				}
				return this.PhaseList[this.currentIndex].RequirementObjectiveList;
			}
		}

		public void HandleAddHooks()
		{
			if (this.PhaseList.Count == 0)
			{
				this.CreateRequirements();
			}
			for (int i = 0; i < this.PhaseList.Count; i++)
			{
				this.PhaseList[i].AddHooks();
			}
			this.CheckPrerequisites();
		}

		public void CheckPrerequisites()
		{
			if (this.PhaseList.Count == 0)
			{
				this.CreateRequirements();
			}
			this.NeedsPreRequisites = false;
			for (int i = 0; i < this.PhaseList.Count; i++)
			{
				if (this.PhaseList[i].HandleCheckStatus())
				{
					this.currentIndex = i;
					this.NeedsPreRequisites = true;
					return;
				}
			}
		}

		public void HandleRemoveHooks()
		{
			for (int i = 0; i < this.PhaseList.Count; i++)
			{
				this.PhaseList[i].HandleRemoveHooks();
			}
		}

		public virtual bool HasPrerequisiteCondition()
		{
			return false;
		}

		public void ResetObjectives()
		{
			if (this.PhaseList != null)
			{
				for (int i = 0; i < this.PhaseList.Count; i++)
				{
					this.PhaseList[i].ResetComplete();
				}
			}
		}

		public void ClonePhases(BaseRequirementObjectiveGroup group)
		{
			if (group.PhaseList != null)
			{
				for (int i = 0; i < group.PhaseList.Count; i++)
				{
					RequirementGroupPhase requirementGroupPhase = group.PhaseList[i].Clone();
					this.PhaseList.Add(requirementGroupPhase);
				}
			}
		}

		public virtual bool HandleCheckStatus()
		{
			if (this.PhaseList.Count == 0)
			{
				this.CreateRequirements();
			}
			this.ResetObjectives();
			for (int i = 0; i < this.PhaseList.Count; i++)
			{
				if (this.CheckPhaseStatus(i) && this.PhaseList[i].HandleCheckStatus())
				{
					this.currentIndex = i;
					this.NeedsPreRequisites = true;
					return true;
				}
				this.PhaseList[i].IsComplete = true;
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual bool CheckPhaseStatus(int index)
		{
			return true;
		}

		public virtual void UpdateStatus()
		{
			for (int i = 0; i < this.PhaseList.Count; i++)
			{
				if (!this.PhaseList[i].IsComplete)
				{
					this.PhaseList[i].UpdateStatus();
				}
			}
		}

		public virtual BaseRequirementObjectiveGroup Clone()
		{
			return null;
		}

		public Recipe GetItemRecipe()
		{
			return this.PhaseList[this.currentIndex].GetItemRecipe();
		}

		public Challenge Owner;

		public List<RequirementGroupPhase> PhaseList = new List<RequirementGroupPhase>();

		[PublicizedFrom(EAccessModifier.Protected)]
		public int currentIndex;

		public bool NeedsPreRequisites;
	}
}
