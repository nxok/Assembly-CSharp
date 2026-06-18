using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

public class DynamicMeshChunkData
{
	public int X { get; set; }

	public int OffsetY { get; set; }

	public int Z { get; set; }

	public int UpdateTime { get; set; }

	public byte MainBiome { get; set; }

	public int EndY { get; set; }

	public List<byte> TerrainHeight { get; set; } = new List<byte>();

	public List<byte> Height { get; set; } = new List<byte>();

	public List<byte> TopSoil { get; set; } = new List<byte>();

	public List<uint> BlockRaw { get; set; } = new List<uint>();

	public List<sbyte> Densities { get; set; } = new List<sbyte>();

	public List<long> Textures { get; set; } = new List<long>();

	public int TotalBlocks
	{
		get
		{
			return Math.Min((this.EndY - this.OffsetY + 1) * 256, this.BlockRaw.Count);
		}
	}

	public DynamicMeshChunkData.ChunkNeighbourData GetNeighbourData(int x, int z)
	{
		return this._neighbours[x + 1, z + 1];
	}

	public void SetNeighbourData(int x, int z, DynamicMeshChunkData.ChunkNeighbourData data)
	{
		this._neighbours[x + 1, z + 1] = data;
	}

	public void Copy(DynamicMeshChunkData other)
	{
		if (other == null)
		{
			return;
		}
		this.Reset();
		this.X = other.X;
		this.OffsetY = other.OffsetY;
		this.MinTerrainHeight = other.MinTerrainHeight;
		this.Z = other.Z;
		this.UpdateTime = other.UpdateTime;
		this.EndY = other.EndY;
		this.MainBiome = other.MainBiome;
		this.TerrainHeight.AddRange(other.TerrainHeight);
		this.Height.AddRange(other.Height);
		this.TopSoil.AddRange(other.TopSoil);
		int totalBlocks = other.TotalBlocks;
		for (int i = 0; i < totalBlocks; i++)
		{
			this.BlockRaw.Add(other.BlockRaw[i]);
		}
		this.Densities.AddRange(other.Densities);
		this.Textures.AddRange(other.Textures);
		for (int j = -1; j < 2; j++)
		{
			for (int k = -1; k < 2; k++)
			{
				if (j != 0 || k != 0)
				{
					this.GetNeighbourData(j, k).Copy(other.GetNeighbourData(j, k));
				}
			}
		}
	}

	public static DynamicMeshChunkData LoadFromStream(MemoryStream stream)
	{
		stream.Position = 0L;
		DynamicMeshChunkData fromCache = DynamicMeshChunkData.GetFromCache("_LoadStream_");
		using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
		{
			pooledBinaryReader.SetBaseStream(stream);
			fromCache.Read(pooledBinaryReader);
		}
		return fromCache;
	}

	public void RecordCounts()
	{
		this.lastRaw = this.BlockRaw.Count;
		this.lastDen = this.Densities.Count;
		this.lastTex = this.Textures.Count;
	}

	public void ClearPreviousLayers()
	{
		if (this.lastRaw > 0)
		{
			this.BlockRaw.RemoveRange(0, this.lastRaw);
		}
		if (this.lastDen > 0)
		{
			this.Densities.RemoveRange(0, this.lastDen);
		}
		if (this.lastTex > 0)
		{
			this.Textures.RemoveRange(0, this.lastTex);
		}
	}

	public void Reset()
	{
		this.X = 0;
		this.OffsetY = 0;
		this.Z = 0;
		this.UpdateTime = 0;
		this.MainBiome = 0;
		this.EndY = 0;
		this.TerrainHeight.Clear();
		this.Height.Clear();
		this.TopSoil.Clear();
		this.BlockRaw.Clear();
		this.Densities.Clear();
		this.Textures.Clear();
		this.MinTerrainHeight = 500;
		this.GetNeighbourData(-1, -1).Clear();
		this.GetNeighbourData(-1, 0).Clear();
		this.GetNeighbourData(-1, 1).Clear();
		this.GetNeighbourData(1, 1).Clear();
		this.GetNeighbourData(1, 0).Clear();
		this.GetNeighbourData(1, -1).Clear();
		this.GetNeighbourData(0, -1).Clear();
		this.GetNeighbourData(0, 1).Clear();
	}

	public void SetTopSoil(byte[] soil)
	{
		this.TopSoil.AddRange(soil);
	}

