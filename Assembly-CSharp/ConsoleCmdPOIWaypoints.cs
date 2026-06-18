using System;
using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdPOIWaypoints : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient
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

	public override DeviceFlag AllowedDeviceTypesClient
	{
		get
		{
			return DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "poiwaypoints", "pwp" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Adds waypoints for specified POIs.";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return this.getDescription() + "\n\npwp * - adds waypoints to all POIs in the world.\npwp <name> - adds waypoints to all POIs that starts with the name.\npwp <distance> - adds waypoints to all POIs with the specified distance.\npwp * <distance> - adds waypoints to all POIs within the specified distance.\npwp <name> <distance> - adds waypoints to all POIs within the specified distance that start with the name.\npwp -clear - removes all POI waypoints.";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(this.GetHelp());
			return;
		}
		if (_params.Count != 1)
		{
			if (_params.Count == 2)
			{
				float num;
				if (float.TryParse(_params[1], out num))
				{
					if (_params[0] == "*")
					{
						this.CreateWaypoints("", num);
						return;
					}
					this.CreateWaypoints(_params[0], num);
					return;
				}
				else
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("\"" + _params[1] + "\" is not a valid distance.");
				}
			}
			return;
		}
		if (_params[0] == "-clear")
		{
			POIWaypoint.ClearAll(GameManager.Instance.World.GetPrimaryPlayer());
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("POI Waypoints have been cleared.");
			return;
		}
		if (_params[0] == "*")
		{
			this.CreateWaypoints("", 0f);
			return;
		}
		float num2;
		if (float.TryParse(_params[0], out num2))
		{
			this.CreateWaypoints("", num2);
			return;
		}
		this.CreateWaypoints(_params[0], 0f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateWaypoints(string filterName, float distance)
	{
		GameManager instance = GameManager.Instance;
		int num = 0;
		if (instance != null && instance.GetDynamicPrefabDecorator() != null)
		{
			List<PrefabInstance> list = new List<PrefabInstance>();
			instance.GetDynamicPrefabDecorator().GetWorldPrefabs(list);
			if (list != null)
			{
				float num2 = distance * distance;
				EntityPlayer primaryPlayer = instance.World.GetPrimaryPlayer();
				foreach (PrefabInstance prefabInstance in list)
				{
					if ((distance == 0f || (primaryPlayer.position - prefabInstance.boundingBoxPosition).sqrMagnitude < num2) && prefabInstance.boundingBoxSize.Volume() >= 100 && prefabInstance.name.StartsWith(filterName) && POIWaypoint.TrySet(primaryPlayer, prefabInstance.id, true))
					{
						num++;
					}
				}
			}
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Added {0} POI waypoints.", new object[] { num });
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int SmallPoiVolumeLimit = 100;
}
