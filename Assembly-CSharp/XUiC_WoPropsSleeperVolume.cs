using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using PrefabVolumes;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_WoPropsSleeperVolume : XUiController, ISelectionBoxCallback, ISelectionCategoryCallback
{
	public static XUiC_WoPropsSleeperVolume Instance
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public List<byte> TriggeredByIndices
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			SelectionBox selectionBox;
			PrefabSleeperVolume prefabSleeperVolume;
			PrefabInstance prefabInstance;
			int num;
			if (!PrefabVolumeManager.TryGetSelectedVolume<PrefabSleeperVolume>(SelectionBoxManager.Instance.CategorySleeperVolume, out selectionBox, out prefabSleeperVolume, out prefabInstance, out num))
			{
				return null;
			}
			return prefabSleeperVolume.triggeredByIndices;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiC_WoPropsSleeperVolume.ID = base.WindowGroup.Id;
		XUiC_WoPropsSleeperVolume.Instance = this;
		this.labelIndex = base.GetChildById("labelIndex").ViewComponent as XUiV_Label;
		this.labelPosition = base.GetChildById("labelPosition").ViewComponent as XUiV_Label;
		this.labelSize = base.GetChildById("labelSize").ViewComponent as XUiV_Label;
		this.labelSleeperCount = base.GetChildById("labelSleeperCount").ViewComponent as XUiV_Label;
		this.labelGroup = base.GetChildById("labelGroup").ViewComponent as XUiV_Label;
		this.txtGroupId = (XUiC_TextInput)base.GetChildById("groupId");
		this.txtGroupId.OnChangeHandler += this.TxtGroupId_OnChangeHandler;
		this.cbxPriority = (XUiC_ComboBoxBool)base.GetChildById("cbxPriority");
		this.cbxPriority.OnValueChanged += this.CbxPriority_OnValueChanged;
		this.cbxQuestExclude = (XUiC_ComboBoxBool)base.GetChildById("cbxQuestExclude");
		this.cbxQuestExclude.OnValueChanged += this.CbxQuestExclude_OnValueChanged;
		this.cbxCountPreset = (XUiC_ComboBoxList<XUiC_WoPropsSleeperVolume.CountPreset>)base.GetChildById("cbxCountPreset");
		this.cbxCountPreset.OnValueChanged += this.CbxCountPreset_OnValueChanged;
		this.cbxCountPreset.Elements.Add(new XUiC_WoPropsSleeperVolume.CountPreset(-1, -1, "Custom"));
		this.cbxCountPreset.Elements.Add(new XUiC_WoPropsSleeperVolume.CountPreset(1, 2, "12"));
		this.cbxCountPreset.Elements.Add(new XUiC_WoPropsSleeperVolume.CountPreset(2, 3, "23"));
		this.cbxCountPreset.Elements.Add(new XUiC_WoPropsSleeperVolume.CountPreset(3, 4, "34"));
		this.cbxCountPreset.Elements.Add(new XUiC_WoPropsSleeperVolume.CountPreset(4, 5, "45"));
		this.cbxCountPreset.Elements.Add(new XUiC_WoPropsSleeperVolume.CountPreset(5, 6, "56"));
		this.cbxCountPreset.Elements.Add(new XUiC_WoPropsSleeperVolume.CountPreset(6, 7, "67"));
		this.cbxCountPreset.Elements.Add(new XUiC_WoPropsSleeperVolume.CountPreset(7, 8, "78"));
		this.cbxCountPreset.Elements.Add(new XUiC_WoPropsSleeperVolume.CountPreset(8, 9, "89"));
		this.cbxCountPreset.Elements.Add(new XUiC_WoPropsSleeperVolume.CountPreset(9, 10, "910"));
		this.cbxCountPreset.MinIndex = 1;
		this.txtSpawnMin = (XUiC_TextInput)base.GetChildById("spawnMin");
		this.txtSpawnMin.OnChangeHandler += delegate(XUiController _, string _text, bool _fromCode)
		{
			this.TxtSpawnMinMax_OnChangeHandler(_text, _fromCode, false);
		};
		this.txtSpawnMax = (XUiC_TextInput)base.GetChildById("spawnMax");
		this.txtSpawnMax.OnChangeHandler += delegate(XUiController _, string _text, bool _fromCode)
		{
			this.TxtSpawnMinMax_OnChangeHandler(_text, _fromCode, true);
		};
		this.cbxTrigger = (XUiC_ComboBoxEnum<SleeperVolume.ETriggerType>)base.GetChildById("cbxTrigger");
		this.cbxTrigger.OnValueChanged += this.CbxTrigger_OnValueChanged;
		this.txtMinScript = (XUiC_TextInput)base.GetChildById("script");
		this.txtMinScript.OnChangeHandler += this.TxtMinScript_OnChangeHandler;
		this.spawnersList = (XUiC_SpawnersList)base.GetChildById("spawners");
		this.spawnersList.SelectionChanged += this.SpawnersList_SelectionChanged;
		this.spawnersList.SelectableEntries = false;
		this.triggersBox = base.GetChildById("triggersBox").ViewComponent;
		this.triggeredByList = base.GetChildById("triggeredBy") as XUiC_PrefabTriggerEditorList;
		if (this.triggeredByList != null)
		{
			this.triggeredByList.SelectionChanged += this.TriggeredByList_SelectionChanged;
			this.triggeredByList.GetCurrentTriggerIndicesList = () => this.TriggeredByIndices;
			this.triggeredByList.GetCurrentPrefabTriggerLayers = delegate
			{
				SelectionBox selectionBox;
				PrefabSleeperVolume prefabSleeperVolume;
				PrefabInstance prefabInstance;
				int num;
				if (!PrefabVolumeManager.TryGetSelectedVolume<PrefabSleeperVolume>(SelectionBoxManager.Instance.CategorySleeperVolume, out selectionBox, out prefabSleeperVolume, out prefabInstance, out num))
				{
					return null;
				}
				return prefabInstance.prefab.TriggerLayers;
			};
		}
		XUiController childById = base.GetChildById("addTriggeredByButton");
		if (childById != null)
		{
			childById.OnPress += this.TriggerOnAddTriggersPressed;
		}
		XUiC_SimpleButton xuiC_SimpleButton = base.GetChildById("btnCloneVolume") as XUiC_SimpleButton;
		if (xuiC_SimpleButton != null)
		{
			xuiC_SimpleButton.OnPressed += this.BtnCloneVolume_OnPress;
		}
		if (SelectionBoxManager.Instance != null)
		{
			SelectionCategory categorySleeperVolume = SelectionBoxManager.Instance.CategorySleeperVolume;
			categorySleeperVolume.BoxCallbacks = this;
			categorySleeperVolume.CategoryCallbacks = this;
			categorySleeperVolume.CheckKeysCallback = new Action(SleeperVolumeToolManager.CheckKeys);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCloneVolume_OnPress(XUiController _sender, int _mouseButton)
	{
		SelectionBox selectionBox;
		PrefabSleeperVolume prefabSleeperVolume;
		PrefabInstance prefabInstance;
		int num;
		if (!PrefabVolumeManager.TryGetSelectedVolume<PrefabSleeperVolume>(SelectionBoxManager.Instance.CategorySleeperVolume, out selectionBox, out prefabSleeperVolume, out prefabInstance, out num))
		{
			return;
		}
		PrefabVolumeManager.Instance.CloneVolumeServer(PrefabVolumeAbs.EVolumeType.Sleeper, prefabInstance.id, num, new Vector3i(0, prefabSleeperVolume.size.y + 1, 0));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool modifyCurrentVolume(Action<PrefabSleeperVolume> _modifyAction)
	{
		SelectionBox selectionBox;
		PrefabSleeperVolume prefabSleeperVolume;
		PrefabInstance prefabInstance;
		int num;
		if (!PrefabVolumeManager.TryGetSelectedVolume<PrefabSleeperVolume>(SelectionBoxManager.Instance.CategorySleeperVolume, out selectionBox, out prefabSleeperVolume, out prefabInstance, out num))
		{
			return false;
		}
		PrefabSleeperVolume prefabSleeperVolume2 = prefabSleeperVolume.CloneGeneric();
		_modifyAction(prefabSleeperVolume2);
		PrefabVolumeManager.Instance.UpdatePropertiesServer(prefabInstance.id, num, prefabSleeperVolume2);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtGroupId_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (_changeFromCode || _text.Length <= 0)
		{
			return;
		}
		this.modifyCurrentVolume(delegate(PrefabSleeperVolume _volumeCopy)
		{
			_volumeCopy.groupId = StringParsers.ParseSInt16(_text, 0, -1, NumberStyles.Integer);
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxPriority_OnValueChanged(XUiController _sender, bool _oldValue, bool _newValue)
	{
		this.modifyCurrentVolume(delegate(PrefabSleeperVolume _volumeCopy)
		{
			_volumeCopy.isPriority = _newValue;
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxQuestExclude_OnValueChanged(XUiController _sender, bool _oldValue, bool _newValue)
	{
		this.modifyCurrentVolume(delegate(PrefabSleeperVolume _volumeCopy)
		{
			_volumeCopy.isQuestExclude = _newValue;
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int findCountPresetIndex(int _min, int _max)
	{
		for (int i = 0; i < this.cbxCountPreset.Elements.Count; i++)
		{
			if ((int)this.cbxCountPreset.Elements[i].Min == _min && (int)this.cbxCountPreset.Elements[i].Max == _max)
			{
				return i;
			}
		}
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateCountPresetLabel()
	{
		SelectionBox selectionBox;
		PrefabSleeperVolume prefabSleeperVolume;
		PrefabInstance prefabInstance;
		int num;
		if (!PrefabVolumeManager.TryGetSelectedVolume<PrefabSleeperVolume>(SelectionBoxManager.Instance.CategorySleeperVolume, out selectionBox, out prefabSleeperVolume, out prefabInstance, out num))
		{
			return;
		}
		int num2 = this.findCountPresetIndex((int)prefabSleeperVolume.spawnCountMin, (int)prefabSleeperVolume.spawnCountMax);
		if (num2 < 0)
		{
			this.cbxCountPreset.MinIndex = 0;
			this.cbxCountPreset.SelectedIndex = 0;
			return;
		}
		this.cbxCountPreset.MinIndex = 1;
		this.cbxCountPreset.SelectedIndex = num2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxCountPreset_OnValueChanged(XUiController _sender, XUiC_WoPropsSleeperVolume.CountPreset _oldValue, XUiC_WoPropsSleeperVolume.CountPreset _newValue)
	{
		this.cbxCountPreset.MinIndex = 1;
		this.modifyCurrentVolume(delegate(PrefabSleeperVolume _volumeCopy)
		{
			_volumeCopy.spawnCountMin = _newValue.Min;
			_volumeCopy.spawnCountMax = _newValue.Max;
		});
		this.updateCountPresetLabel();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtSpawnMinMax_OnChangeHandler(string _text, bool _changeFromCode, bool _isMax)
	{
		if (_changeFromCode || _text.Length <= 0)
		{
			return;
		}
		this.modifyCurrentVolume(delegate(PrefabSleeperVolume _volumeCopy)
		{
			short num = StringParsers.ParseSInt16(_text, 0, -1, NumberStyles.Integer);
			if (_isMax)
			{
				_volumeCopy.spawnCountMax = num;
				return;
			}
			_volumeCopy.spawnCountMin = num;
		});
		this.updateCountPresetLabel();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxTrigger_OnValueChanged(XUiController _sender, SleeperVolume.ETriggerType _oldValue, SleeperVolume.ETriggerType _newValue)
	{
		this.modifyCurrentVolume(delegate(PrefabSleeperVolume _volumeCopy)
		{
			_volumeCopy.SetTrigger(_newValue);
		});
		this.triggersBox.IsVisible = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtMinScript_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (_changeFromCode)
		{
			return;
		}
		this.modifyCurrentVolume(delegate(PrefabSleeperVolume _volumeCopy)
		{
			_volumeCopy.minScript = MinScript.ConvertFromUIText(_text);
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SpawnersList_SelectionChanged(XUiC_List<XUiC_SpawnersList.SpawnerEntry> _list, XUiC_SpawnersList.SpawnerEntry _previousEntry, XUiC_SpawnersList.SpawnerEntry _newEntry)
	{
		string spawnerName = null;
		if (_newEntry != null)
		{
			spawnerName = _newEntry.name;
		}
		this.modifyCurrentVolume(delegate(PrefabSleeperVolume _volumeCopy)
		{
			_volumeCopy.groupName = spawnerName;
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TriggeredByList_SelectionChanged(XUiC_List<XUiC_PrefabTriggerEditorList.PrefabTriggerEntry> _list, XUiC_PrefabTriggerEditorList.PrefabTriggerEntry _previousEntry, XUiC_PrefabTriggerEditorList.PrefabTriggerEntry _newEntry)
	{
		if (_newEntry == null)
		{
			return;
		}
		this.modifyCurrentVolume(delegate(PrefabSleeperVolume _volumeCopy)
		{
			_volumeCopy.ToggleTriggeredByFlag(_newEntry.TriggerLayer);
		});
		_newEntry.UiDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TriggerOnAddTriggersPressed(XUiController _sender, int _mouseButton)
	{
		SelectionBox selectionBox;
		PrefabSleeperVolume prefabSleeperVolume;
		PrefabInstance prefabInstance;
		int num;
		if (!PrefabVolumeManager.TryGetSelectedVolume<PrefabSleeperVolume>(SelectionBoxManager.Instance.CategorySleeperVolume, out selectionBox, out prefabSleeperVolume, out prefabInstance, out num))
		{
			return;
		}
		prefabInstance.prefab.AddNewTriggerLayer();
		this.triggeredByList.RebuildList(false);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		SelectionBox selectionBox;
		PrefabSleeperVolume prefabSleeperVolume;
		PrefabInstance prefabInstance;
		int num;
		PrefabVolumeManager.TryGetSelectedVolume<PrefabSleeperVolume>(SelectionBoxManager.Instance.CategorySleeperVolume, out selectionBox, out prefabSleeperVolume, out prefabInstance, out num);
		if (((prefabInstance != null) ? prefabInstance.prefab : null) != null && prefabInstance.prefab.TriggerLayers.Count == 0)
		{
			prefabInstance.prefab.AddInitialTriggerLayers();
		}
		this.updateCountPresetLabel();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		XUiC_WoPropsSleeperVolume.VolumeStats volumeStats;
		if (this.getSelectedVolumeStats(out volumeStats))
		{
			this.txtGroupId.Text = volumeStats.GroupId.ToString();
			this.cbxPriority.Value = volumeStats.IsPriority;
			this.cbxQuestExclude.Value = volumeStats.IsQuestExclude;
			this.labelIndex.Text = volumeStats.Index.ToString();
			this.labelPosition.Text = volumeStats.VolumePos.ToString();
			this.labelSize.Text = volumeStats.Size.ToString();
			this.labelSleeperCount.Text = volumeStats.SleeperCount.ToString();
			this.labelGroup.Text = volumeStats.GroupName;
			this.txtSpawnMin.Text = volumeStats.SpawnCountMin.ToString();
			this.txtSpawnMax.Text = volumeStats.SpawnCountMax.ToString();
			this.cbxTrigger.Value = volumeStats.TriggerType;
			this.txtMinScript.Text = MinScript.ConvertToUIText(volumeStats.MinScript);
		}
		else
		{
			this.<Update>g__SetDefaultBoxContents|42_0();
		}
		this.triggersBox.IsVisible = true;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		if (SelectionBoxManager.Instance != null)
		{
			SelectionCategory categorySleeperVolume = SelectionBoxManager.Instance.CategorySleeperVolume;
			categorySleeperVolume.BoxCallbacks = null;
			categorySleeperVolume.CategoryCallbacks = null;
			categorySleeperVolume.CheckKeysCallback = null;
		}
		XUiC_WoPropsSleeperVolume.Instance = null;
	}

	public bool OnSelectionBoxActivated(SelectionBox _box, bool _bActivated)
	{
		if (_bActivated)
		{
			this.bSleeperVolumeChanged = true;
		}
		SleeperVolumeToolManager.SelectionChanged(_bActivated ? _box : null);
		return true;
	}

	public void SleeperVolumeChanged(int _prefabInstanceId, int _volumeId)
	{
		SelectionBox selectionBox;
		PrefabSleeperVolume prefabSleeperVolume;
		PrefabInstance prefabInstance;
		int num;
		if (!PrefabVolumeManager.TryGetSelectedVolume<PrefabSleeperVolume>(SelectionBoxManager.Instance.CategorySleeperVolume, out selectionBox, out prefabSleeperVolume, out prefabInstance, out num))
		{
			return;
		}
		if (prefabInstance.id != _prefabInstanceId || num != _volumeId)
		{
			return;
		}
		this.bSleeperVolumeChanged = true;
	}

	public void OnSelectionBoxMoved(SelectionBox _box, Vector3 _moveVector)
	{
		PrefabVolumeManager.Instance.SelectionBoxMoved(PrefabVolumeAbs.EVolumeType.Sleeper, _box, _moveVector);
		SleeperVolumeToolManager.UpdateAllVisuals();
	}

	public void OnSelectionBoxSized(SelectionBox _box, int _dTop, int _dBottom, int _dNorth, int _dSouth, int _dEast, int _dWest)
	{
		PrefabVolumeManager.Instance.SelectionBoxSized(PrefabVolumeAbs.EVolumeType.Sleeper, _box, _dTop, _dBottom, _dNorth, _dSouth, _dEast, _dWest);
		SleeperVolumeToolManager.UpdateAllVisuals();
	}

	public void OnSelectionBoxMirrored(Vector3i _axis)
	{
		SleeperVolumeToolManager.UpdateAllVisuals();
	}

	public bool OnSelectionBoxDelete(SelectionBox _box, bool _checkCanDeleteOnly)
	{
		int num;
		PrefabInstance prefabInstance;
		return PrefabVolumeManager.GetPrefabIdAndVolumeId(_box.name, out num, out prefabInstance) && PrefabVolumeManager.Instance.SelectionBoxDelete(PrefabVolumeAbs.EVolumeType.Sleeper, _box, _checkCanDeleteOnly);
	}

	public bool OnSelectionBoxIsAvailable(EnumSelectionBoxAvailabilities _criteria)
	{
		return _criteria == EnumSelectionBoxAvailabilities.CanShowProperties || _criteria == EnumSelectionBoxAvailabilities.CanResize;
	}

	public void OnSelectionBoxShowProperties(bool _bVisible, GUIWindowManager _windowManager)
	{
		_windowManager.SwitchVisible(this.windowGroup, true);
	}

	public void OnSelectionBoxRotated(SelectionBox _box)
	{
	}

	public void OnSelectionBoxUserDataChanged(SelectionBox _box)
	{
		SleeperVolumeToolManager.RegisterSleeperVolume(_box);
		SleeperVolumeToolManager.UpdateAllVisuals();
	}

	public void OnSelectionCategoryVisibilityChanged(SelectionCategory _category, bool _visible)
	{
		SleeperVolumeToolManager.SetVisible(_visible);
	}

	public void OnSelectionCategoryBoxAdded(SelectionBox _box)
	{
		SleeperVolumeToolManager.RegisterSleeperVolume(_box);
		SleeperVolumeToolManager.UpdateAllVisuals();
	}

	public void OnSelectionCategoryBoxRemoved(SelectionBox _box)
	{
		SleeperVolumeToolManager.UnRegisterSleeperVolume(_box);
		SleeperVolumeToolManager.UpdateAllVisuals();
	}

	public void OnSelectionCategoryCleared(SelectionCategory _category)
	{
		SleeperVolumeToolManager.ClearSleeperVolumes();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void handleSleeperVolumeChanged()
	{
		if (!this.bSleeperVolumeChanged)
		{
			return;
		}
		SelectionBox selectionBox;
		PrefabSleeperVolume prefabSleeperVolume;
		PrefabInstance prefabInstance;
		int num;
		if (!PrefabVolumeManager.TryGetSelectedVolume<PrefabSleeperVolume>(SelectionBoxManager.Instance.CategorySleeperVolume, out selectionBox, out prefabSleeperVolume, out prefabInstance, out num))
		{
			return;
		}
		this.bSleeperVolumeChanged = false;
		prefabInstance.prefab.CountSleeperSpawnsInVolume(GameManager.Instance.World, prefabInstance.boundingBoxPosition, num);
		this.updateCountPresetLabel();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool getSelectedVolumeStats(out XUiC_WoPropsSleeperVolume.VolumeStats _stats)
	{
		_stats = default(XUiC_WoPropsSleeperVolume.VolumeStats);
		SelectionBox selectionBox;
		PrefabSleeperVolume prefabSleeperVolume;
		PrefabInstance prefabInstance;
		int num;
		if (!PrefabVolumeManager.TryGetSelectedVolume<PrefabSleeperVolume>(SelectionBoxManager.Instance.CategorySleeperVolume, out selectionBox, out prefabSleeperVolume, out prefabInstance, out num))
		{
			return false;
		}
		this.handleSleeperVolumeChanged();
		_stats = new XUiC_WoPropsSleeperVolume.VolumeStats(num, prefabSleeperVolume.startPos, prefabInstance.boundingBoxPosition + prefabSleeperVolume.startPos, prefabSleeperVolume.size, prefabSleeperVolume.groupId, GameStageGroup.MakeDisplayName(prefabSleeperVolume.groupName), prefabInstance.prefab.Transient_NumSleeperSpawns, (int)prefabSleeperVolume.spawnCountMin, (int)prefabSleeperVolume.spawnCountMax, prefabSleeperVolume.isPriority, prefabSleeperVolume.isQuestExclude, (SleeperVolume.ETriggerType)(prefabSleeperVolume.flags & 7), prefabSleeperVolume.minScript);
		return true;
	}

	public static bool GetSelectedVolumeStats(out XUiC_WoPropsSleeperVolume.VolumeStats _stats)
	{
		return XUiC_WoPropsSleeperVolume.Instance.getSelectedVolumeStats(out _stats);
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public void <Update>g__SetDefaultBoxContents|42_0()
	{
		this.txtGroupId.Text = string.Empty;
		this.cbxPriority.Value = false;
		this.cbxQuestExclude.Value = false;
		this.labelIndex.Text = string.Empty;
		this.labelPosition.Text = string.Empty;
		this.labelSize.Text = string.Empty;
		this.labelSleeperCount.Text = string.Empty;
		this.labelGroup.Text = string.Empty;
		this.txtSpawnMin.Text = string.Empty;
		this.txtSpawnMax.Text = string.Empty;
		this.cbxTrigger.Value = SleeperVolume.ETriggerType.Active;
		this.txtMinScript.Text = string.Empty;
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiView triggersBox;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label labelIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label labelPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label labelSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label labelSleeperCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label labelGroup;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtGroupId;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool cbxPriority;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool cbxQuestExclude;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<XUiC_WoPropsSleeperVolume.CountPreset> cbxCountPreset;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtSpawnMin;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtSpawnMax;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxEnum<SleeperVolume.ETriggerType> cbxTrigger;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtMinScript;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SpawnersList spawnersList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PrefabTriggerEditorList triggeredByList;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bSleeperVolumeChanged;

	public struct VolumeStats
	{
		public VolumeStats(int _index, Vector3i _volumePos, Vector3i _worldPos, Vector3i _size, short _groupId, string _groupName, int _sleeperCount, int _spawnCountMin, int _spawnCountMax, bool _isPriority, bool _isQuestExclude, SleeperVolume.ETriggerType _triggerType, string _minScript)
		{
			this.Index = _index;
			this.VolumePos = _volumePos;
			this.WorldPos = _worldPos;
			this.Size = _size;
			this.GroupId = _groupId;
			this.GroupName = _groupName;
			this.SleeperCount = _sleeperCount;
			this.SpawnCountMin = _spawnCountMin;
			this.SpawnCountMax = _spawnCountMax;
			this.IsPriority = _isPriority;
			this.IsQuestExclude = _isQuestExclude;
			this.TriggerType = _triggerType;
			this.MinScript = _minScript;
		}

		public readonly int Index;

		public readonly Vector3i VolumePos;

		public readonly Vector3i WorldPos;

		public readonly Vector3i Size;

		public readonly short GroupId;

		public readonly string GroupName;

		public readonly int SleeperCount;

		public readonly int SpawnCountMin;

		public readonly int SpawnCountMax;

		public readonly bool IsPriority;

		public readonly bool IsQuestExclude;

		public readonly SleeperVolume.ETriggerType TriggerType;

		public readonly string MinScript;
	}

	public readonly struct CountPreset
	{
		public CountPreset(short _min, short _max, string _name)
		{
			this.Min = _min;
			this.Max = _max;
			this.name = _name;
		}

		public override string ToString()
		{
			return this.name;
		}

		public readonly short Min;

		public readonly short Max;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string name;
	}
}
