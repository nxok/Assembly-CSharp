using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageDropItemsContainer : NetPackage
{
	public NetPackageDropItemsContainer Setup(int _droppedByID, string _containerEntity, Vector3 _worldPos, ItemStack[] _items)
	{
		this.droppedByID = _droppedByID;
		this.worldPos = _worldPos;
		this.items = _items;
		this.containerEntity = _containerEntity;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.droppedByID = _br.ReadInt32();
		this.containerEntity = _br.ReadString();
		this.worldPos = StreamUtils.ReadVector3(_br);
		this.items = GameUtils.ReadItemStack(_br);
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.droppedByID);
		_bw.Write(this.containerEntity);
		StreamUtils.Write(_bw, this.worldPos);
		_bw.Write((ushort)this.items.Length);
		for (int i = 0; i < this.items.Length; i++)
		{
			this.items[i].Write(_bw);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		_world.GetGameManager().DropContentInLootContainerServer(this.droppedByID, this.containerEntity, this.worldPos, this.items, false, null);
	}

	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToServer;
		}
	}

	public override int GetLength()
	{
		return 16;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int droppedByID;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 worldPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public string containerEntity = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] items;
}
