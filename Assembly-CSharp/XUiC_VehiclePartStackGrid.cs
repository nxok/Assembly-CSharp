using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_VehiclePartStackGrid : XUiC_ItemPartStackGrid
{
	public override XUiC_ItemStack.StackLocationTypes StackLocation
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return XUiC_ItemStack.StackLocationTypes.Vehicle;
		}
	}

	public Vehicle CurrentVehicle { get; set; }

	public override void Init()
	{
		base.Init();
	}

	public void SetMods(ItemValue[] mods)
	{
		base.SetParts(mods);
	}
}
