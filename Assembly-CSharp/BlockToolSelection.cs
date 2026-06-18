using System;
using System.Collections;
using System.Collections.Generic;
using Audio;
using Platform;
using UnityEngine;

public class BlockToolSelection : ISelectionBoxCallback, IBlockTool
{
	public SelectionBox SelectionBox
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			SelectionBox selectionBox;
			if (SelectionBoxManager.Instance.CategorySelection.TryGetBox("SingleInstance", out selectionBox))
			{
				return selectionBox;
			}
			selectionBox = SelectionBoxManager.Instance.CategorySelection.AddBox("SingleInstance", Vector3i.zero, Vector3i.one, false, false);
			selectionBox.SetVisible(false);
			selectionBox.SetSizeVisibility(true);
			return selectionBox;
		}
	}

	public BlockToolSelection()
	{
		BlockToolSelection.Instance = this;
		SelectionBoxManager.Instance.CategorySelection.SetCallback(this);
		PlayerActionsLocal primaryPlayer = PlatformManager.NativePlatform.Input.PrimaryPlayer;
		NGuiAction nguiAction = new NGuiAction(Localization.Get("selectionToolsEditBlocksVolume", false, null), null, true);
		nguiAction.SetClickActionDelegate(delegate
		{
			GameManager.bVolumeBlocksEditing = !GameManager.bVolumeBlocksEditing;
		});
		nguiAction.SetIsCheckedDelegate(() => GameManager.bVolumeBlocksEditing);
		nguiAction.SetIsVisibleDelegate(() => GameManager.Instance.IsEditMode());
		NGuiAction nguiAction2 = new NGuiAction(Localization.Get("selectionToolsCopyAirBlocks", false, null), null, true);
		nguiAction2.SetClickActionDelegate(delegate
		{
			this.copyPasteAirBlocks = !this.copyPasteAirBlocks;
		});
		nguiAction2.SetIsCheckedDelegate(() => this.copyPasteAirBlocks);
		nguiAction2.SetIsVisibleDelegate(new NGuiAction.IsVisibleDelegate(GameManager.Instance.IsEditMode));
		NGuiAction nguiAction3 = new NGuiAction(Localization.Get("selectionToolsClearSelection", false, null), primaryPlayer.SelectionClear);
		nguiAction3.SetClickActionDelegate(delegate
		{
			if (this.SelectionLockMode == 2)
			{
				this.SelectionLockMode = 0;
				this.SelectionActive = false;
				return;
			}
			this.BeginUndo();
			BlockTools.CubeRPC(GameManager.Instance, this.SelectionStart, this.SelectionEnd, BlockValue.Air, MarchingCubes.DensityAir, 0, TextureFullArray.Default);
			BlockTools.CubeWaterRPC(GameManager.Instance, this.SelectionStart, this.SelectionEnd, WaterValue.Empty);
			this.EndUndo(false);
		});
		nguiAction3.SetIsEnabledDelegate(() => GameManager.Instance.IsEditMode() && this.SelectionActive);
		nguiAction3.SetIsVisibleDelegate(() => GameManager.Instance.IsEditMode());
		nguiAction3.SetTooltip("selectionToolsClearSelectionTip");
		NGuiAction nguiAction4 = new NGuiAction(Localization.Get("selectionToolsFillSelection", false, null), primaryPlayer.SelectionFill);
		nguiAction4.SetClickActionDelegate(delegate
		{
			this.BeginUndo();
			EntityPlayerLocal primaryPlayer2 = GameManager.Instance.World.GetPrimaryPlayer();
			ItemValue holdingItemItemValue = primaryPlayer2.inventory.holdingItemItemValue;
			BlockValue blockValue = holdingItemItemValue.ToBlockValue(false);
			if (blockValue.isair)
			{
				return;
			}
			Block block = blockValue.Block;
			BlockPlacement.Result result = new BlockPlacement.Result(BlockPlacement.EnumPlacement.Voxel, Vector3.one * 0.5f, Vector3i.zero, BlockFace.None, blockValue, PropTransform.identity);
			block.OnBlockPlaceBefore(GameManager.Instance.World, ref result, primaryPlayer2, GameManager.Instance.World.GetGameRandom());
			blockValue = result.blockValue;
			blockValue.rotation = ((primaryPlayer2.inventory.holdingItemData is ItemClassBlock.ItemBlockInventoryData) ? ((ItemClassBlock.ItemBlockInventoryData)primaryPlayer2.inventory.holdingItemData).rotation : blockValue.rotation);
			BlockTools.CubeRPC(GameManager.Instance, this.m_selectionStartPoint, this.m_SelectionEndPoint, blockValue, blockValue.Block.shape.IsTerrain() ? MarchingCubes.DensityTerrain : MarchingCubes.DensityAir, 0, holdingItemItemValue.TextureFullArray);
			this.EndUndo(false);
		});
		nguiAction4.SetIsEnabledDelegate(() => GameManager.Instance.IsEditMode() && this.SelectionActive);
		nguiAction4.SetIsVisibleDelegate(() => GameManager.Instance.IsEditMode());
		nguiAction4.SetTooltip("selectionToolsFillSelectionTip");
		NGuiAction nguiAction5 = new NGuiAction(Localization.Get("selectionToolsRandomFillSelection", false, null), null);
		nguiAction5.SetClickActionDelegate(delegate
		{
			this.BeginUndo();
			BlockTools.CubeRandomRPC(GameManager.Instance, this.m_selectionStartPoint, this.m_SelectionEndPoint, GameManager.Instance.World.GetPrimaryPlayer().inventory.holdingItemItemValue.ToBlockValue(false), 0.1f, new EBlockRotationClasses?(EBlockRotationClasses.Basic90));
			this.EndUndo(false);
		});
		nguiAction5.SetIsEnabledDelegate(() => this.SelectionActive);
		nguiAction5.SetIsVisibleDelegate(() => GameManager.Instance.IsEditMode());
		nguiAction5.SetTooltip("selectionToolsRandomFillSelectionTip");
		NGuiAction nguiAction6 = new NGuiAction(Localization.Get("selectionToolsUndo", false, null), null);
		nguiAction6.SetClickActionDelegate(delegate
		{
			this.blockUndoRedo(false);
		});
		nguiAction6.SetIsEnabledDelegate(() => this.undoQueue.Count > 0);
		nguiAction6.SetIsVisibleDelegate(() => GameManager.Instance.IsEditMode());
		nguiAction6.SetTooltip("selectionToolsUndoTip");
		NGuiAction nguiAction7 = new NGuiAction(Localization.Get("selectionToolsRedo", false, null), null);
		nguiAction7.SetClickActionDelegate(delegate
		{
			this.blockUndoRedo(true);
		});
		nguiAction7.SetIsEnabledDelegate(() => this.redoQueue.Count > 0);
		nguiAction7.SetIsVisibleDelegate(() => GameManager.Instance.IsEditMode());
		nguiAction7.SetTooltip("selectionToolsRedoTip");
		this.actions = new Dictionary<string, NGuiAction>
		{
			{ "volumeBlocksEditing", nguiAction },
			{ "copyAirBlocks", nguiAction2 },
			{
				"sep1",
				NGuiAction.Separator
			},
			{ "clearSelection", nguiAction3 },
			{ "fillSelection", nguiAction4 },
			{ "randomFillSelection", nguiAction5 },
			{
				"sep2",
				NGuiAction.Separator
			},
			{ "undo", nguiAction6 },
			{ "redo", nguiAction7 }
		};
		foreach (KeyValuePair<string, NGuiAction> keyValuePair in this.actions)
		{
			string text;
			NGuiAction nguiAction8;
			keyValuePair.Deconstruct(out text, out nguiAction8);
			NGuiAction nguiAction9 = nguiAction8;
			LocalPlayerUI.primaryUI.windowManager.AddGlobalAction(nguiAction9);
		}
		Origin.OriginChanged = (Action<Vector3>)Delegate.Combine(Origin.OriginChanged, new Action<Vector3>(this.OnOriginChanged));
	}

	public void CheckSpecialKeys(Event ev, PlayerActionsLocal playerActions)
	{
		if (this.hitInfo == null)
		{
			return;
		}
		Vector3i vector3i = ((GameManager.Instance.IsEditMode() && playerActions.Run.IsPressed) ? this.hitInfo.hit.blockPos : this.hitInfo.lastBlockPos);
		bool flag = (InputUtils.IsMac ? ((ev.modifiers & 8) > 0) : ((ev.modifiers & 2) > 0));
		bool flag2 = (ev.modifiers & 1) > 0;
		KeyCode keyCode = ev.keyCode;
		if (keyCode != 99)
		{
			if (keyCode != 118)
			{
				if (keyCode != 122)
				{
					return;
				}
				if (flag)
				{
					this.blockUndoRedo(false);
				}
			}
			else if (flag)
			{
				if (!flag2 && this.SelectionLockMode != 2)
				{
					if (this.SelectionActive && this.clipboard.size.Equals(Vector3i.one) && !this.SelectionSize.Equals(this.clipboard.size))
					{
						this.BeginUndo();
						BlockValue block = this.clipboard.GetBlock(0, 0, 0);
						WaterValue water = this.clipboard.GetWater(0, 0, 0);
						TextureFullArray texture = this.clipboard.GetTexture(0, 0, 0);
						BlockTools.CubeRPC(GameManager.Instance, this.m_selectionStartPoint, this.m_SelectionEndPoint, block, block.Block.shape.IsTerrain() ? MarchingCubes.DensityTerrain : MarchingCubes.DensityAir, 0, texture);
						BlockTools.CubeWaterRPC(GameManager.Instance, this.m_selectionStartPoint, this.m_SelectionEndPoint, water);
						this.EndUndo(false);
						return;
					}
					if (this.SelectionActive && !this.SelectionSize.Equals(this.clipboard.size))
					{
						this.SelectionEnd = this.SelectionStart + this.clipboard.size - Vector3i.one;
						return;
					}
					if (!this.SelectionActive)
					{
						this.SelectionStart = vector3i;
						this.SelectionEnd = this.SelectionStart + this.clipboard.size - Vector3i.one;
						this.SelectionActive = true;
						return;
					}
					if (this.SelectionActive && this.SelectionSize.Equals(this.clipboard.size))
					{
						this.blockPaste(this.SelectionMin, this.clipboard);
						return;
					}
				}
				else
				{
					if (this.SelectionLockMode != 2)
					{
						if (this.SelectionSize != this.clipboard.size)
						{
							this.SelectionEnd = this.SelectionStart + this.clipboard.size - Vector3i.one;
						}
						this.SelectionActive = true;
						this.SelectionLockMode = 2;
						this.createBlockPreviewFrom(this.clipboard);
						return;
					}
					this.SelectionLockMode = 0;
					this.blockPaste(this.SelectionMin, this.clipboard);
					return;
				}
			}
		}
		else if (flag)
		{
			if (!this.SelectionActive)
			{
				this.SelectionStart = vector3i;
				this.SelectionEnd = vector3i;
			}
			this.blockCopy(this.clipboard);
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void rotatePreviewAroundY()
	{
		if (this.previewGORot2 == null)
		{
			return;
		}
		this.previewGORot2.transform.localRotation = Quaternion.AngleAxis(90f, Vector3.up) * this.previewGORot2.transform.localRotation;
		this.clipboard.RotateY(false, 1);
		Vector3 vector = this.previewGORot2.transform.localRotation * this.offsetToMin;
		Vector3 vector2 = this.selectionRotCenter + vector;
		Vector3 vector3 = this.previewGORot2.transform.localRotation * this.offsetToMax;
		Vector3 vector4 = this.selectionRotCenter + vector3;
		Vector3i vector3i = new Vector3i(Utils.Fastfloor(Utils.FastMin(vector2.x, vector4.x)), Utils.Fastfloor(Utils.FastMin(vector2.y, vector4.y)), Utils.Fastfloor(Utils.FastMin(vector2.z, vector4.z)));
		this.SelectionStart = vector3i;
		this.SelectionEnd = vector3i + this.clipboard.size - Vector3i.one;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void removeBlockPreview()
	{
		NGUITools.DestroyChildren(this.previewGORot3.transform);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void createBlockPreviewFrom(Prefab _prefab)
	{
		if (this.previewGOParent == null)
		{
			this.previewGOParent = new GameObject("Preview");
			this.previewGOParent.transform.parent = null;
			this.previewGOParent.transform.localPosition = Vector3.zero;
			this.previewGORot1 = new GameObject("Rot1");
			this.previewGORot1.transform.parent = this.previewGOParent.transform;
			this.previewGORot2 = new GameObject("Rot2");
			this.previewGORot2.transform.parent = this.previewGORot1.transform;
			this.previewGORot3 = new GameObject("Rot3");
			this.previewGORot3.transform.parent = this.previewGORot2.transform;
		}
		else
		{
			this.removeBlockPreview();
		}
		ThreadManager.RunCoroutineSync(_prefab.ToTransform(true, true, true, false, this.previewGORot3.transform, "PrefabImposter", Vector3.zero, DynamicPrefabDecorator.PrefabPreviewLimit));
		Transform transform = this.previewGORot3.transform.Find("PrefabImposter");
		transform.localRotation = Quaternion.identity;
		transform.localPosition = Vector3.zero;
		Vector3 vector;
		vector..ctor((float)(_prefab.size.x / 2), 0f, (float)(_prefab.size.z / 2));
		this.previewGORot1.transform.position = this.SelectionMin.ToVector3() - Origin.position;
		this.previewGORot1.transform.rotation = Quaternion.identity;
		this.previewGORot2.transform.localPosition = vector;
		this.previewGORot2.transform.localRotation = Quaternion.identity;
		this.previewGORot3.transform.localPosition = -vector;
		this.previewGORot3.transform.localRotation = Quaternion.identity;
		vector = -vector;
		vector.y = (float)(-(float)_prefab.size.y / 2);
		this.offsetToMax = vector + (_prefab.size - Vector3i.one).ToVector3() + Vector3.one * 0.5f;
		this.offsetToMin = vector + Vector3.one * 0.5f;
		this.selectionRotCenter = this.SelectionMin.ToVector3() - vector;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnOriginChanged(Vector3 _newOrigin)
	{
		if (this.previewGORot1 == null)
		{
			return;
		}
		this.previewGORot1.transform.position = this.SelectionMin.ToVector3() - Origin.position;
	}

	public void RotateFocusedBlock(WorldRayHitInfo _hitInfo, PlayerActionsLocal _playerActions)
	{
		if (!_hitInfo.bHitValid)
		{
			return;
		}
		Vector3i vector3i = ((GameManager.Instance.World.IsEditor() && _playerActions.Run.IsPressed) ? _hitInfo.hit.blockPos : _hitInfo.lastBlockPos);
		BlockValue block = GameManager.Instance.World.ChunkCache.GetBlock(vector3i);
		if (block.Block.shape.IsRotatable)
		{
			block.rotation = block.Block.shape.Rotate(false, (int)block.rotation);
			this.setBlock(vector3i, block);
		}
	}

	public void CheckKeys(ItemInventoryData _data, WorldRayHitInfo _hitInfo, PlayerActionsLocal playerActions)
	{
		if (LocalPlayerUI.primaryUI.windowManager.IsInputActive())
		{
			return;
		}
		this.hitInfo = _hitInfo;
		bool flag = _data.world.IsEditor() && playerActions.Run.IsPressed;
		Vector3i vector3i = (flag ? _hitInfo.hit.blockPos : _hitInfo.lastBlockPos);
		BlockValueRef blockValueRef = (flag ? _hitInfo.hit.blockValueRef : new BlockValueRef(_hitInfo.lastBlockPos));
		ItemClassBlock.ItemBlockInventoryData itemBlockInventoryData = _data as ItemClassBlock.ItemBlockInventoryData;
		if (itemBlockInventoryData != null)
		{
			BlockValue blockValue = itemBlockInventoryData.itemValue.ToBlockValue(false);
			blockValue.rotation = itemBlockInventoryData.rotation;
			itemBlockInventoryData.rotation = blockValue.Block.BlockPlacementHelper.OnPlaceBlock(itemBlockInventoryData.Placement, itemBlockInventoryData.mode, itemBlockInventoryData.localRot, GameManager.Instance.World, blockValue, itemBlockInventoryData.propTransform, this.hitInfo.hit, itemBlockInventoryData.holdingEntity.position).blockValue.rotation;
		}
		if (!GameManager.Instance.IsEditMode() && !GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled))
		{
			return;
		}
		if (playerActions.SelectionSet.IsPressed)
		{
			if (GameManager.Instance.World.ChunkCache == null)
			{
				return;
			}
			if (InputUtils.ControlKeyPressed)
			{
				return;
			}
			this.SelectionLockMode = 0;
			Vector3i vector3i2 = vector3i;
			if (!this.SelectionActive)
			{
				Vector3i selectionSize = this.SelectionSize;
				this.SelectionStart = vector3i2;
				if (this.SelectionLockMode == 1)
				{
					this.SelectionEnd = this.SelectionStart + selectionSize - Vector3i.one;
				}
				else
				{
					this.SelectionEnd = this.SelectionStart;
				}
				this.SelectionActive = true;
			}
			else
			{
				this.SelectionEnd = vector3i2;
			}
		}
		if (!GameManager.Instance.IsEditMode())
		{
			return;
		}
		if (playerActions.DensityM1.WasPressed || playerActions.DensityP1.WasPressed || playerActions.DensityM10.WasPressed || playerActions.DensityP10.WasPressed)
		{
			int num = ((playerActions.DensityM1.WasPressed || playerActions.DensityP1.WasPressed) ? 1 : 10);
			if (playerActions.DensityM1.WasPressed || playerActions.DensityM10.WasPressed)
			{
				num = -num;
			}
			if (InputUtils.ControlKeyPressed)
			{
				num *= 50;
			}
			BlockValue block = GameManager.Instance.World.GetBlock(blockValueRef);
			Block block2 = block.Block;
			if (block2.BlockTag == BlockTags.Door)
			{
				if (num > 0)
				{
					num = ((block.damage + num >= block2.MaxDamagePlusDowngrades) ? (block2.MaxDamagePlusDowngrades - block.damage - 1) : num);
				}
				block2.DamageBlock(GameManager.Instance.World, blockValueRef, block, num, -1, null, false, false);
			}
			else
			{
				int num2;
				if (!this.SelectionActive)
				{
					num2 = (int)GameManager.Instance.World.GetDensity(blockValueRef);
				}
				else
				{
					num2 = (int)GameManager.Instance.World.GetDensity(this.m_selectionStartPoint);
				}
				num2 += num;
				num2 = Utils.FastClamp(num2, (int)MarchingCubes.DensityTerrain, (int)MarchingCubes.DensityAir);
				if (!this.SelectionActive)
				{
					GameManager.Instance.World.SetBlocksRPC(new List<BlockChangeInfo>
					{
						new BlockChangeInfo(blockValueRef, (sbyte)num2, false)
					});
				}
				else
				{
					BlockTools.CubeDensityRPC(GameManager.Instance, this.m_selectionStartPoint, this.m_SelectionEndPoint, (sbyte)num2);
				}
			}
		}
		if ((playerActions.FocusCopyBlock.WasPressed || (playerActions.Secondary.WasPressed && InputUtils.ControlKeyPressed)) && GameManager.Instance.IsEditMode() && _hitInfo.bHitValid && !_hitInfo.hit.blockValue.isair)
		{
			EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
			BlockValue blockValue2 = _hitInfo.hit.blockValue;
			if (blockValue2.ischild)
			{
				Vector3i parentPos = blockValue2.Block.multiBlockPos.GetParentPos(_hitInfo.hit.blockPos, blockValue2);
				blockValue2 = GameManager.Instance.World.GetBlock(parentPos);
			}
			ItemStack itemStack = new ItemStack(blockValue2.ToItemValue(), 99);
			if (blockValue2.Block.GetAutoShapeType() != EAutoShapeType.Helper)
			{
				itemStack.itemValue.TextureFullArray = GameManager.Instance.World.ChunkCache.GetTextureFullArray(_hitInfo.hit.blockValueRef);
			}
			if (primaryPlayer.inventory.GetItemCount(itemStack.itemValue, true, -1, -1, true) == 0 && primaryPlayer.inventory.CanTakeItem(itemStack))
			{
				int num3;
				if (primaryPlayer.inventory.AddItem(itemStack, out num3))
				{
					ItemClassBlock.ItemBlockInventoryData itemBlockInventoryData2 = primaryPlayer.inventory.GetItemDataInSlot(num3) as ItemClassBlock.ItemBlockInventoryData;
					if (itemBlockInventoryData2 != null)
					{
						itemBlockInventoryData2.damage = blockValue2.damage;
						return;
					}
				}
			}
			else
			{
				ItemClassBlock.ItemBlockInventoryData itemBlockInventoryData3 = _data as ItemClassBlock.ItemBlockInventoryData;
				if (itemBlockInventoryData3 != null && this.hasSameShape(blockValue2.type, primaryPlayer.inventory.holdingItemItemValue.type))
				{
					itemBlockInventoryData3.rotation = blockValue2.rotation;
					itemBlockInventoryData3.damage = blockValue2.damage;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasSameShape(int _blockId1, int _blockId2)
	{
		Block block = Block.list[_blockId1];
		Block block2 = Block.list[_blockId2];
		return !(block.shape.GetType() != block2.shape.GetType()) && (!(block.shape is BlockShapeNew) || block.Properties.GetValue("Model") == block2.Properties.GetValue("Model"));
	}

	public bool ConsumeScrollWheel(ItemInventoryData _data, float _scrollWheelInput, PlayerActionsLocal _playerInput)
	{
		if ((_playerInput.Reload.IsPressed || _playerInput.PermanentActions.Reload.IsPressed) && _data is ItemClassBlock.ItemBlockInventoryData && Mathf.Abs(_scrollWheelInput) >= 0.001f)
		{
			ItemClassBlock.ItemBlockInventoryData itemBlockInventoryData = (ItemClassBlock.ItemBlockInventoryData)_data;
			itemBlockInventoryData.rotation = itemBlockInventoryData.itemValue.ToBlockValue(false).Block.BlockPlacementHelper.LimitRotation(itemBlockInventoryData.mode, ref itemBlockInventoryData.localRot, ((EntityPlayerLocal)_data.holdingEntity).HitInfo.hit, _scrollWheelInput > 0f, itemBlockInventoryData.itemValue.ToBlockValue(false), itemBlockInventoryData.rotation);
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i createBlockMoveVector(Vector3 _relPlayerAxis)
	{
		Vector3i zero = Vector3i.zero;
		if (Math.Abs(_relPlayerAxis.x) > Math.Abs(_relPlayerAxis.z))
		{
			zero = new Vector3i(Mathf.Sign(_relPlayerAxis.x), 0f, 0f);
		}
		else
		{
			zero = new Vector3i(0f, 0f, Mathf.Sign(_relPlayerAxis.z));
		}
		return zero;
	}

	public bool ExecuteUseAction(ItemInventoryData _data, bool _bReleased, PlayerActionsLocal playerActions)
	{
		if (!(_data is ItemClassBlock.ItemBlockInventoryData))
		{
			return false;
		}
		bool flag = GameManager.Instance.IsEditMode() || GameStats.GetInt(EnumGameStats.GameModeId) == 2;
		if (flag && playerActions.Drop.IsPressed)
		{
			return false;
		}
		if (_bReleased)
		{
			return false;
		}
		if (Time.time - this.lastBuildTime < Constants.cBuildIntervall)
		{
			return true;
		}
		this.lastBuildTime = Time.time;
		ItemClassBlock.ItemBlockInventoryData itemBlockInventoryData = (ItemClassBlock.ItemBlockInventoryData)_data;
		EntityAlive holdingEntity = itemBlockInventoryData.holdingEntity;
		FastTags<TagGroup.Global> fastTags = FastTags<TagGroup.Global>.none;
		ItemClassBlock itemClassBlock = itemBlockInventoryData.item as ItemClassBlock;
		if (itemClassBlock != null)
		{
			fastTags = itemClassBlock.GetBlock().Tags;
		}
		if (EffectManager.GetValue(PassiveEffects.DisableItem, holdingEntity.inventory.holdingItemItemValue, 0f, holdingEntity, null, fastTags, true, true, true, true, true, 1, true, false) > 0f)
		{
			this.lastBuildTime = Time.time + 1f;
			Manager.PlayInsidePlayerHead("twitch_no_attack", -1, 0f, false, false);
			return false;
		}
		WorldRayHitInfo worldRayHitInfo = ((EntityPlayerLocal)itemBlockInventoryData.holdingEntity).HitInfo;
		HitInfoDetails hitInfoDetails = worldRayHitInfo.hit.Clone();
		if (!worldRayHitInfo.bHitValid)
		{
			return false;
		}
		hitInfoDetails.blockPos = ((flag && playerActions.Run.IsPressed) ? worldRayHitInfo.hit.blockPos : worldRayHitInfo.lastBlockPos);
		BlockValue blockValue = itemBlockInventoryData.itemValue.ToBlockValue(false);
		Block block = blockValue.Block;
		blockValue.damage = itemBlockInventoryData.damage;
		blockValue.rotation = itemBlockInventoryData.rotation;
		World world = GameManager.Instance.World;
		if (!GameManager.Instance.IsEditMode())
		{
			int placementDistanceSq = block.GetPlacementDistanceSq();
			if (hitInfoDetails.distanceSq > (float)placementDistanceSq)
			{
				return true;
			}
			Vector3i freePlacementPosition = block.GetFreePlacementPosition(world, hitInfoDetails.blockPos, blockValue, holdingEntity);
			if (!holdingEntity.IsGodMode.Value && GameUtils.IsColliderWithinBlock(freePlacementPosition, blockValue))
			{
				return true;
			}
			if (hitInfoDetails.blockPos == Vector3i.zero)
			{
				return true;
			}
		}
		_data.holdingEntity.RightArmAnimationUse = true;
		BlockPlacement.Result result = block.BlockPlacementHelper.OnPlaceBlock(itemBlockInventoryData.Placement, itemBlockInventoryData.mode, itemBlockInventoryData.localRot, GameManager.Instance.World, blockValue, itemBlockInventoryData.propTransform, hitInfoDetails, itemBlockInventoryData.holdingEntity.position);
		block.OnBlockPlaceBefore(itemBlockInventoryData.world, ref result, itemBlockInventoryData.holdingEntity, itemBlockInventoryData.world.GetGameRandom());
		blockValue = result.blockValue;
		block = blockValue.Block;
		if (blockValue.damage == 0)
		{
			blockValue.damage = block.StartDamage;
			result.blockValue.damage = block.StartDamage;
		}
		if (!playerActions.Run.IsPressed)
		{
			result.blockPos = block.GetFreePlacementPosition(itemBlockInventoryData.holdingEntity.world, result.blockPos, blockValue, itemBlockInventoryData.holdingEntity);
		}
		if (!block.CanPlaceBlockAt(itemBlockInventoryData.world, result.blockPos, blockValue, false))
		{
			itemBlockInventoryData.holdingEntity.PlayOneShot("keystone_build_warning", false, false, false, null, 1f);
			return true;
		}
		eSetBlockResponse eSetBlockResponse;
		if (!BlockLimitTracker.instance.CanAddBlock(blockValue, result.blockPos, out eSetBlockResponse))
		{
			if (eSetBlockResponse != eSetBlockResponse.PowerBlockLimitExceeded)
			{
				if (eSetBlockResponse == eSetBlockResponse.StorageBlockLimitExceeded)
				{
					GameManager.ShowTooltip(GameManager.Instance.World.GetPrimaryPlayer(), "uicannotaddstorageblock", false, false, 0f);
				}
			}
			else
			{
				GameManager.ShowTooltip(GameManager.Instance.World.GetPrimaryPlayer(), "uicannotaddpowerblock", false, false, 0f);
			}
			return true;
		}
		if (!GameManager.Instance.IsEditMode())
		{
			if (block.IndexName == "lpblock")
			{
				if (!itemBlockInventoryData.world.CanPlaceLandProtectionBlockAt(worldRayHitInfo.lastBlockPos, itemBlockInventoryData.world.gameManager.GetPersistentLocalPlayer()))
				{
					itemBlockInventoryData.holdingEntity.PlayOneShot("keystone_build_warning", false, false, false, null, 1f);
					return true;
				}
				itemBlockInventoryData.holdingEntity.PlayOneShot("keystone_placed", false, false, false, null, 1f);
			}
			else if (!itemBlockInventoryData.world.CanPlaceBlockAt(worldRayHitInfo.lastBlockPos, itemBlockInventoryData.world.gameManager.GetPersistentLocalPlayer(), false))
			{
				itemBlockInventoryData.holdingEntity.PlayOneShot("keystone_build_warning", false, false, false, null, 1f);
				return true;
			}
		}
		BiomeDefinition biome = itemBlockInventoryData.world.GetBiome(result.blockPos.x, result.blockPos.z);
		if (biome != null && biome.Replacements.ContainsKey(result.blockValue.type))
		{
			result.blockValue.type = biome.Replacements[result.blockValue.type];
		}
		BlockPlacement.EnumPlacement placement = result.placement;
		if (placement != BlockPlacement.EnumPlacement.Voxel)
		{
			if (placement != BlockPlacement.EnumPlacement.Free)
			{
				throw new ArgumentOutOfRangeException();
			}
			if (!this.PlaceProp(_data, result, itemBlockInventoryData, block, blockValue))
			{
				return true;
			}
		}
		else if (!this.PlaceBlock(_data, result, itemBlockInventoryData, block, blockValue))
		{
			return true;
		}
		itemBlockInventoryData.holdingEntity.RightArmAnimationUse = true;
		itemBlockInventoryData.lastBuildTime = Time.time;
		GameManager.Instance.StartCoroutine(this.decInventoryLater(itemBlockInventoryData, itemBlockInventoryData.holdingEntity.inventory.holdingItemIdx));
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool PlaceBlock(ItemInventoryData _data, BlockPlacement.Result result, ItemClassBlock.ItemBlockInventoryData data, Block block, BlockValue blockValue)
	{
		this.addToUndo(result.blockPos, GameManager.Instance.World.GetBlock(result.blockPos));
		if (Block.list[data.itemValue.type].SelectAlternates)
		{
			if (data.itemValue.TextureFullArray.IsDefault)
			{
				block.PlaceBlock(data.world, result, data.holdingEntity);
			}
			else
			{
				BlockChangeInfo blockChangeInfo = new BlockChangeInfo(result.blockPos, blockValue, data.holdingEntity.entityId);
				blockChangeInfo.textureFull = data.itemValue.TextureFullArray;
				blockChangeInfo.bChangeTexture = true;
				GameManager.Instance.World.SetBlocksRPC(new List<BlockChangeInfo> { blockChangeInfo });
			}
		}
		else if (data.itemValue.TextureFullArray.IsDefault)
		{
			block.PlaceBlock(data.world, result, data.holdingEntity);
		}
		else
		{
			BlockChangeInfo blockChangeInfo2 = new BlockChangeInfo(result.blockPos, blockValue, data.holdingEntity.entityId);
			blockChangeInfo2.textureFull = data.itemValue.TextureFullArray;
			blockChangeInfo2.bChangeTexture = true;
			GameManager.Instance.World.SetBlocksRPC(new List<BlockChangeInfo> { blockChangeInfo2 });
		}
		QuestEventManager.Current.BlockPlaced(block.GetBlockName(), result.blockPos);
		data.holdingEntity.MinEventContext.ItemActionData = data.actionData[0];
		data.holdingEntity.MinEventContext.BlockValue = result.blockValue;
		data.holdingEntity.MinEventContext.Position = result.pos;
		data.holdingEntity.FireEvent(MinEventTypes.onSelfPlaceBlock, true);
		if (!block.shape.IsOmitTerrainSnappingUp && !block.IsTerrainDecoration)
		{
			data.world.ChunkCache.SnapTerrainToPositionAroundRPC(data.world, this.hitInfo.lastBlockPos - Vector3i.up);
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool PlaceProp(ItemInventoryData _data, BlockPlacement.Result result, ItemClassBlock.ItemBlockInventoryData data, Block block, BlockValue blockValue)
	{
		block.PlaceProp(data.world, result, data.holdingEntity);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator decInventoryLater(ItemInventoryData data, int index)
	{
		data.holdingEntity.inventory.WaitForSecondaryRelease = data.holdingEntity.inventory.holdingItemStack.count == 1;
		yield return new WaitForSeconds(0.1f);
		if (!GameManager.Instance.IsEditMode())
		{
			ItemStack itemStack = data.holdingEntity.inventory.GetItem(index).Clone();
			if (itemStack.count > 0)
			{
				itemStack.count--;
			}
			data.holdingEntity.inventory.SetItem(index, itemStack);
		}
		BlockValue blockValue = data.itemValue.ToBlockValue(false);
		string text = "placeblock";
		Block block = blockValue.Block;
		if (block.CustomPlaceSound != null)
		{
			text = block.CustomPlaceSound;
		}
		data.holdingEntity.PlayOneShot(text, false, false, false, null, 1f);
		yield break;
	}

	public bool ExecuteAttackAction(ItemInventoryData _data, bool _bReleased, PlayerActionsLocal playerActions)
	{
		if (!_bReleased)
		{
			return false;
		}
		bool flag = false;
		if (GameManager.Instance.IsEditMode() && playerActions.Drop.IsPressed)
		{
			return false;
		}
		if (!playerActions.SelectionSet.IsPressed && this.SelectionActive)
		{
			if (!playerActions.Drop.IsPressed)
			{
				flag = flag || this.SelectionActive;
				if (this.SelectionLockMode == 1)
				{
					Vector3i selectionSize = this.SelectionSize;
					this.SelectionStart = this.hitInfo.hit.blockPos;
					this.SelectionEnd = this.SelectionStart + selectionSize - Vector3i.one;
				}
				else if (this.SelectionLockMode == 0)
				{
					this.SelectionActive = false;
				}
			}
		}
		else if (GameManager.Instance.IsEditMode() && playerActions.Run.IsPressed && ((EntityPlayerLocal)_data.holdingEntity).HitInfo.bHitValid)
		{
			Vector3i vector3i = (playerActions.Run.IsPressed ? this.hitInfo.hit.blockPos : this.hitInfo.lastBlockPos);
			this.setBlock(vector3i, BlockValue.Air);
			flag = true;
		}
		else if (_data is ItemClassBlock.ItemBlockInventoryData)
		{
			ItemClassBlock.ItemBlockInventoryData itemBlockInventoryData = (ItemClassBlock.ItemBlockInventoryData)_data;
			itemBlockInventoryData.itemValue.ToBlockValue(false).Block.RotateHoldingBlock(itemBlockInventoryData, true, true);
			flag = true;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void rotateSelectionAroundY()
	{
		Vector3i vector3i = new Vector3i(Mathf.Abs(this.m_selectionStartPoint.x - this.m_SelectionEndPoint.x), Mathf.Abs(this.m_selectionStartPoint.y - this.m_SelectionEndPoint.y), Mathf.Abs(this.m_selectionStartPoint.z - this.m_SelectionEndPoint.z));
		Vector3i vector3i2 = new Vector3i(Mathf.Min(this.m_selectionStartPoint.x, this.m_SelectionEndPoint.x), Mathf.Min(this.m_selectionStartPoint.y, this.m_SelectionEndPoint.y), Mathf.Min(this.m_selectionStartPoint.z, this.m_SelectionEndPoint.z));
		Prefab prefab = BlockTools.CopyIntoStorage(GameManager.Instance, vector3i2, vector3i2 + vector3i);
		this.BeginUndo();
		new Prefab(prefab.size)
		{
			bCopyAirBlocks = true
		}.CopyIntoRPC(GameManager.Instance, vector3i2, false);
		prefab.RotateY(false, 1);
		prefab.CopyIntoRPC(GameManager.Instance, vector3i2, this.copyPasteAirBlocks);
		this.SelectionStart = vector3i2;
		this.SelectionEnd = vector3i2 + prefab.size - Vector3i.one;
		this.EndUndo(false);
	}

	public bool SelectionActive
	{
		get
		{
			return this.SelectionBox.IsActive;
		}
		set
		{
			if (this.SelectionActive != value)
			{
				SelectionBoxManager.Instance.SetActive(this.SelectionBox, value);
			}
		}
	}

	public int SelectionLockMode
	{
		get
		{
			return this.m_iSelectionLockMode;
		}
		set
		{
			if (this.m_iSelectionLockMode != value)
			{
				this.m_iSelectionLockMode = value;
				this.SelectionBox.SetVisible(this.SelectionActive);
				Color color = BlockToolSelection.colInactive;
				if (this.m_iSelectionLockMode == 1)
				{
					color..ctor(0.5f, 0f, 1f, 0.5f);
					this.removeBlockPreview();
				}
				else if (this.m_iSelectionLockMode == 2)
				{
					color = BlockToolSelection.colActive;
				}
				else
				{
					this.removeBlockPreview();
				}
				this.SelectionBox.SetAllFacesColor(color, true);
			}
		}
	}

	public Vector3i SelectionMin
	{
		get
		{
			return new Vector3i(Utils.FastMin(this.SelectionStart.x, this.SelectionEnd.x), Utils.FastMin(this.SelectionStart.y, this.SelectionEnd.y), Utils.FastMin(this.SelectionStart.z, this.SelectionEnd.z));
		}
	}

	public Vector3i SelectionStart
	{
		get
		{
			return this.m_selectionStartPoint;
		}
		set
		{
			if (!this.m_selectionStartPoint.Equals(value))
			{
				this.m_selectionStartPoint = value;
				this.updateSelection();
			}
		}
	}

	public Vector3i SelectionEnd
	{
		get
		{
			return this.m_SelectionEndPoint;
		}
		set
		{
			if (!this.m_SelectionEndPoint.Equals(value))
			{
				this.m_SelectionEndPoint = value;
				this.updateSelection();
			}
		}
	}

	public Vector3i SelectionSize
	{
		get
		{
			return new Vector3i(Mathf.Abs(this.m_selectionStartPoint.x - this.m_SelectionEndPoint.x) + 1, Mathf.Abs(this.m_selectionStartPoint.y - this.m_SelectionEndPoint.y) + 1, Mathf.Abs(this.m_selectionStartPoint.z - this.m_SelectionEndPoint.z) + 1);
		}
	}

	public void SelectionSizeSet(Vector3i _size)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateSelection()
	{
		Vector3 vector = this.SelectionSize.ToVector3();
		Vector3 vector2;
		vector2..ctor((float)Mathf.Min(this.m_selectionStartPoint.x, this.m_SelectionEndPoint.x), (float)Mathf.Min(this.m_selectionStartPoint.y, this.m_SelectionEndPoint.y), (float)Mathf.Min(this.m_selectionStartPoint.z, this.m_SelectionEndPoint.z));
		this.SelectionBox.SetPositionAndSize(new Vector3i(vector2), new Vector3i(vector));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool setBlock(Vector3i _blockPos, BlockValue _blockValue)
	{
		BlockValue block = GameManager.Instance.World.GetBlock(_blockPos);
		if (block.rawData == _blockValue.rawData)
		{
			return false;
		}
		TextureFullArray textureFullArray = GameManager.Instance.World.GetTextureFullArray(_blockPos.x, _blockPos.y, _blockPos.z);
		this.undoQueue.Add(new List<BlockChangeInfo>
		{
			new BlockChangeInfo(_blockPos, block, MarchingCubes.DensityAir, textureFullArray)
		});
		if (this.undoQueue.Count > 100)
		{
			this.undoQueue.RemoveAt(0);
		}
		if (_blockValue.Block.shape.IsTerrain())
		{
			GameManager.Instance.World.SetBlockRPC(_blockPos, _blockValue, MarchingCubes.DensityTerrain);
		}
		else
		{
			GameManager.Instance.World.SetBlockRPC(_blockPos, _blockValue);
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addToUndo(Vector3i _blockPos, BlockValue _oldBlockValue)
	{
		TextureFullArray textureFullArray = GameManager.Instance.World.GetTextureFullArray(_blockPos.x, _blockPos.y, _blockPos.z);
		this.undoQueue.Add(new List<BlockChangeInfo>
		{
			new BlockChangeInfo(_blockPos, _oldBlockValue, MarchingCubes.DensityAir, textureFullArray)
		});
		if (this.undoQueue.Count > 100)
		{
			this.undoQueue.RemoveAt(0);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void blockUndoRedo(bool _redo)
	{
		List<List<BlockChangeInfo>> list = (_redo ? this.redoQueue : this.undoQueue);
		if (list.Count == 0)
		{
			return;
		}
		List<BlockChangeInfo> list2 = list[list.Count - 1];
		this.BeginUndo();
		GameManager.Instance.SetBlocksRPC(list2, null);
		list.RemoveAt(list.Count - 1);
		this.EndUndo(!_redo);
	}

	public void BeginUndo()
	{
		this.undoChanges = new List<BlockChangeInfo>();
		ChunkCluster chunkCache = GameManager.Instance.World.ChunkCache;
		if (chunkCache != null)
		{
			chunkCache.OnBlockChangedDelegates += this.undoBlockChangeDelegate;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void undoBlockChangeDelegate(Vector3i pos, BlockValue bvOld, sbyte oldDens, TextureFullArray oldTex, BlockValue bvNew)
	{
		if (this.undoChanges != null && !bvOld.ischild)
		{
			this.undoChanges.Add(new BlockChangeInfo(pos, bvOld, oldDens, oldTex));
		}
	}

	public void EndUndo(bool _bRedo = false)
	{
		ChunkCluster chunkCache = GameManager.Instance.World.ChunkCache;
		if (chunkCache != null)
		{
			chunkCache.OnBlockChangedDelegates -= this.undoBlockChangeDelegate;
		}
		if (this.undoChanges.Count <= 0)
		{
			this.undoChanges = null;
			return;
		}
		this.undoChanges.Reverse();
		List<List<BlockChangeInfo>> list = (_bRedo ? this.redoQueue : this.undoQueue);
		list.Add(this.undoChanges);
		if (list.Count > 100)
		{
			list.RemoveAt(0);
		}
		this.undoChanges = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i blockCopy(Prefab _storage)
	{
		return _storage.CopyFromWorldWithEntities(GameManager.Instance.World, this.SelectionStart, this.SelectionEnd, null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void blockPaste(Vector3i _destPos, Prefab _storage)
	{
		this.BeginUndo();
		_storage.CopyIntoRPC(GameManager.Instance, _destPos, this.copyPasteAirBlocks);
		this.SelectionActive = true;
		this.SelectionStart = _destPos;
		this.SelectionEnd = _destPos + _storage.size - Vector3i.one;
		this.EndUndo(false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i sizeFromPositions(Vector3i _posStart, Vector3i _posEnd)
	{
		Vector3i vector3i = new Vector3i(Math.Min(_posStart.x, _posEnd.x), Math.Min(_posStart.y, _posEnd.y), Math.Min(_posStart.z, _posEnd.z));
		Vector3i vector3i2 = new Vector3i(Math.Max(_posStart.x, _posEnd.x), Math.Max(_posStart.y, _posEnd.y), Math.Max(_posStart.z, _posEnd.z));
		return new Vector3i(Math.Abs(vector3i2.x - vector3i.x) + 1, Math.Abs(vector3i2.y - vector3i.y) + 1, Math.Abs(vector3i2.z - vector3i.z) + 1);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void union(Vector3i _pos1Start, Vector3i _pos1End, Vector3i _pos2Start, Vector3i _pos2End, out Vector3i _unionStart, out Vector3i _unionEnd)
	{
		_unionStart = new Vector3i(Utils.FastMin(_pos1Start.x, _pos1End.x, _pos2Start.x, _pos2End.x), Utils.FastMin(_pos1Start.y, _pos1End.y, _pos2Start.y, _pos2End.y), Utils.FastMin(_pos1Start.z, _pos1End.z, _pos2Start.z, _pos2End.z));
		_unionEnd = new Vector3i(Utils.FastMax(_pos1Start.x, _pos1End.x, _pos2Start.x, _pos2End.x), Utils.FastMax(_pos1Start.y, _pos1End.y, _pos2Start.y, _pos2End.y), Utils.FastMax(_pos1Start.z, _pos1End.z, _pos2Start.z, _pos2End.z));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isPrefabActive()
	{
		return GameManager.Instance.GetDynamicPrefabDecorator() != null && GameManager.Instance.GetDynamicPrefabDecorator().ActivePrefab != null;
	}

	public bool OnSelectionBoxActivated(SelectionBox _box, bool _bActivated)
	{
		this.SelectionBox.SetVisible(_bActivated);
		if (!_bActivated)
		{
			this.SelectionLockMode = 0;
		}
		return true;
	}

	public void OnSelectionBoxMoved(SelectionBox _box, Vector3 _moveVector)
	{
		Vector3i vector3i = new Vector3i(_moveVector);
		Vector3i zero = Vector3i.zero;
		int selectionLockMode = this.SelectionLockMode;
		this.SelectionStart += vector3i;
		this.SelectionEnd += vector3i;
		this.selectionRotCenter += vector3i.ToVector3();
		if (this.SelectionLockMode == 2)
		{
			this.previewGORot1.transform.position += _moveVector;
		}
	}

	public void OnSelectionBoxSized(SelectionBox _box, int _dTop, int _dBottom, int _dNorth, int _dSouth, int _dEast, int _dWest)
	{
		if (this.SelectionLockMode == 2)
		{
			this.SelectionLockMode = 0;
			return;
		}
		if (_dEast != 0 && (_dEast >= 0 || this.SelectionSize.x > 1))
		{
			if (this.SelectionEnd.x > this.SelectionStart.x)
			{
				this.SelectionEnd = new Vector3i(this.SelectionEnd.x + _dEast, this.SelectionEnd.y, this.SelectionEnd.z);
			}
			else
			{
				this.SelectionStart = new Vector3i(this.SelectionStart.x + _dEast, this.SelectionStart.y, this.SelectionStart.z);
			}
		}
		if (_dWest != 0 && (_dWest >= 0 || this.SelectionSize.x > 1))
		{
			if (this.SelectionEnd.x <= this.SelectionStart.x)
			{
				this.SelectionEnd = new Vector3i(this.SelectionEnd.x - _dWest, this.SelectionEnd.y, this.SelectionEnd.z);
			}
			else
			{
				this.SelectionStart = new Vector3i(this.SelectionStart.x - _dWest, this.SelectionStart.y, this.SelectionStart.z);
			}
		}
		if (_dTop != 0 && (_dTop >= 0 || this.SelectionSize.y > 1))
		{
			if (this.SelectionEnd.y > this.SelectionStart.y)
			{
				this.SelectionEnd = new Vector3i(this.SelectionEnd.x, this.SelectionEnd.y + _dTop, this.SelectionEnd.z);
			}
			else
			{
				this.SelectionStart = new Vector3i(this.SelectionStart.x, this.SelectionStart.y + _dTop, this.SelectionStart.z);
			}
		}
		if (_dBottom != 0 && (_dBottom >= 0 || this.SelectionSize.y > 1))
		{
			if (this.SelectionEnd.y <= this.SelectionStart.y)
			{
				this.SelectionEnd = new Vector3i(this.SelectionEnd.x, this.SelectionEnd.y - _dBottom, this.SelectionEnd.z);
			}
			else
			{
				this.SelectionStart = new Vector3i(this.SelectionStart.x, this.SelectionStart.y - _dBottom, this.SelectionStart.z);
			}
		}
		if (_dNorth != 0 && (_dNorth >= 0 || this.SelectionSize.z > 1))
		{
			if (this.SelectionEnd.z > this.SelectionStart.z)
			{
				this.SelectionEnd = new Vector3i(this.SelectionEnd.x, this.SelectionEnd.y, this.SelectionEnd.z + _dNorth);
			}
			else
			{
				this.SelectionStart = new Vector3i(this.SelectionStart.x, this.SelectionStart.y, this.SelectionStart.z + _dNorth);
			}
		}
		if (_dSouth != 0 && (_dSouth >= 0 || this.SelectionSize.z > 1))
		{
			if (this.SelectionEnd.z <= this.SelectionStart.z)
			{
				this.SelectionEnd = new Vector3i(this.SelectionEnd.x, this.SelectionEnd.y, this.SelectionEnd.z - _dSouth);
				return;
			}
			this.SelectionStart = new Vector3i(this.SelectionStart.x, this.SelectionStart.y, this.SelectionStart.z - _dSouth);
		}
	}

	public void OnSelectionBoxMirrored(Vector3i _selAxis)
	{
		EnumMirrorAlong enumMirrorAlong = EnumMirrorAlong.XAxis;
		if (_selAxis.y != 0)
		{
			enumMirrorAlong = EnumMirrorAlong.YAxis;
		}
		else if (_selAxis.z != 0)
		{
			enumMirrorAlong = EnumMirrorAlong.ZAxis;
		}
		if (this.previewGORot3 != null && this.previewGORot3.transform.childCount > 0)
		{
			this.clipboard.Mirror(enumMirrorAlong);
			this.removeBlockPreview();
			this.createBlockPreviewFrom(this.clipboard);
			return;
		}
		Prefab prefab = new Prefab();
		prefab.CopyFromWorldWithEntities(GameManager.Instance.World, this.SelectionStart, this.SelectionEnd, null);
		prefab.Mirror(enumMirrorAlong);
		prefab.CopyIntoRPC(GameManager.Instance, this.SelectionMin, this.copyPasteAirBlocks);
	}

	public bool OnSelectionBoxDelete(SelectionBox _box, bool _checkCanDeleteOnly)
	{
		return false;
	}

	public bool OnSelectionBoxIsAvailable(EnumSelectionBoxAvailabilities _criteria)
	{
		return _criteria == EnumSelectionBoxAvailabilities.CanResize || _criteria == EnumSelectionBoxAvailabilities.CanMirror;
	}

	public void OnSelectionBoxShowProperties(bool _bVisible, GUIWindowManager _windowManager)
	{
	}

	public void OnSelectionBoxRotated(SelectionBox _box)
	{
		if (this.SelectionLockMode == 2)
		{
			this.rotatePreviewAroundY();
			return;
		}
		this.rotateSelectionAroundY();
	}

	public void OnSelectionBoxUserDataChanged(SelectionBox _box)
	{
	}

	public string GetDebugOutput()
	{
		if (this.SelectionActive)
		{
			return string.Format("Selection pos/size: {0}/{1}", this.SelectionStart.ToString(), this.SelectionSize.ToString());
		}
		return "-";
	}

	public Dictionary<string, NGuiAction> GetActions()
	{
		return this.actions;
	}

	public void LoadPrefabIntoClipboard(Prefab _prefab)
	{
		this.clipboard = _prefab;
		this.SelectionLockMode = 2;
		if (this.SelectionSize != this.clipboard.size)
		{
			this.SelectionEnd = this.SelectionStart + this.clipboard.size - Vector3i.one;
		}
		this.SelectionActive = true;
		this.createBlockPreviewFrom(this.clipboard);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cBlockUndoRedoCount = 100;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Color colActive = new Color(1f, 0f, 0f, 0.5f);

	[PublicizedFrom(EAccessModifier.Private)]
	public static Color colInactive = new Color(0f, 0f, 1f, 0.5f);

	public static BlockToolSelection Instance;

	public Prefab clipboard = new Prefab();

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i m_selectionStartPoint;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i m_SelectionEndPoint;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_iSelectionLockMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<List<BlockChangeInfo>> undoQueue = new List<List<BlockChangeInfo>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<List<BlockChangeInfo>> redoQueue = new List<List<BlockChangeInfo>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastBuildTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public const string SelectionBoxName = "SingleInstance";

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, NGuiAction> actions;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject previewGOParent;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject previewGORot1;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject previewGORot2;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject previewGORot3;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool copyPasteAirBlocks = true;

	public PlayerActionsLocal playerInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 selectionRotCenter;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 offsetToMin;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 offsetToMax;

	[PublicizedFrom(EAccessModifier.Private)]
	public WorldRayHitInfo hitInfo;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bWaitForRelease;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<BlockChangeInfo> undoChanges = new List<BlockChangeInfo>();
}
