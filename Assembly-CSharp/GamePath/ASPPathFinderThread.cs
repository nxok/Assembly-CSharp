using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePath
{
	public class ASPPathFinderThread : PathFinderThread
	{
		public ASPPathFinderThread()
		{
			PathFinderThread.Instance = this;
		}

		public override int GetFinishedCount()
		{
			return this.finishedPaths.Count;
		}

		public override int GetQueueCount()
		{
			return this.entityWaitQueue.list.Count;
		}

		public override void StartWorkerThreads()
		{
			this.coroutine = GameManager.Instance.StartCoroutine(this.FindPaths());
		}

		public override void Cleanup()
		{
			GameManager.Instance.StopCoroutine(this.coroutine);
			this.entityWaitQueue.Clear();
			this.finishedPaths.Clear();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator FindPaths()
		{
			for (;;)
			{
				int num = 0;
				while (num < 8 && this.entityWaitQueue.list.Count != 0)
				{
					int num2 = this.entityWaitQueue.list[0];
					this.entityWaitQueue.Remove(num2);
					PathInfo pathInfo;
					if (!this.finishedPaths.TryGetValue(num2, out pathInfo))
					{
						Log.Warning("{0} path dup id {1}", new object[]
						{
							GameManager.frameCount,
							num2
						});
					}
					else
					{
						pathInfo.entity.navigator.GetPathTo(pathInfo);
						if (pathInfo.state == PathInfo.State.Queued)
						{
							this.finishedPaths.Remove(num2);
						}
					}
					num++;
				}
				yield return null;
			}
			yield break;
		}

		public override void FindPath(EntityAlive _entity, Vector3 _targetPos, float _speed, bool _canBreak, EAIBase _aiTask)
		{
			this.entityWaitQueue.Add(_entity.entityId);
			this.finishedPaths[_entity.entityId] = new PathInfoSingleTarget(_entity, _targetPos, _canBreak, _speed, _aiTask);
		}

		public override void FindPath(EntityAlive _entity, Vector3 _startPos, Vector3 _targetPos, float _speed, bool _canBreak, EAIBase _aiTask)
		{
			this.entityWaitQueue.Add(_entity.entityId);
			PathInfoSingleTarget pathInfoSingleTarget = new PathInfoSingleTarget(_entity, _targetPos, _canBreak, _speed, _aiTask);
			pathInfoSingleTarget.SetStartPos(_startPos);
			this.finishedPaths[_entity.entityId] = pathInfoSingleTarget;
		}

		public override void FindPath(PathInfo _pathInfo)
		{
			this.entityWaitQueue.Add(_pathInfo.entity.entityId);
			this.finishedPaths[_pathInfo.entity.entityId] = _pathInfo;
		}

		public override PathInfo GetPath(int _entityId)
		{
			PathInfo pathInfo;
			if (this.finishedPaths.TryGetValue(_entityId, out pathInfo) && pathInfo.state == PathInfo.State.Done)
			{
				this.finishedPaths.Remove(_entityId);
				return pathInfo;
			}
			return null;
		}

		public override bool IsCalculatingPath(int _entityId)
		{
			return this.finishedPaths.ContainsKey(_entityId);
		}

		public override void RemovePathsFor(int _entityId)
		{
			this.finishedPaths.Remove(_entityId);
			this.entityWaitQueue.Remove(_entityId);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Coroutine coroutine;

		[PublicizedFrom(EAccessModifier.Private)]
		public HashSetList<int> entityWaitQueue = new HashSetList<int>();

		[PublicizedFrom(EAccessModifier.Private)]
		public Dictionary<int, PathInfo> finishedPaths = new Dictionary<int, PathInfo>();
	}
}
