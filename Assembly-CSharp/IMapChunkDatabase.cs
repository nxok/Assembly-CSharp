using System;
using System.Collections.Generic;
using System.ComponentModel;
using Platform;

public interface IMapChunkDatabase
{
	void Clear();

	ushort[] GetMapColors(long _chunkKey);

	void Add(Vector3i _chunkPos, World _world)
	{
		int num = (_world.IsEditor() ? 7 : 4);
		for (int i = -num; i <= num; i++)
		{
			for (int j = -num; j <= num; j++)
			{
				Chunk chunk = (Chunk)_world.GetChunkSync(_chunkPos.x + i, _chunkPos.z + j);
				if (chunk != null && !chunk.NeedsDecoration)
				{
					this.Add(_chunkPos.x + i, _chunkPos.z + j, chunk.GetMapColors());
				}
			}
		}
	}

	void Add(int _chunkX, int _chunkZ, ushort[] _mapColors);

	void Add(List<int> _chunks, List<ushort[]> _mapPieces);

	bool Contains(long _chunkKey);

	bool IsNetworkDataAvail();

	void ResetNetworkDataAvail();

	NetPackage GetMapChunkPackagesToSend();

	void LoadAsync(ThreadManager.TaskInfo _taskInfo);

	void SaveAsync(ThreadManager.TaskInfo _taskInfo);

	void SetClientMapMiddlePosition(Vector2i _pos);

	public static bool TryCreateOrLoad(int _entityId, out IMapChunkDatabase _mapDatabase, Func<IMapChunkDatabase.DirectoryPlayerId> _parameterSupplier)
	{
		if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
		{
			bool flag;
			if (_entityId != -1 && GameManager.Instance)
			{
				World world = GameManager.Instance.World;
				if (world == null)
				{
					flag = false;
				}
				else
				{
					EntityPlayerLocal primaryPlayer = world.GetPrimaryPlayer();
					int? num = ((primaryPlayer != null) ? new int?(primaryPlayer.entityId) : null);
					flag = (num.GetValueOrDefault() == _entityId) & (num != null);
				}
			}
			else
			{
				flag = false;
			}
			if (!flag)
			{
				_mapDatabase = null;
				return false;
			}
		}
		MapChunkDatabaseType value = LaunchPrefs.MapChunkDatabase.Value;
		IMapChunkDatabase mapChunkDatabase;
		if (value != MapChunkDatabaseType.Fixed)
		{
			if (value != MapChunkDatabaseType.Region)
			{
				throw new InvalidEnumArgumentException(string.Format("Unknown {0}: {1}", "MapChunkDatabaseType", LaunchPrefs.MapChunkDatabase.Value));
			}
			mapChunkDatabase = new MapChunkDatabaseByRegion(_entityId);
		}
		else
		{
			mapChunkDatabase = new MapChunkDatabase(_entityId);
		}
		IMapChunkDatabase mapChunkDatabase2 = mapChunkDatabase;
		IMapChunkDatabase.DirectoryPlayerId directoryPlayerId = _parameterSupplier();
		ThreadManager.AddSingleTask(new ThreadManager.TaskFunctionDelegate(mapChunkDatabase2.LoadAsync), directoryPlayerId, null, true);
		_mapDatabase = mapChunkDatabase2;
		return true;
	}

	public static int ToChunkDBKey(long _worldChunkKey)
	{
		return IMapChunkDatabase.ToChunkDBKey(WorldChunkCache.extractX(_worldChunkKey), WorldChunkCache.extractZ(_worldChunkKey));
	}

	public static int ToChunkDBKey(int _chunkX, int _chunkZ)
	{
		return ((_chunkZ & 65535) << 16) | (_chunkX & 65535);
	}

	public static void FromChunkDBKey(int _chunkDBKey, out int _chunkX, out int _chunkZ)
	{
		_chunkX = _chunkDBKey & 65535;
		if (_chunkX > 32767)
		{
			_chunkX |= -65536;
		}
		_chunkZ = (_chunkDBKey >> 16) & 65535;
		if (_chunkZ > 32767)
		{
			_chunkZ |= -65536;
		}
	}

	public class DirectoryPlayerId
	{
		public DirectoryPlayerId(string _dir, string _file)
		{
			this.file = _file;
			this.dir = _dir;
		}

		public string file;

		public string dir;
	}
}
