using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace RaycastPathing
{
	[Preserve]
	public class RaycastNode
	{
		public RaycastNode(Vector3 pos, float scale = 1f, int depth = 0)
		{
			this.info = new RaycastNodeInfo(pos, scale, depth);
			this.hierarchy = new RaycastNodeHierarcy();
		}

		public RaycastNode(Vector3 min, Vector3 max, float scale = 1f, int depth = 0)
		{
			this.info = new RaycastNodeInfo(min, max, scale, depth);
			this.hierarchy = new RaycastNodeHierarcy();
		}

		public Vector3 Position
		{
			get
			{
				return this.info.Position;
			}
		}

		public Vector3 Center
		{
			get
			{
				return this.info.Center;
			}
		}

		public Vector3i BlockPos
		{
			get
			{
				return this.info.BlockPos;
			}
		}

		public float Scale
		{
			get
			{
				return this.info.Scale;
			}
		}

		public int Depth
		{
			get
			{
				return this.info.Depth;
			}
		}

		public Vector3 Min
		{
			get
			{
				return this.info.Min;
			}
		}

		public Vector3 Max
		{
			get
			{
				return this.info.Max;
			}
		}

		public RaycastNode Parent
		{
			get
			{
				return this.hierarchy.parent;
			}
		}

		public List<RaycastNode> Neighbors
		{
			get
			{
				return this.hierarchy.neighbors;
			}
		}

		public void SetParent(RaycastNode node)
		{
			this.hierarchy.parent = node;
		}

		public void AddNeighbor(RaycastNode node)
		{
			this.hierarchy.neighbors.Add(node);
		}

		public RaycastNode GetNeighbor(Vector3 pos)
		{
			return this.hierarchy.GetNeighbor(pos);
		}

		public void AddChild(RaycastNode node)
		{
			this.hierarchy.AddChild(node);
		}

		public List<RaycastNode> Children
		{
			get
			{
				return this.hierarchy.children;
			}
		}

		public List<RaycastNode> ChildAirBlocks
		{
			get
			{
				return this.hierarchy.childAirBlocks;
			}
		}

		public List<RaycastNode> ChildSolidBlocks
		{
			get
			{
				return this.hierarchy.childSolidBlocks;
			}
		}

		public RaycastNode Waypoint
		{
			get
			{
				return this.hierarchy.waypoint;
			}
		}

		public void SetWaypoint(RaycastNode node)
		{
			this.hierarchy.SetWayPoint(node);
		}

		public bool FlowToWaypoint
		{
			get
			{
				return this.hierarchy.flowToWaypoint;
			}
		}

		public void SetType(cPathNodeType _nodeType)
		{
			this.nodeType = _nodeType;
		}

		public virtual void DebugDraw()
		{
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public RaycastNodeInfo info;

		[PublicizedFrom(EAccessModifier.Private)]
		public RaycastNodeHierarcy hierarchy;

		public cPathNodeType nodeType;
	}
}
