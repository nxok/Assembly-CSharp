using System;
using System.Collections;
using System.Collections.Generic;
using GUI_2;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionReplaceBlock : ItemActionRanged
{
	public override ItemActionData CreateModifierData(ItemInventoryData _invData, int _indexInEntityOfAction)
	{
		return new ItemActionReplaceBlock.ItemActionReplaceBlockData(_invData, _indexInEntityOfAction);
	}

	public override void ReadFrom(DynamicProperties _props)
	{
		base.ReadFrom(_props);
		if (_props.Values.ContainsKey("CopyBlock"))
		{
			this.bCopyBlock = StringParsers.ParseBool(_props.Values["CopyBlock"], 0, -1, true);
		}
	}

	public override bool ConsumeScrollWheel(ItemActionData _actionData, float _scrollWheelInput, PlayerActionsLocal _playerInput)
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool checkAmmo(ItemActionData _actionData)
	{
		return true;
	}

	public override bool IsAmmoUsableUnderwater(EntityAlive holdingEntity)
	{
		return true;
	}

	public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
	{
		AnimationDelayData.AnimationDelays animationDelays = AnimationDelayData.AnimationDelay[_actionData.invData.item.HoldType.Value];
		this.rayCastDelay = (((double)_actionData.invData.holdingEntity.speedForward > 0.009) ? animationDelays.RayCastMoving : animationDelays.RayCast);
		base.ExecuteAction(_actionData, _bReleased);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override Vector3 fireShot(int _shotIdx, ItemActionRanged.ItemActionDataRanged _actionData, ref bool hitEntity)
	{
		hitEntity = true;
		GameManager.Instance.StartCoroutine(this.fireShotLater(_shotIdx, _actionData));
		return Vector3.zero;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool checkBlockCanBeChanged(World _world, Vector3i _blockPos, int _entityId)
	{
		PersistentPlayerData playerDataFromEntityID = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(_entityId);
		return _world.CanPlaceBlockAt(_blockPos, playerDataFromEntityID, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator fireShotLater(int _shotIdx, ItemActionRanged.ItemActionDataRanged _actionData)
	{
		yield return new WaitForSeconds(this.rayCastDelay);
		EntityPlayerLocal entityPlayerLocal = (EntityPlayerLocal)_actionData.invData.holdingEntity;
		Vector3i vector3i;
		BlockValue blockValue;
		WorldRayHitInfo worldRayHitInfo;
		if (!this.GetHitBlock(_actionData, out vector3i, out blockValue, out worldRayHitInfo) || worldRayHitInfo == null || !worldRayHitInfo.bHitValid)
		{
			yield break;
		}
		ChunkCluster chunkCache = GameManager.Instance.World.ChunkCache;
		if (chunkCache == null)
		{
			yield break;
		}
		ItemActionReplaceBlock.ItemActionReplaceBlockData itemActionReplaceBlockData = (ItemActionReplaceBlock.ItemActionReplaceBlockData)_actionData;
		if (this.bCopyBlock)
		{
			int num = 1 - _actionData.indexInEntityOfAction;
			if (_actionData.invData.actionData[num] != null)
			{
				ItemActionReplaceBlock.ItemActionReplaceBlockData itemActionReplaceBlockData2 = (ItemActionReplaceBlock.ItemActionReplaceBlockData)_actionData.invData.actionData[num];
				itemActionReplaceBlockData2.Block = new BlockValue?(blockValue);
				itemActionReplaceBlockData2.PaintTextures = chunkCache.GetTextureFullArray(vector3i);
				itemActionReplaceBlockData2.Density = chunkCache.GetDensity(vector3i);
				this.isHUDDirty = true;
			}
			yield break;
		}
		if (itemActionReplaceBlockData.ReplaceBlockClass == null)
		{
			GameManager.ShowTooltip(entityPlayerLocal, Localization.Get("xuiReplaceBlockNoBlockCopied", false, null), false, false, 0f);
			yield break;
		}
		if (!this.checkBlockCanBeChanged(GameManager.Instance.World, vector3i, entityPlayerLocal.entityId))
		{
			yield break;
		}
		if (itemActionReplaceBlockData.ReplaceMode == ItemActionReplaceBlock.EnumReplaceMode.SingleBlock)
		{
			BlockToolSelection.Instance.BeginUndo();
			GameManager.Instance.SetBlocksRPC(new List<BlockChangeInfo> { this.replaceSingleBlock(chunkCache, vector3i, itemActionReplaceBlockData) }, null);
			BlockToolSelection.Instance.EndUndo(false);
		}
		else
		{
			BlockToolSelection blockToolSelection = GameManager.Instance.GetActiveBlockTool() as BlockToolSelection;
			Vector3i vector3i2;
			Vector3i vector3i3;
			if (blockToolSelection == null || !blockToolSelection.SelectionActive)
			{
				if (PrefabEditModeManager.Instance == null || !PrefabEditModeManager.Instance.IsActive())
				{
					GameManager.ShowTooltip(entityPlayerLocal, Localization.Get("xuiReplaceBlockRequiresSelection", false, null), false, false, 0f);
					yield break;
				}
				PrefabEditModeManager.Instance.UpdateMinMax();
				vector3i2 = PrefabEditModeManager.Instance.minPos;
				vector3i3 = PrefabEditModeManager.Instance.maxPos;
			}
			else
			{
				vector3i2 = blockToolSelection.SelectionStart;
				vector3i3 = blockToolSelection.SelectionEnd;
			}
			BlockToolSelection.Instance.BeginUndo();
			this.replace(chunkCache, itemActionReplaceBlockData, blockValue, vector3i2, vector3i3);
			BlockToolSelection.Instance.EndUndo(false);
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockChangeInfo replaceSingleBlock(ChunkCluster _cc, Vector3i _blockPos, ItemActionReplaceBlock.ItemActionReplaceBlockData _actionData)
	{
		BlockValue block = _cc.GetBlock(_blockPos);
		BlockChangeInfo blockChangeInfo = new BlockChangeInfo
		{
			blockValueRef = _blockPos,
			bChangeBlockValue = true
		};
		if (_actionData.ReplacePaintMode == ItemActionReplaceBlock.EnumReplacePaintMode.ReplaceWithAirBlocks)
		{
			blockChangeInfo.blockValue = BlockValue.Air;
			blockChangeInfo.bChangeDensity = true;
			blockChangeInfo.density = MarchingCubes.DensityAir;
		}
		else
		{
			blockChangeInfo.blockValue = _actionData.Block.Value;
			blockChangeInfo.blockValue.rotation = block.rotation;
			Block block2 = block.Block;
			Block replaceBlockClass = _actionData.ReplaceBlockClass;
			if (block2.shape.IsTerrain() != replaceBlockClass.shape.IsTerrain())
			{
				blockChangeInfo.bChangeDensity = true;
				blockChangeInfo.density = _actionData.Density;
			}
			blockChangeInfo.bChangeTexture = true;
			switch (_actionData.ReplacePaintMode)
			{
			case ItemActionReplaceBlock.EnumReplacePaintMode.KeepCurrentPaint:
				blockChangeInfo.textureFull = _cc.GetTextureFullArray(_blockPos);
				break;
			case ItemActionReplaceBlock.EnumReplacePaintMode.RemoveCurrentPaint:
				blockChangeInfo.textureFull.Fill(0L);
				break;
			case ItemActionReplaceBlock.EnumReplacePaintMode.UseNewPaint:
				blockChangeInfo.textureFull = _actionData.PaintTextures;
				break;
			}
		}
		return blockChangeInfo;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void replace(ChunkCluster _cc, ItemActionReplaceBlock.ItemActionReplaceBlockData _actionData, BlockValue _srcBlock, Vector3i _startPos, Vector3i _endPos)
	{
		List<BlockChangeInfo> list = new List<BlockChangeInfo>();
		Vector3i.SortBoundingBoxEdges(ref _startPos, ref _endPos);
		for (int i = _startPos.x; i <= _endPos.x; i++)
		{
			for (int j = _startPos.z; j <= _endPos.z; j++)
			{
				for (int k = _startPos.y; k <= _endPos.y; k++)
				{
					Vector3i vector3i = new Vector3i(i, k, j);
					BlockValue block = _cc.GetBlock(vector3i);
					if (!block.ischild && block.type == _srcBlock.type)
					{
						list.Add(this.replaceSingleBlock(_cc, vector3i, _actionData));
					}
				}
			}
		}
		GameManager.Instance.SetBlocksRPC(list, null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool GetHitBlock(ItemActionRanged.ItemActionDataRanged _actionData, out Vector3i _blockPos, out BlockValue _bv, out WorldRayHitInfo _hitInfo)
	{
		_bv = BlockValue.Air;
		_hitInfo = null;
		_blockPos = Vector3i.zero;
		_hitInfo = this.GetExecuteActionTarget(_actionData);
		if (_hitInfo == null || !_hitInfo.bHitValid || _hitInfo.tag == null || !GameUtils.IsBlockOrTerrain(_hitInfo.tag))
		{
			return false;
		}
		ChunkCluster chunkCache = GameManager.Instance.World.ChunkCache;
		if (chunkCache == null)
		{
			return false;
		}
		_bv = _hitInfo.hit.blockValue;
		_blockPos = _hitInfo.hit.blockPos;
		Block block = _bv.Block;
		if (_bv.ischild)
		{
			_blockPos = block.multiBlockPos.GetParentPos(_blockPos, _bv);
			_bv = chunkCache.GetBlock(_blockPos);
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void onHoldingEntityFired(ItemActionData _actionData)
	{
		if (_actionData.indexInEntityOfAction == 0)
		{
			_actionData.invData.holdingEntity.RightArmAnimationUse = true;
			return;
		}
		_actionData.invData.holdingEntity.RightArmAnimationAttack = true;
	}

	public override EnumCameraShake GetCameraShakeType(ItemActionData _actionData)
	{
		return EnumCameraShake.None;
	}

	public override bool IsEditingTool()
	{
		return true;
	}

	public override string GetStat(ItemActionData _data)
	{
		Block replaceBlockClass = ((ItemActionReplaceBlock.ItemActionReplaceBlockData)_data).ReplaceBlockClass;
		if (replaceBlockClass == null)
		{
			return "No Block";
		}
		return replaceBlockClass.GetLocalizedBlockName();
	}

	public override bool IsStatChanged()
	{
		bool flag = this.isHUDDirty;
		this.isHUDDirty = false;
		return flag;
	}

	public override bool HasRadial()
	{
		return true;
	}

	public override void SetupRadial(XUiC_Radial _xuiRadialWindow, EntityPlayerLocal _epl)
	{
		ItemActionReplaceBlock.ItemActionReplaceBlockData itemActionReplaceBlockData = (ItemActionReplaceBlock.ItemActionReplaceBlockData)_epl.inventory.holdingItemData.actionData[1];
		_xuiRadialWindow.ResetRadialEntries();
		_xuiRadialWindow.CreateRadialEntry(0, "ui_game_symbol_paint_brush", "UIAtlas", "", Localization.Get("xuiReplaceBlockSingle", false, null), itemActionReplaceBlockData.ReplaceMode == ItemActionReplaceBlock.EnumReplaceMode.SingleBlock);
		_xuiRadialWindow.CreateRadialEntry(1, "ui_game_symbol_paint_spraygun", "UIAtlas", "", Localization.Get("xuiReplaceBlockMulti", false, null), itemActionReplaceBlockData.ReplaceMode == ItemActionReplaceBlock.EnumReplaceMode.AllIdenticalBlocks);
		_xuiRadialWindow.CreateRadialEntry(2, "ui_game_symbol_brick", "UIAtlas", "", Localization.Get("xuiReplaceBlockKeepPaint", false, null), itemActionReplaceBlockData.ReplacePaintMode == ItemActionReplaceBlock.EnumReplacePaintMode.KeepCurrentPaint);
		_xuiRadialWindow.CreateRadialEntry(3, "ui_game_symbol_destruction", "UIAtlas", "", Localization.Get("xuiReplaceBlockRemovePaint", false, null), itemActionReplaceBlockData.ReplacePaintMode == ItemActionReplaceBlock.EnumReplacePaintMode.RemoveCurrentPaint);
		_xuiRadialWindow.CreateRadialEntry(4, "ui_game_symbol_paint_copy_block", "UIAtlas", "", Localization.Get("xuiReplaceBlockUseNewPaint", false, null), itemActionReplaceBlockData.ReplacePaintMode == ItemActionReplaceBlock.EnumReplacePaintMode.UseNewPaint);
		_xuiRadialWindow.CreateRadialEntry(5, "ui_game_symbol_x", "UIAtlas", "", Localization.Get("xuiReplaceBlockPlaceAir", false, null), itemActionReplaceBlockData.ReplacePaintMode == ItemActionReplaceBlock.EnumReplacePaintMode.ReplaceWithAirBlocks);
		_xuiRadialWindow.SetCommonData(UIUtils.GetButtonIconForAction(_epl.playerInput.Activate), new XUiC_Radial.CommandHandlerDelegate(this.handleRadialCommand), new XUiC_Radial.RadialContextHoldingSlotIndex(_epl.inventory.holdingItemIdx), -1, false, new XUiC_Radial.RadialStillValidDelegate(XUiC_Radial.RadialValidSameHoldingSlotIndex));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public new void handleRadialCommand(XUiC_Radial _sender, int _commandIndex, XUiC_Radial.RadialContextAbs _context)
	{
		EntityPlayerLocal entityPlayer = _sender.xui.playerUI.entityPlayer;
		ItemClass holdingItem = entityPlayer.inventory.holdingItem;
		ItemInventoryData holdingItemData = entityPlayer.inventory.holdingItemData;
		ItemActionReplaceBlock itemActionReplaceBlock = (ItemActionReplaceBlock)holdingItem.Actions[0];
		ItemActionReplaceBlock itemActionReplaceBlock2 = (ItemActionReplaceBlock)holdingItem.Actions[1];
		ItemActionReplaceBlock.ItemActionReplaceBlockData itemActionReplaceBlockData = (ItemActionReplaceBlock.ItemActionReplaceBlockData)holdingItemData.actionData[0];
		ItemActionReplaceBlock.ItemActionReplaceBlockData itemActionReplaceBlockData2 = (ItemActionReplaceBlock.ItemActionReplaceBlockData)holdingItemData.actionData[1];
		switch (_commandIndex)
		{
		case 0:
			itemActionReplaceBlockData2.ReplaceMode = ItemActionReplaceBlock.EnumReplaceMode.SingleBlock;
			break;
		case 1:
			itemActionReplaceBlockData2.ReplaceMode = ItemActionReplaceBlock.EnumReplaceMode.AllIdenticalBlocks;
			break;
		case 2:
			itemActionReplaceBlockData2.ReplacePaintMode = ItemActionReplaceBlock.EnumReplacePaintMode.KeepCurrentPaint;
			break;
		case 3:
			itemActionReplaceBlockData2.ReplacePaintMode = ItemActionReplaceBlock.EnumReplacePaintMode.RemoveCurrentPaint;
			break;
		case 4:
			itemActionReplaceBlockData2.ReplacePaintMode = ItemActionReplaceBlock.EnumReplacePaintMode.UseNewPaint;
			break;
		case 5:
			itemActionReplaceBlockData2.ReplacePaintMode = ItemActionReplaceBlock.EnumReplacePaintMode.ReplaceWithAirBlocks;
			break;
		}
		this.isHUDDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float rayCastDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bCopyBlock;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isHUDDirty = true;

	public enum EnumReplaceMode
	{
		SingleBlock,
		AllIdenticalBlocks
	}

	public enum EnumReplacePaintMode
	{
		KeepCurrentPaint,
		RemoveCurrentPaint,
		UseNewPaint,
		ReplaceWithAirBlocks
	}

	public class ItemActionReplaceBlockData : ItemActionRanged.ItemActionDataRanged
	{
		public Block ReplaceBlockClass
		{
			get
			{
				if (this.ReplacePaintMode == ItemActionReplaceBlock.EnumReplacePaintMode.ReplaceWithAirBlocks)
				{
					return global::Block.GetBlockByName("air", true);
				}
				if (this.Block == null)
				{
					return null;
				}
				return this.Block.Value.Block;
			}
		}

		public ItemActionReplaceBlockData(ItemInventoryData _invData, int _indexInEntityOfAction)
			: base(_invData, _indexInEntityOfAction)
		{
		}

		public BlockValue? Block;

		public TextureFullArray PaintTextures;

		public sbyte Density;

		public ItemActionReplaceBlock.EnumReplaceMode ReplaceMode;

		public ItemActionReplaceBlock.EnumReplacePaintMode ReplacePaintMode;
	}
}
