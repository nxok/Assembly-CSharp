using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DynamicMeshClientConnection
{
	public bool TriggerSend
	{
		get
		{
			return (DateTime.Now - this.LastSend).TotalSeconds > 1.0;
		}
	}

	public bool HasMessage
	{
		get
		{
			return this.ItemsToSend.Count > 0;
		}
	}

	public DynamicMeshClientConnection(int entityId)
	{
		this.EntityId = entityId;
	}

	public void AddToQueue(DynamicMeshSyncRequest package)
	{
		ValueTuple<int, int> valueTuple = new ValueTuple<int, int>(DynamicMeshUnity.RoundRegion(package.Item.WorldPosition.x), DynamicMeshUnity.RoundRegion(package.Item.WorldPosition.z));
		ConcurrentQueue<DynamicMeshSyncRequest> orAdd = this.ItemsToSend.GetOrAdd(valueTuple, new ConcurrentQueue<DynamicMeshSyncRequest>());
		orAdd.Enqueue(package);
		this.SendMessage = DynamicMeshServer.AutoSend || this.SendMessage || orAdd.Count == 1;
	}

	public bool RequestChunk()
	{
		if (this.CurrentRequestedChunk != 9223372036854775807L)
		{
			return false;
		}
		if (!this.RequestedChunks.TryDequeue(out this.CurrentRequestedChunk))
		{
			return false;
		}
		this.RequestTime = DateTime.Now;
		DynamicMeshThread.RequestChunk(this.CurrentRequestedChunk);
		return true;
	}

	public void UpdateItemsToSend(NetPackageDynamicClientArrive package)
	{
		DynamicMeshClientConnection.UpdateItemsToSend(this, package);
	}

	public static void UpdateItemsToSend(DynamicMeshClientConnection data, NetPackageDynamicClientArrive package)
	{
		if (DynamicMeshManager.Instance == null || DynamicMeshManager.Instance.ItemsDictionary == null)
		{
			DynamicMeshManager.LogMsg(package.Sender.playerName + " connected before the world was ready. Can not sync dymesh data. They must reconnect to start the sync");
			data.SendMessage = true;
			return;
		}
		data.SendMessage = false;
		data.ItemsToSend.Clear();
		DynamicMeshManager.LogMsg(string.Concat(new string[]
		{
			"Update items to send for ",
			package.Sender.playerName,
			" id: ",
			package.Sender.entityId.ToString(),
			"  recieved: ",
			package.Items.Count.ToString()
		}));
		EntityPlayer entityPlayer = GameManager.Instance.World.GetPlayers().FirstOrDefault((EntityPlayer d) => d.entityId == package.Sender.entityId);
		Vector3 playerPos = ((entityPlayer == null) ? Vector3.zero : entityPlayer.GetPosition());
		playerPos.y = 0f;
		List<DynamicMeshRegion> list = DynamicMeshRegion.Regions.Values.OrderBy((DynamicMeshRegion d) => Math.Abs(Vector3.Distance(playerPos, d.WorldPosition.ToVector3()))).ToList<DynamicMeshRegion>();
		int num = 0;
		List<DynamicMeshItem> list2 = new List<DynamicMeshItem>(DynamicMeshManager.Instance.ItemsDictionary.Count);
		NetPackageRegionMetaData package2 = NetPackageManager.GetPackage<NetPackageRegionMetaData>();
		foreach (DynamicMeshRegion dynamicMeshRegion in list)
		{
			num += DynamicMeshClientConnection.ProcessItem(data, dynamicMeshRegion, dynamicMeshRegion.LoadedItems, package, package2);
			num += DynamicMeshClientConnection.ProcessItem(data, dynamicMeshRegion, dynamicMeshRegion.UnloadedItems, package, package2);
		}
		package2.ChunksWithData.AddRange(DynamicMeshThread.PrimaryQueue.Values.Select((DynamicMeshItem d) => new Vector2i(d.WorldPosition.x, d.WorldPosition.z)));
		package2.ChunksWithData.AddRange(DynamicMeshThread.SecondaryQueue.Values.Select((DynamicMeshItem d) => new Vector2i(d.WorldPosition.x, d.WorldPosition.z)));
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package2, false, data.EntityId, -1, -1, null, 192, false);
		DynamicMeshManager.LogMsg(string.Concat(new string[]
		{
			"Items to send: ",
			list2.Count.ToString(),
			"   Added: ",
			num.ToString(),
			"   chunks: ",
			package2.ChunksWithData.Count.ToString()
		}));
		data.SendMessage = data.ItemsToSend.Count > 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int ProcessItem(DynamicMeshClientConnection conn, DynamicMeshRegion r, List<DynamicMeshItem> items, NetPackageDynamicClientArrive package, NetPackageRegionMetaData allChunkData)
	{
		int num = 0;
		using (List<DynamicMeshItem>.Enumerator enumerator = items.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				DynamicMeshItem i = enumerator.Current;
				if (i != null)
				{
					if (i.FileExists())
					{
						allChunkData.ChunksWithData.Add(new Vector2i(i.WorldPosition.x, i.WorldPosition.z));
					}
					if (!package.Items.Any((RegionItemData d) => d.X == i.WorldPosition.x && d.Z == i.WorldPosition.z && d.UpdateTime == i.UpdateTime))
					{
						DynamicMeshSyncRequest dynamicMeshSyncRequest = DynamicMeshSyncRequest.Create(i, false, conn.EntityId);
						conn.AddToQueue(dynamicMeshSyncRequest);
						num++;
					}
				}
			}
		}
		return num;
	}

	public int EntityId;

	public ConcurrentDictionary<ValueTuple<int, int>, ConcurrentQueue<DynamicMeshSyncRequest>> ItemsToSend = new ConcurrentDictionary<ValueTuple<int, int>, ConcurrentQueue<DynamicMeshSyncRequest>>();

	public ConcurrentQueue<long> RequestedChunks = new ConcurrentQueue<long>();

	public List<long> FinalChunks = new List<long>();

	public long CurrentRequestedChunk = long.MaxValue;

	public DateTime RequestTime = DateTime.Now.AddDays(-1.0);

	public DateTime LastSend = DateTime.Now.AddDays(-1.0);

	public bool SendMessage;

	public ValueTuple<int, int> LastKey = ValueTuple.Create<int, int>(0, 0);
}
