using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageHordeEvent : NetPackage
{
	public NetPackageHordeEvent Setup(AIDirector.HordeEvent _event, Vector3 pos, float maxDist)
	{
		this.m_event = _event;
		this.m_pos = pos;
		this.m_maxDist = maxDist;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		int num = (int)_reader.ReadByte();
		this.m_event = (AIDirector.HordeEvent)num;
		this.m_pos[0] = _reader.ReadSingle();
		this.m_pos[1] = _reader.ReadSingle();
		this.m_pos[2] = _reader.ReadSingle();
		this.m_maxDist = _reader.ReadSingle();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write((byte)this.m_event);
		_writer.Write(this.m_pos[0]);
		_writer.Write(this.m_pos[1]);
		_writer.Write(this.m_pos[2]);
		_writer.Write(this.m_maxDist);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		EntityPlayerLocal primaryPlayer = _world.GetPrimaryPlayer();
		if (primaryPlayer == null)
		{
			return;
		}
		if ((primaryPlayer.GetPosition() - this.m_pos).sqrMagnitude <= this.m_maxDist * this.m_maxDist)
		{
			primaryPlayer.HandleHordeEvent(this.m_event);
		}
	}

	public override int GetLength()
	{
		return 21;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public AIDirector.HordeEvent m_event;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 m_pos;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_maxDist;
}
