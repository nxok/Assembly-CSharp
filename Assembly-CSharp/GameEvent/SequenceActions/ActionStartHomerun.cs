using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionStartHomerun : ActionBaseTargetAction
	{
		public override BaseAction.ActionCompleteStates PerformTargetAction(Entity target)
		{
			EntityPlayer entityPlayer = target as EntityPlayer;
			if (entityPlayer != null)
			{
				float floatValue = GameEventManager.GetFloatValue(entityPlayer, this.gameTimeText, 120f);
				GameEventManager.Current.HomerunManager.AddPlayerToHomerun(entityPlayer, this.rewardLevels, this.rewardEvents, floatValue, new Action(this.HomeRunComplete));
			}
			return BaseAction.ActionCompleteStates.Complete;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			for (int i = 1; i <= 5; i++)
			{
				string text = string.Format("reward_level_{0}", i);
				string text2 = string.Format("reward_event_{0}", i);
				if (this.Properties.Contains(text) && this.Properties.Contains(text2))
				{
					this.rewardLevels.Add(StringParsers.ParseSInt32(this.Properties.Values[text], 0, -1, NumberStyles.Integer));
					this.rewardEvents.Add(this.Properties.Values[text2]);
				}
			}
			this.Properties.ParseString(ActionStartHomerun.PropDuration, ref this.gameTimeText);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void HomeRunComplete()
		{
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionStartHomerun
			{
				targetGroup = this.targetGroup,
				rewardEvents = this.rewardEvents,
				rewardLevels = this.rewardLevels,
				gameTimeText = this.gameTimeText
			};
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public List<int> rewardLevels = new List<int>();

		[PublicizedFrom(EAccessModifier.Private)]
		public List<string> rewardEvents = new List<string>();

		[PublicizedFrom(EAccessModifier.Protected)]
		public string gameTimeText;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropDuration = "duration";
	}
}
