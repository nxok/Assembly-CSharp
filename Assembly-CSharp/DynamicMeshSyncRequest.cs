using System;

public class DynamicMeshSyncRequest
{
	public int SecondsAlive
	{
		get
		{
			return (int)(DateTime.Now - this.Created).TotalSeconds;
		}
	}

	public int SecondsAttempted
	{
		get
		{
			if (this.Initiated != null)
			{
				return (int)(DateTime.Now - this.Initiated.Value).TotalSeconds;
			}
			return 0;
		}
	}

	public static DynamicMeshSyncRequest Create(DynamicMeshItem item, bool isDelete)
	{
		return new DynamicMeshSyncRequest
		{
			Item = item,
			IsDelete = isDelete
		};
	}

	public static DynamicMeshSyncRequest Create(DynamicMeshItem item, bool isDelete, int clientId)
	{
		return new DynamicMeshSyncRequest
		{
			Item = item,
			IsDelete = isDelete,
			ClientId = clientId
		};
	}

	public bool TryGetData()
	{
		if (DynamicMeshThread.ChunkDataQueue.CollectBytes(this.Item.Key, out this.Data, out this.Length))
		{
			this.HasData = true;
			return true;
		}
		return false;
	}

	public DynamicMeshItem Item;

	public bool IsDelete;

	public bool SyncComplete;

	public byte[] Data;

	public bool HasData;

	public int Length;

	public int ClientId = -1;

	public DateTime? Initiated;

	public DateTime Created;
}
