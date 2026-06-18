using System;
using System.Xml.Linq;
using Audio;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionAltSounds : MinEventActionTargetedBase
{
	public override void Execute(MinEventParams _params)
	{
		for (int i = 0; i < this.targets.Count; i++)
		{
			if (this.targets[i] is EntityPlayerLocal)
			{
				EntityVehicle entityVehicle = this.targets[i].AttachedToEntity as EntityVehicle;
				if (entityVehicle != null)
				{
					entityVehicle.vehicle.FireEvent(Vehicle.Event.Stop);
					Manager.Instance.bUseAltSounds = this.enabled;
					entityVehicle.vehicle.FireEvent(Vehicle.Event.Start);
				}
				else
				{
					Manager.Instance.bUseAltSounds = this.enabled;
				}
			}
		}
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "enabled")
		{
			this.enabled = StringParsers.ParseBool(_attribute.Value, 0, -1, true);
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool enabled;
}
