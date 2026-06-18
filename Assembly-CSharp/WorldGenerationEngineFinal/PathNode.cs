using System;
using Unity.Burst;

namespace WorldGenerationEngineFinal
{
	[BurstCompile(CompileSynchronously = true)]
	public struct PathNode
	{
		public PathNode(Vector2i _position, float _travelledCost, float _totalCost, int _next)
		{
			this.position = _position;
			this.travelledCost = _travelledCost;
			this.totalCost = _totalCost;
			this.pathNext = _next;
			this.listNext = -1;
		}

		public void Set(Vector2i _position, float _travelledCost, float _totalCost, int _next)
		{
			this.position = _position;
			this.travelledCost = _travelledCost;
			this.totalCost = _totalCost;
			this.pathNext = _next;
		}

		public Vector2i position;

		public float travelledCost;

		public float totalCost;

		public int pathNext;

		public int listNext;
	}
}
