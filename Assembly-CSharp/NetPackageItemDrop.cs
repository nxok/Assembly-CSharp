using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageItemDrop : NetPackage
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToServer;
		}
	}

	public NetPackageItemDrop Setup(ItemStack _itemStack, Vector3 _dropPos, Vector3 _initialMotion, Vector3 _randomPosAdd, float _lifetime, int _entityId, bool _bDropPosIsRelativeToHead, int _clientInstanceId)
	{
		this.itemStack = _itemStack.Clone();
		this.dropPos = _dropPos;
		this.initialMotion = _initialMotion;
		this.randomPosAdd = _randomPosAdd;
		this.lifetime = _lifetime;
		this.entityId = _entityId;
		this.clientInstanceId = _clientInstanceId;
		this.bDropPosIsRelativeToHead = _bDropPosIsRelativeToHead;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.itemStack = new ItemStack();
		this.itemStack.Read(_br);
		this.dropPos = StreamUtils.ReadVector3(_br);
		this.initialMotion = StreamUtils.ReadVector3(_br);
		this.randomPosAdd = StreamUtils.ReadVector3(_br);
		this.lifetime = _br.ReadSingle();
		this.entityId = _br.ReadInt32();
		this.clientInstanceId = _br.ReadInt32();
		this.bDropPosIsRelativeToHead = _br.ReadBoolean();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		this.itemStack.Write(_bw);
		StreamUtils.Write(_bw, this.dropPos);
		StreamUtils.Write(_bw, this.initialMotion);
		StreamUtils.Write(_bw, this.randomPosAdd);
		_bw.Write(this.lifetime);
		_bw.Write(this.entityId);
		_bw.Write(this.clientInstanceId);
		_bw.Write(this.bDropPosIsRelativeToHead);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		_world.GetGameManager().ItemDropServer(this.itemStack, this.dropPos, this.randomPosAdd, this.initialMotion, this.entityId, this.lifetime, this.bDropPosIsRelativeToHead, this.clientInstanceId);
	}

	public override int GetLength()
	{
		return 52;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack itemStack;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 dropPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 initialMotion;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 randomPosAdd;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lifetime;

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int clientInstanceId;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bDropPosIsRelativeToHead;
}
