using System;
using System.Collections.Generic;

public class BlockTracker
{
	public BlockTracker(int _limit)
	{
		this.limit = _limit;
		this.blockLocations = new List<Vector3i>();
	}

	public bool TryAddBlock(Vector3i _position)
	{
		if (this.blockLocations.Contains(_position))
		{
			return true;
		}
		if (this.blockLocations.Count >= this.limit)
		{
			return false;
		}
		this.blockLocations.Add(_position);
		return true;
	}

	public bool RemoveBlock(Vector3i _position)
	{
		if (this.blockLocations.Contains(_position))
		{
			this.blockLocations.Remove(_position);
			return true;
		}
		return false;
	}

	public bool CanAdd(Vector3i _position)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return this.blockLocations.Count < this.limit || this.blockLocations.Contains(_position);
		}
		return this.clientAmount < this.limit;
	}

	public void Clear()
	{
		this.blockLocations.Clear();
	}

	public void Read(PooledBinaryReader _reader)
	{
		this.blockLocations = new List<Vector3i>();
		int num = _reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			this.blockLocations.Add(new Vector3i(_reader.ReadInt32(), _reader.ReadInt32(), _reader.ReadInt32()));
		}
	}

	public void Write(PooledBinaryWriter _writer)
	{
		_writer.Write(this.blockLocations.Count);
		foreach (Vector3i vector3i in this.blockLocations)
		{
			_writer.Write(vector3i.x);
			_writer.Write(vector3i.y);
			_writer.Write(vector3i.z);
		}
	}

	public int limit;

	public List<Vector3i> blockLocations;

	public int clientAmount;
}
