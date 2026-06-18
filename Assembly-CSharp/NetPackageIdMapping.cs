using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageIdMapping : NetPackage
{
	public override bool Compress
	{
		get
		{
			return true;
		}
	}

	public NetPackageIdMapping Setup(string _name, byte[] _data)
	{
		this.name = _name;
		this.data = _data;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.name = _reader.ReadString();
		int num = _reader.ReadInt32();
		this.data = _reader.ReadBytes(num);
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.name);
		_writer.Write(this.data.Length);
		_writer.Write(this.data);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		GameManager.Instance.IdMappingReceived(this.name, this.data);
	}

	public override int GetLength()
	{
		return this.name.Length * 2 + this.data.Length;
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
