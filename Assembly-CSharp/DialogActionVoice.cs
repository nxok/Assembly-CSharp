using System;
using UnityEngine.Scripting;

[Preserve]
public class DialogActionVoice : BaseDialogAction
{
	public override BaseDialogAction.ActionTypes ActionType
	{
		get
		{
			return BaseDialogAction.ActionTypes.Voice;
		}
	}

	public override void PerformAction(EntityPlayer player)
	{
		LocalPlayerUI.primaryUI.xui.Dialog.Respondent.PlayVoiceSetEntry(base.ID, player, true, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string name = "";
}
