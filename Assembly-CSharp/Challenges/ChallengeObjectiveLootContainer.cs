using System;
using UnityEngine.Scripting;

namespace Challenges
{
	[Preserve]
	public class ChallengeObjectiveLootContainer : BaseChallengeObjective
	{
		public override ChallengeObjectiveType ObjectiveType
		{
			get
			{
				return ChallengeObjectiveType.LootContainer;
			}
		}

		public override string DescriptionText
		{
			get
			{
				return Localization.Get("ObjectiveLootContainer_keyword", false, null);
			}
		}

		public override void Init()
		{
		}

		public override void HandleAddHooks()
		{
			QuestEventManager.Current.ContainerOpened -= this.Current_ContainerOpened;
			QuestEventManager.Current.ContainerOpened += this.Current_ContainerOpened;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Current_ContainerOpened(Vector3i containerLocation, ITileEntityLootable tileEntity)
		{
			if (containerLocation == this.lastLocation)
			{
				return;
			}
			this.lastLocation = containerLocation;
			if (tileEntity.bWasTouched)
			{
				return;
			}
			if (this.CheckBaseRequirements())
			{
				return;
			}
			int num = base.Current;
			base.Current = num + 1;
			this.CheckObjectiveComplete(true);
		}

		public override void HandleRemoveHooks()
		{
			QuestEventManager.Current.ContainerOpened -= this.Current_ContainerOpened;
		}

		public override BaseChallengeObjective Clone()
		{
			return new ChallengeObjectiveLootContainer();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3i lastLocation = Vector3i.zero;
	}
}
