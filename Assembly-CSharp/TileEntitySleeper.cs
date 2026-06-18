using System;

public class TileEntitySleeper : TileEntity
{
	public TileEntitySleeper(Chunk _chunk)
		: base(_chunk)
	{
		this.priorityMultiplier = 1f;
		this.sightAngle = -1;
		this.sightRange = -1;
		this.hearingPercent = 1f;
	}

	public override TileEntity Clone()
	{
		return new TileEntitySleeper(this.chunk)
		{
			localChunkPos = base.localChunkPos,
			priorityMultiplier = this.priorityMultiplier,
			sightAngle = this.sightAngle,
			sightRange = this.sightRange,
			hearingPercent = this.hearingPercent
		};
	}

	public override void CopyFrom(TileEntity _other)
	{
		TileEntitySleeper tileEntitySleeper = (TileEntitySleeper)_other;
		this.priorityMultiplier = tileEntitySleeper.priorityMultiplier;
		this.sightAngle = tileEntitySleeper.sightAngle;
		this.sightRange = tileEntitySleeper.sightRange;
		this.hearingPercent = tileEntitySleeper.hearingPercent;
	}

	public override TileEntityType GetTileEntityType()
	{
		return TileEntityType.Sleeper;
	}

	public void SetPriorityMultiplier(float _priorityMultiplier)
	{
		this.priorityMultiplier = _priorityMultiplier;
		this.setModified();
	}

	public float GetPriorityMultiplier()
	{
		return this.priorityMultiplier;
	}

	public void SetSightAngle(int _sightAngle)
	{
		this.sightAngle = _sightAngle;
		this.setModified();
	}

	public int GetSightAngle()
	{
		return this.sightAngle;
	}

	public void SetSightRange(int _sightRange)
	{
		this.sightRange = _sightRange;
		this.setModified();
	}

	public int GetSightRange()
	{
		return this.sightRange;
	}

	public void SetHearingPercent(float _hearingPercent)
	{
		this.hearingPercent = _hearingPercent;
		this.setModified();
	}

	public float GetHearingPercent()
	{
		return this.hearingPercent;
	}

	public override void read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode)
	{
		base.read(_br, _eStreamMode);
		if (_eStreamMode == TileEntity.StreamModeRead.Persistency)
		{
			if (base.UseLocalVersioning())
			{
				_br.ReadUInt16();
			}
			else
			{
				base.GetLegacyForkVersion();
			}
		}
		this.priorityMultiplier = _br.ReadSingle();
		this.sightRange = (int)_br.ReadInt16();
		this.hearingPercent = _br.ReadSingle();
		this.sightAngle = (int)_br.ReadInt16();
	}

	public override void write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
	{
		base.write(_bw, _eStreamMode);
		if (_eStreamMode == TileEntity.StreamModeWrite.Persistency)
		{
			_bw.Write(18);
		}
		_bw.Write(this.priorityMultiplier);
		_bw.Write((short)this.sightRange);
		_bw.Write(this.hearingPercent);
		_bw.Write((short)this.sightAngle);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public new const int Version = 18;

	[PublicizedFrom(EAccessModifier.Private)]
	public float priorityMultiplier;

	[PublicizedFrom(EAccessModifier.Private)]
	public int sightAngle;

	[PublicizedFrom(EAccessModifier.Private)]
	public int sightRange;

	[PublicizedFrom(EAccessModifier.Private)]
	public float hearingPercent;
}
