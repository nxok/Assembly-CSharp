using System;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DemoWindow : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiC_DemoWindow.ID = base.WindowGroup.Id;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		GameManager.Instance.Pause(true);
	}

	public override void OnClose()
	{
		base.OnClose();
		GameManager.Instance.Pause(false);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		if (bindingName == "is_xbox")
		{
			value = (DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX).IsCurrent().ToString();
			return true;
		}
		if (!(bindingName == "is_ps5"))
		{
			return base.GetBindingValueInternal(ref value, bindingName);
		}
		value = (DeviceFlag.PS5.IsCurrent() || (DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX).IsCurrent()).ToString();
		return true;
	}

	public static string ID = "";
}
