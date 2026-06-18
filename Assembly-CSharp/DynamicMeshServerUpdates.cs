using System;
using System.Collections.Generic;

public class DynamicMeshServerUpdates
{
	public static void AddToPool(DynamicMeshServerUpdates data)
	{
		if (DynamicMeshServerUpdates.Pool.Count > 40)
		{
			data.Bytes = null;
			return;
		}
		DynamicMeshServerUpdates.Pool.Enqueue(data);
	}

	public static DynamicMeshServerUpdates GetFromPool()
	{
		if (DynamicMeshServerUpdates.Pool.Count > 0)
		{
			return DynamicMeshServerUpdates.Pool.Dequeue();
		}
		return new DynamicMeshServerUpdates();
	}

	public long GetKey()
	{
		return WorldChunkCache.MakeChunkKey(World.toChunkXZ(DynamicMeshUnity.RoundChunk(this.ChunkX)), World.toChunkXZ(DynamicMeshUnity.RoundChunk(this.ChunkZ)));
	}

	public int ChunkX { get; set; }

	public int ChunkZ { get; set; }

	public int StartY { get; set; }

	public int EndY { get; set; }

	public int UpdateTime { get; set; }

	public List<byte> Bytes { get; set; }

	public DynamicMeshServerUpdates()
	{
		this.Bytes = new List<byte>();
	}

	public void WriteAir(List<byte> tempArray)
	{
		tempArray.Add(0);
	}

	public void WriteLayer(List<byte> tempArray)
	{
		this.DataLayerCount++;
		this.Bytes.Add(byte.MaxValue);
		this.Bytes.AddRange(tempArray);
	}

	public void WriteEmptyLayer()
	{
		this.EmptyLayerCount++;
		this.Bytes.Add(128);
	}

	public void WriteBinaryBlock(List<byte> tempArray, BlockValue b, sbyte dens, long tex)
	{
		if (b.isair)
		{
			tempArray.Add(0);
			return;
		}
		byte[] array;
		if (!DynamicMeshServerUpdates.BlockBytes.TryGetValue(b.rawData, out array))
		{
			array = BitConverter.GetBytes(b.type);
			DynamicMeshServerUpdates.BlockBytes.Add(b.rawData, array);
		}
		tempArray.AddRange(array);
		if (!DynamicMeshServerUpdates.TexBytes.TryGetValue(tex, out array))
		{
			array = BitConverter.GetBytes(tex);
			DynamicMeshServerUpdates.TexBytes.Add(tex, array);
		}
		tempArray.AddRange(array);
		tempArray.Add((byte)dens);
	}

	public const int DataLayer = 255;

	public const byte EmptyLayer = 128;

	public const int EmptyBlock = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Queue<DynamicMeshServerUpdates> Pool = new Queue<DynamicMeshServerUpdates>(20);

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<uint, byte[]> BlockBytes = new Dictionary<uint, byte[]>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<long, byte[]> TexBytes = new Dictionary<long, byte[]>();

	public int EmptyLayerCount;

	public int DataLayerCount;
}
