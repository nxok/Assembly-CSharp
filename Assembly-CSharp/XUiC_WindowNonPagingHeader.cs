using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_WindowNonPagingHeader : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiController childById = base.GetChildById("lblWindowName");
		if (childById != null)
		{
			this.lblWindowName = (XUiV_Label)childById.ViewComponent;
		}
	}

	public void SetHeader(string name)
	{
		if (this.lblWindowName != null)
		{
			this.lblWindowName.Text = name;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		XUiC_FocusedBlockHealth.SetData(this.xui.playerUI, null, 0f);
		this.xui.DragAndDropWindow.InMenu = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		this.xui.DragAndDropWindow.InMenu = false;
		if (this.xui.CurrentSelectedEntry != null)
		{
			this.xui.CurrentSelectedEntry.IsSelected = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblWindowName;
}
