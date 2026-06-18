using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockElectricWire : BlockPowered
{
	public BlockElectricWire()
	{
		this.HasTileEntity = true;
	}

	public override void Init()
	{
		base.Init();
		if (base.Properties.Values.ContainsKey("BrokenPercentage"))
		{
			this.brokenPercentage = Mathf.Clamp01(StringParsers.ParseFloat(base.Properties.Values["BrokenPercentage"], 0, -1, NumberStyles.Any));
			return;
		}
		this.brokenPercentage = 0.25f;
	}

	public override TileEntityPowered CreateTileEntity(Chunk chunk)
	{
		return new TileEntityPoweredMeleeTrap(chunk)
		{
			PowerItemType = PowerItem.PowerItemTypes.ElectricWireRelay
		};
	}

	public override void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue, PlatformUserIdentifierAbs _addedByPlayer)
	{
		base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue, _addedByPlayer);
		if (!(_world.GetTileEntity(_blockPos) is TileEntityPoweredMeleeTrap))
		{
			TileEntityPoweredMeleeTrap tileEntityPoweredMeleeTrap = this.CreateTileEntity(_chunk) as TileEntityPoweredMeleeTrap;
			tileEntityPoweredMeleeTrap.SetDisableModifiedCheck(true);
			tileEntityPoweredMeleeTrap.localChunkPos = World.toBlock(_blockPos);
			if (_addedByPlayer != null)
			{
				tileEntityPoweredMeleeTrap.SetOwner(_addedByPlayer);
			}
			tileEntityPoweredMeleeTrap.InitializePowerData();
			_chunk.AddTileEntity(tileEntityPoweredMeleeTrap);
			tileEntityPoweredMeleeTrap.SetDisableModifiedCheck(false);
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				tileEntityPoweredMeleeTrap.SetModified();
			}
		}
	}

	public override int OnBlockDamaged(WorldBase _world, BlockValueRef _bvRef, BlockValue _blockValue, int _damagePoints, int _entityIdThatDamaged, ItemActionAttack.AttackHitInfo _attackHitInfo, bool _bUseHarvestTool, bool _bBypassMaxDamage, int _recDepth = 0)
	{
		if ((_blockValue.meta & 2) > 0 && 1f - (float)_blockValue.damage / (float)_blockValue.Block.MaxDamage > this.brokenPercentage)
		{
			if (this.buffActions == null && base.Properties.Values.ContainsKey("Buff"))
			{
				string[] array = base.Properties.Values["Buff"].Split(',', StringSplitOptions.None);
				for (int i = 0; i < array.Length; i++)
				{
					this.buffActions.Add(array[i]);
				}
			}
			if (this.buffActions != null)
			{
				TileEntityPoweredMeleeTrap tileEntityPoweredMeleeTrap = _world.GetTileEntity(_bvRef) as TileEntityPoweredMeleeTrap;
				if (tileEntityPoweredMeleeTrap != null && tileEntityPoweredMeleeTrap.IsPowered)
				{
					EntityAlive entityAlive = _world.GetEntity(_entityIdThatDamaged) as EntityAlive;
					if (entityAlive != null)
					{
						ItemAction itemAction = entityAlive.inventory.holdingItemData.item.Actions[0];
						if (entityAlive != null)
						{
							if (itemAction is ItemActionRanged)
							{
								ItemActionRanged itemActionRanged = itemAction as ItemActionRanged;
								if (itemActionRanged == null || (itemActionRanged.Hitmask & 128) == 0)
								{
									goto IL_014E;
								}
							}
							for (int j = 0; j < this.buffActions.Count; j++)
							{
								entityAlive.Buffs.AddBuff(this.buffActions[j], tileEntityPoweredMeleeTrap.OwnerEntityID, true, false, -1f);
							}
						}
					}
				}
			}
		}
		IL_014E:
		return base.OnBlockDamaged(_world, _bvRef, _blockValue, _damagePoints, _entityIdThatDamaged, _attackHitInfo, _bUseHarvestTool, _bBypassMaxDamage, _recDepth);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> buffActions;

	[PublicizedFrom(EAccessModifier.Private)]
	public float brokenPercentage;
}
