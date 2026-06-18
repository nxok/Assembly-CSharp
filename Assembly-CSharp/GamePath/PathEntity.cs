using System;
using UnityEngine;

namespace GamePath
{
	public class PathEntity
	{
		public void Destruct()
		{
			PathPoint[] array = this.points;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Release();
			}
			this.points = null;
		}

		public void SetPoints(PathPoint[] _points)
		{
			this.points = _points;
			this.pathLength = _points.Length;
		}

		public bool HasPoints()
		{
			return this.points != null;
		}

		public bool isFinished()
		{
			return this.currentPathIndex >= this.pathLength;
		}

		public int NodeCountRemaining()
		{
			return this.pathLength - this.currentPathIndex;
		}

		public Vector3 GetEndPos()
		{
			if (this.pathLength > 0)
			{
				return this.points[this.pathLength - 1].projectedLocation;
			}
			return this.rawEndPos;
		}

		public PathPoint GetEndPoint()
		{
			if (this.pathLength > 0)
			{
				return this.points[this.pathLength - 1];
			}
			return null;
		}

		public void ShortenEnd(float _distance)
		{
			if (this.pathLength >= 2)
			{
				PathPoint pathPoint = this.points[this.pathLength - 2];
				this.points[this.pathLength - 1].projectedLocation = pathPoint.projectedLocation;
			}
		}

		public PathPoint getPathPointFromIndex(int _idx)
		{
			return this.points[_idx];
		}

		public int getCurrentPathLength()
		{
			return this.pathLength;
		}

		public void setCurrentPathLength(int _length)
		{
			this.pathLength = _length;
		}

		public int getCurrentPathIndex()
		{
			return this.currentPathIndex;
		}

		public void setCurrentPathIndex(int _idx, Entity entity, Vector3 entityPos)
		{
			this.currentPathIndex = _idx;
		}

		public PathPoint CurrentPoint
		{
			get
			{
				return this.points[this.currentPathIndex];
			}
		}

		public PathPoint NextPoint
		{
			get
			{
				int num = this.currentPathIndex + 1;
				if (num >= this.points.Length)
				{
					return null;
				}
				return this.points[num];
			}
		}

		public override bool Equals(object _other)
		{
			if (!(_other is PathEntity) || _other == null)
			{
				return false;
			}
			PathEntity pathEntity = (PathEntity)_other;
			if (pathEntity.points.Length != this.points.Length)
			{
				return false;
			}
			for (int i = 0; i < this.points.Length; i++)
			{
				if (!this.points[i].IsSamePos(pathEntity.points[i]))
				{
					return false;
				}
			}
			return true;
		}

		public override int GetHashCode()
		{
			if (this.points == null)
			{
				return 0;
			}
			int num = 0;
			foreach (PathPoint pathPoint in this.points)
			{
				num += pathPoint.GetHashCode();
			}
			return num;
		}

		public PathPoint[] points;

		public Vector3 toPos;

		public Vector3 rawEndPos;

		[PublicizedFrom(EAccessModifier.Private)]
		public int currentPathIndex;

		[PublicizedFrom(EAccessModifier.Private)]
		public int pathLength;
	}
}
