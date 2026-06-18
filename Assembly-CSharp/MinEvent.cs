using System;

public class MinEvent
{
	public static MinEventTypes[] Start = new MinEventTypes[]
	{
		MinEventTypes.onSelfPrimaryActionStart,
		MinEventTypes.onSelfSecondaryActionStart,
		MinEventTypes.onSelfAction2Start
	};

	public static MinEventTypes[] Update = new MinEventTypes[]
	{
		MinEventTypes.onSelfPrimaryActionUpdate,
		MinEventTypes.onSelfSecondaryActionUpdate,
		MinEventTypes.onSelfAction2Update
	};

	public static MinEventTypes[] End = new MinEventTypes[]
	{
		MinEventTypes.onSelfPrimaryActionEnd,
		MinEventTypes.onSelfSecondaryActionEnd,
		MinEventTypes.onSelfAction2End
	};
}
