using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace RaycastPathing
{
	[Preserve]
	public class RaycastNodeHierarcy
	{
		public void SetWayPoint(RaycastNode node)
		{
			this.flowToWaypoint = true;
			this.waypoint = node;
		}

		public void SetParent(RaycastNode node)
		{
			this.parent = node;
		}

		public void AddNeighbor(RaycastNode node)
		{
			if (!this.neighbors.Contains(node))
			{
				this.neighbors.Add(node);
			}
		}

		public RaycastNode GetNeighbor(Vector3 pos)
		{
			for (int i = 0; i < this.neighbors.Count; i++)
			{
				if (this.neighbors[i].Center == pos)
				{
					return this.neighbors[i];
				}
			}
			return null;
		}

		public void AddChild(RaycastNode node)
		{
			if (!this.children.Contains(node))
			{
				this.children.Add(node);
				if (node.nodeType == cPathNodeType.Air)
				{
					if (!this.childAirBlocks.Contains(node))
					{
						this.childAirBlocks.Add(node);
						return;
					}
				}
				else if (!this.childSolidBlocks.Contains(node))
				{
					this.childSolidBlocks.Add(node);
				}
			}
		}

		public RaycastNode parent;

		public List<RaycastNode> neighbors = new List<RaycastNode>();

		public List<RaycastNode> children = new List<RaycastNode>();

		public List<RaycastNode> childAirBlocks = new List<RaycastNode>();

		public List<RaycastNode> childSolidBlocks = new List<RaycastNode>();

		public bool flowToWaypoint;

		public RaycastNode waypoint;
	}
}
