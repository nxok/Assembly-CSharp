using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;
using Unity.Burst;
using WorldGenerationEngineFinal;

[assembly: AssemblyVersion("0.0.0.0")]
[assembly: BurstCompiler.StaticTypeReinitAttribute(typeof(ChunkProviderGenerateWorldFromRaw.RoadSmooth_00005653$BurstDirectCall))]
[assembly: BurstCompiler.StaticTypeReinitAttribute(typeof(PathingUtils.FindDetailedPath_0000B5ED$BurstDirectCall))]
[assembly: BurstCompiler.StaticTypeReinitAttribute(typeof(PathingUtils.FindDetailedPath_0000B5EF$BurstDirectCall))]
[assembly: BurstCompiler.StaticTypeReinitAttribute(typeof(PathingUtils.CalcPathBounds_0000B5F0$BurstDirectCall))]
[assembly: BurstCompiler.StaticTypeReinitAttribute(typeof(PathingUtils.FindClosestPathPoint_0000B5F1$BurstDirectCall))]
[assembly: BurstCompiler.StaticTypeReinitAttribute(typeof(PathingUtils.FindClosestPathPoint_0000B5F2$BurstDirectCall))]
[assembly: BurstCompiler.StaticTypeReinitAttribute(typeof(PathingUtils.IsPointOnPath_0000B5F3$BurstDirectCall))]
[assembly: BurstCompiler.StaticTypeReinitAttribute(typeof(RawStamp.SmoothAlpha_0000B63C$BurstDirectCall))]
[assembly: BurstCompiler.StaticTypeReinitAttribute(typeof(StampManager.DrawStamp_0000B65F$BurstDirectCall))]
[assembly: BurstCompiler.StaticTypeReinitAttribute(typeof(StampManager.DrawWaterStamp_0000B662$BurstDirectCall))]
[assembly: BurstCompiler.StaticTypeReinitAttribute(typeof(WorldBuilder.ClearWaterUnderTerrain_0000B723$BurstDirectCall))]
[assembly: BurstCompiler.StaticTypeReinitAttribute(typeof(WorldBuilder.FinalizeWater_0000B726$BurstDirectCall))]
[assembly: BurstCompiler.StaticTypeReinitAttribute(typeof(WorldBuilder.SmoothRoadTerrainTask_0000B72D$BurstDirectCall))]
[assembly: BurstCompiler.StaticTypeReinitAttribute(typeof(WorldBuilder.AdjustHeights_0000B740$BurstDirectCall))]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
