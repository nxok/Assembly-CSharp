using System;
using UnityEngine;

public struct SpawnPosition : IEquatable<SpawnPosition>
{
	public SpawnPosition(bool _bInvalid)
	{
		this.position = Vector3.zero;
		this.heading = 0f;
		this.bInvalid = true;
	}

	public SpawnPosition(Vector3i _blockPos, float _heading)
	{
		this.position = _blockPos.ToVector3() + new Vector3(0.5f, 0f, 0.5f);
		this.heading = _heading;
		this.bInvalid = false;
	}

	public SpawnPosition(Vector3 _position, float _heading)
	{
		this.position = _position;
		this.heading = _heading;
		this.bInvalid = false;
	}

	public Vector3i ToBlockPos()
	{
		return new Vector3i(Utils.Fastfloor(this.position.x), Utils.Fastfloor(this.position.y), Utils.Fastfloor(this.position.z));
	}

	public void Read(IBinaryReaderOrWriter _readerOrWriter, uint _version)
	{
		if (_version > 1U && _version < 3U)
		{
			_readerOrWriter.ReadWrite(0);
		}
		this.position = _readerOrWriter.ReadWrite(Vector3.zero);
		this.heading = _readerOrWriter.ReadWrite(0f);
	}

	public void Read(PooledBinaryReader _br, uint _version)
	{
		if (_version > 1U)
		{
			_br.ReadUInt16();
		}
		this.position = new Vector3(_br.ReadSingle(), _br.ReadSingle(), _br.ReadSingle());
		this.heading = _br.ReadSingle();
	}

	public void Write(PooledBinaryWriter _bw)
	{
		_bw.Write(0);
		_bw.Write(this.position.x);
		_bw.Write(this.position.y);
		_bw.Write(this.position.z);
		_bw.Write(this.heading);
	}

	public bool IsUndef()
	{
		return this.Equals(SpawnPosition.Undef);
	}

	public bool Equals(SpawnPosition _other)
	{
		return this.position.Equals(_other.position) && this.heading == _other.heading && this.bInvalid == _other.bInvalid;
	}

	public override bool Equals(object obj)
	{
		return obj != null && obj is SpawnPosition && this.Equals((SpawnPosition)obj);
	}

	public override int GetHashCode()
	{
		return (((this.position.GetHashCode() * 397) ^ this.heading.GetHashCode()) * 397) ^ this.bInvalid.GetHashCode();
	}

	public override string ToString()
	{
		return string.Format("SpawnPoint {0}/{1}", this.position.ToCultureInvariantString(), this.heading.ToCultureInvariantString("0.0"));
	}

	public static SpawnPosition Undef = new SpawnPosition(true);

	public Vector3 position;

	public float heading;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bInvalid;
}
