using System;

public class vp_Gameplay
{
	public static bool isMaster
	{
		get
		{
			return !vp_Gameplay.isMultiplayer || vp_Gameplay.m_IsMaster;
		}
		set
		{
			if (!vp_Gameplay.isMultiplayer)
			{
				return;
			}
			vp_Gameplay.m_IsMaster = value;
		}
	}

	public static bool isMultiplayer = false;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static bool m_IsMaster = true;
}
