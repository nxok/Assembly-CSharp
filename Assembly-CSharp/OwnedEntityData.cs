using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
[Serializable]
public class OwnedEntityData
{
	public EntityCreationData EntityCreationData
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	public OwnedEntityData()
	{
	}

	public OwnedEntityData(Entity _entity)
	{
		this.entityId = _entity.entityId;
		this.classId = _entity.entityClass;
		this.EntityCreationData = new EntityCreationData(_entity, true);
	}

	public OwnedEntityData(int _entityId, int _classId)
	{
		this.entityId = _entityId;
		this.classId = _classId;
	}

	public int Id
	{
		get
		{
			return this.entityId;
		}
	}

	public int ClassId
	{
		get
		{
			return this.classId;
		}
	}

	public Vector3 LastKnownPosition
	{
		get
		{
			return this.lastKnownPosition;
		}
	}

	public void SetLastKnownPosition(Vector3 pos)
	{
		this.lastKnownPosition = pos;
		this.saveFlags |= 1;
	}

	public void ClearLastKnownPostition()
	{
		this.lastKnownPosition = Vector3.zero;
		this.saveFlags = (ushort)((int)this.saveFlags & -2);
	}

	public bool hasLastKnownPosition
	{
		get
		{
			return (this.saveFlags & 1) > 0;
		}
	}

	public void Read(PooledBinaryReader _br)
	{
		this.entityId = _br.ReadInt32();
		this.classId = _br.ReadInt32();
		this.saveFlags = _br.ReadUInt16();
		if ((this.saveFlags & 1) > 0)
		{
			this.lastKnownPosition.x = (float)_br.ReadInt32();
			this.lastKnownPosition.y = (float)_br.ReadInt32();
			this.lastKnownPosition.z = (float)_br.ReadInt32();
		}
	}

	public void Write(PooledBinaryWriter _bw)
	{
		_bw.Write(this.Id);
		_bw.Write(this.ClassId);
		_bw.Write(this.saveFlags);
		if ((this.saveFlags & 1) > 0)
		{
			_bw.Write((int)this.lastKnownPosition.x);
			_bw.Write((int)this.lastKnownPosition.y);
			_bw.Write((int)this.lastKnownPosition.z);
		}
	}

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId = -1;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public int classId = -1;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 lastKnownPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ushort saveFlags;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const ushort cHasLastKnownPosition = 1;
}
