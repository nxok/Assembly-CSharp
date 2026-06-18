using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace Challenges
{
	[Preserve]
	public class RequirementObjectiveGroupWindowOpen : BaseRequirementObjectiveGroup
	{
		public RequirementObjectiveGroupWindowOpen(string windowOpen)
		{
			this.WindowOpen = windowOpen;
		}

		public override void CreateRequirements()
		{
			if (this.PhaseList == null)
			{
				this.PhaseList = new List<RequirementGroupPhase>();
			}
			RequirementGroupPhase requirementGroupPhase = new RequirementGroupPhase();
			ChallengeObjectiveWindowOpen challengeObjectiveWindowOpen = new ChallengeObjectiveWindowOpen();
			challengeObjectiveWindowOpen.WindowName = this.WindowOpen;
			challengeObjectiveWindowOpen.Parent = this;
			challengeObjectiveWindowOpen.Owner = this.Owner;
			challengeObjectiveWindowOpen.IsRequirement = true;
			challengeObjectiveWindowOpen.Init();
			requirementGroupPhase.AddChallengeObjective(challengeObjectiveWindowOpen);
			this.PhaseList.Add(requirementGroupPhase);
		}

		public override bool HasPrerequisiteCondition()
		{
			EntityPlayerLocal player = this.Owner.Owner.Player;
			GUIWindow window = LocalPlayerUI.GetUIForPlayer(this.Owner.Owner.Player).windowManager.GetWindow(this.WindowOpen);
			return window != null && !window.isShowing;
		}

		public override BaseRequirementObjectiveGroup Clone()
		{
			return new RequirementObjectiveGroupWindowOpen(this.WindowOpen);
		}

		public string WindowOpen = "";
	}
}
