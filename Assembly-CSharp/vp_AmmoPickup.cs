using System;

public class vp_AmmoPickup : vp_Pickup
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool TryGive(vp_FPPlayerEventHandler player)
	{
		if (player.Dead.Active)
		{
			return false;
		}
		int i = 0;
		while (i < this.GiveAmount)
		{
			if (!base.TryGive(player))
			{
				if (this.TryReloadIfEmpty(player))
				{
					base.TryGive(player);
					return true;
				}
				return false;
			}
			else
			{
				i++;
			}
		}
		this.TryReloadIfEmpty(player);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryReloadIfEmpty(vp_FPPlayerEventHandler player)
	{
		return player.CurrentWeaponAmmoCount.Get() <= 0 && !(player.CurrentWeaponClipType.Get() != this.InventoryName) && player.Reload.TryStart(true);
	}

	public int GiveAmount = 1;
}
