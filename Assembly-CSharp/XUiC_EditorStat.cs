using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_EditorStat : XUiController
{
	public bool hasPrefabLoaded
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return PrefabEditModeManager.Instance.IsActive() && PrefabEditModeManager.Instance.VoxelPrefab != null && PrefabEditModeManager.Instance.VoxelPrefab.location.Type != PathAbstractions.EAbstractedLocationType.None;
		}
	}

	public Prefab selectedPrefab
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.GetDynamicPrefabDecorator();
			if (dynamicPrefabDecorator == null)
			{
				return null;
			}
			PrefabInstance activePrefab = dynamicPrefabDecorator.ActivePrefab;
			if (activePrefab == null)
			{
				return null;
			}
			return activePrefab.prefab;
		}
	}

	public bool hasPrefabSelected
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.selectedPrefab != null;
		}
	}

	public static WorldStats ManualStats
	{
		get
		{
			return XUiC_EditorStat.manualStats;
		}
		set
		{
			XUiC_EditorStat.ManualStatsUpdateTime = DateTime.Now;
			XUiC_EditorStat.manualStats = value;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.IsDirty = true;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty || Time.time - this.lastDirtyTime >= 1f)
		{
			this.lootContainers = 0;
			this.fetchLootContainers = 0;
			this.restorePowerNodes = 0;
			this.totalBlockEntities = 0;
			this.hasSelection = false;
			this.selectionSize = default(Vector3i);
			if (this.hasPrefabLoaded)
			{
				SelectionBox selection = SelectionBoxManager.Instance.Selection;
				if (selection != null)
				{
					this.selectionSize = selection.GetScale();
					this.hasSelection = true;
				}
				if (this.hasLootStat)
				{
					PrefabEditModeManager.Instance.GetLootAndFetchLootContainerCount(out this.lootContainers, out this.fetchLootContainers, out this.restorePowerNodes);
				}
				if (this.hasBlockEntitiesStat)
				{
					foreach (ChunkGameObject chunkGameObject in GameManager.Instance.World.m_ChunkManager.GetUsedChunkGameObjects())
					{
						this.totalBlockEntities += this.countBlockEntities(chunkGameObject.transform);
					}
				}
			}
			base.RefreshBindings();
			this.IsDirty = false;
			this.lastDirtyTime = Time.time;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int countBlockEntities(Transform _t)
	{
		int num = 0;
		for (int i = 0; i < _t.childCount; i++)
		{
			Transform child = _t.GetChild(i);
			if (child.name == "_BlockEntities")
			{
				num += child.childCount;
			}
			else
			{
				num += this.countBlockEntities(child);
			}
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		Prefab prefab = (this.hasPrefabLoaded ? PrefabEditModeManager.Instance.VoxelPrefab : this.selectedPrefab);
		bool flag = prefab != null;
		bool flag2 = ((prefab != null) ? prefab.RenderingCostStats : null) != null;
		bool flag3 = XUiC_EditorStat.ManualStats != null;
		uint num = <PrivateImplementationDetails>.ComputeStringHash(_bindingName);
		if (num <= 2389907569U)
		{
			if (num <= 1501340122U)
			{
				if (num <= 1007605620U)
				{
					if (num != 122989552U)
					{
						if (num != 856865979U)
						{
							if (num == 1007605620U)
							{
								if (_bindingName == "has_selected_prefab")
								{
									_value = this.hasPrefabSelected.ToString();
									return true;
								}
							}
						}
						else if (_bindingName == "show_quest_clear_count")
						{
							_value = ((prefab != null) ? prefab.ShowQuestClearCount.ToString() : null) ?? "";
							return true;
						}
					}
					else if (_bindingName == "drawcalls")
					{
						_value = this.batchesFormatter.Format(this.drawcallsSum / 20);
						return true;
					}
				}
				else if (num != 1281920084U)
				{
					if (num != 1330804927U)
					{
						if (num == 1501340122U)
						{
							if (_bindingName == "has_selection")
							{
								_value = this.hasSelection.ToString();
								return true;
							}
						}
					}
					else if (_bindingName == "selection_size")
					{
						_value = (this.hasSelection ? this.selectionSizeFormatter.Format(this.selectionSize) : "-");
						return true;
					}
				}
				else if (_bindingName == "loot_containers")
				{
					this.hasLootStat = true;
					_value = this.lootFormatter.Format(this.lootContainers);
					return true;
				}
			}
			else if (num <= 1871983191U)
			{
				if (num != 1506237936U)
				{
					if (num != 1724383115U)
					{
						if (num == 1871983191U)
						{
							if (_bindingName == "tris")
							{
								_value = "";
								return true;
							}
						}
					}
					else if (_bindingName == "statsVertices")
					{
						_value = (flag2 ? this.statsVertsFormatter.Format(prefab.RenderingCostStats.TotalVertices) : "-");
						return true;
					}
				}
				else if (_bindingName == "has_loaded_prefab")
				{
					_value = this.hasPrefabLoaded.ToString();
					return true;
				}
			}
			else if (num != 2084430828U)
			{
				if (num != 2297592010U)
				{
					if (num == 2389907569U)
					{
						if (_bindingName == "statsTriangles")
						{
							_value = (flag2 ? this.statsTrisFormatter.Format(prefab.RenderingCostStats.TotalTriangles) : "-");
							return true;
						}
					}
				}
				else if (_bindingName == "statsManualUpdateTime")
				{
					_value = (flag3 ? this.statsManualUpdateTimeFormatter.Format(XUiC_EditorStat.ManualStatsUpdateTime.ToLocalTime()) : "<not captured>");
					return true;
				}
			}
			else if (_bindingName == "restorepower_nodes")
			{
				this.hasLootStat = true;
				_value = this.restorepowerFormatter.Format(this.restorePowerNodes);
				return true;
			}
		}
		else if (num <= 3034812289U)
		{
			if (num <= 2659686822U)
			{
				if (num != 2502672311U)
				{
					if (num != 2524445748U)
					{
						if (num == 2659686822U)
						{
							if (_bindingName == "prefab_volume")
							{
								_value = (flag ? prefab.size.Volume().ToString() : "");
								return true;
							}
						}
					}
					else if (_bindingName == "block_entities")
					{
						this.hasBlockEntitiesStat = true;
						_value = this.blockentitiesFormatter.Format(this.totalBlockEntities);
						return true;
					}
				}
				else if (_bindingName == "loaded_prefab_name")
				{
					_value = ((prefab != null) ? prefab.PrefabName : null) ?? "";
					return true;
				}
			}
			else if (num != 2708140049U)
			{
				if (num != 2723945129U)
				{
					if (num == 3034812289U)
					{
						if (_bindingName == "statsManualLightsVolume")
						{
							if (!flag3)
							{
								_value = "-";
								return true;
							}
							float lightsVolume = XUiC_EditorStat.ManualStats.LightsVolume;
							int num2 = ((prefab != null) ? prefab.size.Volume() : 0);
							_value = string.Format("{0:F0} ({1:P1})", lightsVolume, lightsVolume / (float)num2);
							return true;
						}
					}
				}
				else if (_bindingName == "statsManualVertices")
				{
					_value = (flag3 ? this.statsManualVertsFormatter.Format(XUiC_EditorStat.ManualStats.TotalVertices) : "-");
					return true;
				}
			}
			else if (_bindingName == "difficulty_tier")
			{
				_value = ((prefab != null) ? prefab.DifficultyTier.ToString() : null) ?? "";
				return true;
			}
		}
		else if (num <= 3347817698U)
		{
			if (num != 3088255484U)
			{
				if (num != 3167497763U)
				{
					if (num == 3347817698U)
					{
						if (_bindingName == "fetchloot_containers")
						{
							this.hasLootStat = true;
							_value = this.fetchlootFormatter.Format(this.fetchLootContainers);
							return true;
						}
					}
				}
				else if (_bindingName == "verts")
				{
					_value = "";
					return true;
				}
			}
			else if (_bindingName == "sleeper_info")
			{
				_value = ((prefab != null) ? prefab.CalcSleeperInfo() : null) ?? "";
				return true;
			}
		}
		else if (num <= 3964975885U)
		{
			if (num != 3656015704U)
			{
				if (num == 3964975885U)
				{
					if (_bindingName == "prefab_size")
					{
						_value = (flag ? this.prefabSizeFormatter.Format(prefab.size) : "");
						return true;
					}
				}
			}
			else if (_bindingName == "loaded_prefab_changed")
			{
				_value = ((this.hasPrefabLoaded && PrefabEditModeManager.Instance.NeedsSaving) ? "*" : "");
				return true;
			}
		}
		else if (num != 4048861779U)
		{
			if (num == 4061059603U)
			{
				if (_bindingName == "statsManualTriangles")
				{
					_value = (flag3 ? this.statsManualTrisFormatter.Format(XUiC_EditorStat.ManualStats.TotalTriangles) : "-");
					return true;
				}
			}
		}
		else if (_bindingName == "statsLightsVolume")
		{
			if (!flag2)
			{
				_value = "-";
				return true;
			}
			float lightsVolume2 = prefab.RenderingCostStats.LightsVolume;
			int num3 = prefab.size.Volume();
			_value = string.Format("{0:F0} ({1:P1})", lightsVolume2, lightsVolume2 / (float)num3);
			return true;
		}
		return base.GetBindingValueInternal(ref _value, _bindingName);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastDirtyTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasLootStat;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lootContainers;

	[PublicizedFrom(EAccessModifier.Private)]
	public int fetchLootContainers;

	[PublicizedFrom(EAccessModifier.Private)]
	public int restorePowerNodes;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasBlockEntitiesStat;

	[PublicizedFrom(EAccessModifier.Private)]
	public int totalBlockEntities;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasSelection;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i selectionSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int DC_AVERAGE_FRAMES = 20;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int[] drawcallsBuf = new int[20];

	[PublicizedFrom(EAccessModifier.Private)]
	public int drawcallsBufIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int drawcallsSum;

	[PublicizedFrom(EAccessModifier.Private)]
	public static WorldStats manualStats;

	[PublicizedFrom(EAccessModifier.Private)]
	public static DateTime ManualStatsUpdateTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<Vector3i> prefabSizeFormatter = new CachedStringFormatter<Vector3i>((Vector3i _i) => _i.ToString());

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<Vector3i> selectionSizeFormatter = new CachedStringFormatter<Vector3i>((Vector3i _i) => _i.ToString());

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt lootFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt fetchlootFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt restorepowerFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt blockentitiesFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int> vertsFormatter = new CachedStringFormatter<int>((int _i) => ((double)_i).FormatNumberWithMetricPrefix(true, 3));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int> trisFormatter = new CachedStringFormatter<int>((int _i) => ((double)_i).FormatNumberWithMetricPrefix(true, 3));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt batchesFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt statsVertsFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt statsTrisFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<DateTime> statsManualUpdateTimeFormatter = new CachedStringFormatter<DateTime>((DateTime _dt) => _dt.ToString(Utils.StandardCulture));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt statsManualVertsFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt statsManualTrisFormatter = new CachedStringFormatterInt();
}
