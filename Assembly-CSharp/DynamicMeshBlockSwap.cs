using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using ConcurrentCollections;

public class DynamicMeshBlockSwap
{
	public static bool IsValidBlock(int type)
	{
		return DynamicMeshBlockSwap.OpaqueBlocks.Contains(type) || DynamicMeshBlockSwap.TerrainBlocks.Contains(type);
	}

	public static void Init()
	{
		DynamicMeshBlockSwap.BlockSwaps.Clear();
		DynamicMeshBlockSwap.TextureSwaps.Clear();
		DynamicMeshBlockSwap.OpaqueBlocks.Clear();
		DynamicMeshBlockSwap.DoorBlocks.Clear();
		DynamicMeshBlockSwap.TerrainBlocks.Clear();
		DynamicMeshBlockSwap.DoorReplacement = Block.GetBlockValue("imposterBlock", true);
		if (DynamicMeshBlockSwap.DoorReplacement.isair)
		{
			Log.Warning("Dynamic mesh door replacement block not found");
		}
		else
		{
			Log.Out("Dymesh door replacement: " + DynamicMeshBlockSwap.DoorReplacement.Block.GetBlockName());
		}
		foreach (Block block in Block.list)
		{
			if (block != null && block.blockID != 0)
			{
				BlockCompositeTileEntity blockCompositeTileEntity = block as BlockCompositeTileEntity;
				bool flag = blockCompositeTileEntity != null && blockCompositeTileEntity.CompositeData.HasFeature<TEFeatureDoor>();
				if (block.MeshIndex == 0 || flag)
				{
					int type = Block.GetBlockValue(block.GetBlockName(), false).type;
					bool flag2 = block is BlockModelTree;
					bool flag3 = block.shape is BlockShapeModelEntity;
					if (!flag2 && !block.IsPlant() && !flag3)
					{
						DynamicMeshBlockSwap.OpaqueBlocks.Add(type);
					}
					if (flag)
					{
						DynamicMeshBlockSwap.DoorBlocks.Add(type);
					}
					if (block.bImposterExcludeAndStop || block.bImposterExclude || (block.IsTerrainDecoration && block.ImposterExchange == 0))
					{
						DynamicMeshBlockSwap.BlockSwaps.TryAdd(block.blockID, 0);
					}
					else if (block.ImposterExchange != 0)
					{
						DynamicMeshBlockSwap.BlockSwaps.TryAdd(block.blockID, block.ImposterExchange);
						DynamicMeshBlockSwap.TextureSwaps.TryAdd(block.blockID, (long)((ulong)block.ImposterExchangeTexIdx));
					}
				}
				else if (block.MeshIndex == 5)
				{
					int type2 = Block.GetBlockValue(block.GetBlockName(), false).type;
					DynamicMeshBlockSwap.TerrainBlocks.Add(type2);
				}
			}
		}
	}

	public static BlockValue DoorReplacement;

	public static ConcurrentHashSet<int> DoorBlocks = new ConcurrentHashSet<int>();

	public static ConcurrentHashSet<int> OpaqueBlocks = new ConcurrentHashSet<int>();

	public static ConcurrentHashSet<int> TerrainBlocks = new ConcurrentHashSet<int>();

	public static ConcurrentDictionary<int, int> BlockSwaps = new ConcurrentDictionary<int, int>();

	public static ConcurrentDictionary<int, long> TextureSwaps = new ConcurrentDictionary<int, long>();

	public static HashSet<int> InvalidPaintIds = new HashSet<int>();
}
