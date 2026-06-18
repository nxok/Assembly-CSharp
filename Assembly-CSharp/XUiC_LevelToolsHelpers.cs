using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class XUiC_LevelToolsHelpers
{
	public static NGuiAction BuildAction(string _functionName, string _captionOverride, bool _forToggle)
	{
		if (_functionName.IndexOf(':') < 0)
		{
			return null;
		}
		NGuiAction nguiAction = null;
		if (_functionName.StartsWith("SBM:"))
		{
			nguiAction = XUiC_LevelToolsHelpers.createSelectionBoxAction(_functionName.Substring("SBM:".Length));
		}
		else if (_functionName.StartsWith("BTS:"))
		{
			nguiAction = XUiC_LevelToolsHelpers.createBlockToolSelectionAction(_functionName.Substring("BTS:".Length));
		}
		else if (_functionName.StartsWith("Special:"))
		{
			nguiAction = XUiC_LevelToolsHelpers.createSpecialAction(_functionName.Substring("Special:".Length));
		}
		if (nguiAction == null)
		{
			Log.Error("Function " + _functionName + " for LevelTools UI not found");
			return null;
		}
		if (!string.IsNullOrEmpty(_captionOverride))
		{
			nguiAction.SetText(_captionOverride);
		}
		if (_forToggle != nguiAction.IsToggle())
		{
			Log.Error(_forToggle ? ("Function " + _functionName + " for LevelTools UI is not a toggle action, but bound to a toggle button") : ("Function " + _functionName + " for LevelTools UI is a toggle action, but bound to a regular button"));
			return null;
		}
		return nguiAction;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static NGuiAction createSelectionBoxAction(string _categoryName)
	{
		SelectionCategory selectionCategory = SelectionBoxManager.Instance.GetCategory(_categoryName);
		if (selectionCategory == null)
		{
			return null;
		}
		NGuiAction nguiAction = new NGuiAction(string.Format(Localization.Get("xuiShowVolumesCategory", false, null), Localization.Get("selectionCategory" + _categoryName, false, null)), null, true);
		nguiAction.SetDescription(_categoryName);
		nguiAction.SetClickActionDelegate(delegate
		{
			SelectionCategory selectionCategory2 = selectionCategory;
			selectionCategory2.SetVisible(!selectionCategory2.IsVisible());
		});
		nguiAction.SetIsCheckedDelegate(() => selectionCategory.IsVisible());
		return nguiAction;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static NGuiAction createBlockToolSelectionAction(string _actionName)
	{
		BlockToolSelection blockToolSelection = (BlockToolSelection)((GameManager.Instance.GetActiveBlockTool() is BlockToolSelection) ? GameManager.Instance.GetActiveBlockTool() : null);
		if (blockToolSelection == null)
		{
			return null;
		}
		NGuiAction nguiAction;
		blockToolSelection.GetActions().TryGetValue(_actionName, out nguiAction);
		return nguiAction;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static NGuiAction createSpecialAction(string _substring)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(_substring);
		if (num <= 2251234481U)
		{
			if (num <= 974305612U)
			{
				if (num <= 495924396U)
				{
					if (num <= 332713759U)
					{
						if (num != 205860020U)
						{
							if (num == 332713759U)
							{
								if (_substring == "SleeperXRay")
								{
									NGuiAction nguiAction = new NGuiAction(Localization.Get("leveltoolsSleeperXRay", false, null), null, true);
									nguiAction.SetClickActionDelegate(delegate
									{
										SleeperVolumeToolManager.XRayMode = !SleeperVolumeToolManager.XRayMode;
									});
									nguiAction.SetIsCheckedDelegate(() => SleeperVolumeToolManager.XRayMode);
									return nguiAction;
								}
							}
						}
						else if (_substring == "TexturesStripInternal")
						{
							NGuiAction nguiAction2 = new NGuiAction(Localization.Get("xuiStripInternalTextures", false, null), null, false);
							nguiAction2.SetClickActionDelegate(delegate
							{
								PrefabEditModeManager.Instance.StripInternalTextures();
							});
							return nguiAction2;
						}
					}
					else if (num != 471674119U)
					{
						if (num == 495924396U)
						{
							if (_substring == "PrefabScreenshotTake")
							{
								NGuiAction nguiAction3 = new NGuiAction(Localization.Get("xuiTakeScreenshot", false, null), null, false);
								nguiAction3.SetClickActionDelegate(delegate
								{
									if (PrefabEditModeManager.Instance.VoxelPrefab == null)
									{
										GameManager.ShowTooltip(LocalPlayerUI.GetUIForPrimaryPlayer().entityPlayer, "[FF4444]" + Localization.Get("xuiScreenshotNoPrefabLoaded", false, null), false, false, 0f);
										return;
									}
									ThreadManager.StartCoroutine(XUiC_LevelToolsHelpers.screenshotCo(PrefabEditModeManager.Instance.LoadedPrefab.FullPathNoExtension));
								});
								nguiAction3.SetIsEnabledDelegate(() => SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && PrefabEditModeManager.Instance.VoxelPrefab != null);
								return nguiAction3;
							}
						}
					}
					else if (_substring == "PaintTexturesToggle")
					{
						NGuiAction nguiAction4 = new NGuiAction(Localization.Get("xuiShowPaintTextures", false, null), null, true);
						nguiAction4.SetClickActionDelegate(delegate
						{
							Chunk.IgnorePaintTextures = !Chunk.IgnorePaintTextures;
							foreach (Chunk chunk in GameManager.Instance.World.ChunkCache.GetChunkArrayCopySync())
							{
								chunk.NeedsRegeneration = true;
							}
						});
						nguiAction4.SetIsCheckedDelegate(() => !Chunk.IgnorePaintTextures);
						return nguiAction4;
					}
				}
				else if (num <= 768475078U)
				{
					if (num != 584220759U)
					{
						if (num == 768475078U)
						{
							if (_substring == "GroundGridMoveUp")
							{
								NGuiAction nguiAction5 = new NGuiAction(Localization.Get("xuiShowMoveGroundGridUp", false, null), null, false);
								nguiAction5.SetClickActionDelegate(delegate
								{
									PrefabEditModeManager.Instance.MoveGroundGridUpOrDown(1);
								});
								nguiAction5.SetIsEnabledDelegate(() => PrefabEditModeManager.Instance.IsGroundGrid() && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer);
								return nguiAction5;
							}
						}
					}
					else if (_substring == "ImposterUpdate")
					{
						NGuiAction nguiAction6 = new NGuiAction(Localization.Get("xuiUpdateImposter", false, null), null, false);
						nguiAction6.SetClickActionDelegate(delegate
						{
							XUiC_SaveDirtyPrefab.Show(LocalPlayerUI.GetUIForPrimaryPlayer().xui, new Action<XUiC_SaveDirtyPrefab.ESelectedAction>(XUiC_LevelToolsHelpers.updateImposter), XUiC_SaveDirtyPrefab.EMode.AskSaveIfDirty);
						});
						nguiAction6.SetIsEnabledDelegate(() => SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && PrefabEditModeManager.Instance.LoadedPrefab.Type != PathAbstractions.EAbstractedLocationType.None);
						return nguiAction6;
					}
				}
				else if (num != 862668730U)
				{
					if (num == 974305612U)
					{
						if (_substring == "PrefabFacingUpdate")
						{
							NGuiAction nguiAction7 = new NGuiAction(Localization.Get("xuiUpdateFacing", false, null), null, false);
							nguiAction7.SetClickActionDelegate(delegate
							{
								PrefabEditModeManager.Instance.RotatePrefabFacing();
							});
							nguiAction7.SetIsEnabledDelegate(() => PrefabEditModeManager.Instance.IsPrefabFacing());
							return nguiAction7;
						}
					}
				}
				else if (_substring == "Unpaintable")
				{
					NGuiAction nguiAction8 = new NGuiAction(Localization.Get("leveltoolsShowUnpaintable", false, null), null, true);
					nguiAction8.SetClickActionDelegate(delegate
					{
						GameManager.bShowUnpaintables = !GameManager.bShowUnpaintables;
						XUiC_LevelToolsHelpers.setChunkPartVisible(XUiC_LevelToolsHelpers.goNamesUnpaintable, GameManager.bShowUnpaintables, null);
					});
					nguiAction8.SetIsCheckedDelegate(() => GameManager.bShowUnpaintables);
					return nguiAction8;
				}
			}
			else if (num <= 1564320335U)
			{
				if (num <= 1411352014U)
				{
					if (num != 1277691935U)
					{
						if (num == 1411352014U)
						{
							if (_substring == "PrefabScreenshotToggleBounds")
							{
								NGuiAction nguiAction9 = new NGuiAction(Localization.Get("xuiShowScreenshotBounds", false, null), null, true);
								nguiAction9.SetClickActionDelegate(delegate
								{
									XUiC_LevelToolsHelpers.drawingScreenshotGuide = !XUiC_LevelToolsHelpers.drawingScreenshotGuide;
									if (XUiC_LevelToolsHelpers.drawingScreenshotGuide)
									{
										ThreadManager.StartCoroutine(XUiC_LevelToolsHelpers.drawScreenshotGuide());
									}
								});
								nguiAction9.SetIsCheckedDelegate(() => XUiC_LevelToolsHelpers.drawingScreenshotGuide);
								return nguiAction9;
							}
						}
					}
					else if (_substring == "QuestLoot")
					{
						NGuiAction nguiAction10 = new NGuiAction(Localization.Get("leveltoolsShowQuestLoot", false, null), null, true);
						nguiAction10.SetClickActionDelegate(delegate
						{
							PrefabEditModeManager.Instance.HighlightQuestLoot = !PrefabEditModeManager.Instance.HighlightQuestLoot;
						});
						nguiAction10.SetIsCheckedDelegate(() => PrefabEditModeManager.Instance.HighlightQuestLoot);
						return nguiAction10;
					}
				}
				else if (num != 1547620563U)
				{
					if (num == 1564320335U)
					{
						if (_substring == "GroundGridMoveDown")
						{
							NGuiAction nguiAction11 = new NGuiAction(Localization.Get("xuiShowMoveGroundGridDown", false, null), null, false);
							nguiAction11.SetClickActionDelegate(delegate
							{
								PrefabEditModeManager.Instance.MoveGroundGridUpOrDown(-1);
							});
							nguiAction11.SetIsEnabledDelegate(() => PrefabEditModeManager.Instance.IsGroundGrid() && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer);
							return nguiAction11;
						}
					}
				}
				else if (_substring == "PrefabUpdateBounds")
				{
					NGuiAction nguiAction12 = new NGuiAction(Localization.Get("xuiUpdateBounds", false, null), null, false);
					nguiAction12.SetClickActionDelegate(delegate
					{
						PrefabEditModeManager.Instance.UpdatePrefabBounds();
					});
					nguiAction12.SetIsEnabledDelegate(() => SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && PrefabEditModeManager.Instance.VoxelPrefab != null);
					return nguiAction12;
				}
			}
			else if (num <= 2008226722U)
			{
				if (num != 1687552983U)
				{
					if (num == 2008226722U)
					{
						if (_substring == "PrefabMoveDown")
						{
							NGuiAction nguiAction13 = new NGuiAction(Localization.Get("xuiMovePrefabDown", false, null), null, false);
							nguiAction13.SetClickActionDelegate(delegate
							{
								PrefabEditModeManager.Instance.MovePrefabUpOrDown(-1);
							});
							nguiAction13.SetIsEnabledDelegate(() => SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && PrefabEditModeManager.Instance.VoxelPrefab != null);
							return nguiAction13;
						}
					}
				}
				else if (_substring == "Loot")
				{
					NGuiAction nguiAction14 = new NGuiAction(Localization.Get("leveltoolsShowLoot", false, null), null, true);
					nguiAction14.SetClickActionDelegate(delegate
					{
						GameManager.bShowLootBlocks = !GameManager.bShowLootBlocks;
						foreach (Chunk chunk2 in GameManager.Instance.World.ChunkCache.GetChunkArrayCopySync())
						{
							chunk2.NeedsRegeneration = true;
						}
					});
					nguiAction14.SetIsCheckedDelegate(() => GameManager.bShowLootBlocks);
					return nguiAction14;
				}
			}
			else if (num != 2207638467U)
			{
				if (num == 2251234481U)
				{
					if (_substring == "HighlightBlocksToggle")
					{
						NGuiAction nguiAction15 = new NGuiAction(Localization.Get("xuiHighlightBlocks", false, null), null, true);
						nguiAction15.SetClickActionDelegate(delegate
						{
							PrefabEditModeManager.Instance.ToggleHighlightBlocks();
						});
						nguiAction15.SetIsCheckedDelegate(() => PrefabEditModeManager.Instance.HighlightingBlocks);
						return nguiAction15;
					}
				}
			}
			else if (_substring == "BlockTriggers")
			{
				NGuiAction nguiAction16 = new NGuiAction(Localization.Get("leveltoolsShowBlockTriggers", false, null), null, true);
				nguiAction16.SetClickActionDelegate(delegate
				{
					PrefabEditModeManager.Instance.HighlightBlockTriggers = !PrefabEditModeManager.Instance.HighlightBlockTriggers;
				});
				nguiAction16.SetIsCheckedDelegate(() => PrefabEditModeManager.Instance.HighlightBlockTriggers);
				return nguiAction16;
			}
		}
		else if (num <= 2888697999U)
		{
			if (num <= 2586439314U)
			{
				if (num <= 2415635990U)
				{
					if (num != 2404123864U)
					{
						if (num == 2415635990U)
						{
							if (_substring == "ImposterToggle")
							{
								NGuiAction nguiAction17 = new NGuiAction(Localization.Get("xuiShowImposter", false, null), null, true);
								nguiAction17.SetClickActionDelegate(new NGuiAction.OnClickActionDelegate(XUiC_LevelToolsHelpers.imposterToggleShow));
								nguiAction17.SetIsCheckedDelegate(() => SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && PrefabEditModeManager.Instance.IsShowingImposterPrefab());
								nguiAction17.SetIsEnabledDelegate(() => SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer);
								return nguiAction17;
							}
						}
					}
					else if (_substring == "DensitiesSmoothLand")
					{
						NGuiAction nguiAction18 = new NGuiAction(Localization.Get("xuiSmoothPrefabLand", false, null), null, false);
						nguiAction18.SetClickActionDelegate(delegate
						{
							PrefabHelpers.SmoothPOI(1, true);
						});
						nguiAction18.SetIsEnabledDelegate(() => SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && PrefabEditModeManager.Instance.VoxelPrefab != null);
						return nguiAction18;
					}
				}
				else if (num != 2435663207U)
				{
					if (num == 2586439314U)
					{
						if (_substring == "Decor")
						{
							NGuiAction nguiAction19 = new NGuiAction(Localization.Get("leveltoolsShowDecor", false, null), null, true);
							nguiAction19.SetClickActionDelegate(delegate
							{
								GameManager.bShowDecorBlocks = !GameManager.bShowDecorBlocks;
								foreach (Chunk chunk3 in GameManager.Instance.World.ChunkCache.GetChunkArrayCopySync())
								{
									chunk3.NeedsRegeneration = true;
								}
							});
							nguiAction19.SetIsCheckedDelegate(() => GameManager.bShowDecorBlocks);
							return nguiAction19;
						}
					}
				}
				else if (_substring == "PrefabMoveUp")
				{
					NGuiAction nguiAction20 = new NGuiAction(Localization.Get("xuiMovePrefabUp", false, null), null, false);
					nguiAction20.SetClickActionDelegate(delegate
					{
						PrefabEditModeManager.Instance.MovePrefabUpOrDown(1);
					});
					nguiAction20.SetIsEnabledDelegate(() => SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && PrefabEditModeManager.Instance.VoxelPrefab != null);
					return nguiAction20;
				}
			}
			else if (num <= 2782942213U)
			{
				if (num != 2630448995U)
				{
					if (num == 2782942213U)
					{
						if (_substring == "CompositionGrid")
						{
							NGuiAction nguiAction21 = new NGuiAction(Localization.Get("leveltoolsShowCompositionGrid", false, null), null, true);
							nguiAction21.SetClickActionDelegate(delegate
							{
								PrefabEditModeManager.Instance.ToggleCompositionGrid();
							});
							nguiAction21.SetIsCheckedDelegate(() => PrefabEditModeManager.Instance.IsCompositionGrid());
							return nguiAction21;
						}
					}
				}
				else if (_substring == "LightPerformance")
				{
					NGuiAction nguiAction22 = new NGuiAction(Localization.Get("xuiDebugMenuShowLightPerf", false, null), null, true);
					nguiAction22.SetClickActionDelegate(delegate
					{
						LightViewer.SetEnabled(!LightViewer.IsEnabled);
					});
					nguiAction22.SetIsCheckedDelegate(() => LightViewer.IsEnabled);
					return nguiAction22;
				}
			}
			else if (num != 2818509998U)
			{
				if (num == 2888697999U)
				{
					if (_substring == "Paintable")
					{
						NGuiAction nguiAction23 = new NGuiAction(Localization.Get("leveltoolsShowPaintable", false, null), null, true);
						nguiAction23.SetClickActionDelegate(delegate
						{
							GameManager.bShowPaintables = !GameManager.bShowPaintables;
							XUiC_LevelToolsHelpers.setChunkPartVisible(XUiC_LevelToolsHelpers.goNamesPaintable, GameManager.bShowPaintables, null);
						});
						nguiAction23.SetIsCheckedDelegate(() => GameManager.bShowPaintables);
						return nguiAction23;
					}
				}
			}
			else if (_substring == "Terrain")
			{
				NGuiAction nguiAction24 = new NGuiAction(Localization.Get("leveltoolsShowTerrain", false, null), null, true);
				nguiAction24.SetClickActionDelegate(delegate
				{
					GameManager.bShowTerrain = !GameManager.bShowTerrain;
					XUiC_LevelToolsHelpers.setChunkPartVisible(XUiC_LevelToolsHelpers.goNamesTerrain, GameManager.bShowTerrain, null);
				});
				nguiAction24.SetIsCheckedDelegate(() => GameManager.bShowTerrain);
				return nguiAction24;
			}
		}
		else if (num <= 3725261786U)
		{
			if (num <= 3470872050U)
			{
				if (num != 3269426397U)
				{
					if (num == 3470872050U)
					{
						if (_substring == "CapturePrefabStats")
						{
							NGuiAction nguiAction25 = new NGuiAction(Localization.Get("xuiCapturePrefabStats", false, null), null, false);
							nguiAction25.SetClickActionDelegate(delegate
							{
								if (PrefabEditModeManager.Instance.VoxelPrefab == null)
								{
									GameManager.ShowTooltip(LocalPlayerUI.GetUIForPrimaryPlayer().entityPlayer, "[FF4444]" + Localization.Get("xuiPrefabStatsNoPrefabLoaded", false, null), false, false, 0f);
									return;
								}
								XUiC_EditorStat.ManualStats = WorldStats.CaptureWorldStats();
							});
							return nguiAction25;
						}
					}
				}
				else if (_substring == "PrefabFacingToggle")
				{
					NGuiAction nguiAction26 = new NGuiAction(Localization.Get("xuiShowFacing", false, null), null, true);
					nguiAction26.SetClickActionDelegate(delegate
					{
						PrefabEditModeManager.Instance.TogglePrefabFacing(!PrefabEditModeManager.Instance.IsPrefabFacing());
					});
					nguiAction26.SetIsCheckedDelegate(() => PrefabEditModeManager.Instance.IsPrefabFacing());
					return nguiAction26;
				}
			}
			else if (num != 3711817000U)
			{
				if (num == 3725261786U)
				{
					if (_substring == "PrefabProperties")
					{
						NGuiAction nguiAction27 = new NGuiAction(Localization.Get("xuiPrefabProperties", false, null), null, false);
						nguiAction27.SetClickActionDelegate(delegate
						{
							XUiC_PrefabPropertiesEditor.Show(LocalPlayerUI.GetUIForPrimaryPlayer().xui, XUiC_PrefabPropertiesEditor.EPropertiesFrom.LoadedPrefab, PathAbstractions.AbstractedLocation.None);
						});
						nguiAction27.SetIsEnabledDelegate(() => SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && PrefabEditModeManager.Instance.VoxelPrefab != null);
						return nguiAction27;
					}
				}
			}
			else if (_substring == "ShowChunkBorders")
			{
				NGuiAction nguiAction28 = new NGuiAction(Localization.Get("leveltoolsShowChunkBorders", false, null), null, true);
				nguiAction28.SetClickActionDelegate(delegate
				{
					PlayerMoveController moveController = LocalPlayerUI.GetUIForPrimaryPlayer().entityPlayer.MoveController;
					moveController.drawChunkMode = (moveController.drawChunkMode + 1) % 2;
				});
				nguiAction28.SetIsCheckedDelegate(() => LocalPlayerUI.GetUIForPrimaryPlayer().entityPlayer.MoveController.drawChunkMode > 0);
				return nguiAction28;
			}
		}
		else if (num <= 3948481587U)
		{
			if (num != 3821190062U)
			{
				if (num == 3948481587U)
				{
					if (_substring == "DensitiesSmoothAir")
					{
						NGuiAction nguiAction29 = new NGuiAction(Localization.Get("xuiSmoothPrefabAir", false, null), null, false);
						nguiAction29.SetClickActionDelegate(delegate
						{
							PrefabHelpers.SmoothPOI(1, false);
						});
						nguiAction29.SetIsEnabledDelegate(() => SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && PrefabEditModeManager.Instance.VoxelPrefab != null);
						return nguiAction29;
					}
				}
			}
			else if (_substring == "GroundGridToggle")
			{
				NGuiAction nguiAction30 = new NGuiAction(Localization.Get("xuiShowGroundGrid", false, null), null, true);
				nguiAction30.SetClickActionDelegate(delegate
				{
					PrefabEditModeManager.Instance.ToggleGroundGrid(false);
				});
				nguiAction30.SetIsCheckedDelegate(() => PrefabEditModeManager.Instance.IsGroundGrid());
				return nguiAction30;
			}
		}
		else if (num != 4053226129U)
		{
			if (num == 4167618992U)
			{
				if (_substring == "DensitiesClean")
				{
					NGuiAction nguiAction31 = new NGuiAction(Localization.Get("xuiCleanDensity", false, null), null, false);
					nguiAction31.SetClickActionDelegate(new NGuiAction.OnClickActionDelegate(XUiC_LevelToolsHelpers.DensitiesClean));
					nguiAction31.SetIsEnabledDelegate(() => SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && PrefabEditModeManager.Instance.VoxelPrefab != null);
					return nguiAction31;
				}
			}
		}
		else if (_substring == "TexturesStrip")
		{
			NGuiAction nguiAction32 = new NGuiAction(Localization.Get("xuiStripTextures", false, null), null, false);
			nguiAction32.SetClickActionDelegate(delegate
			{
				PrefabEditModeManager.Instance.StripTextures();
			});
			nguiAction32.SetIsEnabledDelegate(() => SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && PrefabEditModeManager.Instance.VoxelPrefab != null);
			return nguiAction32;
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void setChunkPartVisible(string[] _matchedNames, bool _visible, List<ChunkGameObject> _cgos = null)
	{
		if (_cgos == null)
		{
			_cgos = GameManager.Instance.World.m_ChunkManager.GetUsedChunkGameObjects();
		}
		foreach (ChunkGameObject chunkGameObject in _cgos)
		{
			XUiC_LevelToolsHelpers.setChunkPartVisible(chunkGameObject.transform, _matchedNames, _visible);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void setChunkPartVisible(Transform _parent, string[] _matchedNames, bool _visible)
	{
		for (int i = 0; i < _parent.childCount; i++)
		{
			Transform child = _parent.GetChild(i);
			string name = child.name;
			if (_matchedNames.ContainsCaseInsensitive(name))
			{
				child.gameObject.SetActive(_visible);
			}
			else if (child.childCount > 0)
			{
				XUiC_LevelToolsHelpers.setChunkPartVisible(child, _matchedNames, _visible);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void updateImposter(XUiC_SaveDirtyPrefab.ESelectedAction _action)
	{
		LocalPlayerUI.GetUIForPrimaryPlayer().windowManager.Open(XUiC_InGameMenuWindow.ID, true);
		if (_action == XUiC_SaveDirtyPrefab.ESelectedAction.Cancel)
		{
			return;
		}
		LocalPlayerUI.GetUIForPrimaryPlayer().windowManager.TempHUDDisable();
		XUiC_LevelToolsHelpers.wasShowingImposterBeforeUpdate = PrefabEditModeManager.Instance.IsShowingImposterPrefab();
		PrefabHelpers.convert(new Action(XUiC_LevelToolsHelpers.waitForUpdateImposter));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void waitForUpdateImposter()
	{
		PrefabHelpers.Cleanup();
		if (XUiC_LevelToolsHelpers.wasShowingImposterBeforeUpdate)
		{
			PrefabEditModeManager.Instance.LoadImposterPrefab(PrefabEditModeManager.Instance.LoadedPrefab);
		}
		else
		{
			PrefabEditModeManager.Instance.LoadVoxelPrefab(PrefabEditModeManager.Instance.LoadedPrefab, false, false);
		}
		LocalPlayerUI.GetUIForPrimaryPlayer().windowManager.ReEnableHUD();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void imposterToggleShow()
	{
		if (PrefabEditModeManager.Instance.IsShowingImposterPrefab())
		{
			XUiC_LevelToolsHelpers.showPrefab();
			return;
		}
		XUiC_SaveDirtyPrefab.Show(LocalPlayerUI.GetUIForPrimaryPlayer().xui, new Action<XUiC_SaveDirtyPrefab.ESelectedAction>(XUiC_LevelToolsHelpers.showImposter), XUiC_SaveDirtyPrefab.EMode.AskSaveIfDirty);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void showImposter(XUiC_SaveDirtyPrefab.ESelectedAction _action)
	{
		LocalPlayerUI.GetUIForPrimaryPlayer().windowManager.Open(XUiC_InGameMenuWindow.ID, true);
		if (_action == XUiC_SaveDirtyPrefab.ESelectedAction.Cancel)
		{
			return;
		}
		PathAbstractions.AbstractedLocation loadedPrefab = PrefabEditModeManager.Instance.LoadedPrefab;
		PrefabEditModeManager.Instance.ClearImposterPrefab();
		if (PrefabEditModeManager.Instance.HasPrefabImposter(loadedPrefab))
		{
			PrefabEditModeManager.Instance.LoadImposterPrefab(loadedPrefab);
			return;
		}
		GameManager.ShowTooltip(GameManager.Instance.World.GetLocalPlayers()[0], "Prefab " + loadedPrefab.Name + " has no imposter yet", false, false, 0f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void showPrefab()
	{
		if (PrefabEditModeManager.Instance.LoadedPrefab.Type != PathAbstractions.EAbstractedLocationType.None)
		{
			PrefabEditModeManager.Instance.LoadVoxelPrefab(PrefabEditModeManager.Instance.LoadedPrefab, false, false);
		}
	}

	public static bool IsShowImposter()
	{
		return PrefabEditModeManager.Instance.IsShowingImposterPrefab();
	}

	public static void SetShowImposter()
	{
		XUiC_LevelToolsHelpers.imposterToggleShow();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator drawScreenshotGuide()
	{
		while (XUiC_LevelToolsHelpers.drawingScreenshotGuide)
		{
			yield return new WaitForEndOfFrame();
			Rect screenshotRect = GameUtils.GetScreenshotRect(0.15f, true);
			screenshotRect..ctor(screenshotRect.x - 2f, screenshotRect.y - 2f, screenshotRect.width + 4f, screenshotRect.height + 4f);
			GUIUtils.DrawRect(screenshotRect, Color.green);
			if (!GameManager.Instance.gameStateManager.IsGameStarted())
			{
				XUiC_LevelToolsHelpers.drawingScreenshotGuide = false;
			}
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator screenshotCo(string _filename)
	{
		LocalPlayerUI.GetUIForPrimaryPlayer().windowManager.TempHUDDisable();
		EntityPlayerLocal player = GameManager.Instance.World.GetPrimaryPlayer();
		bool isSpectator = player.IsSpectator;
		player.IsSpectator = true;
		SkyManager.SetSkyEnabled(false);
		yield return null;
		try
		{
			GameUtils.TakeScreenShot(GameUtils.EScreenshotMode.File, _filename, 0.15f, true, 280, 210, false);
		}
		catch (Exception ex)
		{
			Log.Exception(ex);
		}
		yield return null;
		player.IsSpectator = isSpectator;
		SkyManager.SetSkyEnabled(true);
		LocalPlayerUI.GetUIForPrimaryPlayer().windowManager.ReEnableHUD();
		yield break;
	}

	public static void ReplaceBlockId(Block _srcBlockClass, Block _dstBlockClass)
	{
		int sourceBlockId = _srcBlockClass.blockID;
		int targetBlockId = _dstBlockClass.blockID;
		HashSet<Chunk> changedChunks = new HashSet<Chunk>();
		bool bUseSelection = BlockToolSelection.Instance.SelectionActive;
		Vector3i selStart = BlockToolSelection.Instance.SelectionMin;
		Vector3i selEnd = selStart + BlockToolSelection.Instance.SelectionSize - Vector3i.one;
		List<Chunk> chunkArrayCopySync = GameManager.Instance.World.ChunkCache.GetChunkArrayCopySync();
		for (int i = 0; i < chunkArrayCopySync.Count; i++)
		{
			Chunk curChunk = chunkArrayCopySync[i];
			curChunk.LoopOverAllBlocks(delegate(int _x, int _y, int _z, BlockValue _bv)
			{
				if (_bv.type != sourceBlockId)
				{
					return;
				}
				if (bUseSelection)
				{
					Vector3i vector3i = curChunk.ToWorldPos(new Vector3i(_x, _y, _z));
					if (vector3i.x < selStart.x || vector3i.x > selEnd.x || vector3i.y < selStart.y || vector3i.y > selEnd.y || vector3i.z < selStart.z || vector3i.z > selEnd.z)
					{
						return;
					}
				}
				if (_srcBlockClass.shape.IsTerrain() != _dstBlockClass.shape.IsTerrain())
				{
					sbyte b = curChunk.GetDensity(_x, _y, _z);
					if (_dstBlockClass.shape.IsTerrain())
					{
						b = MarchingCubes.DensityTerrain;
					}
					else if (b != 0)
					{
						b = MarchingCubes.DensityAir;
					}
					curChunk.SetDensity(_x, _y, _z, b);
				}
				BlockValue blockValue = new BlockValue((uint)targetBlockId)
				{
					rotation = _bv.rotation,
					meta = _bv.meta
				};
				curChunk.SetBlockRaw(_x, _y, _z, blockValue);
				changedChunks.Add(curChunk);
			}, false, true);
		}
		foreach (Chunk chunk in changedChunks)
		{
			chunk.NeedsRegeneration = true;
		}
		if (changedChunks.Count > 0)
		{
			PrefabEditModeManager.Instance.NeedsSaving = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ReplacePaint(int _sourcePaintId, int _targetPaintId)
	{
		HashSet<Chunk> changedChunks = new HashSet<Chunk>();
		bool bUseSelection = BlockToolSelection.Instance.SelectionActive;
		Vector3i selStart = BlockToolSelection.Instance.SelectionMin;
		Vector3i selEnd = selStart + BlockToolSelection.Instance.SelectionSize - Vector3i.one;
		List<Chunk> chunkArrayCopySync = GameManager.Instance.World.ChunkCache.GetChunkArrayCopySync();
		for (int i = 0; i < chunkArrayCopySync.Count; i++)
		{
			Chunk curChunk = chunkArrayCopySync[i];
			curChunk.LoopOverAllBlocks(delegate(int _x, int _y, int _z, BlockValue _bv)
			{
				if (bUseSelection)
				{
					Vector3i vector3i = curChunk.ToWorldPos(new Vector3i(_x, _y, _z));
					if (vector3i.x < selStart.x || vector3i.x > selEnd.x || vector3i.y < selStart.y || vector3i.y > selEnd.y || vector3i.z < selStart.z || vector3i.z > selEnd.z)
					{
						return;
					}
				}
				bool flag = false;
				long num = curChunk.GetTextureFull(_x, _y, _z, 0);
				for (int j = 0; j < 6; j++)
				{
					if (((num >> j * 8) & 255L) == (long)_sourcePaintId)
					{
						num &= ~(255L << j * 8);
						num |= (long)_targetPaintId << j * 8;
						flag = true;
					}
				}
				if (flag)
				{
					curChunk.SetTextureFull(_x, _y, _z, num, 0);
					changedChunks.Add(curChunk);
				}
			}, false, false);
		}
		foreach (Chunk chunk in changedChunks)
		{
			chunk.NeedsRegeneration = true;
		}
		if (changedChunks.Count > 0)
		{
			PrefabEditModeManager.Instance.NeedsSaving = true;
		}
	}

	public static void ReplaceBlockShapeMaterials(string _oldMaterial, string _newMaterial)
	{
		HashSet<Chunk> changedChunks = new HashSet<Chunk>();
		Dictionary<int, int> blockReplaceCache = new Dictionary<int, int>();
		MicroStopwatch microStopwatch = new MicroStopwatch(true);
		int hits = 0;
		int misses = 0;
		int replaced = 0;
		bool bUseSelection = BlockToolSelection.Instance.SelectionActive;
		Vector3i selStart = BlockToolSelection.Instance.SelectionMin;
		Vector3i selEnd = selStart + BlockToolSelection.Instance.SelectionSize - Vector3i.one;
		List<Chunk> chunkArrayCopySync = GameManager.Instance.World.ChunkCache.GetChunkArrayCopySync();
		for (int i = 0; i < chunkArrayCopySync.Count; i++)
		{
			Chunk curChunk = chunkArrayCopySync[i];
			curChunk.LoopOverAllBlocks(delegate(int _x, int _y, int _z, BlockValue _bv)
			{
				if (bUseSelection)
				{
					Vector3i vector3i = curChunk.ToWorldPos(new Vector3i(_x, _y, _z));
					if (vector3i.x < selStart.x || vector3i.x > selEnd.x || vector3i.y < selStart.y || vector3i.y > selEnd.y || vector3i.z < selStart.z || vector3i.z > selEnd.z)
					{
						return;
					}
				}
				int type = _bv.type;
				int blockID;
				int num;
				if (!blockReplaceCache.TryGetValue(type, out blockID))
				{
					num = misses;
					misses = num + 1;
					Block block = _bv.Block;
					if (block.GetAutoShapeType() != EAutoShapeType.Shape)
					{
						blockReplaceCache[type] = -1;
						return;
					}
					if (!block.GetAutoShapeBlockName().Equals(_oldMaterial))
					{
						blockReplaceCache[type] = -1;
						return;
					}
					string autoShapeShapeName = block.GetAutoShapeShapeName();
					Block blockByName = Block.GetBlockByName(_newMaterial + ":" + autoShapeShapeName, true);
					if (blockByName == null)
					{
						blockReplaceCache[type] = -1;
						return;
					}
					blockID = blockByName.blockID;
					blockReplaceCache[type] = blockID;
				}
				else
				{
					num = hits;
					hits = num + 1;
				}
				if (blockID < 0)
				{
					return;
				}
				num = replaced;
				replaced = num + 1;
				BlockValue blockValue = new BlockValue((uint)blockID)
				{
					rotation = _bv.rotation,
					meta = _bv.meta
				};
				curChunk.SetBlockRaw(_x, _y, _z, blockValue);
				changedChunks.Add(curChunk);
			}, false, true);
		}
		foreach (Chunk chunk in changedChunks)
		{
			chunk.NeedsRegeneration = true;
		}
		if (changedChunks.Count > 0)
		{
			PrefabEditModeManager.Instance.NeedsSaving = true;
		}
		Log.Out(string.Format("Replace material done in {0} ms. Total checked blocks: {1}, replaced: {2}, cache hits: {3}, misses: {4}", new object[]
		{
			microStopwatch.ElapsedMilliseconds,
			hits + misses,
			replaced,
			hits,
			misses
		}));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void DensitiesClean()
	{
		HashSet<Chunk> changedChunks = new HashSet<Chunk>();
		bool bUseSelection = BlockToolSelection.Instance.SelectionActive;
		Vector3i selStart = BlockToolSelection.Instance.SelectionMin;
		Vector3i selEnd = selStart + BlockToolSelection.Instance.SelectionSize - Vector3i.one;
		List<Chunk> chunkArrayCopySync = GameManager.Instance.World.ChunkCache.GetChunkArrayCopySync();
		for (int i = 0; i < chunkArrayCopySync.Count; i++)
		{
			Chunk curChunk = chunkArrayCopySync[i];
			curChunk.LoopOverAllBlocks(delegate(int _x, int _y, int _z, BlockValue _bv)
			{
				if (bUseSelection)
				{
					Vector3i vector3i = curChunk.ToWorldPos(new Vector3i(_x, _y, _z));
					if (vector3i.x < selStart.x || vector3i.x > selEnd.x || vector3i.y < selStart.y || vector3i.y > selEnd.y || vector3i.z < selStart.z || vector3i.z > selEnd.z)
					{
						return;
					}
				}
				Block block = _bv.Block;
				sbyte density = curChunk.GetDensity(_x, _y, _z);
				sbyte b = (block.shape.IsTerrain() ? MarchingCubes.DensityTerrain : MarchingCubes.DensityAir);
				if (b != density)
				{
					curChunk.SetDensity(_x, _y, _z, b);
					changedChunks.Add(curChunk);
				}
			}, false, true);
		}
		foreach (Chunk chunk in changedChunks)
		{
			chunk.NeedsRegeneration = true;
		}
		if (changedChunks.Count > 0)
		{
			PrefabEditModeManager.Instance.NeedsSaving = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string[] goNamesUnpaintable = new string[] { "_BlockEntities", "models", "modelsCollider", "cutout", "cutoutCollider" };

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string[] goNamesPaintable = new string[] { "opaque", "opaqueCollider" };

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string[] goNamesTerrain = new string[] { "terrain", "terrainCollider" };

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool wasShowingImposterBeforeUpdate;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float screenshotBorderPercentage = 0.15f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const bool screenshot4To3 = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool drawingScreenshotGuide;
}
