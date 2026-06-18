using System;
using Audio;
using InControl;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ItemActionEntry : XUiController
{
	public BaseItemActionEntry ItemActionEntry
	{
		get
		{
			return this.itemActionEntry;
		}
		set
		{
			if (this.itemActionEntry != null)
			{
				this.itemActionEntry.ParentItem = null;
			}
			this.itemActionEntry = value;
			this.background.Enabled = value != null;
			if (this.itemActionEntry != null)
			{
				PlayerAction playerAction;
				switch (this.itemActionEntry.ShortCut)
				{
				case BaseItemActionEntry.GamepadShortCut.DPadUp:
					playerAction = this.xui.playerUI.playerInput.GUIActions.DPad_Up;
					break;
				case BaseItemActionEntry.GamepadShortCut.DPadLeft:
					playerAction = this.xui.playerUI.playerInput.GUIActions.DPad_Left;
					break;
				case BaseItemActionEntry.GamepadShortCut.DPadRight:
					playerAction = this.xui.playerUI.playerInput.GUIActions.DPad_Right;
					break;
				case BaseItemActionEntry.GamepadShortCut.DPadDown:
					playerAction = this.xui.playerUI.playerInput.GUIActions.DPad_Down;
					break;
				default:
					playerAction = null;
					break;
				}
				PlayerAction playerAction2 = playerAction;
				this.gamepadIcon.Action = playerAction2;
				this.keyboardButton.Text = playerAction2.GetBindingString(false, PlayerInputManager.InputStyle.Undefined, XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.KeyboardWithAngleBrackets, false, null);
				this.itemActionEntry.ParentItem = this;
				this.itemActionEntry.RefreshEnabled();
			}
			this.background.IsNavigatable = this.itemActionEntry != null;
			this.UpdateBindingsVisibility();
			base.RefreshBindings();
		}
	}

	public XUiController Background
	{
		get
		{
			return this.background.Controller;
		}
	}

	public XUiV_GamepadIcon GamepadIcon
	{
		get
		{
			return this.gamepadIcon;
		}
	}

	public override void Init()
	{
		base.Init();
		this.lblName = base.GetChildById("name").ViewComponent as XUiV_Label;
		this.icoIcon = base.GetChildById("icon").ViewComponent as XUiV_Sprite;
		this.background = base.GetChildById("background").ViewComponent as XUiV_Sprite;
		this.gamepadIcon = base.GetChildById("gamepadIcon").ViewComponent as XUiV_GamepadIcon;
		this.keyboardButton = base.GetChildById("keyboardButton").ViewComponent as XUiV_Label;
		this.background.Controller.OnPress += this.OnPressAction;
		this.background.Controller.OnHover += this.OnHover;
		this.IsDirty = true;
		base.registerForInputStyleChanges();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public new void OnHover(XUiController _sender, bool _isOver)
	{
		XUiV_Sprite xuiV_Sprite = (XUiV_Sprite)_sender.ViewComponent;
		this.isOver = _isOver;
		if (this.itemActionEntry == null)
		{
			xuiV_Sprite.Color = this.defaultBackgroundColor;
			xuiV_Sprite.SpriteName = "menu_empty";
			return;
		}
		if (xuiV_Sprite != null)
		{
			if (_isOver)
			{
				xuiV_Sprite.Color = Color.white;
				xuiV_Sprite.SpriteName = "ui_game_select_row";
				return;
			}
			xuiV_Sprite.Color = this.defaultBackgroundColor;
			xuiV_Sprite.SpriteName = "menu_empty";
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPressAction(XUiController _sender, int _mouseButton)
	{
		EntityPlayerLocal entityPlayer = this.xui.playerUI.entityPlayer;
		if (this.itemActionEntry != null)
		{
			if (this.itemActionEntry.Enabled)
			{
				Manager.PlayInsidePlayerHead(this.itemActionEntry.SoundName, -1, 0f, false, false);
				this.itemActionEntry.OnActivated();
			}
			else
			{
				Manager.PlayInsidePlayerHead(this.itemActionEntry.DisabledSound, -1, 0f, false, false);
				this.itemActionEntry.OnDisabledActivate();
			}
			this.background.Color = this.defaultBackgroundColor;
			this.background.SpriteName = "menu_empty";
			this.wasPressed = true;
		}
	}

	public override void Update(float _dt)
	{
		if (this.isOver && !this.background.UiTransformIsHovered)
		{
			this.background.Color = this.defaultBackgroundColor;
			this.background.SpriteName = "menu_empty";
			this.isOver = false;
		}
		if (this.isOver && this.wasPressed && this.itemActionEntry != null)
		{
			this.background.Color = Color.white;
			this.background.SpriteName = "ui_game_select_row";
			this.wasPressed = false;
		}
		if (this.IsDirty)
		{
			base.RefreshBindings();
			this.IsDirty = false;
		}
		base.RefreshBindings();
		base.Update(_dt);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void inputStyleChanged(PlayerInputManager.InputStyle _oldStyle, PlayerInputManager.InputStyle _newStyle)
	{
		base.inputStyleChanged(_oldStyle, _newStyle);
		this.UpdateBindingsVisibility();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		if (bindingName == "actionicon")
		{
			value = ((this.itemActionEntry != null) ? this.itemActionEntry.IconName : "");
			return true;
		}
		if (bindingName == "actionname")
		{
			value = ((this.itemActionEntry != null) ? this.itemActionEntry.ActionName : "");
			return true;
		}
		if (bindingName == "statuscolor")
		{
			value = "255,255,255,255";
			if (this.itemActionEntry != null)
			{
				Color32 color = (this.itemActionEntry.Enabled ? this.defaultFontColor : this.disabledFontColor);
				value = this.statuscolorFormatter.Format(color);
			}
			return true;
		}
		if (!(bindingName == "inspectheld"))
		{
			return base.GetBindingValueInternal(ref value, bindingName);
		}
		value = ((this.itemActionEntry != null && this.xui.playerUI.playerInput.GUIActions.Inspect.IsPressed) ? "true" : "false");
		return true;
	}

	public void StartTimedAction(float time)
	{
		GameManager.Instance.gameObject.AddComponent<XUiC_ItemActionEntry.TimedAction>().InitiateTimer(this.itemActionEntry, time);
	}

	public override bool ParseAttribute(string name, string value)
	{
		bool flag = base.ParseAttribute(name, value);
		if (!flag)
		{
			if (!(name == "default_font_color"))
			{
				if (!(name == "disabled_font_color"))
				{
					if (!(name == "default_background_color"))
					{
						return false;
					}
					this.defaultBackgroundColor = StringParsers.ParseColor32(value);
				}
				else
				{
					this.disabledFontColor = StringParsers.ParseColor32(value);
				}
			}
			else
			{
				this.defaultFontColor = StringParsers.ParseColor32(value);
			}
			return true;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateBindingsVisibility()
	{
		bool flag = base.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard;
		bool flag2 = this.itemActionEntry != null && this.itemActionEntry.ShortCut != BaseItemActionEntry.GamepadShortCut.None;
		this.gamepadIcon.IsVisible = !flag && flag2;
		this.keyboardButton.IsVisible = flag && flag2;
	}

	public void MarkDirty()
	{
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public BaseItemActionEntry itemActionEntry;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiV_Label lblName;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiV_Sprite icoIcon;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiV_Sprite background;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiV_GamepadIcon gamepadIcon;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiV_Label keyboardButton;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 defaultBackgroundColor = Color.gray;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 disabledFontColor = Color.gray;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 defaultFontColor = Color.white;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isOver;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor statuscolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasPressed;

	public class TimedAction : MonoBehaviour
	{
		public void InitiateTimer(BaseItemActionEntry itemActionEntry, float _amount)
		{
			this.itemActionEntry = itemActionEntry;
			this.waitTime = Time.realtimeSinceStartup + _amount;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Update()
		{
			if (this.waitTime != 0f && Time.realtimeSinceStartup >= this.waitTime)
			{
				this.waitTime = 0f;
				this.itemActionEntry.OnTimerCompleted();
				Object.Destroy(this);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public BaseItemActionEntry itemActionEntry;

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public float waitTime;
	}
}
