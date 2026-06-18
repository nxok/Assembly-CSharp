using System;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_EmptyInfoWindow : XUiC_InfoWindow
{
	public override void Init()
	{
		base.Init();
		this.descriptionText = base.GetChildById("descriptionText").ViewComponent as XUiV_Label;
		base.registerForInputStyleChanges();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.UpdateDescriptionText();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void inputStyleChanged(PlayerInputManager.InputStyle _oldStyle, PlayerInputManager.InputStyle _newStyle)
	{
		base.inputStyleChanged(_oldStyle, _newStyle);
		this.UpdateDescriptionText();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateDescriptionText()
	{
		if (!(DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent() && PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard)
		{
			this.descriptionText.Text = Localization.Get("xuiEmptyInfoPanelText", false, null);
			return;
		}
		PlayerInputManager.InputStyleFromSelectedIconStyle();
		string text = string.Format(Localization.Get("xuiEmptyInfoPanelTextController", false, null), new object[]
		{
			InControlExtensions.GetGamepadSourceString(19),
			InControlExtensions.GetGamepadSourceString(21),
			InControlExtensions.GetGamepadSourceString(21),
			InControlExtensions.GetGamepadSourceString(22),
			InControlExtensions.GetGamepadSourceString(22),
			InControlExtensions.GetGamepadSourceString(11),
			InControlExtensions.GetGamepadSourceString(12),
			InControlExtensions.GetGamepadSourceString(13),
			InControlExtensions.GetGamepadSourceString(14),
			InControlExtensions.GetGamepadSourceString(10),
			InControlExtensions.GetGamepadSourceString(5),
			InControlExtensions.GetGamepadSourceString(100)
		});
		this.descriptionText.Text = text;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label descriptionText;
}
