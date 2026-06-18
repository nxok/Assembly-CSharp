using System;

public class PowerElectricWireRelay : PowerConsumer
{
	public override PowerItem.PowerItemTypes PowerItemType
	{
		get
		{
			return PowerItem.PowerItemTypes.ElectricWireRelay;
		}
	}
}
