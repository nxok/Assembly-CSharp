using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SignGridEntry : XUiC_SelectableEntry
{
	public virtual Color32 BackgroundColor
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return XUiC_SignGridEntry.backgroundColor;
		}
	}

	public virtual bool IsSelectable
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return true;
		}
	}

	public XUiC_SignGridEntry()
	{
		this.IsDirty = true;
	}

	public override void Init()
	{
		base.Init();
		this.highlightOverlay = base.GetChildById("highlightOverlay").ViewComponent as XUiV_Sprite;
		this.background = base.GetChildById("background").ViewComponent as XUiV_Sprite;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SelectedChanged(bool isSelected)
	{
		base.SelectedChanged(isSelected);
		if (!isSelected)
		{
			this.background.Color = this.BackgroundColor;
			return;
		}
		this.background.Color = this.selectColor;
		Action<XUiC_SignGridEntry> onBecameSelected = this.OnBecameSelected;
		if (onBecameSelected == null)
		{
			return;
		}
		onBecameSelected(this);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		PlayerActionsGUI guiactions = this.xui.playerUI.playerInput.GUIActions;
		if (base.WindowGroup.isShowing)
		{
			CursorControllerAbs cursorController = this.xui.playerUI.CursorController;
			cursorController.GetScreenPosition();
			bool mouseButtonUp = cursorController.GetMouseButtonUp(0);
			cursorController.GetMouseButtonDown(0);
			cursorController.GetMouseButton(0);
			cursorController.GetMouseButtonUp(1);
			cursorController.GetMouseButtonDown(1);
			cursorController.GetMouseButton(1);
			if (this.isOver && base.ViewComponent.UiTransformIsHovered && base.ViewComponent.EventOnPress)
			{
				if (guiactions.LastInputType == 1)
				{
					if (guiactions.Submit.WasReleased && this.IsSelectable)
					{
						this.HandleClick();
					}
				}
				else if (mouseButtonUp && this.IsSelectable)
				{
					this.HandleClick();
				}
			}
			else
			{
				if (this.highlightOverlay != null)
				{
					this.highlightOverlay.Color = this.BackgroundColor;
				}
				if (!base.IsSelected)
				{
					this.background.Color = this.BackgroundColor;
				}
				if (this.isOver)
				{
					this.isOver = false;
				}
			}
		}
		if (this.IsDirty)
		{
			this.IsDirty = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void HandleClick()
	{
		base.IsSelected = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnHovered(bool _isOver)
	{
		this.isOver = _isOver;
		if (!base.IsSelected)
		{
			if (_isOver)
			{
				this.background.Color = XUiC_SignGridEntry.highlightColor;
			}
			else
			{
				this.background.Color = this.BackgroundColor;
			}
		}
		base.OnHovered(_isOver);
	}

	public override bool ParseAttribute(string name, string value)
	{
		bool flag = base.ParseAttribute(name, value);
		if (!flag)
		{
			if (!(name == "select_color"))
			{
				if (!(name == "background_color"))
				{
					if (!(name == "highlight_color"))
					{
						return false;
					}
					XUiC_SignGridEntry.highlightColor = StringParsers.ParseColor32(value);
				}
				else
				{
					XUiC_SignGridEntry.backgroundColor = StringParsers.ParseColor32(value);
				}
			}
			else
			{
				this.selectColor = StringParsers.ParseColor32(value);
			}
			return true;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOver;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 selectColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	public static Color32 backgroundColor = new Color32(96, 96, 96, byte.MaxValue);

	public static Color32 highlightColor = new Color32(222, 206, 163, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite highlightOverlay;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite background;

	public Action<XUiC_SignGridEntry> OnBecameSelected;
}
