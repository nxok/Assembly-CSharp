using System;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_NewsScreen : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiC_NewsScreen.ID = base.WindowGroup.Id;
		XUiController childById = base.GetChildById("btnContinue");
		if (childById != null && childById.ViewComponent is XUiV_Button)
		{
			childById.OnPress += this.BtnContinue_OnPress;
		}
		this.controllerContinueLabel = base.GetChildById("continueButtonLabelController").ViewComponent as XUiV_Label;
		if (DeviceFlag.PS5.IsCurrent())
		{
			this.UpdateContinueLabel(PlayerInputManager.InputStyle.PS4);
			return;
		}
		if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX).IsCurrent())
		{
			this.UpdateContinueLabel(PlayerInputManager.InputStyle.XB1);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void inputStyleChanged(PlayerInputManager.InputStyle _oldStyle, PlayerInputManager.InputStyle _newStyle)
	{
		base.inputStyleChanged(_oldStyle, _newStyle);
		base.RefreshBindings();
		this.UpdateContinueLabel(_newStyle);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateContinueLabel(PlayerInputManager.InputStyle _style)
	{
		if (_style != PlayerInputManager.InputStyle.Keyboard)
		{
			string text;
			if (PlatformManager.NativePlatform.Input.CurrentControllerInputStyle == PlayerInputManager.InputStyle.PS4)
			{
				text = "[sp=PS5_Button_Options]";
			}
			else
			{
				text = "[sp=XB_Button_Menu]";
			}
			string text2 = Localization.Get("xuiNewsContinueController", false, null).ToUpper();
			text2 = string.Format(text2, text);
			this.controllerContinueLabel.Text = text2;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnContinue_OnPress(XUiController _sender, int _mousebutton)
	{
		this.xui.playerUI.windowManager.Close(base.WindowGroup, false);
		XUiC_MainMenu.Open(this.xui);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		this.UpdateInput();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateInput()
	{
		PlayerActionsGUI guiactions = this.xui.playerUI.playerInput.GUIActions;
		if ((guiactions.Apply.WasReleased || guiactions.Cancel.WasReleased) && XUiUtils.HotkeysAllowedFor(this.viewComponent, false))
		{
			this.BtnContinue_OnPress(this, -1);
		}
	}

	public static void Open(XUi _xuiInstance)
	{
		_xuiInstance.playerUI.windowManager.Open(XUiC_NewsScreen.ID, true, true);
	}

	public override void OnClose()
	{
		base.OnClose();
		XUiC_MessageBoxWindowGroup.Close(this.xui);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label controllerContinueLabel;
}
