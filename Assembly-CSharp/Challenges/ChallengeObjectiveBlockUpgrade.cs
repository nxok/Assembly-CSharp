using System;
using System.Globalization;
using System.Xml.Linq;
using UnityEngine.Scripting;

namespace Challenges
{
	[Preserve]
	public class ChallengeObjectiveBlockUpgrade : BaseChallengeObjective
	{
		public override ChallengeObjectiveType ObjectiveType
		{
			get
			{
				return ChallengeObjectiveType.BlockUpgrade;
			}
		}

		public override string DescriptionText
		{
			get
			{
				return Localization.Get("challengeObjectiveUpgrade", false, null) + " " + Localization.Get(this.expectedBlock, false, null) + ":";
			}
		}

		public override void HandleOnCreated()
		{
			base.HandleOnCreated();
			this.CreateRequirements();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void CreateRequirements()
		{
			if (!this.ShowRequirements)
			{
				return;
			}
			this.Owner.SetRequirementGroup(new RequirementObjectiveGroupBlockUpgrade(this.heldItemID, this.neededResourceID, this.neededResourceCount));
		}

		public override void HandleAddHooks()
		{
			QuestEventManager.Current.BlockUpgrade -= this.Current_BlockUpgrade;
			QuestEventManager.Current.BlockUpgrade += this.Current_BlockUpgrade;
		}

		public override void HandleRemoveHooks()
		{
			QuestEventManager.Current.BlockUpgrade -= this.Current_BlockUpgrade;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Current_BlockUpgrade(string blockName, Vector3i blockPos)
		{
			bool flag = false;
			if (this.CheckBaseRequirements())
			{
				return;
			}
			if (this.expectedBlock == null || this.expectedBlock == "" || this.expectedBlock.EqualsCaseInsensitive(blockName))
			{
				flag = true;
			}
			if (!flag && blockName.Contains(":") && this.expectedBlock.EqualsCaseInsensitive(blockName.Substring(0, blockName.IndexOf(':'))))
			{
				flag = true;
			}
			if (!flag && this.expectedBlock != null && this.expectedBlock != "")
			{
				Block blockByName = Block.GetBlockByName(this.expectedBlock, true);
				if (blockByName != null && blockByName.SelectAlternates && blockByName.ContainsAlternateBlock(blockName))
				{
					flag = true;
				}
			}
			if (flag)
			{
				int num = base.Current;
				base.Current = num + 1;
				this.CheckObjectiveComplete(true);
			}
		}

		public override void ParseElement(XElement e)
		{
			base.ParseElement(e);
			if (e.HasAttribute("block"))
			{
				this.expectedBlock = e.GetAttribute("block");
			}
			if (e.HasAttribute("held"))
			{
				this.heldItemID = e.GetAttribute("held");
			}
			if (e.HasAttribute("needed_resource"))
			{
				this.neededResourceID = e.GetAttribute("needed_resource");
			}
			if (e.HasAttribute("needed_resource_count"))
			{
				this.neededResourceCount = StringParsers.ParseSInt32(e.GetAttribute("needed_resource_count"), 0, -1, NumberStyles.Integer);
			}
		}

		public override BaseChallengeObjective Clone()
		{
			return new ChallengeObjectiveBlockUpgrade
			{
				expectedBlock = this.expectedBlock,
				heldItemID = this.heldItemID,
				neededResourceID = this.neededResourceID,
				neededResourceCount = this.neededResourceCount
			};
		}

		public string expectedBlock = "";

		public string heldItemID = "";

		public string neededResourceID = "";

		public int neededResourceCount = 1;
	}
}
