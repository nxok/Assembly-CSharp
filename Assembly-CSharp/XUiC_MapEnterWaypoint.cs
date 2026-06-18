using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_MapEnterWaypoint : XUiController
{
	public override void Init()
	{
		base.Init();
		this.txtInput = (XUiC_TextInput)this.windowGroup.Controller.GetChildById("waypointInput");
		if (this.txtInput != null)
		{
			this.txtInput.Text = string.Empty;
		}
		if (this.txtInput != null)
		{
			this.txtInput.OnSubmitHandler += this.waypointOnSubmitHandler;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void waypointOnInputAbortedHandler(XUiController _sender)
	{
		((XUiC_MapArea)this.xui.GetWindow("mapArea").Controller).closeAllPopups();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void waypointOnSubmitHandler(XUiController _sender, string _text)
	{
		((XUiC_MapArea)this.xui.GetWindow("mapArea").Controller).OnWaypointCreated(_text);
	}

	public void Show(Vector2i _position)
	{
		XUiV_Window window = this.xui.GetWindow("mapAreaEnterWaypointName");
		window.Position = _position;
		window.IsVisible = true;
		this.txtInput.Text = string.Empty;
		this.txtInput.SelectOrVirtualKeyboard(true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtInput;
}
