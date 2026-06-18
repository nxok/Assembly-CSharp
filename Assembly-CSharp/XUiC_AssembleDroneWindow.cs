using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_AssembleDroneWindow : XUiC_AssembleWindow
{
	public override void OnOpen()
	{
		base.OnOpen();
		this.IsDirty = true;
	}

	public override ItemStack ItemStack
	{
		set
		{
			this.group.CurrentVehicleEntity.LoadMods();
			base.ItemStack = value;
		}
	}

	public override void OnChanged()
	{
		this.group.OnItemChanged(this.ItemStack);
		this.IsDirty = true;
	}

	public override void Init()
	{
		base.Init();
		XUiController childById = base.GetChildById("btnRepair");
		if (childById != null)
		{
			this.btnRepair_Background = (XUiV_Button)childById.GetChildById("clickable").ViewComponent;
			this.btnRepair_Background.Controller.OnPress += this.BtnRepair_OnPress;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnRepair_OnPress(XUiController _sender, int _mouseButton)
	{
		XUiC_DroneWindowGroup xuiC_DroneWindowGroup = this.group;
		EntityDrone entityDrone = ((xuiC_DroneWindowGroup != null) ? xuiC_DroneWindowGroup.CurrentVehicleEntity : null);
		if (entityDrone)
		{
			entityDrone.DoRepairAction(this.xui.playerUI);
			base.RefreshBindings();
			this.IsDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		XUiC_DroneWindowGroup xuiC_DroneWindowGroup = this.group;
		EntityDrone entityDrone = ((xuiC_DroneWindowGroup != null) ? xuiC_DroneWindowGroup.CurrentVehicleEntity : null);
		if (bindingName == "vehicledurability")
		{
			value = ((entityDrone != null) ? this.vehicleDurabilityFormatter.Format(entityDrone.Health, entityDrone.GetMaxHealth()) : "");
			return true;
		}
		if (!(bindingName == "vehicledurabilitytitle"))
		{
			return false;
		}
		value = Localization.Get("xuiDurability", false, null);
		return true;
	}

	public XUiC_DroneWindowGroup group;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button btnRepair_Background;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int, int> vehicleDurabilityFormatter = new CachedStringFormatter<int, int>((int _i1, int _i2) => string.Format("{0}/{1}", _i1, _i2));
}
