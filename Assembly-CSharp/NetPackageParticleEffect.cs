using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageParticleEffect : NetPackage
{
	public NetPackageParticleEffect Setup(ParticleEffect _pe, int _entityThatCausedIt, bool _forceCreation = false, bool _worldSpawn = false)
	{
		this.pe = _pe;
		this.entityThatCausedIt = _entityThatCausedIt;
		this.worldSpawn = _worldSpawn;
		this.forceCreation = _forceCreation;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.pe = new ParticleEffect();
		this.pe.Read(_br);
		this.entityThatCausedIt = _br.ReadInt32();
		this.forceCreation = _br.ReadBoolean();
		this.worldSpawn = _br.ReadBoolean();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		this.pe.Write(_bw);
		_bw.Write(this.entityThatCausedIt);
		_bw.Write(this.forceCreation);
		_bw.Write(this.worldSpawn);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		if (!_world.IsRemote())
		{
			_world.GetGameManager().SpawnParticleEffectServer(this.pe, this.entityThatCausedIt, this.forceCreation, this.worldSpawn);
			return;
		}
		_world.GetGameManager().SpawnParticleEffectClient(this.pe, this.entityThatCausedIt, this.forceCreation, this.worldSpawn);
	}

	public override int GetLength()
	{
		return 20;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ParticleEffect pe;

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityThatCausedIt;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool forceCreation;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool worldSpawn;
}
