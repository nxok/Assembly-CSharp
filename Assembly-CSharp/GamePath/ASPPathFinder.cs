using System;
using Pathfinding;
using UnityEngine;

namespace GamePath
{
	public class ASPPathFinder : PathFinder
	{
		public ASPPathFinder(PathInfo _pathInfo, bool _bDrn, bool _canClimbLadders, bool _bCanClimbWalls)
			: base(_pathInfo, _bDrn, _canClimbLadders, _bCanClimbWalls)
		{
			this.entity = _pathInfo.entity;
			this.smoothPercent = 0.82f + this.entity.rand.RandomFloat * 0.07f;
		}

		public override void Calculate(Vector3 _fromPos)
		{
			if (AstarPath.active == null)
			{
				return;
			}
			Vector3 vector = _fromPos - Origin.position;
			PathInfoSingleTarget pathInfoSingleTarget = this.pathInfo as PathInfoSingleTarget;
			if (pathInfoSingleTarget != null)
			{
				Vector3 targetPos = pathInfoSingleTarget.targetPos;
				Vector3 vector2 = pathInfoSingleTarget.targetPos - Origin.position;
				if (pathInfoSingleTarget.endingCondition != null)
				{
					XPath.Construct(vector, vector2, null).endingCondition = pathInfoSingleTarget.endingCondition;
				}
				else
				{
					this.path = ABPath.Construct(vector, vector2, new OnPathDelegate(this.OnPathFinished));
				}
			}
			else
			{
				PathInfoMultiTarget pathInfoMultiTarget = this.pathInfo as PathInfoMultiTarget;
				if (pathInfoMultiTarget != null)
				{
					Vector3[] array = pathInfoMultiTarget.targetPositions.ToArray();
					for (int i = 0; i < array.Length; i++)
					{
						array[i] -= Origin.position;
					}
					MultiTargetPath multiTargetPath = MultiTargetPath.Construct(vector, array, pathInfoMultiTarget.OnTargetPathFinished, new OnPathDelegate(this.OnPathFinished));
					multiTargetPath.pathsForAll = pathInfoMultiTarget.pathsForAll;
					this.path = multiTargetPath;
				}
				else
				{
					PathInfoMultiSource pathInfoMultiSource = this.pathInfo as PathInfoMultiSource;
					if (pathInfoMultiSource != null)
					{
						Vector3[] array2 = pathInfoMultiSource.sourcePositions.ToArray();
						for (int j = 0; j < array2.Length; j++)
						{
							array2[j] -= Origin.position;
						}
						MultiTargetPath multiTargetPath2 = MultiTargetPath.Construct(array2, vector, pathInfoMultiSource.OnTargetPathFinished, new OnPathDelegate(this.OnPathFinished));
						multiTargetPath2.pathsForAll = true;
						this.path = multiTargetPath2;
					}
					else
					{
						PathInfoFleeRandom pathInfoFleeRandom = this.pathInfo as PathInfoFleeRandom;
						if (pathInfoFleeRandom != null)
						{
							RandomPath randomPath = RandomPath.Construct(vector, pathInfoFleeRandom.searchLength, null);
							randomPath.aim = pathInfoFleeRandom.aimBias;
							randomPath.aimStrength = pathInfoFleeRandom.aimStrength;
							this.path = randomPath;
						}
						else
						{
							PathInfoFleeTarget pathInfoFleeTarget = this.pathInfo as PathInfoFleeTarget;
							if (pathInfoFleeTarget != null)
							{
								Vector3 vector3 = pathInfoFleeTarget.fleeTarget - Origin.position;
								FleePath fleePath = FleePath.Construct(vector, vector3, pathInfoFleeTarget.searchLength, null);
								this.path = fleePath;
							}
						}
					}
				}
			}
			this.path.calculatePartial = this.pathInfo.calculatePartial;
			this.path.enabledTags = ((!this.pathInfo.canBreakBlocks) ? 265 : 267);
			if (this.pathInfo.entity.bCanClimbLadders)
			{
				this.path.enabledTags |= 16;
			}
			if (this.pathInfo.entity.height <= 1f)
			{
				this.path.enabledTags |= 4;
			}
			if (this.entity is EntityDrone && !this.pathInfo.canBreakBlocks)
			{
				this.path.enabledTags |= 8;
			}
			this.path.CanBreakBlocks = this.pathInfo.canBreakBlocks;
			if (this.entity.aiManager != null)
			{
				float pathCostScale = this.entity.aiManager.pathCostScale;
				if (pathCostScale <= 99f)
				{
					float num = this.entity.aiManager.partialPathHeightScale;
					this.path.traversalProvider = new TraversalProvider();
					this.path.CostScale = pathCostScale;
					this.path.PartialPathHeightScale = num * 0.3f;
					if (pathCostScale >= 0.28f)
					{
						num -= this.entity.rand.RandomFloat * 0.02f * pathCostScale;
						if (num < 0f)
						{
							num = 0f;
						}
						this.entity.aiManager.partialPathHeightScale = num;
					}
				}
				else
				{
					this.path.traversalProvider = new TraversalProviderNoBreak();
					this.path.CostScale = 1f;
					this.path.PartialPathHeightScale = 0f;
				}
			}
			else
			{
				this.path.traversalProvider = new TraversalProviderNoBreak();
				this.path.CostScale = 1f;
				this.path.PartialPathHeightScale = 0f;
			}
			AstarPath.StartPath(this.path, false);
			this.pathInfo.state = PathInfo.State.Pathing;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnPathFinished(Path p)
		{
			this.pathInfo.state = PathInfo.State.Done;
			EntityAlive entityAlive = this.pathInfo.entity;
			if (this.pathInfo.OnPathResult != null)
			{
				this.pathInfo.OnPathResult.Invoke(p);
			}
			if (!entityAlive || entityAlive.navigator == null)
			{
				return;
			}
			PathInfoMultiTarget pathInfoMultiTarget = this.pathInfo as PathInfoMultiTarget;
			if (pathInfoMultiTarget != null)
			{
				if (pathInfoMultiTarget.pathsForAll)
				{
					MultiTargetPath multiTargetPath = this.path as MultiTargetPath;
					if (multiTargetPath != null)
					{
						pathInfoMultiTarget.vectorPaths = multiTargetPath.vectorPaths;
					}
					if (pathInfoMultiTarget.pathSelection == MultiTargetPathSelection.None)
					{
						return;
					}
					if (pathInfoMultiTarget.pathSelection == MultiTargetPathSelection.Random)
					{
						if (pathInfoMultiTarget.vectorPaths.Length != 0)
						{
							this.path.vectorPath = pathInfoMultiTarget.vectorPaths[entityAlive.rand.RandomInt % pathInfoMultiTarget.vectorPaths.Length];
						}
					}
					else if (pathInfoMultiTarget.pathSelection == MultiTargetPathSelection.Closest)
					{
					}
				}
			}
			else
			{
				PathInfoMultiSource pathInfoMultiSource = this.pathInfo as PathInfoMultiSource;
				if (pathInfoMultiSource != null)
				{
					MultiTargetPath multiTargetPath2 = this.path as MultiTargetPath;
					if (multiTargetPath2 != null)
					{
						pathInfoMultiSource.vectorPaths = multiTargetPath2.vectorPaths;
					}
					return;
				}
			}
			if (this.pathInfo != entityAlive.navigator.pathInfo)
			{
				return;
			}
			this.path = p as ABPath;
			int num = this.path.vectorPath.Count;
			if (num == 0)
			{
				return;
			}
			Vector3 vector = this.path.originalEndPoint + Origin.position;
			Vector3 originalEndPoint = this.path.originalEndPoint;
			this.pathInfo.path = new PathEntity();
			this.pathInfo.path.toPos = vector;
			for (int i = 0; i < num - 2; i++)
			{
				Vector3 vector2 = this.path.vectorPath[i];
				Vector3 vector3 = this.path.vectorPath[i + 2];
				float num2 = vector3.x - vector2.x;
				float num3 = vector3.z - vector2.z;
				if ((num2 < -0.1f || num2 > 0.1f) && (num3 < -0.1f || num3 > 0.1f))
				{
					Vector3 vector4 = this.path.vectorPath[i + 1];
					if (Mathf.Abs(vector2.y - vector4.y) <= 0.5f && this.IsLineClear(vector2, vector3, true))
					{
						this.path.vectorPath[i + 1] = (vector2 + vector3) * 0.475f + vector4 * 0.05f;
						i++;
					}
				}
			}
			Vector3 vector5 = entityAlive.position - Origin.position;
			this.pathInfo.path.rawEndPos = this.path.vectorPath[num - 1] + Origin.position;
			if (num >= 2)
			{
				this.path.vectorPath[0] = vector5 * 0.45f + this.path.vectorPath[0] * 0.55f;
			}
			if (this.path.CompleteState == 2)
			{
				if (!this.pathInfo.canBreakBlocks)
				{
					originalEndPoint.y = this.path.vectorPath[num - 1].y;
				}
				this.path.vectorPath[num - 1] = originalEndPoint;
			}
			else if (this.path.CompleteState == 3 && num == 1)
			{
				this.path.vectorPath[0] = vector5 * 0.3f + this.path.vectorPath[0] * 0.7f;
				Vector3 vector6 = Vector3.MoveTowards(this.path.vectorPath[0], originalEndPoint, 5f);
				this.path.vectorPath.Add(vector6);
				num++;
			}
			if (num >= 3)
			{
				float num4 = this.smoothPercent;
				float num5 = (1f - num4) * 0.5f;
				for (int j = 2; j > 0; j--)
				{
					for (int k = 0; k < num - 2; k++)
					{
						Vector3 vector7 = this.path.vectorPath[k];
						Vector3 vector8 = this.path.vectorPath[k + 1];
						if (Mathf.Abs(vector7.y - vector8.y) <= 0.5f)
						{
							Vector3 vector9 = this.path.vectorPath[k + 2];
							vector8.x = vector8.x * num4 + (vector7.x + vector9.x) * num5;
							vector8.z = vector8.z * num4 + (vector7.z + vector9.z) * num5;
							this.path.vectorPath[k + 1] = vector8;
						}
					}
				}
			}
			PathPoint[] array = new PathPoint[num];
			for (int l = 0; l < num; l++)
			{
				PathPoint pathPoint = PathPoint.Allocate(this.path.vectorPath[l] + Origin.position);
				array[l] = pathPoint;
			}
			this.pathInfo.path.SetPoints(array);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool IsLineClear(Vector3 pos1, Vector3 pos2, bool isTall)
		{
			if (Mathf.Abs(pos1.y - pos2.y) > 0.5f)
			{
				return false;
			}
			pos1.y += 0.5f;
			pos2.y += 0.5f;
			if (Physics.Linecast(pos1, pos2, 1082195968))
			{
				return false;
			}
			Vector3 vector = pos2 - pos1;
			Ray ray;
			ray..ctor(pos1, vector);
			if (Physics.SphereCast(ray, 0.25f, vector.magnitude, 1082195968))
			{
				return false;
			}
			if (isTall)
			{
				pos1.y += 1f;
				pos2.y += 1f;
				if (Physics.Linecast(pos1, pos2, 1082195968))
				{
					return false;
				}
				ray.origin = pos1;
				if (Physics.SphereCast(ray, 0.25f, vector.magnitude, 1082195968))
				{
					return false;
				}
			}
			return true;
		}

		public void Cancel()
		{
			if (this.path != null)
			{
				this.path.Error();
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const int cCollisionMask = 1082195968;

		[PublicizedFrom(EAccessModifier.Private)]
		public EntityAlive entity;

		[PublicizedFrom(EAccessModifier.Private)]
		public float smoothPercent;

		[PublicizedFrom(EAccessModifier.Private)]
		public ABPath path;
	}
}
