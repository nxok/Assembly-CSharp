using System;
using Platform;

public static class UIOptions
{
	public static OptionsVideoWindowMode OptionsVideoWindow
	{
		get
		{
			return UIOptions.optionsVideoWindow;
		}
		set
		{
			UIOptions.optionsVideoWindow = value;
			Action<OptionsVideoWindowMode> onOptionsVideoWindowChanged = UIOptions.OnOptionsVideoWindowChanged;
			if (onOptionsVideoWindowChanged == null)
			{
				return;
			}
			onOptionsVideoWindowChanged(value);
		}
	}

	public static event Action<OptionsVideoWindowMode> OnOptionsVideoWindowChanged;

	public static void Init()
	{
		UIOptions.optionsVideoWindow = ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent() ? OptionsVideoWindowMode.Simplified : OptionsVideoWindowMode.Detailed);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static OptionsVideoWindowMode optionsVideoWindow;
}
