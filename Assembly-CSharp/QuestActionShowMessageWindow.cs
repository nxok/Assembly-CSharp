using System;
using UnityEngine.Scripting;

[Preserve]
public class QuestActionShowMessageWindow : BaseQuestAction
{
	public override void SetupAction()
	{
	}

	public override void PerformAction(Quest ownerQuest)
	{
		XUiC_TipWindow.ShowTip(this.message, this.title, XUiM_Player.GetPlayer() as EntityPlayerLocal, null);
	}

	public override BaseQuestAction Clone()
	{
		QuestActionShowMessageWindow questActionShowMessageWindow = new QuestActionShowMessageWindow();
		base.CopyValues(questActionShowMessageWindow);
		questActionShowMessageWindow.message = this.message;
		questActionShowMessageWindow.title = this.title;
		return questActionShowMessageWindow;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(QuestActionShowMessageWindow.PropMessage, ref this.message);
		properties.ParseString(QuestActionShowMessageWindow.PropTitle, ref this.title);
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
