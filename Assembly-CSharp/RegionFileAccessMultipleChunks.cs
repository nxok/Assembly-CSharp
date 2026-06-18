using System;
using System.Collections.Generic;
using System.IO;
using Unity.Profiling;
using UnityEngine;

public abstract class RegionFileAccessMultipleChunks : RegionFileAccessAbstract
{
	public RegionFileAccessMultipleChunks()
	{
		this.writeStream = new ChunkMemoryStreamWriter();
		this.readStream = new ChunkMemoryStreamReader();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void ReadDirectory(string _dir, Action<long, string, uint> _chunkAndTimeStampHandler, int chunksPerRegionPerDimension)
	{
		if (_dir != null)
		{
			if (!SdDirectory.Exists(_dir))
			{
				SdDirectory.CreateDirectory(_dir);
				return;
			}
			foreach (SdFileInfo sdFileInfo in new SdDirectoryInfo(_dir).GetFiles())
			{
				string[] array = sdFileInfo.Name.Split('.', StringSplitOptions.None);
				int num;
				int num2;
				if (array.Length != 4)
				{
					if (!sdFileInfo.Name.EqualsCaseInsensitive("PendingResets.7pr"))
					{
						Debug.LogError("Invalid region file name: " + sdFileInfo.FullName);
					}
				}
				else if (!int.TryParse(array[1], out num) || !int.TryParse(array[2], out num2))
				{
					Debug.LogError("Failed to parse region coordinates from region file name: " + sdFileInfo.FullName);
				}
				else
				{
					string text = array[3];
					RegionFile rfc = this.GetRFC(num, num2, _dir, text);
					for (int j = num * chunksPerRegionPerDimension; j < num * chunksPerRegionPerDimension + chunksPerRegionPerDimension; j++)
					{
						for (int k = num2 * chunksPerRegionPerDimension; k < num2 * chunksPerRegionPerDimension + chunksPerRegionPerDimension; k++)
						{
							if (rfc.HasChunk(j, k))
							{
								uint num3;
								rfc.GetTimestampInfo(j, k, out num3);
								_chunkAndTimeStampHandler(WorldChunkCache.MakeChunkKey(j, k), text, num3);
							}
						}
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public RegionFile GetRFC(int _regionX, int _regionZ, string _dir, string _ext)
	{
		Dictionary<string, RegionFileAccessMultipleChunks.Region> dictionary = this.regionTable;
		RegionFile regionFile2;
		lock (dictionary)
		{
			RegionFileAccessMultipleChunks.Region region;
			if (!this.regionTable.TryGetValue(_dir, out region))
			{
				region = new RegionFileAccessMultipleChunks.Region();
				this.regionTable.Add(_dir, region);
			}
			Vector2 vector;
			vector..ctor((float)_regionX, (float)_regionZ);
			RegionFileAccessMultipleChunks.RegionExtensions regionExtensions;
			if (!region.TryGetValue(vector, out regionExtensions))
			{
				regionExtensions = new RegionFileAccessMultipleChunks.RegionExtensions();
				region.Add(vector, regionExtensions);
			}
			RegionFile regionFile;
			if (!regionExtensions.TryGetValue(_ext, out regionFile))
			{
				regionFile = this.OpenRegionFile(_dir, _regionX, _regionZ, _ext);
				regionExtensions.Add(_ext, regionFile);
			}
			regionFile2 = regionFile;
		}
		return regionFile2;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract RegionFile OpenRegionFile(string _dir, int _regionX, int _regionZ, string _ext);

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void GetRegionCoords(int _chunkX, int _chunkZ, out int _regionX, out int _regionZ);

	public override Stream GetOutputStream(string _dir, int _chunkX, int _chunkZ, string _ext)
	{
		this.writeStream.Init(this, _dir, _chunkX, _chunkZ, _ext);
		return this.writeStream;
	}

	public override Stream GetInputStream(string _dir, int _chunkX, int _chunkZ, string _ext)
	{
		int num;
		int num2;
		this.GetRegionCoords(_chunkX, _chunkZ, out num, out num2);
		RegionFile rfc = this.GetRFC(num, num2, _dir, _ext);
		if (!rfc.HasChunk(_chunkX, _chunkZ))
		{
			return null;
		}
		rfc.ReadData(_chunkX, _chunkZ, this.readStream);
		this.readStream.Position = 0L;
		return this.readStream;
	}

	public void Write(string _dir, int _chunkX, int _chunkZ, string _ext, byte[] _buf, int _bufLength)
	{
		int num;
		int num2;
		this.GetRegionCoords(_chunkX, _chunkZ, out num, out num2);
		this.GetRFC(num, num2, _dir, _ext).WriteData(_chunkX, _chunkZ, _bufLength, 0, _buf, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int MediumIntByteArrayToInt(byte[] bytes)
	{
		return (int)bytes[0] | ((int)bytes[1] << 8) | ((int)bytes[2] << 16);
	}

	public override void Remove(string _dir, int _chunkX, int _chunkZ)
	{
		int num;
		int num2;
		this.GetRegionCoords(_chunkX, _chunkZ, out num, out num2);
		Dictionary<string, RegionFileAccessMultipleChunks.Region> dictionary = this.regionTable;
		lock (dictionary)
		{
			RegionFileAccessMultipleChunks.Region region;
			if (this.regionTable.TryGetValue(_dir, out region))
			{
				Vector2 vector;
				vector..ctor((float)num, (float)num2);
				RegionFileAccessMultipleChunks.RegionExtensions regionExtensions;
				if (region.TryGetValue(vector, out regionExtensions))
				{
					foreach (RegionFile regionFile in regionExtensions.Values)
					{
						if (regionFile.HasChunk(_chunkX, _chunkZ))
						{
							regionFile.RemoveChunk(_chunkX, _chunkZ);
							this.regionsWithRemovedChunks.Add(regionFile);
						}
					}
				}
			}
		}
	}

	public override void OptimizeLayouts()
	{
		foreach (RegionFile regionFile in this.regionsWithRemovedChunks)
		{
			using (RegionFileAccessMultipleChunks.s_OptimizeLayoutsMarker.Auto())
			{
				if (regionFile.ChunkCount() > 0)
				{
					regionFile.OptimizeLayout();
				}
				else
				{
					int num;
					int num2;
					string text;
					regionFile.GetPositionAndPath(out num, out num2, out text);
					this.RemoveRegionFromCache(num, num2, Path.GetDirectoryName(text));
					SdFile.Delete(text);
				}
			}
		}
		this.regionsWithRemovedChunks.Clear();
	}

	public override void Close()
	{
		this.OptimizeLayouts();
		this.ClearCache();
	}

	public override void ClearCache()
	{
		Dictionary<string, RegionFileAccessMultipleChunks.Region> dictionary = this.regionTable;
		lock (dictionary)
		{
			foreach (RegionFileAccessMultipleChunks.Region region in this.regionTable.Values)
			{
				foreach (RegionFileAccessMultipleChunks.RegionExtensions regionExtensions in region.Values)
				{
					foreach (RegionFile regionFile in regionExtensions.Values)
					{
						regionFile.SaveHeaderData();
						regionFile.Close();
					}
					regionExtensions.Clear();
				}
				region.Clear();
			}
			this.regionTable.Clear();
		}
	}

	public override void RemoveRegionFromCache(int _regionX, int _regionZ, string _dir)
	{
		Dictionary<string, RegionFileAccessMultipleChunks.Region> dictionary = this.regionTable;
		lock (dictionary)
		{
			RegionFileAccessMultipleChunks.Region region;
			if (this.regionTable.TryGetValue(_dir, out region))
			{
				Vector2 vector;
				vector..ctor((float)_regionX, (float)_regionZ);
				RegionFileAccessMultipleChunks.RegionExtensions regionExtensions;
				if (region.TryGetValue(vector, out regionExtensions))
				{
					foreach (RegionFile regionFile in regionExtensions.Values)
					{
						regionFile.SaveHeaderData();
						regionFile.Close();
					}
					regionExtensions.Clear();
					region.Remove(vector);
				}
			}
		}
	}

	public override int GetChunkByteCount(string _dir, int _chunkX, int _chunkZ)
	{
		int num;
		int num2;
		this.GetRegionCoords(_chunkX, _chunkZ, out num, out num2);
		int num3 = 0;
		Dictionary<string, RegionFileAccessMultipleChunks.Region> dictionary = this.regionTable;
		lock (dictionary)
		{
			RegionFileAccessMultipleChunks.Region region;
			if (!this.regionTable.TryGetValue(_dir, out region))
			{
				return 0;
			}
			RegionFileAccessMultipleChunks.RegionExtensions regionExtensions;
			if (!region.TryGetValue(new Vector2((float)num, (float)num2), out regionExtensions))
			{
				return 0;
			}
			foreach (RegionFile regionFile in regionExtensions.Values)
			{
				num3 += regionFile.GetChunkByteCount(_chunkX, _chunkZ);
			}
		}
		return num3;
	}

	public override long GetTotalByteCount(string _dir)
	{
		long num = 0L;
		Dictionary<string, RegionFileAccessMultipleChunks.Region> dictionary = this.regionTable;
		lock (dictionary)
		{
			foreach (RegionFileAccessMultipleChunks.Region region in this.regionTable.Values)
			{
				foreach (RegionFileAccessMultipleChunks.RegionExtensions regionExtensions in region.Values)
				{
					foreach (RegionFile regionFile in regionExtensions.Values)
					{
						num += regionFile.Length;
					}
				}
			}
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, RegionFileAccessMultipleChunks.Region> regionTable = new Dictionary<string, RegionFileAccessMultipleChunks.Region>();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<RegionFile> regionsWithRemovedChunks = new HashSet<RegionFile>();

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkMemoryStreamWriter writeStream;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkMemoryStreamReader readStream;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_OptimizeLayoutsMarker = new ProfilerMarker("RegionFileAccess.OptimizeLayout");

	[PublicizedFrom(EAccessModifier.Private)]
	public class Region : Dictionary<Vector2, RegionFileAccessMultipleChunks.RegionExtensions>
	{
		public Region()
			: base(Vector2EqualityComparer.Instance)
		{
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class RegionExtensions : Dictionary<string, RegionFile>
	{
	}
}
