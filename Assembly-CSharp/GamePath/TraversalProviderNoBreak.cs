using System;
using Pathfinding;

namespace GamePath
{
	[PublicizedFrom(EAccessModifier.Internal)]
	public class TraversalProviderNoBreak : ITraversalProvider
	{
		public bool CanTraverseConnection(Path path, Connection conn)
		{
			if (!conn.node.Walkable)
			{
				return false;
			}
			if (conn.tag == 3U)
			{
				TEFeatureDoor tefeatureDoor = conn.payload as TEFeatureDoor;
				if (tefeatureDoor != null)
				{
					if (tefeatureDoor.IsOpen())
					{
						return true;
					}
					if (((path.enabledTags >> 3) & 1) == 0)
					{
						return false;
					}
					bool flag;
					if (!tefeatureDoor.CanOpen(out flag))
					{
						return false;
					}
				}
				return true;
			}
			return ((path.enabledTags >> (int)conn.node.Tag) & 1) != 0;
		}

		public bool CanTraverse(Path path, GraphNode node, int gridDirection)
		{
			if (!node.Walkable)
			{
				return false;
			}
			if (!path.CanBreakBlocks && gridDirection >= 0)
			{
				AstarVoxelGrid.VoxelNode voxelNode = node as AstarVoxelGrid.VoxelNode;
				if (voxelNode != null && ((int)voxelNode.BlockerFlags & (17 << gridDirection)) > 0)
				{
					return false;
				}
			}
			return ((path.enabledTags >> (int)node.Tag) & 1) != 0 && node.Penalty < 1000U;
		}

		public uint GetTraversalCost(Path path, GraphNode node)
		{
			return node.Penalty;
		}
	}
}
