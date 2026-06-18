using System;

public static class BundleTags
{
	public static string Tag
	{
		get
		{
			if (!PlatformOptimizations.LoadHalfResAssets)
			{
				return string.Empty;
			}
			return "_halfres";
		}
	}

	public const string TagHalfRes = "_halfres";
}
