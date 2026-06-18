using System;
using Pathfinding;
using UnityEngine;

namespace GamePath
{
	public abstract class PathInfo
	{
		public PathInfo(EntityAlive _entity, bool _canBreakBlocks, float _speed, EAIBase _aiTask)
		{
			this.entity = _entity;
			this.hasStart = false;
			this.canBreakBlocks = _canBreakBlocks;
			this.speed = _speed;
			this.aiTask = _aiTask;
			this.calculatePartial = false;
			this.path = null;
		}

		public void SetStartPos(Vector3 _startPos)
		{
			this.startPos = _startPos;
			this.hasStart = true;
		}

		public virtual void ResetPathInfo()
		{
			this.path = null;
		}

		public EntityAlive entity;

		public PathInfo.State state;

		public Vector3 startPos;

		public bool hasStart;

		public bool canBreakBlocks;

		public float speed;

		public EAIBase aiTask;

		public bool calculatePartial;

		public ChunkCache chunkcache;

		public OnPathDelegate OnPathResult;

		public PathEntity path;

		public enum State
		{
			Queued,
			Pathing,
			Done
		}
	}
}
