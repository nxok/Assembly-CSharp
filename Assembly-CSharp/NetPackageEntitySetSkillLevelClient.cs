using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntitySetSkillLevelClient : NetPackage
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	public NetPackageEntitySetSkillLevelClient Setup(int _entityId, string _skill, int _level)
	{
		this.entityId = _entityId;
		this.skill = _skill;
		this.level = _level;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.entityId = _reader.ReadInt32();
		this.skill = _reader.ReadString();
		this.level = _reader.ReadInt32();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.entityId);
		_writer.Write(this.skill);
		_writer.Write(this.level);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		EntityPlayer entityPlayer = (EntityPlayer)_world.GetEntity(this.entityId);
		if (entityPlayer != null)
		{
			entityPlayer.Progression.GetProgressionValue(this.skill).Level = this.level;
		}
	}

	public override int GetLength()
	{
		return 8;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string skill;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int level;
}
