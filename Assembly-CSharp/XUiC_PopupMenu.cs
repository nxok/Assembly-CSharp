using System;
using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PopupMenu : XUiController
{
	[XuiXmlAttribute("menu_min_width", false)]
	public int MenuMinWidth
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = 100;

	[XuiXmlAttribute("element_padding_over_label", false)]
	public int MaxWidthPadding
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = 60;

	[XuiXmlAttribute("slider_min_width", false)]
	public int SliderMinWidth { get; set; } = 250;

	[XuiXmlBinding("grid_width")]
	public int GridSizeX
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.gridSize.x;
		}
	}

	[XuiXmlBinding("grid_height")]
	public int GridSizeY
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.gridSize.y;
		}
	}

	[XuiXmlBinding("title")]
	public string Title
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.title ?? "";
		}
	}

	public override void Init()
	{
		base.Init();
		this.xui.PopupMenuWindow = this;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		this.xui.PopupMenuWindow = null;
	}

	public XUiC_PopupMenu Setup(Vector2i _offsetPosition, string _title = "")
	{
		this.clearItems();
		this.IsOver = false;
		this.xuiPosition = this.xui.GetMouseXUiPosition();
		this.offset = _offsetPosition;
		this.title = _title;
		this.IsDirty = true;
		return this;
	}

	public XUiC_PopupMenu AddItem(XUiC_PopupMenuItem.Entry _newMenuItems)
	{
		this.menuItems.Add(_newMenuItems);
		this.IsDirty = true;
		return this;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void clearItems()
	{
		this.menuItems.Clear();
		XUiC_PopupMenuItem[] array = this.popupMenuItems;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetEntry(null);
		}
		this.IsDirty = true;
	}

	public XUiC_PopupMenu Show()
	{
		this.xui.playerUI.windowManager.Open(this.windowGroup, false);
		this.justOpened = true;
		return this;
	}

	public void Close()
	{
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		this.clearItems();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.menuItems.Count > 0 && !this.justOpened)
		{
			if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard)
			{
				if ((this.xui.playerUI.CursorController.GetMouseButtonUp(0) | this.xui.playerUI.playerInput.GUIActions.RightStick.WasReleased) && !this.IsOver)
				{
					this.Close();
				}
			}
			else if (this.xui.playerUI.playerInput.GUIActions.Cancel.WasReleased)
			{
				this.Close();
			}
		}
		if (this.justOpened)
		{
			this.justOpened = false;
			if (this.menuItems.Count == 0)
			{
				this.Close();
				return;
			}
			int num = this.MenuMinWidth - this.MaxWidthPadding;
			int num2 = this.menuItems.Count - 1;
			for (int i = 0; i < this.popupMenuItems.Length; i++)
			{
				int num3 = this.popupMenuItems[i].SetEntry((i < this.menuItems.Count) ? this.menuItems[i] : null);
				if (num3 > num)
				{
					num = num3;
				}
				if (i < num2 && i < this.menuItems.Count && this.menuItems[i].IsEnabled)
				{
					num2 = i;
				}
			}
			num += this.MaxWidthPadding;
			this.gridSize = new Vector2i(num, this.menuItems.Count * this.gridView.CellHeight);
			this.limitPositionToScreenBounds();
			this.xui.playerUI.CursorController.SetNavigationTargetLater(this.popupMenuItems[num2].ViewComponent);
			this.IsDirty = true;
		}
		if (this.IsDirty)
		{
			this.IsDirty = false;
			base.RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void limitPositionToScreenBounds()
	{
		Vector2i vector2i = this.gridSize;
		vector2i.y = -vector2i.y;
		Vector2i vector2i2 = this.xui.GetXUiScreenSize() / 2;
		Vector2i vector2i3 = new Vector2i((int)((double)vector2i2.x * 0.97), (int)((double)vector2i2.y * 0.97));
		Vector2i vector2i4 = this.xuiPosition + this.offset;
		Vector2i vector2i5 = vector2i4;
		Vector2i vector2i6 = vector2i5 + vector2i;
		if (vector2i6.x >= vector2i3.x)
		{
			vector2i4.x = vector2i3.x - vector2i.x;
		}
		else if (vector2i5.x <= -vector2i3.x)
		{
			vector2i4.x = -vector2i3.x;
		}
		if (vector2i6.y <= -vector2i3.y)
		{
			vector2i4.y = -vector2i3.y - vector2i.y;
		}
		else if (vector2i5.y >= vector2i3.y)
		{
			vector2i4.y = vector2i3.y;
		}
		base.ViewComponent.Position = vector2i4;
		base.ViewComponent.TryUpdatePosition();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<XUiC_PopupMenuItem.Entry> menuItems = new List<XUiC_PopupMenuItem.Entry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i xuiPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i offset;

	public bool IsOver;

	[XuiBindComponent("list", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Grid gridView;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_PopupMenuItem[] popupMenuItems;

	[PublicizedFrom(EAccessModifier.Private)]
	public string title;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i gridSize = Vector2i.one;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool justOpened;
}
