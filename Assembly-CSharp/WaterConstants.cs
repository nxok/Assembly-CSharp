using System;

public static class WaterConstants
{
	public static int GetStableMassBelow(int mass, int massBelow)
	{
		return Utils.FastMin(mass + massBelow, 19500);
	}

	public const int MIN_MASS = 195;

	public const int MAX_MASS = 19500;

	public const int OVERFULL_MAX = 58500;

	public const int MIN_FLOW = 195;

	public const float FLOW_SPEED = 0.5f;

	public const int MIN_MASS_SIDE_SPREAD = 4875;
}
