using System;
using UnityEngine;

namespace GamePath
{
	public class PathPoint
	{
		public static PathPoint Allocate(Vector3 _pos)
		{
			DynamicObjectPool<PathPoint> s_pool = MemoryPools.s_pool;
			PathPoint pathPoint2;
			lock (s_pool)
			{
				PathPoint pathPoint = MemoryPools.s_pool.Allocate();
				pathPoint.x = (int)_pos.x;
				pathPoint.y = (int)_pos.y;
				pathPoint.z = (int)_pos.z;
				pathPoint.projectedLocation = _pos;
				pathPoint.hash = PathPoint.makeHash(pathPoint.x, pathPoint.y, pathPoint.z);
				pathPoint2 = pathPoint;
			}
			return pathPoint2;
		}

		public static void CompactPool()
		{
			DynamicObjectPool<PathPoint> s_pool = MemoryPools.s_pool;
			lock (s_pool)
			{
				MemoryPools.s_pool.Compact();
			}
		}

		public void Release()
		{
			DynamicObjectPool<PathPoint> s_pool = MemoryPools.s_pool;
			lock (s_pool)
			{
				MemoryPools.s_pool.Free(this);
			}
		}

		public static int makeHash(int _x, int _y, int _z)
		{
			return (_y & 255) | ((_x & 32767) << 8) | ((_z & 32767) << 24) | ((_x >= 0) ? 0 : int.MinValue) | ((_z >= 0) ? 0 : 32768);
		}

		public override bool Equals(object _obj)
		{
			PathPoint pathPoint = _obj as PathPoint;
			return pathPoint != null && this.hash == pathPoint.hash && this.IsSamePos(pathPoint);
		}

		public bool IsSamePos(PathPoint _p)
		{
			return _p.x == this.x && _p.y == this.y && _p.z == this.z;
		}

		public override int GetHashCode()
		{
			return this.hash;
		}

		public float GetDistanceSq(int _x, int _y, int _z)
		{
			int num = this.x - _x;
			int num2 = this.y - _y;
			int num3 = this.z - _z;
			return (float)(num * num + num2 * num2 + num3 * num3);
		}

		public Vector3 AdjustedPositionForEntity(Entity entity)
		{
			return this.projectedLocation;
		}

		public Vector3 ProjectToGround(Entity entity)
		{
			return this.projectedLocation;
		}

		public Vector3i GetBlockPos()
		{
			return World.worldToBlockPos(this.projectedLocation);
		}

		public string toString()
		{
			return string.Concat(new string[]
			{
				this.x.ToString(),
				", ",
				this.y.ToString(),
				", ",
				this.z.ToString()
			});
		}

		public Vector3 projectedLocation;

		[PublicizedFrom(EAccessModifier.Private)]
		public int x;

		[PublicizedFrom(EAccessModifier.Private)]
		public int y;

		[PublicizedFrom(EAccessModifier.Private)]
		public int z;

		[PublicizedFrom(EAccessModifier.Private)]
		public int hash;
	}
}
