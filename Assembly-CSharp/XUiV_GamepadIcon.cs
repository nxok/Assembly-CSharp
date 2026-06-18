using System;
using GUI_2;
using InControl;
using Platform;

public class XUiV_GamepadIcon : XUiV_Sprite
{
	public XUiV_GamepadIcon(XUi _xui, string _id)
		: base(_xui, _id)
	{
	}

	[XuiXmlAttribute("button", false)]
	public UIUtils.ButtonIcon Button
	{
		get
		{
			UIUtils.ButtonIcon? buttonIcon = this.button;
			if (buttonIcon == null)
			{
				return UIUtils.ButtonIcon.None;
			}
			return buttonIcon.GetValueOrDefault();
		}
		set
		{
			UIUtils.ButtonIcon? buttonIcon = this.button;
			if ((buttonIcon.GetValueOrDefault() == value) & (buttonIcon != null))
			{
				return;
			}
			this.button = new UIUtils.ButtonIcon?(value);
			base.SetDirty();
		}
	}

	[XuiXmlAttribute("actionset", false)]
	public XUiV_GamepadIcon.EActionSet ActionSet
	{
		get
		{
			return this.actionSet;
		}
		set
		{
			if (this.actionSet == value)
			{
				return;
			}
			this.actionSet = value;
			base.SetDirty();
		}
	}

	[XuiXmlAttribute("actionname", false)]
	public string ActionName
	{
		get
		{
			return this.actionName;
		}
		set
		{
			if (this.actionName == value)
			{
				return;
			}
			this.actionName = value;
			base.SetDirty();
		}
	}

	[XuiXmlAttribute("action", false)]
	public PlayerAction Action
	{
		get
		{
			return this.action;
		}
		set
		{
			if (this.action == value)
			{
				return;
			}
			this.action = value;
			base.SetDirty();
		}
	}

	public override void InitView()
	{
		base.UIAtlas = UIUtils.IconAtlas.Name;
		base.InitView();
		PlatformManager.NativePlatform.Input.OnLastInputStyleChanged += this.OnLastInputStyleChanged;
		this.curInput = PlatformManager.NativePlatform.Input.CurrentInputStyle;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnLastInputStyleChanged(PlayerInputManager.InputStyle _style)
	{
		this.curInput = _style;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		IPlatform nativePlatform = PlatformManager.NativePlatform;
		if (((nativePlatform != null) ? nativePlatform.Input : null) != null)
		{
			PlatformManager.NativePlatform.Input.OnLastInputStyleChanged -= this.OnLastInputStyleChanged;
		}
	}

	public override void Update(float _dt)
	{
		if (this.curInput != this.lastInputStyle)
		{
			this.lastInputStyle = this.curInput;
			base.SetDirty();
		}
		base.Update(_dt);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateData()
	{
		base.UIAtlas = ((this.curInput == PlayerInputManager.InputStyle.Keyboard) ? "" : UIUtils.IconAtlas.Name);
		PlayerAction playerAction = null;
		if (!string.IsNullOrEmpty(this.ActionName))
		{
			PlayerActionSet playerActionSet;
			switch (this.ActionSet)
			{
			case XUiV_GamepadIcon.EActionSet.Global:
				playerActionSet = PlayerActionsGlobal.Instance;
				break;
			case XUiV_GamepadIcon.EActionSet.Local:
				playerActionSet = LocalPlayerUI.primaryUI.playerInput;
				break;
			case XUiV_GamepadIcon.EActionSet.Vehicle:
				playerActionSet = LocalPlayerUI.primaryUI.playerInput.VehicleActions;
				break;
			case XUiV_GamepadIcon.EActionSet.Gui:
				playerActionSet = LocalPlayerUI.primaryUI.playerInput.GUIActions;
				break;
			case XUiV_GamepadIcon.EActionSet.Permanent:
				playerActionSet = LocalPlayerUI.primaryUI.playerInput.PermanentActions;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			playerAction = playerActionSet.GetPlayerActionByName(this.ActionName);
		}
		else if (this.Action != null)
		{
			playerAction = this.Action;
		}
		UIUtils.ButtonIcon buttonIcon = ((playerAction != null) ? UIUtils.GetButtonIconForAction(playerAction) : this.Button);
		base.SpriteName = UIUtils.GetSpriteName(buttonIcon);
		base.updateData();
	}

	public override void SetDefaults(XUiController _parent)
	{
		base.SetDefaults(_parent);
		base.KeepSourceAspectRatio = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerInputManager.InputStyle lastInputStyle = PlayerInputManager.InputStyle.Count;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerInputManager.InputStyle curInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public UIUtils.ButtonIcon? button;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_GamepadIcon.EActionSet actionSet = XUiV_GamepadIcon.EActionSet.Gui;

	[PublicizedFrom(EAccessModifier.Private)]
	public string actionName;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerAction action;

	public enum EActionSet
	{
		Global,
		Local,
		Vehicle,
		Gui,
		Permanent
	}
}
