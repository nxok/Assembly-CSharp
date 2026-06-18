using System;

public class vp_WeaponPickup : vp_Pickup
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool TryGive(vp_FPPlayerEventHandler player)
	{
		if (player.Dead.Active)
		{
			return false;
		}
		if (!base.TryGive(player))
		{
			return false;
		}
		player.SetWeaponByName.Try(this.InventoryName);
		if (this.AmmoIncluded > 0)
		{
			player.AddAmmo.Try(new object[] { this.InventoryName, this.AmmoIncluded });
		}
		return true;
	}

	public int AmmoIncluded;
}
