using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class TileAreaCache<[IsUnmanaged] T> : ITileArea<T[,]> where T : struct, ValueType
{
	public TileAreaCache(TileAreaConfig _config, TileFile<T> _tileFile, int _cacheMax)
	{
		this.tilesDatabase = _tileFile;
		this.config = _config;
		this.cacheMax = _cacheMax;
	}

	public TileAreaConfig Config
	{
		get
		{
			return this.config;
		}
	}

	public T[,] this[uint _key]
	{
		get
		{
			T[,] array;
			if (this.cache.TryGetValue(_key, out array))
			{
				this.PromoteEntry(_key);
				return array;
			}
			int tileXPos = TileAreaUtils.GetTileXPos(_key);
			int tileZPos = TileAreaUtils.GetTileZPos(_key);
			return this.Cache(_key, tileXPos, tileZPos);
		}
	}

	public T[,] this[int _tileX, int _tileZ]
	{
		get
		{
			this.config.checkCoordinates(ref _tileX, ref _tileZ);
			uint num = TileAreaUtils.MakeKey(_tileX, _tileZ);
			T[,] array;
			if (this.cache.TryGetValue(num, out array))
			{
				this.PromoteEntry(num);
				return array;
			}
			return this.Cache(num, _tileX, _tileZ);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PromoteEntry(uint _key)
	{
		for (LinkedListNode<uint> linkedListNode = this.cacheQueue.First; linkedListNode != this.cacheQueue.Last; linkedListNode = linkedListNode.Next)
		{
			if (linkedListNode.Value == _key)
			{
				this.cacheQueue.Remove(linkedListNode);
				this.cacheQueue.AddFirst(linkedListNode);
				return;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public T[,] Cache(uint _key, int _tileX, int _tileZ)
	{
		int num = _tileX - this.config.tileStart.x;
		int num2 = _tileZ - this.config.tileStart.y;
		if (!this.tilesDatabase.IsInDatabase(num, num2))
		{
			return null;
		}
		LinkedListNode<uint> linkedListNode = null;
		T[,] array = null;
		if (this.cacheQueue.Count >= this.cacheMax)
		{
			linkedListNode = this.cacheQueue.Last;
			this.cacheQueue.Remove(linkedListNode);
			array = this.cache[linkedListNode.Value];
			this.cache.Remove(linkedListNode.Value);
		}
		this.tilesDatabase.LoadTile(num, num2, ref array);
		this.cache.Add(_key, array);
		if (linkedListNode != null)
		{
			linkedListNode.Value = _key;
			this.cacheQueue.AddFirst(linkedListNode);
		}
		else
		{
			this.cacheQueue.AddFirst(_key);
		}
		return array;
	}

	public void Cleanup()
	{
		if (this.tilesDatabase != null)
		{
			this.tilesDatabase.Dispose();
			this.tilesDatabase = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TileFile<T> tilesDatabase;

	[PublicizedFrom(EAccessModifier.Private)]
	public TileAreaConfig config;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<uint, T[,]> cache = new Dictionary<uint, T[,]>();

	[PublicizedFrom(EAccessModifier.Private)]
	public LinkedList<uint> cacheQueue = new LinkedList<uint>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int cacheMax;
}
