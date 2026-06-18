using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

public class MapChunkDatabaseByRegion : IMapChunkDatabase
{
	public MapChunkDatabaseByRegion(int playerId)
	{
		this.m_regions = new Dictionary<Vector2i, MapChunkDatabaseByRegion.RegionData>();
		this.m_chunksSent = new HashSet<int>();
		this.m_chunkIdsToSend = new List<int>();
		this.m_chunkDataToSend = new List<ushort[]>();
		this.m_playerId = playerId;
	}

	public void Clear()
	{
		object regionsLock = this.m_regionsLock;
		lock (regionsLock)
		{
			this.m_regions.Clear();
		}
	}

	public ushort[] GetMapColors(long _chunkKey)
	{
		Vector2i vector2i;
		Vector2i vector2i2;
		this.ToRegionAndOffset(_chunkKey, out vector2i, out vector2i2);
		object regionsLock = this.m_regionsLock;
		MapChunkDatabaseByRegion.RegionData regionData;
		lock (regionsLock)
		{
			if (!this.m_regions.TryGetValue(vector2i, out regionData))
			{
				return null;
			}
		}
		return regionData.GetChunkData(vector2i2);
	}

	public void Add(int _chunkX, int _chunkZ, ushort[] _mapColors)
	{
		this.Add(_chunkX, _chunkZ, _mapColors, false);
	}

	public void Add(List<int> _chunks, List<ushort[]> _mapPieces)
	{
		for (int i = 0; i < _chunks.Count; i++)
		{
			int num;
			int num2;
			IMapChunkDatabase.FromChunkDBKey(_chunks[i], out num, out num2);
			this.Add(num, num2, _mapPieces[i], true);
		}
		this.m_networkDataAvailable = true;
	}

	public bool Contains(long _chunkKey)
	{
		Vector2i vector2i;
		Vector2i vector2i2;
		this.ToRegionAndOffset(_chunkKey, out vector2i, out vector2i2);
		object regionsLock = this.m_regionsLock;
		MapChunkDatabaseByRegion.RegionData regionData;
		lock (regionsLock)
		{
			if (!this.m_regions.TryGetValue(vector2i, out regionData))
			{
				return false;
			}
		}
		return regionData.GetChunkData(vector2i2) != null;
	}

	public bool IsNetworkDataAvail()
	{
		return this.m_networkDataAvailable;
	}

	public void ResetNetworkDataAvail()
	{
		this.m_networkDataAvailable = false;
	}

	public NetPackage GetMapChunkPackagesToSend()
	{
		if (!this.m_clientMapMiddlePositionUpdated)
		{
			return null;
		}
		this.m_clientMapMiddlePositionUpdated = false;
		this.m_chunkIdsToSend.Clear();
		this.m_chunkDataToSend.Clear();
		int num = World.toChunkXZ(this.m_clientMapMiddlePosition.x);
		int num2 = World.toChunkXZ(this.m_clientMapMiddlePosition.y);
		int num3 = 8;
		object regionsLock = this.m_regionsLock;
		lock (regionsLock)
		{
			for (int i = -num3; i <= num3; i++)
			{
				for (int j = -num3; j <= num3; j++)
				{
					long num4 = WorldChunkCache.MakeChunkKey(num + i, num2 + j);
					int num5 = IMapChunkDatabase.ToChunkDBKey(num4);
					if (!this.m_chunksSent.Contains(num5))
					{
						Vector2i vector2i;
						Vector2i vector2i2;
						this.ToRegionAndOffset(num4, out vector2i, out vector2i2);
						MapChunkDatabaseByRegion.RegionData regionData;
						if (this.m_regions.TryGetValue(vector2i, out regionData))
						{
							ushort[] chunkData = regionData.GetChunkData(vector2i2);
							if (chunkData != null)
							{
								this.m_chunksSent.Add(num5);
								this.m_chunkIdsToSend.Add(num5);
								this.m_chunkDataToSend.Add(chunkData);
							}
						}
					}
				}
			}
		}
		if (this.m_chunkIdsToSend.Count == 0)
		{
			return null;
		}
		return NetPackageManager.GetPackage<NetPackageMapChunks>().Setup(this.m_playerId, this.m_chunkIdsToSend, this.m_chunkDataToSend);
	}

