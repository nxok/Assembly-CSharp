using System;

public abstract class WeatherParams
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public WeatherParams()
	{
	}

	public static float EnclosureDetectionThreshold = 0.15f;
}
