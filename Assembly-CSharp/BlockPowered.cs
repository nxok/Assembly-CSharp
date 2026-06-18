using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockPowered : Block
{
	public int RequiredPower
	{
		get
		{
			return this.requiredPower;
		}
	}

	public BlockPowered()
	{
		this.HasTileEntity = true;
	}

	public override void Init()
	{
		base.Init();
		if (base.Properties.Values.ContainsKey("RequiredPower"))
		{
			this.requiredPower = int.Parse(base.Properties.Values["RequiredPower"]);
		}
		else
		{
			this.requiredPower = 5;
		}
		if (base.Properties.Values.ContainsKey("PoweredType"))
		{
			this.poweredType = base.Properties.Values["PoweredType"];
		}
		if (base.Properties.Values.ContainsKey("TakeDelay"))
		{
			this.TakeDelay = StringParsers.ParseFloat(base.Properties.Values["TakeDelay"], 0, -1, NumberStyles.Any);
			return;
		}
		this.TakeDelay = 2f;
	}

	public override void OnBlockLoaded(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockLoaded(_world, _blockPos, _blockValue);
		if (_blockValue.ischild)
		{
			return;
		}
		ChunkCluster chunkCache = _world.ChunkCache;
		if (chunkCache == null)
		{
			return;
		}
		Chunk chunk = (Chunk)chunkCache.GetChunkFromWorldPos(_blockPos);
		if (chunk == null)
		{
			return;
		}
		chunk.AddEntityBlockStub(new BlockEntityData(_blockValue, _blockPos)
		{
			bNeedsTemperature = true
		});
	}

	public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, BlockEntityData _ebcd)
	{
		base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _blockValue, _ebcd);
		if (_blockValue.ischild)
		{
			return;
		}
		TileEntityPowered tileEntityPowered = _world.GetTileEntity(_blockPos) as TileEntityPowered;
		if (tileEntityPowered != null)
		{
			tileEntityPowered.BlockTransform = _ebcd.transform;
			GameManager.Instance.StartCoroutine(this.drawWiresLater(tileEntityPowered));
			if (tileEntityPowered.GetParent().y != -9999)
			{
				IPowered powered = _world.GetTileEntity(tileEntityPowered.GetParent()) as IPowered;
				if (powered != null)
				{
					GameManager.Instance.StartCoroutine(this.drawWiresLater(powered));
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator drawWiresLater(IPowered powered)
	{
		yield return new WaitForSeconds(0.5f);
		powered.DrawWires();
		yield break;
	}

	public override void OnBlockRemoved(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockRemoved(world, _chunk, _blockPos, _blockValue);
		if (_blockValue.ischild)
		{
			return;
		}
		TileEntityPowered tileEntityPowered = _chunk.GetTileEntity(World.toBlock(_blockPos)) as TileEntityPowered;
		if (tileEntityPowered != null)
		{
			if (!GameManager.IsDedicatedServer)
			{
				EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
				if (primaryPlayer.inventory.holdingItem.Actions[1] is ItemActionConnectPower)
				{
					(primaryPlayer.inventory.holdingItem.Actions[1] as ItemActionConnectPower).CheckForWireRemoveNeeded(primaryPlayer, _blockPos);
				}
			}
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				PowerManager.Instance.RemovePowerNode(tileEntityPowered.GetPowerItem());
			}
			if (tileEntityPowered.GetParent().y != -9999)
			{
				IPowered powered = world.GetTileEntity(tileEntityPowered.GetParent()) as IPowered;
				if (powered != null && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
				{
					powered.SendWireData();
				}
			}
			tileEntityPowered.RemoveWires();
		}
		_chunk.RemoveTileEntityAt<TileEntityPowered>((World)world, World.toBlock(_blockPos));
	}

	public virtual TileEntityPowered CreateTileEntity(Chunk chunk)
	{
		return new TileEntityPoweredBlock(chunk);
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return true;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		bool flag = _world.IsMyLandProtectedBlock(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer(), false);
		this.cmds[0].enabled = flag && this.TakeDelay > 0f;
		return this.cmds;
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (!_world.IsMyLandProtectedBlock(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer(), false) || this.TakeDelay <= 0f)
		{
			return "";
		}
		Block block = _blockValue.Block;
		return string.Format(Localization.Get("pickupPrompt", false, null), block.GetLocalizedBlockName());
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (_blockValue.ischild)
		{
			Vector3i parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
			BlockValue block = _world.GetBlock(parentPos);
			return this.OnBlockActivated(_commandName, _world, parentPos, block, _player);
		}
		if (_commandName == "take")
		{
			base.takeItemWithTimer(_blockPos, _blockValue, _player, this.TakeDelay);
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public int requiredPower = -1;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string poweredType = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public float TakeDelay = 2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[]
	{
		new BlockActivationCommand("take", "hand", false, false, null)
	};
}