	public void LoadAsync(ThreadManager.TaskInfo _taskInfo)
	{
		IMapChunkDatabase.DirectoryPlayerId directoryPlayerId = (IMapChunkDatabase.DirectoryPlayerId)_taskInfo.parameter;
		this.Load(Path.Join(directoryPlayerId.dir, directoryPlayerId.file));
	}

	public void SaveAsync(ThreadManager.TaskInfo _taskInfo)
	{
		IMapChunkDatabase.DirectoryPlayerId directoryPlayerId = (IMapChunkDatabase.DirectoryPlayerId)_taskInfo.parameter;
		this.Save(Path.Join(directoryPlayerId.dir, directoryPlayerId.file));
	}

	public void SetClientMapMiddlePosition(Vector2i _pos)
	{
		this.m_clientMapMiddlePosition = _pos;
		this.m_clientMapMiddlePositionUpdated = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Add(int chunkX, int chunkZ, ushort[] mapColors, bool skipCopy)
	{
		Vector2i vector2i;
		Vector2i vector2i2;
		this.ToRegionAndOffset(chunkX, chunkZ, out vector2i, out vector2i2);
		object regionsLock = this.m_regionsLock;
		MapChunkDatabaseByRegion.RegionData regionData;
		lock (regionsLock)
		{
			if (!this.m_regions.TryGetValue(vector2i, out regionData))
			{
				regionData = new MapChunkDatabaseByRegion.RegionData();
				this.m_regions.Add(vector2i, regionData);
			}
		}
		regionData.SetChunkData(vector2i2, mapColors, skipCopy);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Load(string rootDirectory)
	{
		if (!SdDirectory.Exists(rootDirectory))
		{
			return;
		}
		object regionsLock = this.m_regionsLock;
		lock (regionsLock)
		{
			HashSet<Vector2i> hashSet = new HashSet<Vector2i>(this.m_regions.Keys);
			foreach (SdFileInfo sdFileInfo in new SdDirectoryInfo(rootDirectory).EnumerateFiles())
			{
				Vector2i vector2i;
				if (this.TryGetRegionPosFromFileName(sdFileInfo.Name, out vector2i))
				{
					try
					{
						hashSet.Add(vector2i);
						MapChunkDatabaseByRegion.RegionData regionData;
						if (!this.m_regions.TryGetValue(vector2i, out regionData))
						{
							regionData = new MapChunkDatabaseByRegion.RegionData();
							this.m_regions.Add(vector2i, regionData);
						}
						regionData.Load(sdFileInfo.FullName);
						hashSet.Remove(vector2i);
					}
					catch (Exception ex)
					{
						Log.Warning(string.Format("[{0}] Failed to load region ({1}, {2}) from '{3}': {4}", new object[] { "MapChunkDatabaseByRegion", vector2i.x, vector2i.y, sdFileInfo.FullName, ex }));
					}
				}
			}
			foreach (Vector2i vector2i2 in hashSet)
			{
				this.m_regions.Remove(vector2i2);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Save(string rootDirectory)
	{
		SdDirectory.CreateDirectory(rootDirectory);
		Dictionary<Vector2i, string> dictionary = new Dictionary<Vector2i, string>();
		foreach (SdFileInfo sdFileInfo in new SdDirectoryInfo(rootDirectory).EnumerateFiles())
		{
			Vector2i vector2i;
			if (this.TryGetRegionPosFromFileName(sdFileInfo.Name, out vector2i))
			{
				dictionary[vector2i] = sdFileInfo.FullName;
			}
		}
		object regionsLock = this.m_regionsLock;
		lock (regionsLock)
		{
			foreach (KeyValuePair<Vector2i, MapChunkDatabaseByRegion.RegionData> keyValuePair in this.m_regions)
			{
				Vector2i vector2i2;
				MapChunkDatabaseByRegion.RegionData regionData;
				keyValuePair.Deconstruct(out vector2i2, out regionData);
				Vector2i vector2i3 = vector2i2;
				MapChunkDatabaseByRegion.RegionData regionData2 = regionData;
				string regionDataPath = MapChunkDatabaseByRegion.GetRegionDataPath(rootDirectory, vector2i3);
				try
				{
					dictionary.Remove(vector2i3);
					regionData2.Save(regionDataPath);
				}
				catch (Exception ex)
				{
					Log.Warning(string.Format("[{0}] Failed to save region ({1}, {2}) to '{3}': {4}", new object[] { "MapChunkDatabaseByRegion", vector2i3.x, vector2i3.y, regionDataPath, ex }));
				}
			}
		}
		foreach (KeyValuePair<Vector2i, string> keyValuePair2 in dictionary)
		{
			Vector2i vector2i2;
			string text;
			keyValuePair2.Deconstruct(out vector2i2, out text);
			Vector2i vector2i4 = vector2i2;
			string text2 = text;
			try
			{
				SdFile.Delete(text2);
			}
			catch (Exception ex2)
			{
				Log.Warning(string.Format("[{0}] Failed to delete region ({1}, {2}) to '{3}': {4}", new object[] { "MapChunkDatabaseByRegion", vector2i4.x, vector2i4.y, text2, ex2 }));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToRegionAndOffset(long worldChunkKey, out Vector2i regionPos, out Vector2i chunkOffset)
	{
		int num = WorldChunkCache.extractX(worldChunkKey);
		int num2 = WorldChunkCache.extractZ(worldChunkKey);
		this.ToRegionAndOffset(num, num2, out regionPos, out chunkOffset);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToRegionAndOffset(Vector3i chunkPos, out Vector2i regionPos, out Vector2i chunkOffset)
	{
		this.ToRegionAndOffset(chunkPos.x, chunkPos.y, out regionPos, out chunkOffset);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToRegionAndOffset(int chunkX, int chunkZ, out Vector2i regionPos, out Vector2i chunkOffset)
	{
		int num = chunkX >> 5;
		int num2 = chunkZ >> 5;
		regionPos = new Vector2i(num, num2);
		int num3 = chunkX - num * 32;
		int num4 = chunkZ - num2 * 32;
		chunkOffset = new Vector2i(num3, num4);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetRegionDataPath(string rootDirectory, Vector2i regionPos)
	{
		return Path.Join(rootDirectory, string.Format("{0}{1}{2}{3}{4}", new object[] { "r.", regionPos.x, ".", regionPos.y, ".7rm" }));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryGetRegionPosFromFileName(StringSpan name, out Vector2i regionPos)
	{
		if (name.IndexOf("r.") != 0)
		{
			regionPos = default(Vector2i);
			return false;
		}
		name = name.Slice("r.".Length);
		int num = name.Length - ".7rm".Length;
		if (num < 0 || name.LastIndexOf(".7rm") != num)
		{
			regionPos = default(Vector2i);
			return false;
		}
		name = name.Slice(0, num);
		int num2 = 0;
		bool flag = false;
		int num3 = 0;
		bool flag2 = false;
		foreach (StringSpan stringSpan in name.GetSplitEnumerator(".", StringSplitOptions.None))
		{
			if (flag2)
			{
				regionPos = default(Vector2i);
				return false;
			}
			if (flag)
			{
				if (!int.TryParse(stringSpan.AsSpan(), out num3))
				{
					regionPos = default(Vector2i);
					return false;
				}
				flag2 = true;
			}
			else
			{
				if (!int.TryParse(stringSpan.AsSpan(), out num2))
				{
					regionPos = default(Vector2i);
					return false;
				}
				flag = true;
			}
		}
		if (!flag || !flag2)
		{
			regionPos = default(Vector2i);
			return false;
		}
		regionPos = new Vector2i(num2, num3);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const string FilePrefix = "r.";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string FileElementSeparator = ".";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string FilePostfix = ".7rm";

	[PublicizedFrom(EAccessModifier.Private)]
	public const int REGION_CHUNK_WIDTH = 32;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int CHUNK_TO_REGION_SHIFT = 5;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int REGION_CHUNK_AREA = 1024;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int CHUNK_DATA_LENGTH = 256;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ushort[] EMPTY_CHUNK_DATA = new ushort[256];

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<Vector2i, MapChunkDatabaseByRegion.RegionData> m_regions;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object m_regionsLock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly HashSet<int> m_chunksSent;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<int> m_chunkIdsToSend;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<ushort[]> m_chunkDataToSend;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int m_playerId;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_networkDataAvailable;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i m_clientMapMiddlePosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_clientMapMiddlePositionUpdated;

	[PublicizedFrom(EAccessModifier.Private)]
	public class RegionData
	{
		public RegionData()
		{
			this.m_regionData = new ushort[32][][];
			for (int i = 0; i < 32; i++)
			{
				this.m_regionData[i] = new ushort[32][];
			}
			this.m_dirty = true;
		}

		public ushort[] GetChunkData(Vector2i offset)
		{
			return this.m_regionData[offset.y][offset.x];
		}

		public void SetChunkData(Vector2i offset, ushort[] mapColors, bool skipCopy)
		{
			if (skipCopy)
			{
				this.m_regionData[offset.y][offset.x] = mapColors;
				this.m_dirty = true;
				return;
			}
			ushort[] array = this.m_regionData[offset.y][offset.x];
			if (array == null)
			{
				array = new ushort[256];
				this.m_regionData[offset.y][offset.x] = array;
			}
			Array.Copy(mapColors, array, 256);
			this.m_dirty = true;
		}

		public void Load(string regionFilePath)
		{
			using (Stream stream = SdFile.Open(regionFilePath, FileMode.Open, FileAccess.Read))
			{
				using (GZipStream gzipStream = new GZipStream(stream, CompressionMode.Decompress))
				{
					using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(true))
					{
						pooledBinaryReader.SetBaseStream(stream);
						pooledBinaryReader.ReadInt32();
						pooledBinaryReader.SetBaseStream(gzipStream);
						ushort[] array = null;
						for (int i = 0; i < 32; i++)
						{
							for (int j = 0; j < 32; j++)
							{
								if (array == null)
								{
									array = new ushort[256];
								}
								for (int k = 0; k < 256; k++)
								{
									array[k] = pooledBinaryReader.ReadUInt16();
								}
								if (MapChunkDatabaseByRegion.EMPTY_CHUNK_DATA.SequenceEqual(array))
								{
									this.m_regionData[i][j] = null;
								}
								else
								{
									this.m_regionData[i][j] = array;
									array = null;
								}
							}
						}
						this.m_lastRegionPath = regionFilePath;
						this.m_dirty = false;
					}
				}
			}
		}

		public void Save(string regionFilePath)
		{
			if (!this.m_dirty && regionFilePath == this.m_lastRegionPath)
			{
				return;
			}
			using (Stream stream = SdFile.Open(regionFilePath, FileMode.Create, FileAccess.Write))
			{
				using (GZipStream gzipStream = new GZipStream(stream, CompressionMode.Compress))
				{
					using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(true))
					{
						pooledBinaryWriter.SetBaseStream(stream);
						pooledBinaryWriter.Write(1);
						pooledBinaryWriter.SetBaseStream(gzipStream);
						for (int i = 0; i < 32; i++)
						{
							for (int j = 0; j < 32; j++)
							{
								ushort[] array = this.m_regionData[i][j] ?? MapChunkDatabaseByRegion.EMPTY_CHUNK_DATA;
								for (int k = 0; k < 256; k++)
								{
									pooledBinaryWriter.Write(array[k]);
								}
							}
						}
						this.m_lastRegionPath = regionFilePath;
						this.m_dirty = false;
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const int VERSION = 1;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly ushort[][][] m_regionData;

		[PublicizedFrom(EAccessModifier.Private)]
		public string m_lastRegionPath;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool m_dirty;
	}
}
