using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageWorldAreas : NetPackage
{
	public NetPackageWorldAreas Setup(List<TraderArea> _list)
	{
		this.traders = _list;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		_reader.ReadByte();
		int num = (int)_reader.ReadInt16();
		this.traders = new List<TraderArea>();
		for (int i = 0; i < num; i++)
		{
			TraderArea traderArea = TraderArea.Read(_reader);
			this.traders.Add(traderArea);
		}
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(1);
		_writer.Write((short)this.traders.Count);
		for (int i = 0; i < this.traders.Count; i++)
		{
			this.traders[i].Write(_writer);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		World.SetWorldAreas(this.traders);
	}

	public override int GetLength()
	{
		int num = 2;
		for (int i = 0; i < this.traders.Count; i++)
		{
			num += this.traders[i].GetReadWriteSize();
		}
		return num;
	}

	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const byte cVersion = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<TraderArea> traders;
}
