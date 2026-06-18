using System;

public class RegionFileDebugUtilSectorBased : IRegionFileDebugUtil
{
	public string GetLocationString(int chunkX, int chunkZ)
	{
		int num = (int)Math.Floor((double)chunkX / 32.0);
		int num2 = (int)Math.Floor((double)chunkZ / 32.0);
		return string.Format("XZ: {0}/{1}  Region: r.{2}.{3}", new object[] { chunkX, chunkZ, num, num2 });
	}
}
