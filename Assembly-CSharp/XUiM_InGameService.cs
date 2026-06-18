using System;
using System.Text;

public class XUiM_InGameService : XUiModel
{
	public static string GetServiceStats(XUi _xui, InGameService _service)
	{
		if (_service == null)
		{
			return "";
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (_service.ServiceType == InGameService.InGameServiceTypes.VendingRent)
		{
			TraderInfo traderInfo = (_xui.Trader.Trader as TileEntityVendingMachine).TraderData.TraderInfo;
			stringBuilder.Append(XUiM_InGameService.StringFormatHandler(Localization.Get("xuiCost", false, null), traderInfo.RentCost));
			stringBuilder.Append(XUiM_InGameService.StringFormatHandler(Localization.Get("xuiGameTime", false, null), traderInfo.RentTimeInDays, Localization.Get("xuiGameDays", false, null)));
		}
		return stringBuilder.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string StringFormatHandler(string _title, object _value)
	{
		return string.Format("{0}: [REPLACE_COLOR]{1}[-]\n", _title, _value);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string StringFormatHandler(string _title, object _value, string _units)
	{
		return string.Format("{0}: [REPLACE_COLOR]{1} {2}[-]\n", _title, _value, _units);
	}
}
