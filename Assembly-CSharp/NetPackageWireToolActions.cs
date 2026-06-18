using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageWireToolActions : NetPackage
{
	public NetPackageWireToolActions Setup(NetPackageWireToolActions.WireActions _operation, Vector3i _tileEntityPosition, int _entityID)
	{
		this.currentOperation = _operation;
		this.tileEntityPosition = _tileEntityPosition;
		this.entityID = _entityID;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.currentOperation = (NetPackageWireToolActions.WireActions)_br.ReadByte();
		this.tileEntityPosition = StreamUtils.ReadVector3i(_br);
		this.entityID = _br.ReadInt32();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write((byte)this.currentOperation);
		StreamUtils.Write(_bw, this.tileEntityPosition);
		_bw.Write(this.entityID);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		if (!base.ValidEntityIdForSender(this.entityID, false))
		{
			return;
		}
		NetPackageWireToolActions.WireActions wireActions = this.currentOperation;
		if (wireActions != NetPackageWireToolActions.WireActions.AddWire)
		{
			if (wireActions != NetPackageWireToolActions.WireActions.RemoveWire)
			{
				return;
			}
			EntityPlayer entityPlayer = _world.GetEntity(this.entityID) as EntityPlayer;
			if (entityPlayer != null && entityPlayer.RootTransform.FindInChilds(entityPlayer.GetRightHandTransformName(), false) != null)
			{
				ItemActionConnectPower.ConnectPowerData connectPowerData = entityPlayer.inventory.holdingItemData.actionData[1] as ItemActionConnectPower.ConnectPowerData;
				if (connectPowerData != null && connectPowerData.wireNode != null)
				{
					Object.Destroy(connectPowerData.wireNode.gameObject);
					connectPowerData.wireNode = null;
				}
			}
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageWireToolActions>().Setup(this.currentOperation, this.tileEntityPosition, this.entityID), false, -1, this.entityID, -1, null, 192, false);
			}
		}
		else
		{
			Chunk chunk = _world.GetChunkFromWorldPos(this.tileEntityPosition.x, this.tileEntityPosition.y, this.tileEntityPosition.z) as Chunk;
			if (chunk == null)
			{
				return;
			}
			TileEntityPowered tileEntityPowered = _world.GetTileEntity(this.tileEntityPosition) as TileEntityPowered;
			EntityPlayer entityPlayer2 = _world.GetEntity(this.entityID) as EntityPlayer;
			if (tileEntityPowered == null)
			{
				Block block = _world.GetBlock(this.tileEntityPosition).Block;
				if (block is BlockPowered)
				{
					tileEntityPowered = (block as BlockPowered).CreateTileEntity(chunk);
				}
				tileEntityPowered.localChunkPos = World.toBlock(this.tileEntityPosition);
				BlockEntityData blockEntity = chunk.GetBlockEntity(this.tileEntityPosition);
				if (blockEntity != null)
				{
					tileEntityPowered.BlockTransform = blockEntity.transform;
				}
				tileEntityPowered.InitializePowerData();
				chunk.AddTileEntity(tileEntityPowered);
			}
			if (tileEntityPowered != null && entityPlayer2 != null)
			{
				Transform transform = entityPlayer2.RootTransform.FindInChilds(entityPlayer2.GetRightHandTransformName(), false);
				if (transform != null)
				{
					ItemActionConnectPower.ConnectPowerData connectPowerData2 = (ItemActionConnectPower.ConnectPowerData)entityPlayer2.inventory.holdingItemData.actionData[1];
					WireNode component = ((GameObject)Object.Instantiate(Resources.Load("Prefabs/WireNode"))).GetComponent<WireNode>();
					component.LocalPosition = tileEntityPowered.ToWorldPos().ToVector3() - Origin.position;
					component.localOffset = tileEntityPowered.GetWireOffset();
					WireNode wireNode = component;
					wireNode.localOffset.x = wireNode.localOffset.x + 0.5f;
					WireNode wireNode2 = component;
					wireNode2.localOffset.y = wireNode2.localOffset.y + 0.5f;
					WireNode wireNode3 = component;
					wireNode3.localOffset.z = wireNode3.localOffset.z + 0.5f;
					component.Source = transform.gameObject;
					component.TogglePulse(false);
					component.SetPulseSpeed(360f);
					connectPowerData2.wireNode = component;
				}
			}
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageWireToolActions>().Setup(this.currentOperation, this.tileEntityPosition, this.entityID), false, -1, this.entityID, -1, null, 192, false);
				return;
			}
		}
	}

	public override int GetLength()
	{
		return 12;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i tileEntityPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public NetPackageWireToolActions.WireActions currentOperation;

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityID;

	public enum WireActions
	{
		AddWire,
		RemoveWire
	}
}
