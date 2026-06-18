using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_MapSubPopupEntry : XUiController
{
	public override void Init()
	{
		base.Init();
		base.OnPress += this.onPressed;
		base.OnVisiblity += this.XUiC_MapSubPopupEntry_OnVisiblity;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void XUiC_MapSubPopupEntry_OnVisiblity(XUiController _sender, bool _visibleSelf, bool _visibleInScene)
	{
		this.select(false);
	}

	public void SetIndex(int _idx)
	{
		this.index = _idx;
	}

	public void SetSpriteName(string _s)
	{
		this.spriteName = _s;
		for (int i = 0; i < base.Parent.Children.Count; i++)
		{
			XUiView viewComponent = base.Parent.Children[i].ViewComponent;
			if (viewComponent.ID.EqualsCaseInsensitive("icon"))
			{
				((XUiV_Sprite)viewComponent).SpriteName = _s;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnHovered(bool _isOver)
	{
		this.select(_isOver);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onPressed(XUiController _sender, int _mouseButton)
	{
		this.select(true);
		((XUiC_MapArea)this.xui.GetWindow("mapArea").Controller).OnWaypointEntryChosen(this.spriteName);
		XUiC_MapEnterWaypoint childByType = this.xui.GetWindow("mapAreaEnterWaypointName").Controller.GetChildByType<XUiC_MapEnterWaypoint>();
		int num = this.index / 10;
		int num2 = this.index % 10;
		Vector2i vector2i = this.xui.GetWindow("mapAreaChooseWaypoint").Position + new Vector2i(52 * (num + 1), num2 * -43);
		childByType.Show(vector2i);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void select(bool _bSelect)
	{
		XUiV_Sprite xuiV_Sprite = (XUiV_Sprite)this.viewComponent;
		if (xuiV_Sprite != null)
		{
			xuiV_Sprite.Color = (_bSelect ? new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue) : new Color32(64, 64, 64, byte.MaxValue));
			xuiV_Sprite.SpriteName = (_bSelect ? "ui_game_select_row" : "menu_empty");
		}
	}

	public void Reset()
	{
		this.select(false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int index;

	[PublicizedFrom(EAccessModifier.Private)]
	public string spriteName;
}
