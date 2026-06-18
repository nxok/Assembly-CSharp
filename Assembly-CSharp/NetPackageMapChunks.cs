using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageMapChunks : NetPackage
{
	public override int Channel
	{
		get
		{
			return 1;
		}
	}

	public override bool Compress
	{
		get
		{
			return true;
		}
	}

	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	public NetPackageMapChunks Setup(int _entityId, List<int> _chunks, List<ushort[]> _mapPieces)
	{
		this.entityId = _entityId;
		this.chunks = new List<int>(_chunks);
		this.mapPieces = new List<ushort[]>(_mapPieces);
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.chunks = new List<int>();
		this.mapPieces = new List<ushort[]>();
		this.entityId = _reader.ReadInt32();
		int num = (int)_reader.ReadUInt16();
		for (int i = 0; i < num; i++)
		{
			this.chunks.Add(_reader.ReadInt32());
			ushort[] array = new ushort[256];
			for (int j = 0; j < array.Length; j++)
			{
				array[j] = _reader.ReadUInt16();
			}
			this.mapPieces.Add(array);
		}
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.entityId);
		bool flag = true;
		ushort num = (ushort)this.chunks.Count;
		long position = _writer.BaseStream.Position;
		_writer.Write(num);
		for (int i = 0; i < this.chunks.Count; i++)
		{
			ushort[] array = this.mapPieces[i];
			if (array.Length != 256)
			{
				Log.Warning("Player map data for entityid {0} of invalid size {1}", new object[] { this.entityId, array.Length });
				num -= 1;
				flag = false;
			}
			else
			{
				_writer.Write(this.chunks[i]);
				for (int j = 0; j < array.Length; j++)
				{
					_writer.Write(array[j]);
				}
			}
		}
		if (!flag)
		{
			long position2 = _writer.BaseStream.Position;
			_writer.BaseStream.Position = position;
			_writer.Write(num);
			_writer.BaseStream.Position = position2;
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		EntityPlayer entityPlayer = _world.GetEntity(this.entityId) as EntityPlayer;
		if (entityPlayer != null && entityPlayer.ChunkObserver.mapDatabase != null)
		{
			entityPlayer.ChunkObserver.mapDatabase.Add(this.chunks, this.mapPieces);
		}
	}

	public override int GetLength()
	{
		return 4 + ((this.chunks != null) ? (this.chunks.Count * 8) : 0) + ((this.mapPieces != null) ? (this.mapPieces.Count * 512) : 0);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> chunks;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ushort[]> mapPieces;
}
