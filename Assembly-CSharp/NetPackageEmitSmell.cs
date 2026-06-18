using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEmitSmell : NetPackage
{
	public NetPackageEmitSmell Setup(int entityId, string smellName)
	{
		this.EntityId = entityId;
		this.SmellName = smellName;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.EntityId = _reader.ReadInt32();
		this.SmellName = _reader.ReadString();
		if (string.IsNullOrEmpty(this.SmellName))
		{
			this.SmellName = null;
		}
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.EntityId);
		_writer.Write((this.SmellName != null) ? this.SmellName : "");
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
	}

	public override int GetLength()
	{
		return 10;
	}

	public int EntityId;

	public string SmellName;
}
