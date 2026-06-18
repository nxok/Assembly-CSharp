using System;
using Audio;

public class XUiM_Vehicle : XUiModel
{
	public EntityVehicle CurrentVehicle { get; set; }

	public static string GetEntityName(XUi _xui)
	{
		if (!(_xui.Vehicle.CurrentVehicle != null))
		{
			return "";
		}
		return _xui.Vehicle.CurrentVehicle.EntityName;
	}

	public static float GetSpeed(XUi _xui)
	{
		if (!(_xui.Vehicle.CurrentVehicle != null))
		{
			return 0f;
		}
		return _xui.Vehicle.CurrentVehicle.GetVehicle().MaxPossibleSpeed;
	}

	public static string GetNoise(XUi _xui)
	{
		EntityVehicle currentVehicle = _xui.Vehicle.CurrentVehicle;
		if (currentVehicle == null)
		{
			return "";
		}
		float noise = currentVehicle.GetVehicle().GetNoise();
		string text;
		if (noise > 0.33f)
		{
			if (noise > 0.66f)
			{
				text = Localization.Get("xuiVehicleNoiseLoud", false, null);
			}
			else
			{
				text = Localization.Get("xuiVehicleNoiseModerate", false, null);
			}
		}
		else
		{
			text = Localization.Get("xuiVehicleNoiseSoft", false, null);
		}
		return text;
	}

	public static float GetProtection(XUi _xui)
	{
		EntityVehicle currentVehicle = _xui.Vehicle.CurrentVehicle;
		if (currentVehicle == null)
		{
			return 0f;
		}
		return (1f - currentVehicle.GetVehicle().GetPlayerDamagePercent()) * 100f;
	}

	public static float GetFuelLevel(XUi _xui)
	{
		if (!(_xui.Vehicle.CurrentVehicle != null))
		{
			return 0f;
		}
		return _xui.Vehicle.CurrentVehicle.GetVehicle().GetFuelPercent() * 100f;
	}

	public static float GetFuelFill(XUi _xui)
	{
		if (!(_xui.Vehicle.CurrentVehicle != null))
		{
			return 0f;
		}
		return _xui.Vehicle.CurrentVehicle.GetVehicle().GetFuelPercent();
	}

	public static int GetPassengers(XUi _xui)
	{
		if (!(_xui.Vehicle.CurrentVehicle != null))
		{
			return 1;
		}
		return _xui.Vehicle.CurrentVehicle.GetAttachMaxCount();
	}

	public static string GetSpeedText(XUi _xui)
	{
		float num = ((_xui.Vehicle.CurrentVehicle != null) ? _xui.Vehicle.CurrentVehicle.GetVehicle().MaxPossibleSpeed : 0f);
		string text;
		if (num <= 9f)
		{
			if (num > 0f)
			{
				text = Localization.Get("xuiVehicleSpeedSlow", false, null);
			}
			else
			{
				text = Localization.Get("xuiVehicleSpeedNone", false, null);
			}
		}
		else if (num > 12f)
		{
			text = Localization.Get("xuiVehicleSpeedFast", false, null);
		}
		else
		{
			text = Localization.Get("xuiVehicleSpeedNormal", false, null);
		}
		return text;
	}

	public bool SetPart(XUi _xui, string _vehicleSlotName, ItemStack _stack, out ItemStack _resultStack)
	{
		Log.Warning("XUiM_Vehicle SetPart {0}", new object[] { _vehicleSlotName });
		this.CurrentVehicle == null;
		_resultStack = _stack;
		return false;
	}

	public void RefreshVehicle()
	{
	}

	public static bool RepairVehicle(XUi _xui, Vehicle _vehicle = null)
	{
		if (_vehicle == null)
		{
			_vehicle = _xui.Vehicle.CurrentVehicle.GetVehicle();
		}
		ItemValue item = ItemClass.GetItem("resourceRepairKit", false);
		if (item.ItemClass == null)
		{
			return false;
		}
		EntityPlayerLocal entityPlayer = _xui.playerUI.entityPlayer;
		LocalPlayerUI playerUI = _xui.playerUI;
		int itemCount = entityPlayer.bag.GetItemCount(item, -1, -1, true);
		int itemCount2 = entityPlayer.inventory.GetItemCount(item, false, -1, -1, true);
		int repairAmountNeeded = _vehicle.GetRepairAmountNeeded();
		if (itemCount + itemCount2 <= 0 || repairAmountNeeded <= 0)
		{
			if (repairAmountNeeded > itemCount + itemCount2)
			{
				Manager.PlayInsidePlayerHead("misc/missingitemtorepair", -1, 0f, false, false);
			}
			return false;
		}
		float num = 0f;
		ProgressionValue progressionValue = entityPlayer.Progression.GetProgressionValue("perkGreaseMonkey");
		if (progressionValue != null)
		{
			num += (float)progressionValue.Level * 0.1f;
		}
		_vehicle.RepairParts(1000, num);
		if (itemCount2 > 0)
		{
			entityPlayer.inventory.DecItem(item, 1, false, null);
		}
		else
		{
			entityPlayer.bag.DecItem(item, 1, false, null);
		}
		playerUI.xui.CollectedItemList.RemoveItemStack(new ItemStack(item, 1));
		Manager.PlayInsidePlayerHead("craft_complete_item", -1, 0f, false, false);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cRepairBase = 1000;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cRepairPercent = 0f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cRepairPerkPercent = 0.1f;
}
