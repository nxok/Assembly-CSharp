using System;
using Pathfinding;

namespace GamePath
{
	[PublicizedFrom(EAccessModifier.Internal)]
	public class TraversalProvider : ITraversalProvider
	{
		public bool CanTraverseConnection(Path path, Connection conn)
		{
			return conn.node.Walkable && ((path.enabledTags >> (int)conn.node.Tag) & 1) != 0;
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
			return ((path.enabledTags >> (int)node.Tag) & 1) != 0;
		}

		public uint GetTraversalCost(Path path, GraphNode node)
		{
			return (uint)(node.Penalty * path.CostScale);
		}
	}
}
