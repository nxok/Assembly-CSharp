using System;
using System.Collections.Generic;
using System.Text;
using DynamicMusic;
using GamePath;
using MusicUtils.Enums;
using Pathfinding;
using UnityEngine;

public class NGuiWdwDebugPanels : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public void Awake()
	{
		this.playerUI = base.GetComponentInParent<LocalPlayerUI>();
		if (this.playerUI.IsCleanCopy || LocalPlayerUI.CreatingCleanCopy)
		{
			return;
		}
		NGuiWdwDebugPanels.guiStyleDebug.fontSize = 12;
		NGuiWdwDebugPanels.guiStyleDebug.fontStyle = 1;
		this.debugData = NGuiWdwDebugPanels.EDebugDataType.Off;
		this.guiFPS = base.transform.GetComponentInChildren<GUIFPS>();
		NGuiAction nguiAction = new NGuiAction("Show Debug Data", PlayerActionsGlobal.Instance.ShowDebugData);
		nguiAction.SetClickActionDelegate(delegate
		{
			this.debugData = this.debugData.CycleEnum(false, true);
		});
		nguiAction.SetIsEnabledDelegate(() => GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled));
		NGuiAction nguiAction2 = new NGuiAction("ShowFPS", PlayerActionsGlobal.Instance.ShowFPS);
		nguiAction2.SetClickActionDelegate(delegate
		{
			this.performanceType = this.performanceType.CycleEnum(NGuiWdwDebugPanels.EPerformanceDisplayType.Off, GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled) ? NGuiWdwDebugPanels.EPerformanceDisplayType.FpsAndNetGraphs : NGuiWdwDebugPanels.EPerformanceDisplayType.Fps, false, true);
			this.guiFPS.Enabled = this.performanceType > NGuiWdwDebugPanels.EPerformanceDisplayType.Off;
			this.guiFPS.ShowGraph = this.performanceType == NGuiWdwDebugPanels.EPerformanceDisplayType.FpsAndFpsGraph;
			this.networkMonitorCh0.Enabled = this.performanceType == NGuiWdwDebugPanels.EPerformanceDisplayType.FpsAndNetGraphs;
			this.networkMonitorCh1.Enabled = this.performanceType == NGuiWdwDebugPanels.EPerformanceDisplayType.FpsAndNetGraphs;
		});
		this.playerUI.windowManager.AddGlobalAction(nguiAction);
		this.playerUI.windowManager.AddGlobalAction(nguiAction2);
		GameManager.Instance.OnWorldChanged += this.HandleWorldChanged;
		GameObject gameObject = GameObject.Find("NetworkMonitor");
		this.networkMonitorCh0 = new NetworkMonitor(0, gameObject.transform.Find("Ch0").transform);
		this.networkMonitorCh1 = new NetworkMonitor(1, gameObject.transform.Find("Ch1").transform);
		string text = GamePrefs.GetString(EnumGamePrefs.DebugPanelsEnabled);
		if (text == null || text == "-")
		{
			text = ",Ge,Fo,Pr,";
			if (!GameManager.Instance.IsEditMode())
			{
				text += "Ply,";
			}
			if (!GameManager.Instance.IsEditMode())
			{
				text += "Sp,";
			}
			if (GameManager.Instance.IsEditMode())
			{
				text += "Se,";
			}
		}
		this.Panels.Add(new NGuiWdwDebugPanels.PanelDefinition("Player", "Ply", new Func<int, int, int>(this.showDebugPanel_Player), text, true));
		this.Panels.Add(new NGuiWdwDebugPanels.PanelDefinition("General", "Ge", new Func<int, int, int>(this.showDebugPanel_General), text, true));
		this.Panels.Add(new NGuiWdwDebugPanels.PanelDefinition("Spawning", "Sp", new Func<int, int, int>(this.showDebugPanel_Spawning), text, SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer));
		this.Panels.Add(new NGuiWdwDebugPanels.PanelDefinition("Chunk", "Ch", new Func<int, int, int>(this.showDebugPanel_Chunk), text, true));
		this.Panels.Add(new NGuiWdwDebugPanels.PanelDefinition("Cache", "Ca", new Func<int, int, int>(this.showDebugPanel_Cache), text, true));
		this.Panels.Add(new NGuiWdwDebugPanels.PanelDefinition("Focused Block", "Fo", new Func<int, int, int>(this.showDebugPanel_FocusedBlock), text, true));
		this.Panels.Add(new NGuiWdwDebugPanels.PanelDefinition("Network", "Ne", new Func<int, int, int>(this.showDebugPanel_Network), text, true));
		this.Panels.Add(new NGuiWdwDebugPanels.PanelDefinition("Selection", "Se", new Func<int, int, int>(this.showDebugPanel_Selection), text, true));
		this.Panels.Add(new NGuiWdwDebugPanels.PanelDefinition("Prefab", "Pr", new Func<int, int, int>(this.showDebugPanel_Prefab), text, true));
		this.Panels.Add(new NGuiWdwDebugPanels.PanelDefinition("Stealth", "St", new Func<int, int, int>(this.showDebugPanel_Stealth), text, true));
		this.Panels.Add(new NGuiWdwDebugPanels.PanelDefinition("Player Extended - Buffs and CVars", "Plx", new Func<int, int, int>(this.showDebugPanel_PlayerEffectInfo), text, true));
		this.Panels.Add(new NGuiWdwDebugPanels.PanelDefinition("Texture", "Te", new Func<int, int, int>(this.showDebugPanel_Texture), text, true));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		this.playerEntity = this.playerUI.entityPlayer;
		this.playerUI.OnEntityPlayerLocalAssigned += this.HandleEntityPlayerLocalAssigned;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		this.playerUI.OnEntityPlayerLocalAssigned -= this.HandleEntityPlayerLocalAssigned;
		string text = ",";
		foreach (NGuiWdwDebugPanels.PanelDefinition panelDefinition in this.Panels)
		{
			if (panelDefinition.Active)
			{
				text = text + panelDefinition.ButtonCaption + ",";
			}
		}
		GamePrefs.Set(EnumGamePrefs.DebugPanelsEnabled, text);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnWorldChanged -= this.HandleWorldChanged;
		}
		this.networkMonitorCh0.Cleanup();
		this.networkMonitorCh1.Cleanup();
	}

	public void ToggleDisplay()
	{
		if (this.debugData == NGuiWdwDebugPanels.EDebugDataType.Off)
		{
			this.debugData = NGuiWdwDebugPanels.EDebugDataType.General;
			return;
		}
		this.debugData = NGuiWdwDebugPanels.EDebugDataType.Off;
	}

	public void ShowGeneralData()
	{
		this.debugData = NGuiWdwDebugPanels.EDebugDataType.General;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleWorldChanged(World _world)
	{
		this.debugData = NGuiWdwDebugPanels.EDebugDataType.Off;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleEntityPlayerLocalAssigned(EntityPlayerLocal _entity)
	{
		this.playerEntity = _entity;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnGUI()
	{
		if (!GameManager.Instance.gameStateManager.IsGameStarted())
		{
			return;
		}
		if (!this.playerUI.windowManager.IsHUDEnabled())
		{
			return;
		}
		if (NGuiWdwDebugPanels.guiStyleToggleBox == null)
		{
			NGuiWdwDebugPanels.guiStyleToggleBox = new GUIStyle(GUI.skin.toggle)
			{
				wordWrap = false,
				padding = new RectOffset(17, 0, 3, 0)
			};
			NGuiWdwDebugPanels.guiStyleTooltipLabel = new GUIStyle(GUI.skin.label)
			{
				wordWrap = false,
				clipping = 0
			};
			NGuiWdwDebugPanels.guiStyleLabelRightAligned = new GUIStyle(GUI.skin.label)
			{
				alignment = 2
			};
		}
		if (GameStats.GetInt(EnumGameStats.GameState) == 1)
		{
			GUI.color = Color.white;
			if (this.debugData != NGuiWdwDebugPanels.EDebugDataType.Off)
			{
				this.panelManager();
			}
			if (this.performanceType == NGuiWdwDebugPanels.EPerformanceDisplayType.FpsAndHeat)
			{
				this.debugShowHeatValue();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Update()
	{
		if (GameStats.GetInt(EnumGameStats.GameState) == 0)
		{
			return;
		}
		if (GameManager.IsDedicatedServer)
		{
			return;
		}
		this.networkMonitorCh0.Update();
		this.networkMonitorCh1.Update();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void debugShowChunkCache()
	{
		float num = (float)Screen.width / 2f;
		float num2 = (float)Screen.height / 2f;
		ChunkCluster chunkCache = GameManager.Instance.World.ChunkCache;
		if (chunkCache != null)
		{
			chunkCache.DebugOnGUI(num, num2, 8f);
		}
		GameManager.Instance.World.m_ChunkManager.DebugOnGUI(num, num2, 8);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void debugShowHeatValue()
	{
		if (!GameStats.GetBool(EnumGameStats.ZombieHordeMeter))
		{
			return;
		}
		if (!(GameManager.Instance == null))
		{
			World world = GameManager.Instance.World;
			if (((world != null) ? world.aiDirector : null) != null)
			{
				Vector2i vector2i = new Vector2i(Screen.width, Screen.height);
				if (this.lastResolution != vector2i)
				{
					this.lastResolution = vector2i;
					this.boxStyle = new GUIStyle(GUI.skin.box);
					this.boxStyle.alignment = 3;
					int num = 13;
					if (vector2i.y > 1200)
					{
						num = vector2i.y / 90;
					}
					this.boxStyle.fontSize = num;
					this.boxAreaHeight = num + 10;
					this.boxAreaWidth = num * 22;
				}
				Vector3i vector3i = World.worldToBlockPos(this.playerEntity.GetPosition());
				int num2 = World.toChunkXZ(vector3i.x);
				int num3 = World.toChunkXZ(vector3i.z);
				AIDirectorChunkEventComponent component = GameManager.Instance.World.aiDirector.GetComponent<AIDirectorChunkEventComponent>();
				AIDirectorChunkData chunkDataFromPosition = component.GetChunkDataFromPosition(vector3i, false);
				string text = string.Format("Heat act {0}", component.GetActiveCount());
				float num4 = 0f;
				if (chunkDataFromPosition != null)
				{
					num4 = chunkDataFromPosition.ActivityLevel;
					text += string.Format(", ch {0} {1}, {2:F2}%, {3} evs", new object[] { num2, num3, num4, chunkDataFromPosition.EventCount });
					if (chunkDataFromPosition.cooldownDelay > 0f)
					{
						text += string.Format(", {0} cd", chunkDataFromPosition.cooldownDelay);
					}
				}
				Color color;
				if (num4 < 90f)
				{
					if (num4 < 50f)
					{
						color = Color.green;
					}
					else
					{
						color = Color.yellow;
					}
				}
				else
				{
					color = new Color(1f, 0.5f, 0.5f);
				}
				GUI.color = color;
				float num5 = (float)(Screen.height / 2 + 48) + 18f * GamePrefs.GetFloat(EnumGamePrefs.OptionsUiFpsScaling);
				Rect rect;
				rect..ctor(14f, num5, (float)this.boxAreaWidth, (float)this.boxAreaHeight);
				GUI.Box(rect, text, this.boxStyle);
				if (chunkDataFromPosition != null)
				{
					GUI.color = new Color(0.9f, 0.9f, 0.9f);
					int num6 = Utils.FastMin(10, chunkDataFromPosition.EventCount);
					for (int i = 0; i < num6; i++)
					{
						rect.y += (float)(this.boxAreaHeight + 1);
						AIDirectorChunkEvent @event = chunkDataFromPosition.GetEvent(i);
						GUI.Box(rect, string.Format("{0} {1} ({2}) {3:F2} {4}", new object[]
						{
							i + 1,
							@event.EventType,
							@event.Position,
							@event.Value,
							@event.Duration
						}), this.boxStyle);
					}
				}
				return;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int showDebugPanel_EnablePanels(int x, int y)
	{
		int num = 6;
		int num2 = ((this.Panels.Count == 0) ? 0 : ((this.Panels.Count - 1) / num + 1));
		GUI.Box(new Rect((float)x, (float)(y - 1), 250f, (float)(21 * num2 + 4)), "");
		x += 5;
		int num3 = x;
		GUI.color = Color.yellow;
		for (int i = 0; i < this.Panels.Count; i++)
		{
			NGuiWdwDebugPanels.PanelDefinition panelDefinition = this.Panels[i];
			if (!panelDefinition.Enabled)
			{
				GUI.enabled = false;
			}
			panelDefinition.Active = GUI.Toggle(new Rect((float)x, (float)(y + 1), 38f, 20f), panelDefinition.Active, new GUIContent(panelDefinition.ButtonCaption, panelDefinition.Name), NGuiWdwDebugPanels.guiStyleToggleBox);
			GUI.enabled = true;
			x += 40;
			if (i % num == 5)
			{
				y += 21;
				x = num3;
			}
		}
		if (this.Panels.Count % num != 0)
		{
			y += 21;
		}
		GUI.color = Color.white;
		return y + 10;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PanelBoxWithHeader(NGuiWdwDebugPanels.EGuiState _guiState, int _x, ref int _y, int _boxWidth, int _boxHeight, string _boxCaption)
	{
		if (_guiState == NGuiWdwDebugPanels.EGuiState.Draw)
		{
			GUI.Box(new Rect((float)_x, (float)_y, (float)_boxWidth, (float)_boxHeight), "");
			this.HeaderLabel(_guiState, _x, _y, _boxCaption, 200, 25);
		}
		_y += 21;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HeaderLabel(NGuiWdwDebugPanels.EGuiState _guiState, int _x, int _y, string _text, int _labelWidth = 200, int _labelHeight = 25)
	{
		if (_guiState == NGuiWdwDebugPanels.EGuiState.Draw)
		{
			GUI.color = Color.yellow;
			GUI.Label(new Rect((float)(_x + 5), (float)_y, (float)_labelWidth, (float)_labelHeight), _text);
			GUI.color = Color.white;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LabelWithOutline(int _x, int _y, string _text, int _labelWidth = 200, int _labelHeight = 25, int _xOffset = 5)
	{
		Utils.DrawOutline(new Rect((float)(_x + _xOffset), (float)_y, (float)_labelWidth, (float)_labelHeight), _text, NGuiWdwDebugPanels.guiStyleDebug, Color.black, Color.white);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GreenWithOutline(int _x, int _y, string _text, int _labelWidth = 200, int _labelHeight = 25, int _xOffset = 5)
	{
		Utils.DrawOutline(new Rect((float)(_x + _xOffset), (float)_y, (float)_labelWidth, (float)_labelHeight), _text, NGuiWdwDebugPanels.guiStyleDebug, Color.black, Color.green);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BlueWithOutline(int _x, int _y, string _text, int _labelWidth = 200, int _labelHeight = 25, int _xOffset = 5)
	{
		Utils.DrawOutline(new Rect((float)(_x + _xOffset), (float)_y, (float)_labelWidth, (float)_labelHeight), _text, NGuiWdwDebugPanels.guiStyleDebug, Color.black, Color.cyan);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FakeTextField(int _x, int _y, int _width, int _height, string _text)
	{
		GUI.Box(new Rect((float)_x, (float)_y, (float)_width, (float)_height), "", GUI.skin.textField);
		GUI.Label(new Rect((float)(_x + 3), (float)(_y + 3), (float)_width, (float)_height), _text);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int showDebugPanel_Player(int x, int y)
	{
		int num = 0;
		int num2 = y;
		int num3 = 340;
		for (NGuiWdwDebugPanels.EGuiState eguiState = NGuiWdwDebugPanels.EGuiState.CalcSize; eguiState < NGuiWdwDebugPanels.EGuiState.Count; eguiState++)
		{
			y = num2;
			this.PanelBoxWithHeader(eguiState, x, ref y, num3, num - num2 + 5, "Player");
			EntityPlayer entityPlayer = this.playerEntity;
			if (entityPlayer == null)
			{
				return y;
			}
			if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
			{
				float num4 = Time.time - this.lastPlayerTime;
				if (num4 >= 0.5f)
				{
					this.playerSpeed = (entityPlayer.position - this.lastPlayerPos).magnitude / num4;
					this.lastPlayerPos = entityPlayer.position;
					this.lastPlayerTime = Time.time;
				}
				this.LabelWithOutline(x, y, string.Format("X/Y/Z: {0:F1}/{1:F1}/{2:F1}, Speed {3:F3}", new object[]
				{
					entityPlayer.position.x,
					entityPlayer.position.y,
					entityPlayer.position.z,
					this.playerSpeed
				}), 200, 25, 5);
			}
			y += 16;
			if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
			{
				this.LabelWithOutline(x, y, string.Format("Rot: {0:F1}/{1:F1}/{2:F1}", entityPlayer.rotation.x, entityPlayer.rotation.y, entityPlayer.rotation.z), 200, 25, 5);
			}
			y += 16;
			if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
			{
				string text = string.Empty;
				string text2 = string.Empty;
				BiomeDefinition biomeStandingOn = entityPlayer.biomeStandingOn;
				if (biomeStandingOn != null)
				{
					text = biomeStandingOn.m_sBiomeName;
					IBiomeProvider biomeProvider = entityPlayer.world.ChunkCache.ChunkProvider.GetBiomeProvider();
					if (biomeProvider != null)
					{
						Vector3i blockPosition = entityPlayer.GetBlockPosition();
						int subBiomeIdxAt = biomeProvider.GetSubBiomeIdxAt(biomeStandingOn, blockPosition.x, blockPosition.y, blockPosition.z);
						if (subBiomeIdxAt >= 0)
						{
							text2 = string.Format(", sub {0}", subBiomeIdxAt);
						}
					}
				}
				this.LabelWithOutline(x, y, "Biome: " + text + text2, 200, 25, 5);
			}
			y += 16;
			if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
			{
				PrefabInstance poiatPosition = entityPlayer.world.GetPOIAtPosition(entityPlayer.position, new FastTags<TagGroup.Poi>?(FastTags<TagGroup.Poi>.none), null);
				string text3 = ((poiatPosition == null) ? string.Empty : string.Format("{0}, {1}, r {2}, sl {3}, tr {4}", new object[]
				{
					poiatPosition.name,
					poiatPosition.boundingBoxPosition,
					poiatPosition.rotation,
					poiatPosition.sleeperVolumes.Count,
					poiatPosition.triggerVolumes.Count
				}));
				this.LabelWithOutline(x, y, "POI: " + text3, 200, 25, 5);
				y += 16;
				string text4 = ((poiatPosition == null) ? string.Empty : poiatPosition.GetPositionRelativeToPoi(Vector3i.Floor(entityPlayer.position)).ToString());
				this.LabelWithOutline(x, y, "X/Y/Z in prefab: " + text4, 200, 25, 5);
				y += 16;
			}
			else
			{
				y += 32;
			}
			if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
			{
				this.LabelWithOutline(x, y, string.Format("DM Threat Lvl: {0} : {1:0.##}", this.playerEntity.ThreatLevel.Category.ToStringCached<ThreatLevelType>(), this.playerEntity.ThreatLevel.Numeric) + string.Format(", Zeds: {0}, Targeting: {1}", ThreatLevelUtility.Zombies, ThreatLevelUtility.Targeting), 200, 25, 5);
			}
			y += 16;
			if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
			{
				this.LabelWithOutline(x, y, string.Format("TemperatureF: Outside {0:0.0}, Core {1:0.0}, Absorb {2:0.0}", entityPlayer.PlayerStats.GetOutsideTemperature(), entityPlayer.Buffs.GetCustomVar("_coretemp"), entityPlayer.Buffs.GetCustomVar("_degreesabsorbed")), 200, 25, 5);
			}
			y += 16;
			num = y;
		}
		return y + 10;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int showDebugPanel_PlayerEffectInfo(int x, int y)
	{
		int num = y;
		EntityAlive entityAlive = this.playerEntity;
		if (InputUtils.ShiftKeyPressed)
		{
			Ray ray = this.playerEntity.GetLookRay();
			if (GameManager.Instance.IsEditMode() && GamePrefs.GetInt(EnumGamePrefs.SelectionOperationMode) == 4)
			{
				ray = this.playerEntity.cameraTransform.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
				ray.origin += Origin.position;
			}
			ray.origin += ray.direction.normalized * 0.1f;
			float num2 = Utils.FastMax(Utils.FastMax(Constants.cDigAndBuildDistance, Constants.cCollectItemDistance), 30f);
			int num3 = 69;
			if (Voxel.Raycast(GameManager.Instance.World, ray, num2, -555528221, num3, 0f))
			{
				Transform hitRootTransform = GameUtils.GetHitRootTransform(Voxel.voxelRayHitInfo.tag, Voxel.voxelRayHitInfo.transform);
				if (hitRootTransform != null)
				{
					entityAlive = hitRootTransform.gameObject.GetComponent<Entity>() as EntityAlive;
				}
			}
		}
		if (entityAlive == null)
		{
			return num;
		}
		int num4 = entityAlive.Buffs.ActiveBuffs.Count + Mathf.Min(25, entityAlive.Buffs.CountCustomVars()) * 16;
		num4 += 96;
		num4 += 15;
		int num5 = 440;
		float num6 = Utils.FastClamp((float)Screen.height / 1080f * GameOptionsManager.GetActiveUiScale(), 0.4f, 2f);
		x = (int)((float)Screen.width / num6) - (num5 + 16);
		y = 64;
		this.PanelBoxWithHeader(NGuiWdwDebugPanels.EGuiState.Draw, x, ref y, num5, num4, entityAlive.EntityName + " Buffs (" + entityAlive.Buffs.ActiveBuffs.Count.ToString() + ")");
		for (int i = 0; i < entityAlive.Buffs.ActiveBuffs.Count; i++)
		{
			BuffValue buffValue = entityAlive.Buffs.ActiveBuffs[i];
			BuffClass buffClass = buffValue.BuffClass;
			GUI.color = ((buffClass != null) ? buffClass.IconColor : Color.magenta);
			Entity entity = GameManager.Instance.World.GetEntity(buffValue.InstigatorId);
			string text = string.Format("none (id {0})", buffValue.InstigatorId);
			string text2 = string.Concat(new string[]
			{
				buffValue.BuffName,
				" : From ",
				entity ? entity.GetDebugName() : text,
				" ",
				entity ? entity.entityId.ToString() : ""
			});
			if (buffClass == null)
			{
				text2 += " : BuffClass Missing";
			}
			this.LabelWithOutline(x, y, text2, 200, 25, 5);
			GUI.color = Color.white;
			y += 16;
		}
		y += 21;
		this.HeaderLabel(NGuiWdwDebugPanels.EGuiState.Draw, x, y, entityAlive.EntityName + " CVars (" + entityAlive.Buffs.CountCustomVars().ToString() + ")", 200, 25);
		GUI.Label(new Rect((float)(x + 150), (float)y, 50f, 25f), "Filter:", NGuiWdwDebugPanels.guiStyleLabelRightAligned);
		if (Cursor.visible)
		{
			NGuiWdwDebugPanels.filterCVar = GUI.TextField(new Rect((float)(x + 205), (float)y, 200f, 25f), NGuiWdwDebugPanels.filterCVar);
		}
		else
		{
			this.FakeTextField(x + 205, y, 200, 25, NGuiWdwDebugPanels.filterCVar);
		}
		y += 21;
		int num7 = y;
		int num8 = 1;
		num4 = -1;
		foreach (KeyValuePair<string, float> keyValuePair in entityAlive.Buffs.EnumerateCustomVars(NGuiWdwDebugPanels.filterCVar, false))
		{
			string text3;
			float num9;
			keyValuePair.Deconstruct(out text3, out num9);
			string text4 = text3;
			float num10 = num9;
			if (num10 != 0f)
			{
				this.LabelWithOutline(x, y, string.Format("{0} : {1}", text4, num10), 200, 25, 5);
				if (num8 % 25 == 0)
				{
					x += 220;
					if (num4 == -1)
					{
						num4 = y + 16 + 5;
					}
					y = num7;
				}
				else
				{
					y += 16;
				}
				num8++;
			}
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int showDebugPanel_DynamicMusicInfo(int x, int y)
	{
		int num = 0;
		int num2 = y;
		int num3 = 220;
		for (NGuiWdwDebugPanels.EGuiState eguiState = NGuiWdwDebugPanels.EGuiState.CalcSize; eguiState < NGuiWdwDebugPanels.EGuiState.Count; eguiState++)
		{
			y = num2;
			this.PanelBoxWithHeader(eguiState, x, ref y, num3, num - num2 + 5, "Dynamic Music");
			if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
			{
				this.LabelWithOutline(x, y, "SomeStringData", 200, 25, 5);
			}
			y += 16;
			num = y;
		}
		return y + 10;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int showDebugPanel_Spawning(int x, int y)
	{
		int count = GameManager.Instance.World.Last4Spawned.Count;
		int num = 21 + count * 16 + 5;
		this.PanelBoxWithHeader(NGuiWdwDebugPanels.EGuiState.Draw, x, ref y, 325, num, "Spawning");
		for (int i = count - 1; i >= 0; i--)
		{
			SSpawnedEntity sspawnedEntity = GameManager.Instance.World.Last4Spawned[i];
			this.LabelWithOutline(x, y, string.Format("{0}:{1} - {2:F1}m", sspawnedEntity.name, sspawnedEntity.pos, sspawnedEntity.distanceToLocalPlayer), 300, 25, 5);
			y += 16;
		}
		return y + 10;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int showDebugPanel_Chunk(int x, int y)
	{
		EntityPlayer entityPlayer = this.playerEntity;
		if (entityPlayer == null)
		{
			return y;
		}
		int x2 = entityPlayer.chunkPosAddedEntityTo.x;
		int z = entityPlayer.chunkPosAddedEntityTo.z;
		Chunk chunk = (Chunk)GameManager.Instance.World.GetChunkSync(x2, z);
		if (chunk == null)
		{
			return y;
		}
		Vector3i vector3i = Chunk.ToAreaMasterChunkPos(chunk.ToWorldPos(Vector3i.zero));
		Chunk chunk2 = (Chunk)GameManager.Instance.World.GetChunkSync(vector3i.x, vector3i.z);
		int num = 0;
		int num2 = y;
		int num3 = 550;
		for (NGuiWdwDebugPanels.EGuiState eguiState = NGuiWdwDebugPanels.EGuiState.CalcSize; eguiState < NGuiWdwDebugPanels.EGuiState.Count; eguiState++)
		{
			y = num2;
			this.PanelBoxWithHeader(eguiState, x, ref y, num3, num - num2 + 5, "Chunk");
			if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
			{
				this.LabelWithOutline(x, y, RegionFileManager.DebugUtil.GetLocationString(chunk.X, chunk.Z), 200, 25, 5);
			}
			y += 16;
			if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
			{
				string text = "";
				int num4 = 0;
				ChunkAreaBiomeSpawnData chunkAreaBiomeSpawnData = ((chunk2 != null) ? chunk2.GetChunkBiomeSpawnData() : null);
				if (chunkAreaBiomeSpawnData != null)
				{
					text = chunkAreaBiomeSpawnData.poiTags.ToString();
					num4 = chunkAreaBiomeSpawnData.groupsEnabledFlags;
				}
				this.LabelWithOutline(x, y, string.Format("AreaMaster: {0}/{1} {2} {3:x}", new object[] { vector3i.x, vector3i.z, text, num4 }), 200, 25, 5);
			}
			y += 16;
			if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
			{
				ChunkAreaBiomeSpawnData chunkAreaBiomeSpawnData2 = ((chunk2 != null) ? chunk2.GetChunkBiomeSpawnData() : null);
				this.LabelWithOutline(x, y, ((chunkAreaBiomeSpawnData2 != null) ? chunkAreaBiomeSpawnData2.ToString() : string.Empty) ?? "", 200, 25, 5);
			}
			y += 16;
			string text2 = "Tris sum: " + chunk.GetTris().ToString();
			int num5 = 0;
			int num6 = 0;
			while (eguiState == NGuiWdwDebugPanels.EGuiState.Draw && num6 < MeshDescription.meshes.Length)
			{
				text2 = string.Concat(new string[]
				{
					text2,
					" [",
					num6.ToString(),
					"]: ",
					chunk.GetTrisInMesh(num6).ToString()
				});
				num5 += chunk.GetSizeOfMesh(num6);
				num6++;
			}
			if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
			{
				this.LabelWithOutline(x, y, string.Format("{0} Size: {1}kB", text2, num5 / 1024), 300, 25, 5);
			}
			y += 16;
			num = y;
		}
		return y + 10;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int showDebugPanel_Cache(int x, int y)
	{
		int num = 0;
		int num2 = y;
		int num3 = 550;
		for (NGuiWdwDebugPanels.EGuiState eguiState = NGuiWdwDebugPanels.EGuiState.CalcSize; eguiState < NGuiWdwDebugPanels.EGuiState.Count; eguiState++)
		{
			y = num2;
			this.PanelBoxWithHeader(eguiState, x, ref y, num3, num - num2 + 5, "Cache");
			if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
			{
				int displayedChunkGameObjectsCount = GameManager.Instance.World.m_ChunkManager.GetDisplayedChunkGameObjectsCount();
				int count = GameManager.Instance.World.m_ChunkManager.GetFreeChunkGameObjects().Count;
				this.LabelWithOutline(x, y, string.Format("CGO: {0} Displayed: {1} Free: {2}", ChunkGameObject.InstanceCount, displayedChunkGameObjectsCount, count), 200, 25, 5);
			}
			y += 16;
			if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
			{
				this.LabelWithOutline(x, y, MemoryPools.GetDebugInfo(), 200, 25, 5);
			}
			y += 16;
			if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
			{
				this.LabelWithOutline(x, y, MemoryPools.GetDebugInfoEx(), 200, 25, 5);
			}
			y += 16;
			num = y;
		}
		return y + 10;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int showDebugPanel_General(int x, int y)
	{
		World world = GameManager.Instance.World;
		if (world == null)
		{
			return y;
		}
		int num = 0;
		int num2 = y;
		int num3 = 340;
		for (NGuiWdwDebugPanels.EGuiState eguiState = NGuiWdwDebugPanels.EGuiState.CalcSize; eguiState < NGuiWdwDebugPanels.EGuiState.Count; eguiState++)
		{
			y = num2;
			this.PanelBoxWithHeader(eguiState, x, ref y, num3, num - num2 + 5, "General");
			if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
			{
				this.LabelWithOutline(x, y, string.Concat(new string[]
				{
					"Save/World Name='",
					SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? GamePrefs.GetString(EnumGamePrefs.GameName) : GamePrefs.GetString(EnumGamePrefs.GameNameClient),
					"'/'",
					GamePrefs.GetString(EnumGamePrefs.GameWorld),
					"'"
				}), 200, 25, 5);
			}
			y += 16;
			if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
			{
				this.LabelWithOutline(x, y, string.Format("Seed={0}", world.Seed), 200, 25, 5);
			}
			y += 16;
			if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
			{
				this.LabelWithOutline(x, y, string.Format("Time scale: {0}", Time.timeScale), 200, 25, 5);
			}
			y += 16;
			if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
			{
				int entityAliveCount = world.GetEntityAliveCount(EntityFlags.Animal, EntityFlags.Animal);
				int entityAliveCount2 = world.GetEntityAliveCount(EntityFlags.Bandit, EntityFlags.Bandit);
				int entityAliveCount3 = world.GetEntityAliveCount(EntityFlags.Zombie, EntityFlags.Zombie);
				this.LabelWithOutline(x, y, string.Format("World Ent: {0} ({1}) An: {2} Ban: {3} Zom: {4}", new object[]
				{
					world.Entities.Count,
					Entity.InstanceCount,
					entityAliveCount,
					entityAliveCount2,
					entityAliveCount3
				}), 200, 25, 5);
			}
			y += 16;
			PathFinderThread instance = PathFinderThread.Instance;
			if (instance != null)
			{
				if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
				{
					this.LabelWithOutline(x, y, string.Format("Paths: q {0}, finish {1}", instance.GetQueueCount(), instance.GetFinishedCount()), 200, 25, 5);
				}
				y += 16;
			}
			if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
			{
				this.LabelWithOutline(x, y, string.Format("Memory used: {0}MB", GC.GetTotalMemory(false) / 1048576L), 200, 25, 5);
			}
			y += 16;
			if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
			{
				this.LabelWithOutline(x, y, string.Format("Active threads: {0} tasks: {1}", ThreadManager.ActiveThreads.Count, ThreadManager.QueuedCount), 200, 25, 5);
			}
			y += 16;
			if (!world.IsRemote())
			{
				if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
				{
					this.LabelWithOutline(x, y, "Ticked blocks: " + world.GetWBT().GetCount().ToString(), 200, 25, 5);
				}
				y += 16;
			}
			num = y;
		}
		return y + 10;
	}

	public int showDebugPanel_FocusedBlock(int x, int y)
	{
		return this.showDebugPanel_FocusedBlock(x, y, false);
	}

	public int showDebugPanel_FocusedBlock(int x, int y, bool forceFocusedBlock = false)
	{
		if (this.playerEntity == null || this.playerEntity.inventory.holdingItemData == null || !this.playerEntity.HitInfo.bHitValid)
		{
			return y;
		}
		WorldRayHitInfo hitInfo = this.playerEntity.HitInfo;
		Vector3i vector3i = ((InputUtils.ShiftKeyPressed || forceFocusedBlock) ? hitInfo.hit.blockPos : hitInfo.lastBlockPos);
		BlockFace blockFace = hitInfo.hit.blockFace;
		if (vector3i.y < 0 || vector3i.y >= 256)
		{
			return y;
		}
		ChunkCluster chunkCache = GameManager.Instance.World.ChunkCache;
		if (chunkCache == null)
		{
			return y;
		}
		Chunk chunk = (Chunk)chunkCache.GetChunkFromWorldPos(vector3i);
		if (chunk == null)
		{
			return y;
		}
		Vector3i vector3i2 = World.toBlock(vector3i);
		int x2 = vector3i2.x;
		int y2 = vector3i2.y;
		int z = vector3i2.z;
		BlockValue block = chunk.GetBlock(vector3i2);
		Block block2 = block.Block;
		BlockShape shape = block2.shape;
		BlockFace rotatedBlockFace = shape.GetRotatedBlockFace(block, blockFace);
		string[] array = new string[1];
		int[] array2 = new int[1];
		for (int i = 0; i < 1; i++)
		{
			array2[i] = GameManager.Instance.World.ChunkCache.GetBlockFaceTexture(vector3i, rotatedBlockFace, i);
			if (array2[i] == 0)
			{
				string text;
				array2[i] = GameUtils.FindPaintIdForBlockFace(block, rotatedBlockFace, out text, i);
				array[i] = text;
			}
			else
			{
				string[] array3 = array;
				int num = i;
				string text2;
				if (array2[i] < 0 || array2[i] >= BlockTextureData.list.Length)
				{
					text2 = string.Empty;
				}
				else
				{
					BlockTextureData blockTextureData = BlockTextureData.list[array2[i]];
					text2 = ((blockTextureData != null) ? blockTextureData.Name : null) ?? "N/A";
				}
				array3[num] = text2;
			}
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int j = 0; j < 1; j++)
		{
			bool flag = false;
			if (array2[0] >= 0 && array2[0] < BlockTextureData.list.Length && block2.MeshIndex == 0)
			{
				BlockTextureData blockTextureData2 = BlockTextureData.list[array2[0]];
				int num2 = (int)((blockTextureData2 != null) ? blockTextureData2.TextureID : 0);
				num2 = ((num2 == 0) ? block2.GetSideTextureId(block, rotatedBlockFace, 0) : num2);
				flag = MeshDescription.meshes[0].textureAtlas.uvMapping[num2].bGlobalUV;
				if (rotatedBlockFace != BlockFace.None)
				{
					Block.UVMode uvmode = block2.GetUVMode((int)rotatedBlockFace, j);
					if (uvmode != Block.UVMode.Global)
					{
						if (uvmode == Block.UVMode.Local)
						{
							flag = false;
						}
					}
					else
					{
						flag = true;
					}
				}
			}
			if (j > 0)
			{
				stringBuilder.Append(",");
			}
			stringBuilder.Append(flag ? "G" : "L");
		}
		int num3 = 0;
		int num4 = y;
		int num5 = 260;
		for (NGuiWdwDebugPanels.EGuiState eguiState = NGuiWdwDebugPanels.EGuiState.CalcSize; eguiState < NGuiWdwDebugPanels.EGuiState.Count; eguiState++)
		{
			y = num4;
			this.PanelBoxWithHeader(eguiState, x, ref y, num5, num3 - num4 + 5, "Focused Block");
			if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
			{
				int num6 = y;
				string text3 = "Pos: ";
				Vector3i vector3i3 = vector3i;
				this.LabelWithOutline(x, num6, text3 + vector3i3.ToString(), 200, 25, 5);
			}
			y += 16;
			if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
			{
				BlockValue blockValue2;
				if (block.isair)
				{
					ItemClassBlock.ItemBlockInventoryData itemBlockInventoryData = this.playerEntity.inventory.holdingItemData as ItemClassBlock.ItemBlockInventoryData;
					if (itemBlockInventoryData != null && !itemBlockInventoryData.itemValue.ToBlockValue(false).Block.shape.IsTerrain())
					{
						BlockValue blockValue = itemBlockInventoryData.itemValue.ToBlockValue(false);
						blockValue.rotation = itemBlockInventoryData.rotation;
						int num7 = y;
						string text4 = "Data: ";
						blockValue2 = blockValue;
						this.LabelWithOutline(x, num7, text4 + blockValue2.ToString(), 200, 25, 5);
						goto IL_03AF;
					}
				}
				int num8 = y;
				string text5 = "Data: ";
				blockValue2 = block;
				this.LabelWithOutline(x, num8, text5 + blockValue2.ToString(), 200, 25, 5);
			}
			IL_03AF:
			y += 16;
			if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
			{
				this.LabelWithOutline(x, y, string.Concat(new string[]
				{
					"Name: ",
					block2.GetBlockName(),
					" (W=",
					blockFace.ToStringCached<BlockFace>(),
					"->B=",
					rotatedBlockFace.ToStringCached<BlockFace>(),
					")"
				}), 200, 25, 5);
			}
			y += 16;
			for (int k = 0; k < 1; k++)
			{
				if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
				{
					this.LabelWithOutline(x, y, string.Format("Paint Id: {0} ({1})", array2[k], array[k]), 200, 25, 5);
				}
				y += 16;
			}
			if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
			{
				BlockShapeModelEntity blockShapeModelEntity = shape as BlockShapeModelEntity;
				if (blockShapeModelEntity != null)
				{
					this.BlueWithOutline(x, y, "Prefab: " + blockShapeModelEntity.modelName, 200, 25, 5);
					if (Input.GetKeyDown(305))
					{
						GUIUtility.systemCopyBuffer = blockShapeModelEntity.modelName;
					}
				}
				else
				{
					this.GreenWithOutline(x, y, string.Format("Shape: {0} V={1} T={2} UV: {3}", new object[]
					{
						shape.GetName(),
						shape.GetVertexCount(),
						shape.GetTriangleCount(),
						stringBuilder.ToString()
					}), 200, 25, 5);
					if (Input.GetKeyDown(305))
					{
						string text6 = "@:Shapes/";
						string text7 = ".fbx";
						GUIUtility.systemCopyBuffer = shape.GetName().Replace(text6, "").Replace(text7, "");
					}
				}
			}
			y += 16;
			if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
			{
				this.LabelWithOutline(x, y, string.Concat(new string[]
				{
					"Light: emit=",
					block2.GetLightValue(block).ToString(),
					" opac=",
					block2.lightOpacity.ToString(),
					" sun=",
					chunk.GetLight(x2, y2, z, Chunk.LIGHT_TYPE.SUN).ToString(),
					" blk=",
					chunk.GetLight(x2, y2, z, Chunk.LIGHT_TYPE.BLOCK).ToString()
				}), 200, 25, 5);
			}
			y += 16;
			if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
			{
				this.LabelWithOutline(x, y, string.Concat(new string[]
				{
					"Stability: ",
					chunk.GetStability(x2, y2, z).ToString(),
					" Density: ",
					chunk.GetDensity(x2, y2, z).ToString("0.00"),
					" "
				}), 200, 25, 5);
			}
			y += 16;
			if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
			{
				this.LabelWithOutline(x, y, string.Concat(new string[]
				{
					"Height: ",
					chunk.GetHeight(x2, z).ToString(),
					" Terrain: ",
					chunk.GetTerrainHeight(x2, z).ToString(),
					" Deco: ",
					chunk.GetDecoAllowedAt(x2, z).ToStringFriendlyCached()
				}), 200, 25, 5);
			}
			y += 16;
			if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
			{
				string text8 = "Normal: " + GameManager.Instance.World.GetTerrainNormalAt(vector3i.x, vector3i.z).ToCultureInvariantString();
				int mass = chunk.GetWater(x2, y2, z).GetMass();
				if (mass > 0)
				{
					text8 = text8 + " Water: " + mass.ToString();
				}
				this.LabelWithOutline(x, y, text8, 200, 25, 5);
			}
			y += 16;
			if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
			{
				if (block2.HasTileEntity)
				{
					ITileEntityLootable tileEntityLootable;
					if (chunk.GetTileEntity(vector3i2).TryGetSelfOrFeature(out tileEntityLootable))
					{
						this.LabelWithOutline(x, y, "LootStage: " + this.playerEntity.GetLootStage(tileEntityLootable.LootStageMod, tileEntityLootable.LootStageBonus).ToString(), 200, 25, 5);
					}
					else
					{
						this.LabelWithOutline(x, y, "LootStage: " + this.playerEntity.GetLootStage(0f, 0f).ToString(), 200, 25, 5);
					}
				}
				else
				{
					this.LabelWithOutline(x, y, "LootStage: " + this.playerEntity.GetLootStage(0f, 0f).ToString(), 200, 25, 5);
				}
			}
			y += 16;
			if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
			{
				AstarVoxelGrid.VoxelNode voxelNode = (AstarPath.active ? ((AstarVoxelGrid.VoxelNode)AstarPath.active.GetNearest(World.blockToTransformPos(vector3i), NNConstraint.Default).node) : null);
				if (voxelNode != null)
				{
					GridGraph gridGraph = GridNode.GetGridGraph(voxelNode.GraphIndex);
					Color[] array4 = new Color[]
					{
						Color.cyan,
						Color.red,
						Color.blue,
						Color.yellow
					};
					Vector3 vector = (Vector3)voxelNode.position - Origin.position;
					Vector3 vector2;
					vector2..ctor(0.4f, 0f, 0.4f);
					Vector3 vector3;
					vector3..ctor(0f, 0.1f, 0f);
					Utils.DrawRectLinesHorzontal(vector + vector3 - vector2, vector + vector3 + vector2, Color.green, 0.2f);
					this.LabelWithOutline(x, y, string.Format("Penalty: {0}, H: {1}, L: {2}", voxelNode.Penalty, voxelNode.PenaltyHigh, voxelNode.PenaltyLow), 200, 25, 5);
					y += 16;
					this.LabelWithOutline(x, y, string.Format("Tag: {0}", voxelNode.Tag), 200, 25, 5);
					y += 16;
					for (int l = 0; l < 4; l++)
					{
						string text9 = string.Empty;
						AstarVoxelGrid.VoxelNode voxelNode2 = (AstarVoxelGrid.VoxelNode)voxelNode.GetNeighbourAlongDirection(l);
						if (voxelNode2 != null)
						{
							text9 = string.Format("Dir[{0} {1},{2}]: BlockerFlags[{3:X}] Tag[{4}]", new object[]
							{
								l,
								gridGraph.neighbourXOffsets[l],
								gridGraph.neighbourZOffsets[l],
								voxelNode2.BlockerFlags,
								voxelNode2.Tag
							});
							Vector3 vector4 = (Vector3)voxelNode2.position;
							Utils.DrawRectLinesHorzontal(vector4 + vector3 - vector2, vector4 + vector3 + vector2, array4[l], 0.2f);
							Debug.DrawLine(vector, vector4, array4[l], 0.2f);
						}
						else
						{
							text9 = string.Format("Dir[{0} {1},{2}]: -", l, gridGraph.neighbourXOffsets[l], gridGraph.neighbourZOffsets[l]);
						}
						Utils.DrawOutline(new Rect((float)(x + 5), (float)y, 200f, 25f), text9, NGuiWdwDebugPanels.guiStyleDebug, Color.black, array4[l]);
						y += 16;
					}
					if (voxelNode.connections != null)
					{
						foreach (Connection connection in voxelNode.connections)
						{
							if (connection.cost != 4294967295U)
							{
								Vector3 vector5 = (Vector3)connection.node.position - Origin.position;
								Utils.DrawRectLinesHorzontal(vector5 - vector2 * 0.5f, vector5 + vector2 * 0.5f, Color.white, 0.2f);
								Debug.DrawLine(vector, vector5, Color.gray, 0.2f);
							}
						}
					}
				}
			}
			else
			{
				y += 64;
			}
			num3 = y;
		}
		return y + 10;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int showDebugPanel_Network(int x, int y)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer || SingletonMonoBehaviour<ConnectionManager>.Instance.ClientCount() == 0)
		{
			return y;
		}
		int num = 0;
		int num2 = y;
		int num3 = 220;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		for (NGuiWdwDebugPanels.EGuiState eguiState = NGuiWdwDebugPanels.EGuiState.CalcSize; eguiState < NGuiWdwDebugPanels.EGuiState.Count; eguiState++)
		{
			y = num2;
			this.PanelBoxWithHeader(eguiState, x, ref y, num3, num - num2 + 5, "Network");
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
				{
					this.LabelWithOutline(x, y, "Clients: " + SingletonMonoBehaviour<ConnectionManager>.Instance.ClientCount().ToString(), 200, 25, 5);
				}
				y += 16;
				if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
				{
					string text = string.Format("{0}{1}B", (num4 > 1024) ? ((float)num4 / 1024f).ToCultureInvariantString("0.0") : num4.ToString(), (num4 > 1024) ? "k" : "");
					this.LabelWithOutline(x, y, string.Format("   total sent: #{1,3}  {0}", text, num6), 200, 25, 5);
				}
				y += 16;
				if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
				{
					string text2 = string.Format("{0}{1}B", (num5 > 1024) ? ((float)num5 / 1024f).ToCultureInvariantString("0.0") : num5.ToString(), (num5 > 1024) ? "k" : "");
					this.LabelWithOutline(x, y, string.Format("   total recv: #{1,3}  {0}", text2, num7), 200, 25, 5);
				}
				y += 16;
				int num8 = 0;
				int num9 = 0;
				int num10 = 0;
				int num11 = 0;
				foreach (ClientInfo clientInfo in SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.List)
				{
					if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
					{
						this.LabelWithOutline(x, y, string.Format("Client {0,2}", clientInfo.InternalId.CombinedString), 200, 25, 5);
					}
					y += 16;
					if (eguiState == NGuiWdwDebugPanels.EGuiState.CalcSize)
					{
						clientInfo.netConnection[0].GetStats().GetStats(0.5f, out num8, out num10, out num9, out num11);
						num4 += num8;
						num5 += num9;
						num6 += num10;
						num7 += num11;
					}
					if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
					{
						clientInfo.netConnection[0].GetStats().GetStats(0.5f, out num8, out num10, out num9, out num11);
						string text3 = string.Format("{0}{1}B", (num8 > 1024) ? ((float)num8 / 1024f).ToCultureInvariantString("0.0") : num8.ToString(), (num8 > 1024) ? "k" : "");
						this.LabelWithOutline(x, y, string.Format("   stream0 sent: #{1,3}  {0}", text3, num10), 200, 25, 5);
					}
					y += 16;
					if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
					{
						string text4 = string.Format("{0}{1}B", (num9 > 1024) ? ((float)num8 / 1024f).ToCultureInvariantString("0.0") : num9.ToString(), (num9 > 1024) ? "k" : "");
						this.LabelWithOutline(x, y, string.Format("   stream0 rcvd: #{1,3}  {0}", text4, num11), 200, 25, 5);
					}
					y += 16;
					if (eguiState == NGuiWdwDebugPanels.EGuiState.CalcSize)
					{
						clientInfo.netConnection[1].GetStats().GetStats(0.5f, out num8, out num10, out num9, out num11);
						num4 += num8;
						num5 += num9;
						num6 += num10;
						num7 += num11;
					}
					if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
					{
						clientInfo.netConnection[1].GetStats().GetStats(0.5f, out num8, out num10, out num9, out num11);
						string text5 = string.Format("{0}{1}B", (num8 > 1024) ? ((float)num8 / 1024f).ToCultureInvariantString("0.0") : num8.ToString(), (num8 > 1024) ? "k" : "");
						this.LabelWithOutline(x, y, string.Format("   stream1 sent: #{1,3}  {0}", text5, num10), 200, 25, 5);
					}
					y += 16;
					if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
					{
						string text6 = string.Format("{0}{1}B", (num9 > 1024) ? ((float)num9 / 1024f).ToCultureInvariantString("0.0") : num9.ToString(), (num9 > 1024) ? "k" : "");
						this.LabelWithOutline(x, y, string.Format("   stream1 rcvd: #{1,3}  {0}", text6, num11), 200, 25, 5);
					}
					y += 16;
				}
			}
			num = y;
		}
		return y + 10;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int showDebugPanel_Selection(int x, int y)
	{
		if (GameManager.Instance.GetActiveBlockTool() == null)
		{
			return y;
		}
		int num = 0;
		int num2 = y;
		int num3 = 220;
		for (NGuiWdwDebugPanels.EGuiState eguiState = NGuiWdwDebugPanels.EGuiState.CalcSize; eguiState < NGuiWdwDebugPanels.EGuiState.Count; eguiState++)
		{
			y = num2;
			this.PanelBoxWithHeader(eguiState, x, ref y, num3, num - num2 + 5, "Selection");
			if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
			{
				string debugOutput = GameManager.Instance.GetActiveBlockTool().GetDebugOutput();
				this.LabelWithOutline(x, y, debugOutput, 200, 25, 5);
			}
			y += 16;
			if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
			{
				XUiC_WoPropsSleeperVolume.VolumeStats volumeStats;
				if (XUiC_WoPropsSleeperVolume.GetSelectedVolumeStats(out volumeStats))
				{
					this.LabelWithOutline(x, y, "Sleeper Volume", 200, 25, 5);
					y += 16;
					this.LabelWithOutline(x, y, string.Format("Index: {0}", volumeStats.Index), 200, 25, 5);
					y += 16;
					this.LabelWithOutline(x, y, string.Format("Pos: {0}", volumeStats.WorldPos), 200, 25, 5);
					y += 16;
					this.LabelWithOutline(x, y, string.Format("Size: {0}", volumeStats.Size), 200, 25, 5);
					y += 16;
					this.LabelWithOutline(x, y, "Group: " + volumeStats.GroupName, 200, 25, 5);
					y += 16;
					this.LabelWithOutline(x, y, string.Format("Priority: {0}   QuestExc: {1}", volumeStats.IsPriority, volumeStats.IsQuestExclude), 200, 25, 5);
					y += 16;
					this.LabelWithOutline(x, y, string.Format("Sleepers: {0}   MinMax: {1}-{2}", volumeStats.SleeperCount, volumeStats.SpawnCountMin, volumeStats.SpawnCountMax), 200, 25, 5);
					y += 16;
				}
			}
			else
			{
				y += 112;
			}
			num = y;
		}
		return y + 10;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int showDebugPanel_Prefab(int x, int y)
	{
		DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.GetDynamicPrefabDecorator();
		PrefabInstance prefabInstance = ((dynamicPrefabDecorator != null) ? dynamicPrefabDecorator.ActivePrefab : null);
		if (prefabInstance == null)
		{
			return y;
		}
		Prefab.BlockStatistics blockStatistics = prefabInstance.prefab.GetBlockStatistics();
		int num = 0;
		int num2 = y;
		int num3 = 220;
		for (NGuiWdwDebugPanels.EGuiState eguiState = NGuiWdwDebugPanels.EGuiState.CalcSize; eguiState < NGuiWdwDebugPanels.EGuiState.Count; eguiState++)
		{
			y = num2;
			this.PanelBoxWithHeader(eguiState, x, ref y, num3, num - num2 + 5, "Prefab");
			if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
			{
				this.LabelWithOutline(x, y, "Name: " + prefabInstance.prefab.PrefabName, 200, 25, 5);
			}
			y += 16;
			if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
			{
				int num4 = y;
				string text = "Pos: ";
				Vector3i vector3i = prefabInstance.boundingBoxPosition;
				this.LabelWithOutline(x, num4, text + vector3i.ToString(), 200, 25, 5);
			}
			y += 16;
			if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
			{
				Vector3i vector3i2 = prefabInstance.prefab.size;
				SelectionBox selection = SelectionBoxManager.Instance.Selection;
				if (selection != null)
				{
					vector3i2 = selection.GetScale();
				}
				int num5 = y;
				string text2 = "Size: ";
				Vector3i vector3i = vector3i2;
				this.LabelWithOutline(x, num5, text2 + vector3i.ToString(), 200, 25, 5);
			}
			y += 16;
			if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
			{
				this.LabelWithOutline(x, y, "Rot: " + prefabInstance.rotation.ToString(), 70, 25, 5);
				this.LabelWithOutline(x, y, "RotToNorth: " + prefabInstance.prefab.rotationToFaceNorth.ToString(), 130, 25, 75);
			}
			y += 16;
			if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
			{
				this.LabelWithOutline(x, y, string.Format("BEnts: {0} BMods: {1} Wdws: {2}", blockStatistics.cntBlockEntities, blockStatistics.cntBlockModels, blockStatistics.cntWindows), 200, 25, 5);
			}
			y += 16;
			num = y;
		}
		return y + 10;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int showDebugPanel_Stealth(int x, int y)
	{
		int num = 0;
		int num2 = y;
		int num3 = 220;
		for (NGuiWdwDebugPanels.EGuiState eguiState = NGuiWdwDebugPanels.EGuiState.CalcSize; eguiState < NGuiWdwDebugPanels.EGuiState.Count; eguiState++)
		{
			y = num2;
			this.PanelBoxWithHeader(eguiState, x, ref y, num3, num - num2 + 5, "Stealth");
			EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
			if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
			{
				float num4;
				float stealthLightLevel = LightManager.GetStealthLightLevel(primaryPlayer, out num4);
				this.LabelWithOutline(x, y, string.Format("Player light: {0} + {1}", (int)(stealthLightLevel * 100f), (int)(num4 * 100f)), 200, 25, 5);
			}
			y += 16;
			if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
			{
				this.LabelWithOutline(x, y, string.Format("Light: {0}", primaryPlayer.Stealth.lightLevel), 200, 25, 5);
			}
			y += 16;
			if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
			{
				this.LabelWithOutline(x, y, string.Format("Noise: {0}", primaryPlayer.Stealth.noiseVolume), 200, 25, 5);
			}
			y += 16;
			num = y;
		}
		return y + 10;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int showDebugPanel_Texture(int x, int y)
	{
		int num = 0;
		int num2 = y;
		int num3 = 220;
		for (NGuiWdwDebugPanels.EGuiState eguiState = NGuiWdwDebugPanels.EGuiState.CalcSize; eguiState < NGuiWdwDebugPanels.EGuiState.Count; eguiState++)
		{
			y = num2;
			this.PanelBoxWithHeader(eguiState, x, ref y, num3, num - num2 + 5, "Texture");
			bool streamingMipmapsActive = QualitySettings.streamingMipmapsActive;
			if (streamingMipmapsActive)
			{
				if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
				{
					this.LabelWithOutline(x, y, string.Format("Streaming mipmaps enabled: {0}", streamingMipmapsActive), 200, 25, 5);
				}
				y += 16;
				if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
				{
					this.LabelWithOutline(x, y, string.Format("Streaming budget: {0} MB", QualitySettings.streamingMipmapsMemoryBudget), 200, 25, 5);
				}
				y += 16;
				if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
				{
					this.LabelWithOutline(x, y, string.Format("Memory desired: {0:F2} MB", Texture.desiredTextureMemory * 9.5367431640625E-07), 200, 25, 5);
				}
				y += 16;
				if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
				{
					this.LabelWithOutline(x, y, string.Format("Memory target: {0:F2} MB", Texture.targetTextureMemory * 9.5367431640625E-07), 200, 25, 5);
				}
				y += 16;
				if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
				{
					this.LabelWithOutline(x, y, string.Format("Memory current: {0:F2} MB", Texture.currentTextureMemory * 9.5367431640625E-07), 200, 25, 5);
				}
				y += 16;
				if (eguiState == NGuiWdwDebugPanels.EGuiState.Draw)
				{
					this.LabelWithOutline(x, y, string.Format("Non-streamed memory: {0:F2} MB", Texture.nonStreamingTextureMemory * 9.5367431640625E-07), 200, 25, 5);
				}
				y += 16;
			}
			num = y;
		}
		return y + 10;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void panelManager()
	{
		float num;
		float num2;
		Matrix4x4 matrix4x = GUIWindow.UiScaleMatrix(out num, out num2, 0f, 0f, 0.4f, 2f);
		int num3 = (GameManager.Instance.IsEditMode() ? 365 : 247);
		num3 += 55;
		float num4 = num / num2;
		num3 = (int)((float)num3 * num4);
		num3 = this.showDebugPanel_EnablePanels(18, num3);
		for (int i = 0; i < this.Panels.Count; i++)
		{
			NGuiWdwDebugPanels.PanelDefinition panelDefinition = this.Panels[i];
			if (panelDefinition.Enabled && panelDefinition.Active)
			{
				num3 = panelDefinition.GuiHandler(18, num3);
			}
		}
		if (!string.IsNullOrEmpty(GUI.tooltip))
		{
			Vector3 vector = Input.mousePosition;
			vector.y = (float)Screen.height - vector.y;
			vector /= num2;
			vector.y -= 20f;
			GUI.color = Color.white;
			GUI.Label(new Rect(vector.x, vector.y, 100f, 20f), GUI.tooltip ?? "", NGuiWdwDebugPanels.guiStyleTooltipLabel);
		}
		GUI.matrix = matrix4x;
	}

	public void SetActivePanels(params string[] panelCaptions)
	{
		foreach (NGuiWdwDebugPanels.PanelDefinition panelDefinition in this.Panels)
		{
			bool flag = false;
			foreach (string text in panelCaptions)
			{
				if (panelDefinition.ButtonCaption == text)
				{
					flag = true;
					break;
				}
			}
			panelDefinition.Active = flag;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public NGuiWdwDebugPanels.EDebugDataType debugData;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public NGuiWdwDebugPanels.EPerformanceDisplayType performanceType;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly GUIStyle guiStyleDebug = new GUIStyle();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static GUIStyle guiStyleToggleBox;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static GUIStyle guiStyleTooltipLabel;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static GUIStyle guiStyleLabelRightAligned;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GUIFPS guiFPS;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public NetworkMonitor networkMonitorCh0;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public NetworkMonitor networkMonitorCh1;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityPlayerLocal playerEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public LocalPlayerUI playerUI;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector2i lastResolution;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GUIStyle boxStyle;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int boxAreaHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int boxAreaWidth;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cLineHeight = 16;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cHeaderLabelWidth = 200;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cHeaderLabelHeight = 25;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 lastPlayerPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lastPlayerTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float playerSpeed;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static string filterCVar = "";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public readonly List<NGuiWdwDebugPanels.PanelDefinition> Panels = new List<NGuiWdwDebugPanels.PanelDefinition>();

	[PublicizedFrom(EAccessModifier.Private)]
	public enum EDebugDataType
	{
		Off,
		General
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum EPerformanceDisplayType
	{
		Off,
		Fps,
		FpsAndHeat,
		FpsAndFpsGraph,
		FpsAndNetGraphs
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum EGuiState
	{
		CalcSize,
		Draw,
		Count
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class PanelDefinition
	{
		public PanelDefinition(string _name, string _buttonCaption, Func<int, int, int> _guiHandler, string _enabledPanels, bool _enabled = true)
		{
			this.Name = _name;
			this.ButtonCaption = _buttonCaption;
			this.GuiHandler = _guiHandler;
			this.Enabled = _enabled;
			this.Active = _enabledPanels.Contains("," + _buttonCaption + ",");
		}

		public string Name;

		public string ButtonCaption;

		public Func<int, int, int> GuiHandler;

		public bool Enabled;

		public bool Active;
	}
}
