using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_MapPopupList : XUiController
{
	public override void Init()
	{
		base.Init();
		this.children[0].OnPress += this.onPressEntry1;
		this.children[1].OnPress += this.onPressEntry2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onPressEntry1(XUiController _sender, int _mouseButton)
	{
		XUiC_MapArea xuiC_MapArea = this.xui.GetWindow("mapArea").Controller as XUiC_MapArea;
		xuiC_MapArea.OnSetWaypoint();
		xuiC_MapArea.GetChildById("mapView").SelectCursorElement(true, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onPressEntry2(XUiController _sender, int _mouseButton)
	{
		if (_sender is XUiC_MapPopupEntry)
		{
			XUiV_Window window = this.xui.GetWindow("mapAreaSetWaypoint");
			XUiV_Window window2 = this.xui.GetWindow("mapAreaChooseWaypoint");
			int num = 46;
			window2.Position = window.Position + new Vector2i(199, -num);
			if (window2.Position.y < 0)
			{
				window2.Position = new Vector2i(window2.Position.x, window2.Position.y + window2.Size.y);
			}
			window2.IsVisible = true;
			window2.Controller.GetChildByType<XUiC_MapSubPopupList>().ResetList();
		}
	}
}
