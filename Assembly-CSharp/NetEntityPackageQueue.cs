using System;
using System.Collections.Generic;

public class NetEntityPackageQueue
{
	public NetEntityPackageQueue(World world)
	{
		this.world = world;
	}

	public bool HasPackagesForEntity(int entityId)
	{
		return this.entityPackageQueues.ContainsKey(entityId);
	}

	public void EnqueueNetPackageForEntity(int entityId, NetPackage netPackage)
	{
		Queue<NetPackage> queue;
		if (!this.entityPackageQueues.TryGetValue(entityId, out queue))
		{
			queue = new Queue<NetPackage>(10);
			this.entityPackageQueues.Add(entityId, queue);
		}
		queue.Enqueue(netPackage);
	}

	public void ProcessPackagesForEntity(int entityId)
	{
		Queue<NetPackage> queue;
		if (!this.entityPackageQueues.Remove(entityId, out queue))
		{
			return;
		}
		NetPackage netPackage;
		while (queue.TryDequeue(out netPackage))
		{
			netPackage.ProcessPackage(this.world, GameManager.Instance);
			NetPackageManager.FreePackage(netPackage);
		}
	}

	public void Cleanup()
	{
		foreach (Queue<NetPackage> queue in this.entityPackageQueues.Values)
		{
			NetPackage netPackage;
			while (queue.TryDequeue(out netPackage))
			{
				NetPackageManager.FreePackage(netPackage);
			}
		}
		this.entityPackageQueues.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly World world;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<int, Queue<NetPackage>> entityPackageQueues = new Dictionary<int, Queue<NetPackage>>(64);
}
