using System;
using UnityEngine;
using WorldGenerationEngineFinal;

[PublicizedFrom(EAccessModifier.Internal)]
public static class $BurstDirectCallInitializer
{
	[RuntimeInitializeOnLoadMethod(2)]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void Initialize()
	{
		ChunkProviderGenerateWorldFromRaw.RoadSmooth_00005653$BurstDirectCall.Initialize();
		PathingUtils.FindDetailedPath_0000B5ED$BurstDirectCall.Initialize();
		PathingUtils.FindDetailedPath_0000B5EF$BurstDirectCall.Initialize();
		PathingUtils.CalcPathBounds_0000B5F0$BurstDirectCall.Initialize();
		PathingUtils.FindClosestPathPoint_0000B5F1$BurstDirectCall.Initialize();
		PathingUtils.FindClosestPathPoint_0000B5F2$BurstDirectCall.Initialize();
		PathingUtils.IsPointOnPath_0000B5F3$BurstDirectCall.Initialize();
		RawStamp.SmoothAlpha_0000B63C$BurstDirectCall.Initialize();
		StampManager.DrawStamp_0000B65F$BurstDirectCall.Initialize();
		StampManager.DrawWaterStamp_0000B662$BurstDirectCall.Initialize();
		WorldBuilder.ClearWaterUnderTerrain_0000B723$BurstDirectCall.Initialize();
		WorldBuilder.FinalizeWater_0000B726$BurstDirectCall.Initialize();
		WorldBuilder.SmoothRoadTerrainTask_0000B72D$BurstDirectCall.Initialize();
		WorldBuilder.AdjustHeights_0000B740$BurstDirectCall.Initialize();
	}
}
