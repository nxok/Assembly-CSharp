using System;

public class PowerTripWireRelay : PowerTrigger
{
	public override PowerItem.PowerItemTypes PowerItemType
	{
		get
		{
			return PowerItem.PowerItemTypes.TripWireRelay;
		}
	}
}
