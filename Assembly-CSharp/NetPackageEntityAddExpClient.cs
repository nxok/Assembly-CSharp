using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityAddExpClient : NetPackage
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	public NetPackageEntityAddExpClient Setup(int _entityId, int _xp, Progression.XPTypes _xpType, ItemValue _usedItem)
	{
		this.entityId = _entityId;
		this.xp = _xp;
		this.xpType = (int)_xpType;
		this.includeItem = _usedItem != null;
		this.usedItem = _usedItem;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.entityId = _reader.ReadInt32();
		this.xp = _reader.ReadInt32();
		this.xpType = (int)_reader.ReadInt16();
		this.includeItem = _reader.ReadBoolean();
		if (this.includeItem)
		{
			this.usedItem = ItemValue.ReadOrNull(_reader);
		}
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.entityId);
		_writer.Write(this.xp);
		_writer.Write((short)this.xpType);
		_writer.Write(this.usedItem != null);
		if (this.usedItem != null)
		{
			this.usedItem.Write(_writer);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		EntityAlive entityAlive = (EntityAlive)_world.GetEntity(this.entityId);
		if (entityAlive != null)
		{
			string text = "_xpOther";
			if (this.xpType == 0)
			{
				text = "_xpFromKill";
			}
			entityAlive.Progression.AddLevelExp(this.xp, text, (Progression.XPTypes)this.xpType, true, true, this.entityId, this.usedItem);
		}
	}

	public override int GetLength()
	{
		return 8;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int xp;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int xpType;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool includeItem;

	[PublicizedFrom(EAccessModifier.Protected)]
	public ItemValue usedItem;
}