	public int GetStreamSize()
	{
		int num = 81 + this.TerrainHeight.Count + this.Height.Count + this.TopSoil.Count + 4 * this.BlockRaw.Count + this.Densities.Count + 8 * this.Textures.Count;
		for (int i = -1; i < 2; i++)
		{
			for (int j = -1; j < 2; j++)
			{
				if (i != 0 || j != 0)
				{
					DynamicMeshChunkData.ChunkNeighbourData neighbourData = this.GetNeighbourData(i, j);
					num += 4 * neighbourData.BlockRaw.Count + neighbourData.Densities.Count + 8 * neighbourData.Textures.Count;
				}
			}
		}
		return num;
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(this.X);
		writer.Write(this.OffsetY);
		writer.Write(this.Z);
		writer.Write(this.EndY);
		writer.Write(this.MinTerrainHeight);
		writer.Write(this.UpdateTime);
		writer.Write(this.MainBiome);
		writer.Write(this.TerrainHeight.Count);
		foreach (byte b in this.TerrainHeight)
		{
			writer.Write(b);
		}
		writer.Write(this.Height.Count);
		foreach (byte b2 in this.Height)
		{
			writer.Write(b2);
		}
		writer.Write(this.TopSoil.Count);
		foreach (byte b3 in this.TopSoil)
		{
			writer.Write(b3);
		}
		int totalBlocks = this.TotalBlocks;
		writer.Write(totalBlocks);
		for (int i = 0; i < totalBlocks; i++)
		{
			writer.Write(this.BlockRaw[i]);
		}
		writer.Write(this.Densities.Count);
		foreach (sbyte b4 in this.Densities)
		{
			writer.Write(b4);
		}
		writer.Write(this.Textures.Count);
		foreach (long num in this.Textures)
		{
			writer.Write(num);
		}
		for (int j = -1; j < 2; j++)
		{
			for (int k = -1; k < 2; k++)
			{
				if (j != 0 || k != 0)
				{
					this.GetNeighbourData(j, k).Write(writer);
				}
			}
		}
	}

