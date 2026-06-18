using System;
using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdListDLC : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "listdlc", "dlcs" };
	}

	public override bool AllowedInMainMenu
	{
		get
		{
			return true;
		}
	}

	public override DeviceFlag AllowedDeviceTypes
	{
		get
		{
			return DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "List the available DLC and their current entitlement status.";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "List DLCs and their entitlement states.\nUsage:\n   listdlc\n   dlcs\n";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		ConsoleCmdListDLC.ExecuteList();
	}

	public static void ExecuteList()
	{
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("DLC List:");
		foreach (object obj in Enum.GetValues(typeof(EntitlementSetEnum)))
		{
			EntitlementSetEnum entitlementSetEnum = (EntitlementSetEnum)obj;
			if (entitlementSetEnum != EntitlementSetEnum.None)
			{
				ValueTuple<bool, bool> valueTuple = EntitlementManager.Instance.CheckOverride(entitlementSetEnum);
				bool item = valueTuple.Item1;
				bool item2 = valueTuple.Item2;
				bool flag = EntitlementManager.Instance.HasEntitlement(entitlementSetEnum);
				bool flag2 = EntitlementManager.Instance.IsAvailableOnPlatform(entitlementSetEnum);
				bool flag3 = EntitlementManager.Instance.IsEntitlementPurchasable(entitlementSetEnum);
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("  {0}: {1} => Entitlement State: {2}, Available on Platform: {3}, Purchasable: {4}, Override State: {5}", new object[]
				{
					(int)entitlementSetEnum,
					entitlementSetEnum,
					flag,
					flag2,
					flag3,
					item ? item2.ToString() : "Not Overridden"
				}));
			}
		}
	}
}
