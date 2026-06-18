using System;

public static class SaveDataLimitUtils
{
	public static long CalculatePlayerMapSize(Vector2i worldSize)
	{
		int num = worldSize.x * worldSize.y;
		if (num <= 0)
		{
			throw new ArgumentException(string.Format("Expected a positive value for the world area, but was: {0}", num), "worldSize");
		}
		return Math.Min((long)num / 256L * 516L, 270532608L);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const long CHUNK_SIZE = 16L;

	[PublicizedFrom(EAccessModifier.Private)]
	public const long CHUNK_AREA = 256L;

	[PublicizedFrom(EAccessModifier.Private)]
	public const long PLAYER_MAP_OVERHEAD_PER_CHUNK = 516L;

	[PublicizedFrom(EAccessModifier.Private)]
	public const long PLAYER_MAP_MAX_OVERHEAD = 270532608L;
}
