using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_MainMenuPlayerName : XUiController
{
	[XuiXmlBinding("name")]
	public string PlayerName
	{
		get
		{
			return GamePrefs.GetString(EnumGamePrefs.PlayerName) ?? "";
		}
	}
}
