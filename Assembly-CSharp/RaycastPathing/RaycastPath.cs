using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace RaycastPathing
{
	[Preserve]
	public class RaycastPath
	{
		public RaycastPathInfo Info
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public Vector3 Start
		{
			get
			{
				return this.Info.Start;
			}
		}

		public Vector3 Target
		{
			get
			{
				return this.Info.Target;
			}
		}

		public Vector3i StartBlockPos
		{
			get
			{
				return this.Info.StartBlockPos;
			}
		}

		public Vector3i TargetBlockPos
		{
			get
			{
				return this.Info.TargetBlockPos;
			}
		}

		public bool PathStartsIndoors
		{
			get
			{
				return this.Info.PathStartsIndoors;
			}
		}

		public bool PathEndsIndoors
		{
			get
			{
				return this.Info.PathEndsIndoors;
			}
		}

		public RaycastPath(Vector3 start, Vector3 target)
		{
			this.Info = new RaycastPathInfo(start, target);
			RaycastPathManager.Instance.Add(this);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public ~RaycastPath()
		{
			this.Destruct();
		}

		public void Destruct()
		{
			RaycastPathManager.Instance.Remove(this);
		}

		public void AddNode(RaycastNode node)
		{
			if (!this.Nodes.Contains(node))
			{
				this.Nodes.Add(node);
			}
		}

		public void AddProjectedPoint(Vector3 point)
		{
			if (!this.ProjectedPoints.Contains(point))
			{
				this.ProjectedPoints.Add(point);
			}
		}

		public virtual void DebugDraw()
		{
			for (int i = 0; i < this.ProjectedPoints.Count - 1; i++)
			{
				Utils.DrawLine(World.blockToTransformPos(new Vector3i(this.ProjectedPoints[i] - Origin.position)), World.blockToTransformPos(new Vector3i(this.ProjectedPoints[i + 1] - Origin.position)), Color.white, Color.cyan, 2, 0f);
			}
			for (int j = 0; j < this.Nodes.Count; j++)
			{
				RaycastNode raycastNode = this.Nodes[j];
				for (int k = 0; k < raycastNode.Neighbors.Count; k++)
				{
					RaycastNode raycastNode2 = raycastNode.Neighbors[k];
					for (int l = 0; l < raycastNode2.ChildSolidBlocks.Count; l++)
					{
						RaycastPathUtils.DrawNode(raycastNode2.ChildSolidBlocks[l], Color.red, 0f);
					}
					for (int m = 0; m < raycastNode2.ChildAirBlocks.Count; m++)
					{
						RaycastPathUtils.DrawNode(raycastNode2.ChildAirBlocks[m], Color.cyan, 0f);
					}
				}
				if (raycastNode.Children.Count < 1)
				{
					cPathNodeType nodeType = raycastNode.nodeType;
					if (nodeType != cPathNodeType.Air)
					{
						if (nodeType == cPathNodeType.Door)
						{
							RaycastPathUtils.DrawNode(raycastNode, Color.green, 0f);
						}
					}
					else
					{
						RaycastPathUtils.DrawNode(raycastNode, Color.cyan, 0f);
					}
				}
				else
				{
					for (int n = 0; n < raycastNode.ChildSolidBlocks.Count; n++)
					{
						RaycastPathUtils.DrawNode(raycastNode.ChildSolidBlocks[n], Color.red, 0f);
					}
					for (int num = 0; num < raycastNode.ChildAirBlocks.Count; num++)
					{
						RaycastPathUtils.DrawNode(raycastNode.ChildAirBlocks[num], Color.cyan, 0f);
					}
				}
			}
			for (int num2 = 0; num2 < this.Nodes.Count - 1; num2++)
			{
				RaycastNode raycastNode3 = this.Nodes[num2];
				RaycastNode raycastNode4 = this.Nodes[num2 + 1];
				Utils.DrawLine(raycastNode3.Position - Origin.position, raycastNode4.Position - Origin.position, Color.white, Color.green, 2, 0f);
			}
		}

		public List<RaycastNode> Nodes = new List<RaycastNode>();

		public List<Vector3> ProjectedPoints = new List<Vector3>();
	}
}
