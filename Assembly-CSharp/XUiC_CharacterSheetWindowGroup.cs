using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_CharacterSheetWindowGroup : XUiController
{
	public override void Init()
	{
		base.Init();
		this.buffList = base.GetChildByType<XUiC_ActiveBuffList>();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (this.buffList != null)
		{
			this.buffList.setFirstEntry = true;
		}
		this.xui.playerUI.windowManager.Open("windowpaging", false);
		XUiC_WindowSelector childByType = this.xui.FindWindowGroupByName("windowpaging").GetChildByType<XUiC_WindowSelector>();
		if (childByType != null)
		{
			childByType.SetSelected("character");
		}
		if (this.xui.PlayerEquipment != null)
		{
			this.xui.PlayerEquipment.IsOpen = true;
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		this.xui.playerUI.windowManager.Close("windowpaging");
		if (this.xui.PlayerEquipment != null)
		{
			this.xui.PlayerEquipment.IsOpen = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ActiveBuffList buffList;
}
