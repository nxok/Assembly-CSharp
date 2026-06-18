using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageConfigFile : NetPackage
{
	public override bool Compress
	{
		get
		{
			return true;
		}
	}

	public NetPackageConfigFile Setup(string _name, byte[] _data)
	{
		this.name = _name;
		this.data = _data;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.name = _reader.ReadString();
		int num = _reader.ReadInt32();
		this.data = ((num >= 0) ? _reader.ReadBytes(num) : null);
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.name);
		if (this.data != null)
		{
			_writer.Write(this.data.Length);
			_writer.Write(this.data);
			return;
		}
		_writer.Write(-1);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		WorldStaticData.ReceivedConfigFile(this.name, this.data);
	}

	public override int GetLength()
	{
		int num = this.name.Length * 2;
		byte[] array = this.data;
		return num + ((array != null) ? array.Length : 0);
	}

	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string name;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] data;
}