	public void Read(PooledBinaryReader reader)
	{
		this.X = reader.ReadInt32();
		this.OffsetY = reader.ReadInt32();
		this.Z = reader.ReadInt32();
		this.EndY = reader.ReadInt32();
		this.MinTerrainHeight = reader.ReadInt32();
		this.UpdateTime = reader.ReadInt32();
		this.MainBiome = reader.ReadByte();
		int num = reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			this.TerrainHeight.Add(reader.ReadByte());
		}
		num = reader.ReadInt32();
		for (int j = 0; j < num; j++)
		{
			this.Height.Add(reader.ReadByte());
		}
		num = reader.ReadInt32();
		for (int k = 0; k < num; k++)
		{
			this.TopSoil.Add(reader.ReadByte());
		}
		num = reader.ReadInt32();
		for (int l = 0; l < num; l++)
		{
			this.BlockRaw.Add(reader.ReadUInt32());
		}
		num = reader.ReadInt32();
		for (int m = 0; m < num; m++)
		{
			this.Densities.Add(reader.ReadSByte());
		}
		num = reader.ReadInt32();
		for (int n = 0; n < num; n++)
		{
			this.Textures.Add(reader.ReadInt64());
		}
		for (int num2 = -1; num2 < 2; num2++)
		{
			for (int num3 = -1; num3 < 2; num3++)
			{
				if (num2 != 0 || num3 != 0)
				{
					this.GetNeighbourData(num2, num3).Read(reader);
				}
			}
		}
	}

	public void ApplyToChunk(Chunk chunk, ChunkCacheNeighborChunks cacheNeighbourChunks)
	{
		int num = 0;
		chunk.X = this.X;
		chunk.Z = this.Z;
		chunk.SetTopSoil(this.TopSoil);
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				chunk.SetHeight(i, j, this.Height[num]);
				chunk.SetTerrainHeight(i, j, this.TerrainHeight[num++]);
			}
		}
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		BlockValue blockValue = default(BlockValue);
		for (int k = 0; k < 256; k++)
		{
			for (int i = 0; i < 16; i++)
			{
				for (int j = 0; j < 16; j++)
				{
					chunk.SetLight(i, k, j, 15, Chunk.LIGHT_TYPE.SUN);
					if (k < this.OffsetY - 1 || k >= this.EndY)
					{
						chunk.SetBlockRaw(i, k, j, BlockValue.Air);
						chunk.SetDensity(i, k, j, MarchingCubes.DensityAir);
						chunk.SetTextureFull(i, k, j, 0L, 0);
					}
					else
					{
						blockValue.rawData = this.BlockRaw[num2];
						bool flag;
						if (flag = DynamicMeshSettings.UseImposterValues && DynamicMeshBlockSwap.BlockSwaps.TryGetValue(blockValue.type, out num4))
						{
							if (num4 == 0)
							{
								blockValue.rawData = 0U;
								num3++;
							}
							else
							{
								blockValue.type = num4;
							}
						}
						if (blockValue.rawData == 0U)
						{
							chunk.SetBlockRaw(i, k, j, BlockValue.Air);
							chunk.SetDensity(i, k, j, MarchingCubes.DensityAir);
							chunk.SetTextureFull(i, k, j, 0L, 0);
						}
						else
						{
							long num5 = (Block.list[blockValue.type].shape.IsTerrain() ? 0L : this.Textures[num3]);
							if (flag)
							{
								long num6;
								DynamicMeshBlockSwap.TextureSwaps.TryGetValue(num4, out num6);
								if (num5 == 0L && num6 != 0L)
								{
									num5 = num6 | (num6 << 8) | (num6 << 16) | (num6 << 24) | (num6 << 32) | (num6 << 40);
								}
							}
							chunk.SetBlockRaw(i, k, j, blockValue);
							chunk.SetDensity(i, k, j, this.Densities[num3]);
							chunk.SetTextureFull(i, k, j, num5, 0);
							num3++;
						}
						num2++;
					}
				}
			}
		}
		this.SetXNeighbour(15, (Chunk)cacheNeighbourChunks[-1, 0], this.GetNeighbourData(-1, 0));
		this.SetXNeighbour(0, (Chunk)cacheNeighbourChunks[1, 0], this.GetNeighbourData(1, 0));
		this.SetZNeighbour(15, (Chunk)cacheNeighbourChunks[0, -1], this.GetNeighbourData(0, -1));
		this.SetZNeighbour(0, (Chunk)cacheNeighbourChunks[0, 1], this.GetNeighbourData(0, 1));
		this.SetNeighbourCorner(15, 15, (Chunk)cacheNeighbourChunks[-1, -1], this.GetNeighbourData(-1, -1));
		this.SetNeighbourCorner(0, 15, (Chunk)cacheNeighbourChunks[1, -1], this.GetNeighbourData(1, -1));
		this.SetNeighbourCorner(15, 0, (Chunk)cacheNeighbourChunks[-1, 1], this.GetNeighbourData(-1, 1));
		this.SetNeighbourCorner(0, 0, (Chunk)cacheNeighbourChunks[1, 1], this.GetNeighbourData(1, 1));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetNeighbourCorner(int x, int z, Chunk chunk, DynamicMeshChunkData.ChunkNeighbourData data)
	{
		BlockValue blockValue = new BlockValue(0U);
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < 256; i++)
		{
			blockValue.rawData = data.BlockRaw[num];
			num++;
			chunk.SetBlockRaw(x, i, z, blockValue);
			if (blockValue.rawData != 0U)
			{
				chunk.SetDensity(x, i, z, data.Densities[num2]);
				chunk.SetTextureFull(x, i, z, (long)data.Densities[num2], 0);
				num2++;
			}
			else
			{
				chunk.SetDensity(x, i, z, MarchingCubes.DensityAir);
				chunk.SetTextureFull(x, i, z, 0L, 0);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetXNeighbour(int x, Chunk chunk, DynamicMeshChunkData.ChunkNeighbourData data)
	{
		BlockValue blockValue = new BlockValue(0U);
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < 256; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				blockValue.rawData = data.BlockRaw[num];
				num++;
				chunk.SetBlockRaw(x, i, j, blockValue);
				if (blockValue.rawData != 0U)
				{
					chunk.SetDensity(x, i, j, data.Densities[num2]);
					chunk.SetTextureFull(x, i, j, (long)data.Densities[num2], 0);
					num2++;
				}
				else
				{
					chunk.SetDensity(x, i, j, MarchingCubes.DensityAir);
					chunk.SetTextureFull(x, i, j, 0L, 0);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetZNeighbour(int z, Chunk chunk, DynamicMeshChunkData.ChunkNeighbourData data)
	{
		BlockValue blockValue = new BlockValue(0U);
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < 256; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				blockValue.rawData = data.BlockRaw[num];
				num++;
				chunk.SetBlockRaw(j, i, z, blockValue);
				if (blockValue.rawData != 0U)
				{
					chunk.SetDensity(j, i, z, data.Densities[num2]);
					chunk.SetTextureFull(j, i, z, (long)data.Densities[num2], 0);
					num2++;
				}
				else
				{
					chunk.SetDensity(j, i, z, MarchingCubes.DensityAir);
					chunk.SetTextureFull(j, i, z, 0L, 0);
				}
			}
		}
	}

	public static DynamicMeshChunkData GetFromCache(string debug)
	{
		DynamicMeshChunkData dynamicMeshChunkData;
		if (!DynamicMeshChunkData.Cache.TryDequeue(out dynamicMeshChunkData))
		{
			dynamicMeshChunkData = DynamicMeshChunkData.Creates();
		}
		DynamicMeshChunkData.ActiveDataItems++;
		return dynamicMeshChunkData;
	}

	public static void AddToCache(DynamicMeshChunkData data, string debug)
	{
		data.Reset();
		DynamicMeshChunkData.Cache.Enqueue(data);
		DynamicMeshChunkData.ActiveDataItems--;
	}

	public static DynamicMeshChunkData Creates()
	{
		DynamicMeshChunkData dynamicMeshChunkData = new DynamicMeshChunkData
		{
			BlockRaw = new List<uint>(),
			Densities = new List<sbyte>(),
			Textures = new List<long>(),
			TopSoil = new List<byte>(32),
			Height = new List<byte>(256),
			TerrainHeight = new List<byte>(256)
		};
		for (int i = -1; i < 2; i++)
		{
			for (int j = -1; j < 2; j++)
			{
				if (i != 0 || j != 0)
				{
					dynamicMeshChunkData.SetNeighbourData(i, j, DynamicMeshChunkData.ChunkNeighbourData.Create());
				}
			}
		}
		return dynamicMeshChunkData;
	}

	public int MinTerrainHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	public DynamicMeshChunkData.ChunkNeighbourData[,] _neighbours = new DynamicMeshChunkData.ChunkNeighbourData[3, 3];

	[PublicizedFrom(EAccessModifier.Private)]
	public int lastRaw;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lastDen;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lastTex;

	public static int ActiveDataItems = 0;

	public static ConcurrentQueue<DynamicMeshChunkData> Cache = new ConcurrentQueue<DynamicMeshChunkData>();

	public class ChunkNeighbourData
	{
		public List<uint> BlockRaw { get; set; }

		public List<sbyte> Densities { get; set; }

		public List<long> Textures { get; set; }

		public void Clear()
		{
			this.BlockRaw.Clear();
			this.Densities.Clear();
			this.Textures.Clear();
		}

		public void SetData(uint blockraw, sbyte density, long texture)
		{
			this.BlockRaw.Add(blockraw);
			if (blockraw == 0U)
			{
				return;
			}
			this.Densities.Add(density);
			this.Textures.Add(texture);
		}

		public void Copy(DynamicMeshChunkData.ChunkNeighbourData other)
		{
			this.BlockRaw.Clear();
			this.Densities.Clear();
			this.Textures.Clear();
			this.BlockRaw.AddRange(other.BlockRaw);
			this.Densities.AddRange(other.Densities);
			this.Textures.AddRange(other.Textures);
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(this.BlockRaw.Count);
			foreach (uint num in this.BlockRaw)
			{
				writer.Write(num);
			}
			writer.Write(this.Densities.Count);
			foreach (sbyte b in this.Densities)
			{
				writer.Write(b);
			}
			writer.Write(this.Textures.Count);
			foreach (long num2 in this.Textures)
			{
				writer.Write(num2);
			}
		}

		public void Read(PooledBinaryReader reader)
		{
			int num = reader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				this.BlockRaw.Add(reader.ReadUInt32());
			}
			num = reader.ReadInt32();
			for (int j = 0; j < num; j++)
			{
				this.Densities.Add(reader.ReadSByte());
			}
			num = reader.ReadInt32();
			for (int k = 0; k < num; k++)
			{
				this.Textures.Add(reader.ReadInt64());
			}
		}

		public static DynamicMeshChunkData.ChunkNeighbourData Create()
		{
			return new DynamicMeshChunkData.ChunkNeighbourData
			{
				BlockRaw = new List<uint>(),
				Densities = new List<sbyte>(),
				Textures = new List<long>()
			};
		}

		public static DynamicMeshChunkData.ChunkNeighbourData CreateMax()
		{
			return new DynamicMeshChunkData.ChunkNeighbourData
			{
				BlockRaw = new List<uint>(65280),
				Densities = new List<sbyte>(65280),
				Textures = new List<long>(65280)
			};
		}

		public static DynamicMeshChunkData.ChunkNeighbourData CreateCorner()
		{
			return new DynamicMeshChunkData.ChunkNeighbourData
			{
				BlockRaw = new List<uint>(255),
				Densities = new List<sbyte>(255),
				Textures = new List<long>(255)
			};
		}
	}
}
