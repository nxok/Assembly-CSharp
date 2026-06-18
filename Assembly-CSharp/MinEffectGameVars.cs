using System;

public class MinEffectGameVars
{
	public static float GetValueForVar(string varName)
	{
		if (!(varName == "#self") && !(varName == "#buff"))
		{
			varName == "#other";
		}
		return 0f;
	}
}
