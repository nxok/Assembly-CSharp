using System;
using System.Collections.Generic;
using System.IO;

public class MapChunkDatabase : DatabaseWithFixedDS<int, ushort[]>, IMapChunkDatabase
{
	public MapChunkDatabase(int _playerId)
		: base(7364973, 4, GamePrefs.GetInt(EnumGamePrefs.MaxUncoveredMapChunksPerPlayer), 512, 0, 131072)
	{
		this.playerId = _playerId;
	}

	public void Add(int _chunkX, int _chunkZ, ushort[] _mapColors)
	{
		int num = IMapChunkDatabase.ToChunkDBKey(_chunkX, _chunkZ);
		ushort[] array = base.GetDS(num);
		if (array == null)
		{
			array = new ushort[_mapColors.Length];
		}
		Array.Copy(_mapColors, array, _mapColors.Length);
		base.SetDS(num, array);
	}

	public override void Clear()
	{
		base.Clear();
		this.chunksSent.Clear();
		this.bNetworkDataAvail = false;
	}

	public ushort[] GetMapColors(long _chunkKey)
	{
		return base.GetDS(IMapChunkDatabase.ToChunkDBKey(_chunkKey));
	}

	public void Add(List<int> _chunks, List<ushort[]> _mapPieces)
	{
		for (int i = 0; i < _chunks.Count; i++)
		{
			base.SetDS(_chunks[i], _mapPieces[i]);
		}
		this.bNetworkDataAvail = true;
	}

	public bool Contains(long _chunkKey)
	{
		return base.ContainsDS(IMapChunkDatabase.ToChunkDBKey(_chunkKey));
	}

	public bool IsNetworkDataAvail()
	{
		return this.bNetworkDataAvail;
	}

	public void ResetNetworkDataAvail()
	{
		this.bNetworkDataAvail = false;
	}

	public NetPackage GetMapChunkPackagesToSend()
	{
		if (!this.bClientMapMiddlePositionUpdated)
		{
			return null;
		}
		this.bClientMapMiddlePositionUpdated = false;
		this.toSendList.Clear();
		this.mapPiecesList.Clear();
		int num = World.toChunkXZ(this.clientMapMiddlePosition.x);
		int num2 = World.toChunkXZ(this.clientMapMiddlePosition.y);
		int num3 = 8;
		for (int i = -num3; i <= num3; i++)
		{
			for (int j = -num3; j <= num3; j++)
			{
				int num4 = IMapChunkDatabase.ToChunkDBKey(WorldChunkCache.MakeChunkKey(num + i, num2 + j));
				if (!this.chunksSent.Contains(num4) && base.ContainsDS(num4))
				{
					this.toSendList.Add(num4);
					this.chunksSent.Add(num4);
					this.mapPiecesList.Add(base.GetDS(num4));
				}
			}
		}
		if (this.toSendList.Count == 0)
		{
			return null;
		}
		return NetPackageManager.GetPackage<NetPackageMapChunks>().Setup(this.playerId, this.toSendList, this.mapPiecesList);
	}

	public void LoadAsync(ThreadManager.TaskInfo _taskInfo)
	{
		base.Load(((IMapChunkDatabase.DirectoryPlayerId)_taskInfo.parameter).dir, ((IMapChunkDatabase.DirectoryPlayerId)_taskInfo.parameter).file + ".map");
	}

	public void SaveAsync(ThreadManager.TaskInfo _taskInfo)
	{
		base.Save(((IMapChunkDatabase.DirectoryPlayerId)_taskInfo.parameter).dir, ((IMapChunkDatabase.DirectoryPlayerId)_taskInfo.parameter).file + ".map");
	}

	public void SetClientMapMiddlePosition(Vector2i _pos)
	{
		if (!this.clientMapMiddlePosition.Equals(_pos))
		{
			this.clientMapMiddlePosition = _pos;
			this.bClientMapMiddlePositionUpdated = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override int readKey(BinaryReader _br)
	{
		return _br.ReadInt32();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void writeKey(BinaryWriter _bw, int _key)
	{
		_bw.Write(_key);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void copyFromRead(byte[] _dataRead, ushort[] _data)
	{
		for (int i = 0; i < _data.Length; i++)
		{
			_data[i] = (ushort)((int)_dataRead[i * 2] | ((int)_dataRead[i * 2 + 1] << 8));
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void copyToWrite(ushort[] _data, byte[] _dataWrite)
	{
		for (int i = 0; i < _data.Length; i++)
		{
			_dataWrite[i * 2] = (byte)(_data[i] & 255);
			_dataWrite[i * 2 + 1] = (byte)((_data[i] >> 8) & 255);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override ushort[] allocateDataStorage()
	{
		return new ushort[256];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cMaxMapChunks = 131072;

	[PublicizedFrom(EAccessModifier.Private)]
	public const string EXT = "map";

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<int> chunksSent = new HashSet<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int playerId;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i clientMapMiddlePosition = new Vector2i(int.MaxValue, int.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bClientMapMiddlePositionUpdated;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bNetworkDataAvail;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<int> toSendList = new List<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<ushort[]> mapPiecesList = new List<ushort[]>();
}
