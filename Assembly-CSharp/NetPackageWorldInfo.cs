using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageWorldInfo : NetPackage
{
	public NetPackageWorldInfo Setup(string _gameMode, string _levelName, string _gameName, string _guid, PersistentPlayerList _playerList, ulong _ticks, bool _fixedSizeCC, bool _firstTimeJoin)
	{
		this.gameMode = _gameMode;
		this.levelName = _levelName;
		this.gameName = _gameName;
		this.ppList = _playerList;
		this.ticks = _ticks;
		this.guid = _guid;
		this.fixedSizeCC = _fixedSizeCC;
		this.firstTimeJoin = _firstTimeJoin;
		return this;
	}

	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.gameMode = _reader.ReadString();
		this.levelName = _reader.ReadString();
		this.gameName = _reader.ReadString();
		this.guid = _reader.ReadString();
		this.ppList = (_reader.ReadBoolean() ? PersistentPlayerList.Read(_reader) : new PersistentPlayerList());
		this.ticks = _reader.ReadUInt64();
		this.fixedSizeCC = _reader.ReadBoolean();
		this.firstTimeJoin = _reader.ReadBoolean();
		int num = _reader.ReadInt32();
		this.worldFileHashes = new Dictionary<string, uint>();
		for (int i = 0; i < num; i++)
		{
			string text = _reader.ReadString();
			uint num2 = _reader.ReadUInt32();
			this.worldFileHashes.Add(text, num2);
		}
		NetPackageWorldInfo.worldDataSize = _reader.ReadInt64();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.gameMode);
		_writer.Write(this.levelName);
		_writer.Write(this.gameName);
		_writer.Write(this.guid);
		_writer.Write(this.ppList != null);
		PersistentPlayerList persistentPlayerList = this.ppList;
		if (persistentPlayerList != null)
		{
			persistentPlayerList.Write(_writer);
		}
		_writer.Write(this.ticks);
		_writer.Write(this.fixedSizeCC);
		_writer.Write(this.firstTimeJoin);
		_writer.Write(NetPackageWorldInfo.worldHashesData);
		_writer.Write(NetPackageWorldInfo.worldDataSize);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		_callbacks.WorldInfo(this.gameMode, this.levelName, this.gameName, this.guid, this.ppList, this.ticks, this.fixedSizeCC, this.firstTimeJoin, this.worldFileHashes, NetPackageWorldInfo.worldDataSize);
	}

	public override int GetLength()
	{
		return 48 + NetPackageWorldInfo.worldHashesData.Length + 4 + 8;
	}

	public static void PrepareWorldHashes()
	{
		NetPackageWorldInfo.worldHashesData = null;
		ChunkProviderGenerateWorldFromRaw chunkProviderGenerateWorldFromRaw = GameManager.Instance.World.ChunkCache.ChunkProvider as ChunkProviderGenerateWorldFromRaw;
		Dictionary<string, uint> dictionary = ((chunkProviderGenerateWorldFromRaw != null) ? chunkProviderGenerateWorldFromRaw.worldFileCrcs : null);
		NetPackageWorldInfo.worldDataSize = ((chunkProviderGenerateWorldFromRaw != null) ? chunkProviderGenerateWorldFromRaw.worldFileTotalSize : 0L);
		List<string> list = null;
		if (dictionary != null)
		{
			list = GameUtils.GetWorldFilesToTransmitToClient(dictionary.Keys);
		}
		using (MemoryStream memoryStream = new MemoryStream())
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
			{
				if (dictionary != null)
				{
					binaryWriter.Write(list.Count);
					using (List<string>.Enumerator enumerator = list.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							string text = enumerator.Current;
							binaryWriter.Write(text);
							binaryWriter.Write(dictionary[text]);
						}
						goto IL_00B5;
					}
				}
				binaryWriter.Write(0);
				IL_00B5:
				NetPackageWorldInfo.worldHashesData = memoryStream.ToArray();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string gameMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public string levelName;

	[PublicizedFrom(EAccessModifier.Private)]
	public string gameName;

	[PublicizedFrom(EAccessModifier.Private)]
	public string guid;

	[PublicizedFrom(EAccessModifier.Private)]
	public PersistentPlayerList ppList;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong ticks;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool fixedSizeCC;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool firstTimeJoin;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, uint> worldFileHashes;

	[PublicizedFrom(EAccessModifier.Private)]
	public static byte[] worldHashesData;

	[PublicizedFrom(EAccessModifier.Private)]
	public static long worldDataSize;
}
