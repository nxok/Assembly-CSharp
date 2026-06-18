using System;
using System.Collections.Generic;
using PrefabVolumes;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TriggerProperties : XUiController, ISelectionBoxCallback
{
	public Prefab Prefab
	{
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			this.prefab = value;
			if (this.prefab != null && this.prefab.TriggerLayers.Count == 0)
			{
				this.prefab.AddInitialTriggerLayers();
			}
		}
	}

	public Vector3i BlockPos
	{
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			this.blockPos = value;
			this.blockTrigger = this.BlockChunk.GetBlockTrigger(World.toBlock(this.blockPos));
		}
	}

	public Chunk BlockChunk
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return (Chunk)GameManager.Instance.World.ChunkCache.GetChunkFromWorldPos(this.blockPos);
		}
	}

	public List<byte> TriggersIndices
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_TriggerProperties.EOpenedFor eopenedFor = this.openedFor;
			List<byte> list;
			if (eopenedFor != XUiC_TriggerProperties.EOpenedFor.Block)
			{
				if (eopenedFor != XUiC_TriggerProperties.EOpenedFor.TriggerVolume)
				{
					list = null;
				}
				else
				{
					SelectionBox selectionBox;
					PrefabTriggerVolume prefabTriggerVolume;
					PrefabInstance prefabInstance;
					int num;
					list = (PrefabVolumeManager.TryGetSelectedVolume<PrefabTriggerVolume>(SelectionBoxManager.Instance.CategoryTriggerVolume, out selectionBox, out prefabTriggerVolume, out prefabInstance, out num) ? prefabTriggerVolume.TriggersIndices : null);
				}
			}
			else
			{
				BlockTrigger blockTrigger = this.blockTrigger;
				list = ((blockTrigger != null) ? blockTrigger.TriggersIndices : null);
			}
			return list;
		}
	}

	public List<byte> TriggeredByIndices
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			BlockTrigger blockTrigger = this.blockTrigger;
			if (blockTrigger == null)
			{
				return null;
			}
			return blockTrigger.TriggeredByIndices;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiC_TriggerProperties.ID = base.WindowGroup.Id;
		this.triggersList = base.GetChildById("triggers") as XUiC_PrefabTriggerEditorList;
		if (this.triggersList != null)
		{
			this.triggersList.SelectionChanged += this.TriggersList_SelectionChanged;
			this.triggersList.GetCurrentTriggerIndicesList = () => this.TriggersIndices;
			this.triggersList.GetCurrentPrefabTriggerLayers = delegate
			{
				Prefab prefab = this.prefab;
				if (prefab == null)
				{
					return null;
				}
				return prefab.TriggerLayers;
			};
		}
		XUiController childById = base.GetChildById("addTriggersButton");
		if (childById != null)
		{
			childById.OnPress += this.TriggerOnAddTriggersPressed;
		}
		this.triggeredByList = base.GetChildById("triggeredBy") as XUiC_PrefabTriggerEditorList;
		if (this.triggeredByList != null)
		{
			this.triggeredByList.SelectionChanged += this.TriggeredByList_SelectionChanged;
			this.triggeredByList.GetCurrentTriggerIndicesList = () => this.TriggeredByIndices;
			this.triggeredByList.GetCurrentPrefabTriggerLayers = delegate
			{
				Prefab prefab2 = this.prefab;
				if (prefab2 == null)
				{
					return null;
				}
				return prefab2.TriggerLayers;
			};
		}
		XUiController childById2 = base.GetChildById("addTriggeredByButton");
		if (childById2 != null)
		{
			childById2.OnPress += this.TriggerOnAddTriggersPressed;
		}
		XUiController xuiController = base.GetChildById("exclude");
		if (xuiController != null)
		{
			xuiController.OnPress += this.triggerExclude_OnPressed;
		}
		xuiController = base.GetChildById("operation");
		if (xuiController != null)
		{
			xuiController.OnPress += this.triggerOperation_OnPressed;
		}
		xuiController = base.GetChildById("unlock");
		if (xuiController != null)
		{
			xuiController.OnPress += this.triggerUnlock_OnPressed;
		}
		if (SelectionBoxManager.Instance != null)
		{
			SelectionBoxManager.Instance.CategoryTriggerVolume.SetCallback(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void triggerExclude_OnPressed(XUiController _controller, int _button)
	{
		if (this.blockTrigger == null)
		{
			return;
		}
		this.blockTrigger.ExcludeIcon = !this.blockTrigger.ExcludeIcon;
		this.setChunkModified();
		base.RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void triggerOperation_OnPressed(XUiController _controller, int _button)
	{
		if (this.blockTrigger == null)
		{
			return;
		}
		this.blockTrigger.UseOrForMultipleTriggers = !this.blockTrigger.UseOrForMultipleTriggers;
		this.setChunkModified();
		base.RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void triggerUnlock_OnPressed(XUiController _controller, int _button)
	{
		if (this.blockTrigger == null)
		{
			return;
		}
		this.blockTrigger.Unlock = !this.blockTrigger.Unlock;
		this.setChunkModified();
		base.RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setChunkModified()
	{
		PrefabEditModeManager.Instance.NeedsSaving = true;
		this.BlockChunk.isModified = true;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		if (SelectionBoxManager.Instance != null)
		{
			SelectionBoxManager.Instance.CategoryTriggerVolume.SetCallback(null);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TriggerOnAddTriggersPressed(XUiController _sender, int _mouseButton)
	{
		this.prefab.AddNewTriggerLayer();
		this.triggersList.RebuildList(false);
		this.triggeredByList.RebuildList(false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TriggersList_SelectionChanged(XUiC_List<XUiC_PrefabTriggerEditorList.PrefabTriggerEntry> _list, XUiC_PrefabTriggerEditorList.PrefabTriggerEntry _previousEntry, XUiC_PrefabTriggerEditorList.PrefabTriggerEntry _newEntry)
	{
		if (_newEntry == null)
		{
			return;
		}
		byte triggerLayer = _newEntry.TriggerLayer;
		if (this.openedFor == XUiC_TriggerProperties.EOpenedFor.TriggerVolume)
		{
			this.handleTriggerVolumeSetting(triggerLayer, true);
		}
		else
		{
			this.handleTriggersSetting(triggerLayer, true, GameManager.Instance.World, this.blockPos);
		}
		_newEntry.UiDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TriggeredByList_SelectionChanged(XUiC_List<XUiC_PrefabTriggerEditorList.PrefabTriggerEntry> _list, XUiC_PrefabTriggerEditorList.PrefabTriggerEntry _previousEntry, XUiC_PrefabTriggerEditorList.PrefabTriggerEntry _newEntry)
	{
		if (_newEntry == null)
		{
			return;
		}
		byte triggerLayer = _newEntry.TriggerLayer;
		if (this.openedFor == XUiC_TriggerProperties.EOpenedFor.TriggerVolume)
		{
			this.handleTriggerVolumeSetting(triggerLayer, false);
		}
		else
		{
			this.handleTriggersSetting(triggerLayer, false, GameManager.Instance.World, this.blockPos);
		}
		_newEntry.UiDirty = true;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.IsDirty = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		this.Prefab = null;
		this.blockTrigger = null;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty)
		{
			base.RefreshBindings();
			this.IsDirty = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (_bindingName == "triggers_enabled")
		{
			_value = this.showTriggers.ToString();
			return true;
		}
		if (_bindingName == "triggeredby_enabled")
		{
			_value = this.showTriggeredBy.ToString();
			return true;
		}
		if (_bindingName == "excludeTickmarkSelected")
		{
			_value = ((this.blockTrigger != null) ? this.blockTrigger.ExcludeIcon.ToString() : "false");
			return true;
		}
		if (_bindingName == "operationTickmarkSelected")
		{
			_value = ((this.blockTrigger != null) ? this.blockTrigger.UseOrForMultipleTriggers.ToString() : "false");
			return true;
		}
		if (!(_bindingName == "unlockTickmarkSelected"))
		{
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
		_value = ((this.blockTrigger != null) ? this.blockTrigger.Unlock.ToString() : "false");
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void handleTriggersSetting(byte _triggerLayer, bool _isTriggers, World _world, Vector3i _blockPos)
	{
		if (!_world.IsEditor())
		{
			return;
		}
		Chunk blockChunk = this.BlockChunk;
		this.blockTrigger = blockChunk.GetBlockTrigger(World.toBlock(_blockPos));
		if (_triggerLayer == 0)
		{
			if (this.blockTrigger != null)
			{
				blockChunk.RemoveBlockTrigger(this.blockTrigger);
				this.blockTrigger = null;
			}
		}
		else
		{
			if (this.blockTrigger == null)
			{
				this.blockTrigger = new BlockTrigger(blockChunk)
				{
					LocalChunkPos = World.toBlock(_blockPos)
				};
				blockChunk.AddBlockTrigger(this.blockTrigger);
			}
			if (_isTriggers)
			{
				this.blockTrigger.ToggleTriggersFlag(_triggerLayer);
			}
			else
			{
				this.blockTrigger.ToggleTriggeredByFlag(_triggerLayer);
			}
			if (!this.blockTrigger.HasAnyTriggers() && !this.blockTrigger.HasAnyTriggeredBy() && this.blockTrigger != null)
			{
				blockChunk.RemoveBlockTrigger(this.blockTrigger);
				this.blockTrigger = null;
			}
		}
		this.setChunkModified();
		BlockTrigger blockTrigger = this.blockTrigger;
		if (blockTrigger == null)
		{
			return;
		}
		blockTrigger.TriggerUpdated(null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void handleTriggerVolumeSetting(byte _triggerLayer, bool _isTriggers)
	{
		SelectionBox selectionBox;
		PrefabTriggerVolume prefabTriggerVolume;
		PrefabInstance prefabInstance;
		int num;
		if (!PrefabVolumeManager.TryGetSelectedVolume<PrefabTriggerVolume>(SelectionBoxManager.Instance.CategoryTriggerVolume, out selectionBox, out prefabTriggerVolume, out prefabInstance, out num))
		{
			return;
		}
		if (_isTriggers)
		{
			PrefabTriggerVolume prefabTriggerVolume2 = prefabTriggerVolume.CloneGeneric();
			prefabTriggerVolume2.ToggleTriggersFlag(_triggerLayer);
			PrefabVolumeManager.Instance.UpdatePropertiesServer(prefabInstance.id, num, prefabTriggerVolume2);
		}
		BlockTrigger blockTrigger = this.blockTrigger;
		if (blockTrigger == null)
		{
			return;
		}
		blockTrigger.TriggerUpdated(null);
	}

	public static void Show(XUi _xui, Vector3i _blockPos, bool _showTriggers, bool _showTriggeredBy)
	{
		XUiC_TriggerProperties childByType = ((XUiWindowGroup)_xui.playerUI.windowManager.GetWindow(XUiC_TriggerProperties.ID)).Controller.GetChildByType<XUiC_TriggerProperties>();
		childByType.openedFor = XUiC_TriggerProperties.EOpenedFor.Block;
		childByType.Prefab = PrefabEditModeManager.Instance.VoxelPrefab;
		childByType.BlockPos = _blockPos;
		childByType.showTriggers = _showTriggers;
		childByType.showTriggeredBy = _showTriggeredBy;
		childByType.RefreshBindings();
		_xui.playerUI.windowManager.Open(XUiC_TriggerProperties.ID, true);
	}

	public bool OnSelectionBoxActivated(SelectionBox _box, bool _bActivated)
	{
		return true;
	}

	public void OnSelectionBoxMoved(SelectionBox _box, Vector3 _moveVector)
	{
		PrefabVolumeManager.Instance.SelectionBoxMoved(PrefabVolumeAbs.EVolumeType.Trigger, _box, _moveVector);
	}

	public void OnSelectionBoxSized(SelectionBox _box, int _dTop, int _dBottom, int _dNorth, int _dSouth, int _dEast, int _dWest)
	{
		PrefabVolumeManager.Instance.SelectionBoxSized(PrefabVolumeAbs.EVolumeType.Trigger, _box, _dTop, _dBottom, _dNorth, _dSouth, _dEast, _dWest);
	}

	public void OnSelectionBoxMirrored(Vector3i _axis)
	{
	}

	public bool OnSelectionBoxDelete(SelectionBox _box, bool _checkCanDeleteOnly)
	{
		int num;
		PrefabInstance prefabInstance;
		return PrefabVolumeManager.GetPrefabIdAndVolumeId(_box.name, out num, out prefabInstance) && PrefabVolumeManager.Instance.SelectionBoxDelete(PrefabVolumeAbs.EVolumeType.Trigger, _box, _checkCanDeleteOnly);
	}

	public bool OnSelectionBoxIsAvailable(EnumSelectionBoxAvailabilities _criteria)
	{
		return _criteria == EnumSelectionBoxAvailabilities.CanShowProperties || _criteria == EnumSelectionBoxAvailabilities.CanResize;
	}

	public void OnSelectionBoxShowProperties(bool _bVisible, GUIWindowManager _windowManager)
	{
		SelectionBox selectionBox;
		PrefabTriggerVolume prefabTriggerVolume;
		PrefabInstance prefabInstance;
		int num;
		if (!PrefabVolumeManager.TryGetSelectedVolume<PrefabTriggerVolume>(SelectionBoxManager.Instance.CategoryTriggerVolume, out selectionBox, out prefabTriggerVolume, out prefabInstance, out num))
		{
			return;
		}
		this.openedFor = XUiC_TriggerProperties.EOpenedFor.TriggerVolume;
		this.Prefab = prefabInstance.prefab;
		this.showTriggers = true;
		this.showTriggeredBy = false;
		_windowManager.Open(this.windowGroup, true);
	}

	public void OnSelectionBoxRotated(SelectionBox _box)
	{
	}

	public void OnSelectionBoxUserDataChanged(SelectionBox _box)
	{
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PrefabTriggerEditorList triggersList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PrefabTriggerEditorList triggeredByList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TriggerProperties.EOpenedFor openedFor;

	[PublicizedFrom(EAccessModifier.Private)]
	public Prefab prefab;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i blockPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockTrigger blockTrigger;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showTriggers = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showTriggeredBy = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public enum EOpenedFor
	{
		Block,
		TriggerVolume
	}
}
