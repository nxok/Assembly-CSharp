using System;
using System.Collections.Generic;
using System.Diagnostics;

public class EntityAsyncManager
{
	public EntityAsyncManager(NetEntityPackageQueue netEntityPackageQueue)
	{
		this.netEntityPackageQueue = netEntityPackageQueue;
	}

	public EntityAsyncManager.EntityCreateHandle StartCreateEntity(EntityCreationData _ecd, Action<EntityAsyncManager.EntityCreateHandle> _onComplete = null)
	{
		EntityFactory.CreateEntityOperation createEntityOperation = EntityFactory.CreateEntityAsync(_ecd);
		EntityAsyncManager.EntityCreateHandle entityCreateHandle = new EntityAsyncManager.EntityCreateHandle(this, createEntityOperation, _onComplete);
		if (!this.requestIdMap.TryAdd(entityCreateHandle.Id, entityCreateHandle))
		{
			throw new Exception(string.Format("Request already exists for entity id {0}. EntityCreationData: {1}", entityCreateHandle.Id, _ecd));
		}
		this.requestQueue.Enqueue(entityCreateHandle);
		return entityCreateHandle;
	}

	public void Update()
	{
		EntityAsyncManager.EntityCreateHandle entityCreateHandle;
		while (this.requestQueue.TryPeek(out entityCreateHandle))
		{
			if (entityCreateHandle.IsCompleted)
			{
				this.requestQueue.Dequeue();
			}
			else
			{
				if (!entityCreateHandle.TryComplete())
				{
					break;
				}
				this.requestQueue.Dequeue();
			}
		}
	}

	public bool IsEntityPending(int _entityId)
	{
		EntityAsyncManager.EntityCreateHandle entityCreateHandle;
		return this.requestIdMap.TryGetValue(_entityId, out entityCreateHandle) && !entityCreateHandle.IsCompleted;
	}

	public void EnsureEntity(int _entityId)
	{
		EntityAsyncManager.EntityCreateHandle entityCreateHandle;
		if (!this.requestIdMap.TryGetValue(_entityId, out entityCreateHandle))
		{
			return;
		}
		EntityAsyncManager.LogWarning("force spawning pending entity " + entityCreateHandle.DebugInfo);
		entityCreateHandle.WaitForComplete();
		this.requestIdMap.Remove(entityCreateHandle.Id);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnCreateEntityRequestFinalized(int id)
	{
		this.requestIdMap.Remove(id);
		this.netEntityPackageQueue.ProcessPackagesForEntity(id);
	}

	public void CompletePendingCreateTasks()
	{
		if (this.requestQueue.Count == 0)
		{
			return;
		}
		EntityAsyncManager.LogInfo(string.Format("completing {0} pending create tasks", this.requestQueue.Count));
		EntityAsyncManager.EntityCreateHandle entityCreateHandle;
		while (this.requestQueue.TryDequeue(out entityCreateHandle))
		{
			entityCreateHandle.WaitForComplete();
		}
		this.requestIdMap.Clear();
	}

	public void Cleanup()
	{
		this.CompletePendingCreateTasks();
	}

	[Conditional("DEBUG_ENTITY_ASYNC")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void LogDebug(string message)
	{
		Log.Out("[EntityAsync] " + message);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LogInfo(string message)
	{
		Log.Out("[EntityAsync] " + message);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LogWarning(string message)
	{
		Log.Warning("[EntityAsync] " + message);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Queue<EntityAsyncManager.EntityCreateHandle> requestQueue = new Queue<EntityAsyncManager.EntityCreateHandle>(64);

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<int, EntityAsyncManager.EntityCreateHandle> requestIdMap = new Dictionary<int, EntityAsyncManager.EntityCreateHandle>(64);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly NetEntityPackageQueue netEntityPackageQueue;

	public class EntityCreateHandle
	{
		public bool IsCompleted
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public int Id
		{
			get
			{
				return this.op.EntityId;
			}
		}

		public string DebugInfo
		{
			get
			{
				return this.op.DebugEntityInfo;
			}
		}

		public Entity Entity
		{
			get
			{
				return this.op.entity;
			}
		}

		public EntityCreateHandle(EntityAsyncManager manager, EntityFactory.CreateEntityOperation op, Action<EntityAsyncManager.EntityCreateHandle> onComplete)
		{
			this.manager = manager;
			this.op = op;
			this.onComplete = onComplete;
		}

		public bool TryComplete()
		{
			if (this.IsCompleted)
			{
				return true;
			}
			if (!this.op.IsLoadingComplete)
			{
				return false;
			}
			try
			{
				this.IsCompleted = true;
				this.op.CompleteEntity();
				Action<EntityAsyncManager.EntityCreateHandle> action = this.onComplete;
				if (action != null)
				{
					action(this);
				}
				this.onComplete = null;
			}
			finally
			{
				this.manager.OnCreateEntityRequestFinalized(this.Id);
			}
			return true;
		}

		public Entity WaitForComplete()
		{
			this.op.WaitForLoadingComplete();
			if (!this.TryComplete())
			{
				return null;
			}
			return this.op.entity;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly EntityAsyncManager manager;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly EntityFactory.CreateEntityOperation op;

		[PublicizedFrom(EAccessModifier.Private)]
		public Action<EntityAsyncManager.EntityCreateHandle> onComplete;
	}
}
