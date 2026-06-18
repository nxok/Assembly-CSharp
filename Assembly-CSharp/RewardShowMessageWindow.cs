using System;
using UnityEngine.Scripting;

[Preserve]
public class RewardShowMessageWindow : BaseReward
{
	public RewardShowMessageWindow()
	{
		base.HiddenReward = true;
	}

	public override void SetupReward()
	{
		base.HiddenReward = true;
	}

	public override void GiveReward(EntityPlayer player)
	{
		XUiC_TipWindow.ShowTip(this.message, this.title, player as EntityPlayerLocal, null);
	}

	public override BaseReward Clone()
	{
		RewardShowMessageWindow rewardShowMessageWindow = new RewardShowMessageWindow();
		base.CopyValues(rewardShowMessageWindow);
		rewardShowMessageWindow.title = this.title;
		rewardShowMessageWindow.message = this.message;
		return rewardShowMessageWindow;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(RewardShowMessageWindow.PropMessage, ref this.message);
		properties.ParseString(RewardShowMessageWindow.PropTitle, ref this.title);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string PropMessage = "message";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string PropTitle = "title";

	[PublicizedFrom(EAccessModifier.Private)]
	public string message = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string title = "";
}
