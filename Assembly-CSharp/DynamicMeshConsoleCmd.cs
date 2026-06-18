using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime;
using System.Runtime.InteropServices;
using Audio;
using Platform;
using UniLinq;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Scripting;

[Preserve]
public class DynamicMeshConsoleCmd : ConsoleCmdAbstract
{
	public override bool AllowedInMainMenu
	{
		get
		{
			return true;
		}
	}

	public override bool IsExecuteOnClient
	{
		get
		{
			return true;
		}
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params == null || _params.Count == 0)
		{
			DynamicMeshManager.Instance.AddChunk(new Vector3i(GameManager.Instance.World.GetPrimaryPlayer().GetPosition()), true);
			return;
		}
		string text = _params[0].ToLower();
		if (text == "block")
		{
			Vector3i vector3i = new Vector3i(GameManager.Instance.World.GetPrimaryPlayer().GetPosition());
			vector3i.y += 3;
			int num = int.Parse(_params[1]);
			int num2 = int.Parse(_params[2]);
			int num3 = int.Parse(_params[3]);
			List<BlockChangeInfo> list = new List<BlockChangeInfo>();
			BlockValue blockValue = Block.GetBlockValue("concreteBlock", true);
			for (int i = vector3i.y; i < vector3i.y + num2; i++)
			{
				for (int j = vector3i.x - num; j < vector3i.x + num; j++)
				{
					for (int k = vector3i.z - num3; k < vector3i.z + num3; k++)
					{
						BlockChangeInfo blockChangeInfo = new BlockChangeInfo(new Vector3i(j, i, k), blockValue, 0);
						list.Add(blockChangeInfo);
					}
				}
			}
			GameManager.Instance.SetBlocksRPC(list, null);
		}
		if (text == "pause")
		{
			DynamicMeshThread.Paused = !DynamicMeshThread.Paused;
			if (DynamicMeshManager.DoLog)
			{
				DynamicMeshManager.LogMsg("Dynamic Paused: " + DynamicMeshThread.Paused.ToString());
				return;
			}
		}
		else if (text == "fog")
		{
			float minValue = float.MinValue;
			float minValue2 = float.MinValue;
			if (_params.Count >= 1)
			{
				float num4 = StringParsers.ParseFloat(_params[1], 0, -1, NumberStyles.Any);
				SkyManager.SetFogDebug(num4, minValue, minValue2);
				Log.Out("Fog " + num4.ToString());
				return;
			}
		}
		else
		{
			if (text == "orphans")
			{
				Log.Out("Running orphan checks...");
				DynamicMeshManager.Instance.ForceOrphanChecks();
				return;
			}
			if (text == "dolog")
			{
				DynamicMeshManager.DoLog = !DynamicMeshManager.DoLog;
				Log.Out("Dolog: " + DynamicMeshManager.DoLog.ToString());
				return;
			}
			if (text == "qef")
			{
				DynamicMeshVoxel.QefToFile("C:\\Users\\D\\Documents\\Qubicle 3.0\\Tars.qef", "C:\\Users\\D\\Documents\\Qubicle 3.0\\", "tars");
				return;
			}
			if (text == "tars")
			{
				DynamicMeshManager.ImportVox("tars", GameManager.Instance.World.GetPrimaryPlayer().position, 502);
				return;
			}
			if (text == "vox")
			{
				string param = this.GetParam(_params, 1);
				int num5;
				int.TryParse(this.GetParam(_params, 2) ?? "502", out num5);
				DynamicMeshManager.ImportVox(param, GameManager.Instance.World.GetPrimaryPlayer().position, num5);
				return;
			}
			if (text == "lognet")
			{
				DynamicMeshManager.DoLogNet = !DynamicMeshManager.DoLogNet;
				Log.Out("DoLogNet: " + DynamicMeshManager.DoLogNet.ToString());
				return;
			}
			if (text == "settings")
			{
				DynamicMeshSettings.LogSettings();
				return;
			}
			if (text == "imp")
			{
				DynamicMeshSettings.UseImposterValues = !DynamicMeshSettings.UseImposterValues;
				Log.Out("Use Imposter Values: " + DynamicMeshSettings.UseImposterValues.ToString());
				return;
			}
			if (text == "useimpostervalues")
			{
				DynamicMeshSettings.UseImposterValues = !DynamicMeshSettings.UseImposterValues;
				DynamicMeshBlockSwap.Init();
				Log.Out("Use Imposter Values: " + DynamicMeshSettings.UseImposterValues.ToString());
				return;
			}
			if (text == "playerareaonly" || text == "pao")
			{
				DynamicMeshSettings.OnlyPlayerAreas = !DynamicMeshSettings.OnlyPlayerAreas;
				DynamicMeshSettings.Validate();
				Log.Out("Player Area Only: " + DynamicMeshSettings.OnlyPlayerAreas.ToString());
				return;
			}
			if (text == "playerareabuffer" || text == "pab")
			{
				DynamicMeshSettings.PlayerAreaChunkBuffer = Math.Max(1, this.GetParamAsInt(_params, 1));
				Log.Out("Player Area Buffer: " + DynamicMeshSettings.PlayerAreaChunkBuffer.ToString());
				return;
			}
			if (text == "newworldregen")
			{
				DynamicMeshSettings.NewWorldFullRegen = !DynamicMeshSettings.NewWorldFullRegen;
				Log.Out("World full regen: " + DynamicMeshSettings.NewWorldFullRegen.ToString());
				return;
			}
			if (!(text == "loadregion"))
			{
				if (text == "regenall")
				{
					using (IEnumerator<KeyValuePair<long, DynamicMeshItem>> enumerator = DynamicMeshManager.Instance.ItemsDictionary.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							KeyValuePair<long, DynamicMeshItem> keyValuePair = enumerator.Current;
							if (keyValuePair.Value.FileExists())
							{
								DynamicMeshThread.RequestSecondaryQueue(keyValuePair.Value);
							}
						}
						return;
					}
				}
				if (text == "regenregion")
				{
					int paramAsInt = this.GetParamAsInt(_params, 1);
					int paramAsInt2 = this.GetParamAsInt(_params, 2);
					DynamicMeshThread.AddRegionUpdateData(paramAsInt, paramAsInt2, true);
					return;
				}
				if (text == "setmaxregion")
				{
					DynamicMeshSettings.MaxViewDistance = this.GetParamAsInt(_params, 1);
					DynamicMeshManager.LogMsg("New limit: " + DynamicMeshSettings.MaxViewDistance.ToString());
					return;
				}
				if (text == "setmaxitem")
				{
					DynamicMeshRegion.ItemLoadIndex = this.GetParamAsInt(_params, 1);
					DynamicMeshRegion.ItemUnloadIndex = DynamicMeshRegion.ItemLoadIndex + 1;
					DynamicMeshManager.LogMsg("New limit: " + DynamicMeshRegion.ItemLoadIndex.ToString());
					return;
				}
				if (text == "clearlod")
				{
					GameManager.Instance.prefabLODManager.Cleanup();
					return;
				}
				if (text == "lod")
				{
					DynamicMeshManager.DisableLOD = !DynamicMeshManager.DisableLOD;
					foreach (PrefabLODManager.PrefabGameObject prefabGameObject in GameManager.Instance.prefabLODManager.displayedPrefabs.Values)
					{
						if (prefabGameObject.go != null)
						{
							prefabGameObject.go.SetActive(!DynamicMeshManager.DisableLOD);
						}
					}
					if (DynamicMeshManager.DoLog)
					{
						DynamicMeshManager.LogMsg("Disable LOD: " + DynamicMeshManager.DisableLOD.ToString());
						return;
					}
				}
				else
				{
					if (text == "index")
					{
						int paramAsInt3 = this.GetParamAsInt(_params, 1);
						int paramAsInt4 = this.GetParamAsInt(_params, 2);
						bool flag = DynamicMeshRegion.IsInBuffer(paramAsInt3, paramAsInt4);
						Log.Out(string.Concat(new string[]
						{
							"Is ",
							paramAsInt3.ToString(),
							",",
							paramAsInt4.ToString(),
							" in buffer: ",
							flag.ToString()
						}));
						return;
					}
					if (text == "meshlock")
					{
						DynamicMeshThread.LockMeshesAfterGenerating = !DynamicMeshThread.LockMeshesAfterGenerating;
						Log.Out("Lock Mesh: " + DynamicMeshThread.LockMeshesAfterGenerating.ToString());
						return;
					}
					if (text == "regioncount")
					{
						int paramAsInt5 = this.GetParamAsInt(_params, 1);
						DynamicMeshSettings.MaxRegionMeshData = (DynamicMeshManager.Instance.AvailableRegionLoadRequests = paramAsInt5);
						Log.Out("MaxRegionMeshData: " + paramAsInt5.ToString());
						return;
					}
					if (text == "update")
					{
						int paramAsInt6 = this.GetParamAsInt(_params, 1);
						int paramAsInt7 = this.GetParamAsInt(_params, 2);
						DynamicMeshManager.ChunkChanged(new Vector3i(paramAsInt6, 0, paramAsInt7), DynamicMeshManager.player.entityId, 1);
						Log.Out("added " + paramAsInt6.ToString() + "," + paramAsInt7.ToString());
						return;
					}
					if (text == "chunkname")
					{
						EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
						DynamicMeshItem itemOrNull = DynamicMeshManager.Instance.GetItemOrNull(new Vector3i(primaryPlayer.position));
						DynamicMeshRegion dynamicMeshRegion = ((itemOrNull != null) ? itemOrNull.GetRegion() : null);
						string text2;
						if (itemOrNull == null)
						{
							text2 = null;
						}
						else
						{
							GameObject chunkObject = itemOrNull.ChunkObject;
							text2 = ((chunkObject != null) ? chunkObject.name : null);
						}
						string text3 = text2 ?? "missing";
						string text4;
						if (dynamicMeshRegion == null)
						{
							text4 = null;
						}
						else
						{
							GameObject regionObject = dynamicMeshRegion.RegionObject;
							text4 = ((regionObject != null) ? regionObject.name : null);
						}
						string text5 = text4 ?? "missing";
						Log.Out("Chunk " + text3 + "  Region:  " + text5);
						return;
					}
					if (text == "traders")
					{
						GameManager.Instance.World.GetPrimaryPlayer();
						List<PrefabInstance> list2 = new List<PrefabInstance>();
						GameManager.Instance.World.ChunkCache.ChunkProvider.GetDynamicPrefabDecorator().GetPOIPrefabs(list2);
						using (IEnumerator<PrefabInstance> enumerator3 = list2.Where((PrefabInstance d) => d.prefab.HasAnyQuestTag(QuestEventManager.traderTag)).GetEnumerator())
						{
							while (enumerator3.MoveNext())
							{
								PrefabInstance prefabInstance = enumerator3.Current;
								string text6 = "Trader at ";
								Vector3i vector3i2 = prefabInstance.boundingBoxPosition;
								Log.Out(text6 + vector3i2.ToString());
								Waypoint waypoint = new Waypoint();
								waypoint.pos = World.worldToBlockPos(prefabInstance.boundingBoxPosition.ToVector3() + prefabInstance.boundingBoxSize.ToVector3() / 2f);
								waypoint.icon = "ui_game_symbol_safe";
								waypoint.name.Update("Trader", PlatformManager.MultiPlatform.User.PlatformUserId);
								waypoint.ownerId = null;
								waypoint.lastKnownPositionEntityId = -1;
								EntityPlayer primaryPlayer2 = GameManager.Instance.World.GetPrimaryPlayer();
								if (!primaryPlayer2.Waypoints.ContainsWaypoint(waypoint))
								{
									primaryPlayer2.Waypoints.Collection.Add(waypoint);
									if (waypoint.CanBeViewedBy(PlatformManager.InternalLocalUserIdentifier))
									{
										MapObjectWaypoint mapObjectWaypoint = new MapObjectWaypoint(waypoint);
										GameManager.Instance.World.ObjectOnMapAdd(mapObjectWaypoint);
									}
								}
							}
							return;
						}
					}
					if (text == "cloth")
					{
						using (List<XmlData>.Enumerator enumerator4 = Manager.audioData.Values.Where((XmlData d) => d.audioClipMap.Any((ClipSourceMap e) => e.clipName.ContainsCaseInsensitive("hitmetal"))).ToList<XmlData>().GetEnumerator())
						{
							while (enumerator4.MoveNext())
							{
								XmlData xmlData = enumerator4.Current;
								for (int l = 0; l < xmlData.audioClipMap.Count; l++)
								{
									ClipSourceMap clipSourceMap = xmlData.audioClipMap[l];
									Log.Out("Updating " + clipSourceMap.clipName);
									clipSourceMap.clipName = clipSourceMap.clipName.Replace("hitmetal", "hitcloth");
									xmlData.audioClipMap[l] = clipSourceMap;
								}
							}
							return;
						}
					}
					if (text == "ms")
					{
						int paramAsInt8 = this.GetParamAsInt(_params, 1);
						if (paramAsInt8 > 0)
						{
							DynamicMeshFile.ReadMeshMax = paramAsInt8;
						}
						if (DynamicMeshManager.DoLog)
						{
							DynamicMeshManager.LogMsg("New max MS: " + DynamicMeshFile.ReadMeshMax.ToString());
							return;
						}
					}
					else
					{
						if (text == "listblocks")
						{
							foreach (Block block in Block.list)
							{
								if (block != null && DynamicMeshManager.DoLog)
								{
									DynamicMeshManager.LogMsg("Block " + block.GetBlockName() + "  Id: " + block.blockID.ToString());
								}
							}
							return;
						}
						if (text == "nopro")
						{
							DynamicMeshThread.NoProcessing = !DynamicMeshThread.NoProcessing;
							if (DynamicMeshManager.DoLog)
							{
								DynamicMeshManager.LogMsg("Dynamic No Processing: " + DynamicMeshThread.NoProcessing.ToString());
								return;
							}
						}
						else
						{
							if (text == "meshthreads" || text == "threads")
							{
								int paramAsInt9 = this.GetParamAsInt(_params, 1);
								DynamicMeshThread.BuilderManager.SetNewLimit(paramAsInt9);
								return;
							}
							if (text == "activesyncs")
							{
								DynamicMeshServer.MaxActiveSyncs = this.GetParamAsInt(_params, 1);
								Log.Out("Max active syncs: " + DynamicMeshServer.MaxActiveSyncs.ToString());
								return;
							}
							if (text == "guiy")
							{
								DynamicMeshManager.GuiY = this.GetParamAsInt(_params, 1);
								Log.Out("GUI y: " + DynamicMeshManager.GuiY.ToString());
								return;
							}
							if (text == "imax")
							{
								int paramAsInt10 = this.GetParamAsInt(_params, 1);
								DynamicMeshThread.ChunkDataQueue.MaxAllowedItems = paramAsInt10;
								Log.Out("Max Item: " + DynamicMeshThread.ChunkDataQueue.MaxAllowedItems.ToString());
								Log.Out("Max Region: " + DynamicMeshThread.ChunkDataQueue.MaxAllowedItems.ToString());
								return;
							}
							if (text == "loadqueues")
							{
								DynamicMeshThread.SetNextChunksFromQueues();
								return;
							}
							if (text == "farreach" || text == "fr")
							{
								if (Constants.cDigAndBuildDistance != 50f)
								{
									Constants.cDigAndBuildDistance = 50f;
									Constants.cBuildIntervall = 0.2f;
									Constants.cCollectItemDistance = 50f;
								}
								else
								{
									Constants.cDigAndBuildDistance = 5f;
									Constants.cBuildIntervall = 0.5f;
									Constants.cCollectItemDistance = 3.5f;
								}
								Log.Out("Reach distance: " + Constants.cDigAndBuildDistance.ToString());
								return;
							}
							if (text == "reorder")
							{
								try
								{
									DynamicMeshManager.Instance.ReorderGameObjects();
									return;
								}
								catch (Exception ex)
								{
									Log.Error("Reorder error: " + ex.Message);
									return;
								}
							}
							if (text == "scope")
							{
								DynamicMeshManager.DisableScopeTexture = !DynamicMeshManager.DisableScopeTexture;
								return;
							}
							if (text == "kit")
							{
								DynamicMeshConsoleCmd.AddKit();
								return;
							}
							if (text == "info")
							{
								Vector3 position = GameManager.Instance.World.GetPrimaryPlayer().position;
								string x2 = this.GetParam(_params, 1) ?? DynamicMeshUnity.GetChunkPositionFromWorldPosition(position.x).ToString();
								string z2 = this.GetParam(_params, 2) ?? DynamicMeshUnity.GetChunkPositionFromWorldPosition(position.z).ToString();
								DynamicMeshRegion dynamicMeshRegion2 = DynamicMeshRegion.Regions.Values.FirstOrDefault((DynamicMeshRegion d) => d.WorldPosition.x.ToString() == x2 && d.WorldPosition.z.ToString() == z2);
								if (dynamicMeshRegion2 == null)
								{
									return;
								}
								DynamicMeshManager.LogMsg(dynamicMeshRegion2.ToDebugLocation() + " UnloadedItems");
								foreach (DynamicMeshItem dynamicMeshItem in from d in dynamicMeshRegion2.UnloadedItems
									orderby d.WorldPosition.x, d.WorldPosition.z
									select d)
								{
									DynamicMeshManager.LogMsg("Item " + dynamicMeshItem.ToDebugLocation() + "  state: " + dynamicMeshItem.State.ToString());
								}
								DynamicMeshManager.LogMsg(dynamicMeshRegion2.ToDebugLocation() + " LoadedItems");
								using (IEnumerator<DynamicMeshItem> enumerator5 = (from d in dynamicMeshRegion2.LoadedItems
									orderby d.WorldPosition.x, d.WorldPosition.z
									select d).GetEnumerator())
								{
									while (enumerator5.MoveNext())
									{
										DynamicMeshItem dynamicMeshItem2 = enumerator5.Current;
										DynamicMeshManager.LogMsg(string.Concat(new string[]
										{
											"Item ",
											dynamicMeshItem2.ToDebugLocation(),
											" state: ",
											dynamicMeshItem2.State.ToString(),
											"  chunk: ",
											(dynamicMeshItem2.ChunkObject == null) ? "null" : dynamicMeshItem2.ChunkObject.activeSelf.ToString()
										}));
										if (dynamicMeshItem2.ChunkObject != null)
										{
											dynamicMeshItem2.ChunkObject.SetActive(true);
										}
									}
									return;
								}
							}
							if (text == "tnt")
							{
								if (_params.Count < 1)
								{
									DynamicMeshManager.LogMsg("Specify a radius");
									return;
								}
								int num6 = int.Parse(this.GetParam(_params, 1));
								BlockValue blockValue2 = Block.GetBlockValue("cntBarrelOilSingle00", false);
								Vector3i vector3i3 = new Vector3i(GameManager.Instance.World.GetPrimaryPlayer().GetPosition());
								World world = GameManager.Instance.World;
								for (int n = vector3i3.x - num6; n < vector3i3.x + num6; n++)
								{
									for (int num7 = vector3i3.z - num6; num7 < vector3i3.z + num6; num7++)
									{
										if (n % 2 != 0 && num7 % 2 != 0)
										{
											Vector3i vector3i4 = new Vector3i(n, vector3i3.y, num7);
											if (world.GetBlock(vector3i4).isair)
											{
												world.SetBlockRPC(vector3i4, blockValue2);
											}
										}
									}
								}
								return;
							}
							else if (text == "air")
							{
								if (_params.Count < 1)
								{
									DynamicMeshManager.LogMsg("Specify a radius");
									return;
								}
								int num8 = int.Parse(this.GetParam(_params, 1));
								BlockValue air = BlockValue.Air;
								Vector3i vector3i5 = new Vector3i(GameManager.Instance.World.GetPrimaryPlayer().GetPosition());
								World world2 = GameManager.Instance.World;
								for (int num9 = vector3i5.x - num8; num9 < vector3i5.x + num8; num9++)
								{
									for (int num10 = vector3i5.z - num8; num10 < vector3i5.z + num8; num10++)
									{
										Vector3i vector3i6 = new Vector3i(num9, vector3i5.y, num10);
										if (world2.GetBlock(vector3i6).type != 0)
										{
											world2.SetBlockRPC(vector3i6, air);
										}
									}
								}
								return;
							}
							else
							{
								if (text == "checkprefabs" || text == "cp" || text == "forcegen")
								{
									if (_params.Count > 1 && _params[1] == "all")
									{
										DynamicMeshConsoleCmd.DebugAll();
									}
									DynamicMeshManager.Instance.CheckPrefabs("Console", true);
									return;
								}
								if (text == "sc")
								{
									using (IEnumerator<DynamicMeshItem> enumerator5 = DynamicMeshManager.Instance.ItemsDictionary.Values.GetEnumerator())
									{
										while (enumerator5.MoveNext())
										{
											DynamicMeshItem dynamicMeshItem3 = enumerator5.Current;
											if (dynamicMeshItem3.ChunkObject != null)
											{
												dynamicMeshItem3.SetVisible(true, "show chunk cmd");
											}
											else if (DynamicMeshManager.DoLog)
											{
												string text7 = "Item chunk null ";
												Vector3i vector3i2 = dynamicMeshItem3.WorldPosition;
												DynamicMeshManager.LogMsg(text7 + vector3i2.ToString());
											}
										}
										return;
									}
								}
								if (text == "showhide")
								{
									DynamicMeshManager.DebugReport = true;
									DynamicMeshManager.Instance.ShowOrHidePrefabs();
									if (DynamicMeshManager.DoLog)
									{
										DynamicMeshManager.LogMsg("ShowHide run");
									}
									DynamicMeshManager.DebugReport = false;
									return;
								}
								if (text == "copter")
								{
									int paramAsInt11 = this.GetParamAsInt(_params, 1);
									DynamicProperties dynamicProperties = Vehicle.PropertyMap["vehicleGyrocopter".ToLower()];
									dynamicProperties.Values["velocityMax"] = "9, " + paramAsInt11.ToString();
									Log.Out("Max: " + dynamicProperties.Values["velocityMax"]);
									return;
								}
								if (text == "gctoggle")
								{
									if (DynamicMeshConsoleCmd.GC_ENABLED)
									{
										GarbageCollector.GCMode = 0;
									}
									else
									{
										GarbageCollector.GCMode = 1;
										GC.Collect();
									}
									DynamicMeshConsoleCmd.GC_ENABLED = !DynamicMeshConsoleCmd.GC_ENABLED;
									Log.Out("GC: " + DynamicMeshConsoleCmd.GC_ENABLED.ToString());
									return;
								}
								if (text == "gc")
								{
									if (this.GetParam(_params, 1).EqualsCaseInsensitive("all"))
									{
										Log.Out("collecting...");
										GarbageCollector.CollectIncremental(2147483647UL);
									}
									else
									{
										GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
										GC.Collect();
									}
									Log.Out("gc done");
									return;
								}
								if (text == "toggle")
								{
									bool flag2 = !GamePrefs.GetBool(EnumGamePrefs.DynamicMeshEnabled);
									GamePrefs.Set(EnumGamePrefs.DynamicMeshEnabled, flag2);
									DynamicMeshManager.EnabledChanged(flag2);
									Log.Out("DM changed. Enabled: " + flag2.ToString());
									return;
								}
								if (text == "deco")
								{
									DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.World.ChunkCache.ChunkProvider.GetDynamicPrefabDecorator();
									Vector3 playerPos = GameManager.Instance.World.GetPrimaryPlayer().position;
									List<PrefabInstance> list4 = new List<PrefabInstance>();
									dynamicPrefabDecorator.GetAllPrefabs(list4);
									IEnumerable<PrefabInstance> enumerable = list4;
									Func<PrefabInstance, float> func;
									Func<PrefabInstance, float> <>9__8;
									if ((func = <>9__8) == null)
									{
										func = (<>9__8 = (PrefabInstance d) => Math.Abs(Vector3.Distance(playerPos, d.boundingBoxPosition.ToVector3())));
									}
									using (IEnumerator<PrefabInstance> enumerator3 = enumerable.OrderBy(func).GetEnumerator())
									{
										while (enumerator3.MoveNext())
										{
											PrefabInstance prefabInstance2 = enumerator3.Current;
											string text8 = "Deco Prefab at ";
											Vector3i vector3i2 = prefabInstance2.boundingBoxPosition;
											DynamicMeshManager.LogMsg(text8 + vector3i2.ToString() + " name: " + prefabInstance2.name);
										}
										return;
									}
								}
								if (text == "resend")
								{
									DynamicMeshServer.ResendPackages = !DynamicMeshServer.ResendPackages;
									Log.Out("Resending dymesh packages: " + DynamicMeshServer.ResendPackages.ToString());
									return;
								}
								if (text == "restart")
								{
									DynamicMeshManager.OnWorldUnload();
									DynamicMeshManager.Init();
									return;
								}
								if (text == "wipe")
								{
									string param2 = this.GetParam(_params, 1);
									if (param2 == null || param2 == "1")
									{
										if (DynamicMeshManager.DoLog)
										{
											DynamicMeshManager.LogMsg("Clear mesh pool");
										}
										DynamicMeshFile.VoxelMeshPool = new ConcurrentQueue<VoxelMesh>();
									}
									if (param2 == null || param2 == "2")
									{
										if (DynamicMeshManager.DoLog)
										{
											DynamicMeshManager.LogMsg("Clear unsed assets");
										}
										Resources.UnloadUnusedAssets();
										return;
									}
								}
								else if (text == "pool")
								{
									if (DynamicMeshManager.DoLog)
									{
										DynamicMeshManager.LogMsg("Items in pool: " + DynamicMeshFile.VoxelMeshPool.Count.ToString());
									}
									long num11 = 0L;
									foreach (VoxelMesh voxelMesh in DynamicMeshFile.VoxelMeshPool)
									{
										if (voxelMesh == null)
										{
											if (DynamicMeshManager.DoLog)
											{
												DynamicMeshManager.LogMsg("Null item in pool");
											}
										}
										else
										{
											if (voxelMesh.CollIndices.Items != null)
											{
												num11 += (long)(4 * voxelMesh.CollIndices.Items.Length);
											}
											if (voxelMesh.CollVertices.Items != null)
											{
												num11 += (long)(Marshal.SizeOf(typeof(Vector3)) * voxelMesh.CollVertices.Items.Length);
											}
											if (voxelMesh.Indices.Items != null)
											{
												num11 += (long)(4 * voxelMesh.Indices.Items.Length);
											}
											if (voxelMesh.Normals.Items != null)
											{
												num11 += (long)(Marshal.SizeOf(typeof(Vector3)) * voxelMesh.Normals.Items.Length);
											}
											if (voxelMesh.ColorVertices.Items != null)
											{
												num11 += (long)(Marshal.SizeOf(typeof(Color)) * voxelMesh.ColorVertices.Items.Length);
											}
											if (voxelMesh.Tangents.Items != null)
											{
												num11 += (long)(Marshal.SizeOf(typeof(Vector4)) * voxelMesh.Tangents.Items.Length);
											}
											if (voxelMesh.Uvs.Items != null)
											{
												num11 += (long)(Marshal.SizeOf(typeof(Vector2)) * voxelMesh.Uvs.Items.Length);
											}
											if (voxelMesh.UvsCrack.Items != null)
											{
												num11 += (long)(Marshal.SizeOf(typeof(Vector2)) * voxelMesh.UvsCrack.Items.Length);
											}
											if (voxelMesh.Vertices.Items != null)
											{
												num11 += (long)(Marshal.SizeOf(typeof(Vector3)) * voxelMesh.Vertices.Items.Length);
											}
										}
									}
									if (DynamicMeshManager.DoLog)
									{
										DynamicMeshManager.LogMsg(string.Concat(new string[]
										{
											"Total bytes: ",
											num11.ToString(),
											" = ",
											((double)num11 / 1024.0 / 1024.0).ToString(),
											"MB  or ",
											num11.ToString(),
											"  bytes. v3: ",
											Marshal.SizeOf(typeof(Vector3)).ToString()
										}));
										return;
									}
								}
								else
								{
									if (text == "font")
									{
										int paramAsInt12 = this.GetParamAsInt(_params, 1);
										Log.Out("Font size from " + DynamicMeshManager.DebugStyle.fontSize.ToString() + " to " + paramAsInt12.ToString());
										DynamicMeshManager.DebugStyle.fontSize = paramAsInt12;
										return;
									}
									if (text == "meshsize")
									{
										long num12 = 0L;
										long num13 = 0L;
										foreach (object obj in DynamicMeshManager.Parent.transform)
										{
											Transform transform = (Transform)obj;
											if (transform.gameObject.name.StartsWith("C", StringComparison.OrdinalIgnoreCase))
											{
												num13 += DynamicMeshUnity.GetMeshSize(transform.gameObject);
											}
											else
											{
												num12 += DynamicMeshUnity.GetMeshSize(transform.gameObject);
											}
										}
										Log.Out("Total chunk mesh: " + (num13 / 1024L / 1024L).ToString() + "MB");
										Log.Out("Total region mesh: " + (num12 / 1024L / 1024L).ToString() + "MB");
										return;
									}
									if (text == "maxdata")
									{
										DynamicMeshSettings.MaxDyMeshData = this.GetParamAsInt(_params, 1);
										Log.Out(string.Format("MaxItems: {0}", DynamicMeshSettings.MaxDyMeshData));
										return;
									}
									if (text == "enabled")
									{
										DynamicMeshManager.CONTENT_ENABLED = !DynamicMeshManager.CONTENT_ENABLED;
										if (DynamicMeshManager.Instance != null)
										{
											DynamicMeshManager.Instance.Awake();
										}
										Log.Out("Dynamic Mesh enabled: " + DynamicMeshManager.CONTENT_ENABLED.ToString());
										return;
									}
									if (text == "gocheck")
									{
										DynamicMeshManager.Instance.CheckGameObjects();
										return;
									}
									if (text == "debugreleases")
									{
										DynamicMeshManager.DebugReleases = !DynamicMeshManager.DebugReleases;
										Log.Out("Debug releases: " + DynamicMeshManager.DebugReleases.ToString());
										return;
									}
									if (text == "freequeues" || text == "fq")
									{
										DynamicMeshThread.ChunkDataQueue.FreeMemory();
										return;
									}
									if (text == "showchunks" || text == "sc")
									{
										using (IEnumerator<DynamicMeshItem> enumerator5 = DynamicMeshManager.Instance.ItemsDictionary.Values.GetEnumerator())
										{
											while (enumerator5.MoveNext())
											{
												DynamicMeshItem dynamicMeshItem4 = enumerator5.Current;
												if (dynamicMeshItem4.ChunkObject != null && !dynamicMeshItem4.ChunkObject.activeSelf)
												{
													if (DynamicMeshManager.DoLog)
													{
														DynamicMeshManager.LogMsg("Showing hidden chunk: " + dynamicMeshItem4.ToDebugLocation());
													}
													dynamicMeshItem4.SetVisible(true, "forced");
												}
											}
											return;
										}
									}
									if (text == "hidechunks" || text == "hc")
									{
										using (IEnumerator<DynamicMeshItem> enumerator5 = DynamicMeshManager.Instance.ItemsDictionary.Values.GetEnumerator())
										{
											while (enumerator5.MoveNext())
											{
												DynamicMeshItem dynamicMeshItem5 = enumerator5.Current;
												if (dynamicMeshItem5.ChunkObject != null && dynamicMeshItem5.ChunkObject.activeSelf)
												{
													if (DynamicMeshManager.DoLog)
													{
														DynamicMeshManager.LogMsg("Showing hidden chunk: " + dynamicMeshItem5.ToDebugLocation());
													}
													dynamicMeshItem5.SetVisible(false, "forced");
												}
											}
											return;
										}
									}
									if (text == "ur")
									{
										int paramAsInt13 = this.GetParamAsInt(_params, 1);
										int paramAsInt14 = this.GetParamAsInt(_params, 2);
										DynamicMeshRegion region = DynamicMeshManager.Instance.GetRegion(paramAsInt13, paramAsInt14);
										DynamicMeshManager.Instance.UpdateDynamicPrefabDecoratorRegion(region);
										return;
									}
									if (text == "checkregions" || text == "cr" || text == "crr")
									{
										if (DynamicMeshManager.DoLog)
										{
											DynamicMeshManager.LogMsg("Region count: " + DynamicMeshRegion.Regions.Count.ToString());
										}
										string filter = this.GetParam(_params, 1) ?? "";
										if (filter.ToLower() == "this")
										{
											Vector3i regionPositionFromWorldPosition = DynamicMeshUnity.GetRegionPositionFromWorldPosition(GameManager.Instance.World.GetPrimaryPlayer().position);
											filter = regionPositionFromWorldPosition.x.ToString() + "," + regionPositionFromWorldPosition.z.ToString();
										}
										Vector3 position2 = GameManager.Instance.World.GetPrimaryPlayer().position;
										int num14 = 0;
										int num15 = 0;
										int num16 = 0;
										int num17 = 0;
										int num18 = 0;
										int num19 = 0;
										IEnumerable<DynamicMeshRegion> values = DynamicMeshRegion.Regions.Values;
										int num20 = 0;
										int num21 = 0;
										int num22 = 0;
										int num23 = 0;
										long num24 = 0L;
										IEnumerable<DynamicMeshRegion> enumerable2 = values.OrderBy((DynamicMeshRegion d) => d.WorldPosition.ToString());
										Func<DynamicMeshRegion, bool> func2;
										Func<DynamicMeshRegion, bool> <>9__10;
										if ((func2 = <>9__10) == null)
										{
											func2 = (<>9__10 = (DynamicMeshRegion d) => filter == "" || d.ToDebugLocation().Contains(filter));
										}
										foreach (DynamicMeshRegion dynamicMeshRegion3 in enumerable2.Where(func2))
										{
											int num25 = ((dynamicMeshRegion3.RegionObject == null) ? 0 : dynamicMeshRegion3.RegionObjects);
											int num26 = ((dynamicMeshRegion3.RegionObject == null) ? 0 : dynamicMeshRegion3.Vertices);
											int num27 = ((dynamicMeshRegion3.RegionObject == null) ? 0 : dynamicMeshRegion3.Triangles);
											num14 += num25;
											num16 += num27;
											num15 += num26;
											if (dynamicMeshRegion3.RegionObject != null)
											{
												num24 += Profiler.GetRuntimeMemorySizeLong(dynamicMeshRegion3.RegionObject.GetComponent<MeshFilter>().mesh);
												if (dynamicMeshRegion3.RegionObject.GetComponent<MeshRenderer>().isVisible)
												{
													num17 += num25;
													num19 += num27;
													num18 += num26;
													num21++;
												}
												if (dynamicMeshRegion3.RegionObject.activeSelf)
												{
													num20++;
												}
											}
											string[] array = new string[26];
											array[0] = "Region ";
											array[1] = dynamicMeshRegion3.ToDebugLocation();
											array[2] = " state: ";
											array[3] = dynamicMeshRegion3.State.ToString();
											array[4] = " | Index: ?   xi: ";
											array[5] = dynamicMeshRegion3.xIndex.ToString();
											array[6] = ",";
											array[7] = dynamicMeshRegion3.zIndex.ToString();
											array[8] = " | UnloadedItems: ";
											array[9] = dynamicMeshRegion3.UnloadedItems.Count.ToString();
											array[10] = "  LoadedItems: ";
											array[11] = dynamicMeshRegion3.LoadedItems.Count.ToString();
											array[12] = " Chunks: ";
											array[13] = dynamicMeshRegion3.LoadedChunks.Count.ToString();
											array[14] = " Visible: ";
											array[15] = dynamicMeshRegion3.LoadedItems.Where((DynamicMeshItem d) => d.ChunkObject != null && d.ChunkObject.activeSelf).Count<DynamicMeshItem>().ToString();
											array[16] = " Distance: ";
											array[17] = dynamicMeshRegion3.DistanceToPlayer().ToString();
											array[18] = "  RegionVisible: ";
											array[19] = ((dynamicMeshRegion3.RegionObject == null) ? "null" : dynamicMeshRegion3.RegionObject.activeSelf.ToString());
											array[20] = "  RegionObjects: ";
											array[21] = num25.ToString();
											array[22] = "  Triangles: ";
											array[23] = num27.ToString();
											array[24] = "  Vertices: ";
											array[25] = num26.ToString();
											DynamicMeshManager.LogMsg(string.Concat(array));
										}
										foreach (DynamicMeshItem dynamicMeshItem6 in DynamicMeshManager.Instance.ItemsDictionary.Values)
										{
											int num28 = ((dynamicMeshItem6.ChunkObject == null) ? 0 : dynamicMeshItem6.Vertices);
											int num29 = ((dynamicMeshItem6.ChunkObject == null) ? 0 : dynamicMeshItem6.Triangles);
											num16 += num29;
											num15 += num28;
											if (dynamicMeshItem6.ChunkObject != null)
											{
												if (dynamicMeshItem6.ChunkObject.GetComponent<MeshRenderer>().isVisible)
												{
													num19 += num29;
													num18 += num28;
													num23++;
												}
												if (dynamicMeshItem6.ChunkObject.activeSelf)
												{
													num22++;
												}
											}
										}
										DynamicMeshManager.LogMsg(string.Concat(new string[]
										{
											"Total Objects: ",
											num14.ToString(),
											"   Total Tris: ",
											num16.ToString(),
											"    Total Verts: ",
											num15.ToString()
										}));
										DynamicMeshManager.LogMsg(string.Concat(new string[]
										{
											"Visible Objects: ",
											num17.ToString(),
											"   Total Tris: ",
											num19.ToString(),
											"    Total Verts: ",
											num18.ToString()
										}));
										DynamicMeshManager.LogMsg("Total Active Regions: " + num20.ToString() + "    Active Items: " + num22.ToString());
										DynamicMeshManager.LogMsg("Total Rendered Regions: " + num21.ToString() + "    Rendered Items: " + num23.ToString());
										return;
									}
									if (text == "findchunk")
									{
										int x = this.GetParamAsInt(_params, 1);
										int z = this.GetParamAsInt(_params, 2);
										string text9 = "chunk not found";
										Func<Vector3i, bool> <>9__12;
										foreach (KeyValuePair<long, DynamicMeshRegion> keyValuePair2 in DynamicMeshRegion.Regions)
										{
											IEnumerable<Vector3i> loadedChunks = keyValuePair2.Value.LoadedChunks;
											Func<Vector3i, bool> func3;
											if ((func3 = <>9__12) == null)
											{
												func3 = (<>9__12 = (Vector3i d) => d.x == x && d.z == z);
											}
											if (loadedChunks.Any(func3))
											{
												text9 = "Chunk found in region " + keyValuePair2.Value.ToDebugLocation();
												break;
											}
										}
										if (DynamicMeshManager.DoLog)
										{
											DynamicMeshManager.LogMsg(text9);
											return;
										}
									}
									else if (text == "meshinfo")
									{
										int paramAsInt15 = this.GetParamAsInt(_params, 2);
										int paramAsInt16 = this.GetParamAsInt(_params, 3);
										DynamicMeshItem itemFromWorldPosition = DynamicMeshManager.Instance.GetItemFromWorldPosition(paramAsInt15, paramAsInt16);
										if (itemFromWorldPosition == null)
										{
											if (DynamicMeshManager.DoLog)
											{
												DynamicMeshManager.LogMsg("Mesh not found at " + paramAsInt15.ToString() + "," + paramAsInt16.ToString());
												return;
											}
										}
										else if (DynamicMeshManager.DoLog)
										{
											DynamicMeshManager.LogMsg(string.Concat(new string[]
											{
												"Mesh ",
												paramAsInt15.ToString(),
												",",
												paramAsInt16.ToString(),
												" GO Vis: ",
												(itemFromWorldPosition.ChunkObject == null) ? "null" : string.Concat(new string[]
												{
													itemFromWorldPosition.ChunkObject.activeSelf.ToString(),
													" tris: ",
													itemFromWorldPosition.Triangles.ToString(),
													" verts: ",
													itemFromWorldPosition.Vertices.ToString()
												})
											}));
											return;
										}
									}
									else
									{
										if (text == "showpos")
										{
											DynamicMeshManager.DebugItemPositions = !DynamicMeshManager.DebugItemPositions;
											return;
										}
										if (text == "dr")
										{
											DynamicMeshManager.DebugReport = true;
											return;
										}
										if (text == "dx")
										{
											DynamicMeshManager.DebugX = int.Parse(_params[1]);
											return;
										}
										if (text == "dz")
										{
											DynamicMeshManager.DebugZ = int.Parse(_params[1]);
											return;
										}
										if (text == "dr")
										{
											DynamicMeshManager.DebugReport = true;
											return;
										}
										if (text == "itemload")
										{
											int num30 = int.Parse(_params[1]);
											int num31 = int.Parse(_params[2]);
											DynamicMeshItem itemOrNull2 = DynamicMeshManager.Instance.GetItemOrNull(new Vector3i(num30, 0, num31));
											if (itemOrNull2 != null)
											{
												DynamicMeshManager.Instance.AddItemLoadRequest(itemOrNull2, true);
												return;
											}
										}
										else
										{
											if (text == "area")
											{
												int num32 = int.Parse(_params[1]);
												int num33 = int.Parse(_params[2]);
												int num34 = int.Parse(_params[3]);
												int num35 = int.Parse(_params[4]);
												for (int num36 = num32; num36 < num34; num36 += 16)
												{
													for (int num37 = num33; num37 < num35; num37 += 16)
													{
														DynamicMeshManager.Instance.AddChunk(new Vector3i(num36, 0, num37), true);
													}
												}
												return;
											}
											if (text == "areaaround" || text == "aa")
											{
												int num38 = ((_params.Count < 2) ? 150 : int.Parse(_params[1]));
												Vector3 position3 = GameManager.Instance.World.GetPrimaryPlayer().position;
												int num39 = (int)position3.x - num38;
												while ((float)num39 < position3.x + (float)num38)
												{
													int num40 = (int)position3.z - num38;
													while ((float)num40 < position3.z + (float)num38)
													{
														DynamicMeshManager.Instance.AddChunk(new Vector3i(num39, 0, num40), true);
														num40 += 16;
													}
													num39 += 16;
												}
												return;
											}
											if (text == "refreshall")
											{
												DynamicMeshManager.Instance.RefreshAll();
												return;
											}
											if (text == "debug" || text == "dd")
											{
												DynamicMeshManager.ShowDebug = !DynamicMeshManager.ShowDebug;
												return;
											}
											if (text == "stop")
											{
												DynamicMeshThread.PrimaryQueue.Clear();
												DynamicMeshThread.SecondaryQueue.Clear();
												return;
											}
											if (text == "reload")
											{
												DynamicMeshManager.Parent.AddComponent<DynamicMeshManager>();
												return;
											}
											if (text == "clear")
											{
												DynamicMeshManager.Instance.ClearPrefabs();
												return;
											}
											if (text == "max")
											{
												DynamicMeshSettings.MaxViewDistance = int.Parse(this.GetParam(_params, 1));
												PrefabLODManager.lodPoiDistance = DynamicMeshSettings.MaxViewDistance;
												Log.Out("Max Dynamic Mesh: " + DynamicMeshSettings.MaxViewDistance.ToString());
												return;
											}
											if (text == "autosend")
											{
												DynamicMeshServer.AutoSend = !DynamicMeshServer.AutoSend;
												Log.Out("Autosend: " + DynamicMeshServer.AutoSend.ToString());
												return;
											}
											if (text == "all")
											{
												Constants.cDigAndBuildDistance = 50f;
												Constants.cBuildIntervall = 0.2f;
												Constants.cCollectItemDistance = 50f;
												DynamicMeshConsoleCmd.AddKit();
												GamePrefs.Set(EnumGamePrefs.DebugMenuEnabled, true);
												GameManager.Instance.World.GetPrimaryPlayer().GodModeSpeedModifier = 15f;
												float num41 = 0.06f;
												float minValue3 = float.MinValue;
												float minValue4 = float.MinValue;
												SkyManager.SetFogDebug(num41, minValue3, minValue4);
												DynamicMeshManager.ShowGui = true;
												return;
											}
											if (text == "down")
											{
												Vector3 position4 = GameManager.Instance.World.GetPrimaryPlayer().position;
												if (_params.Count == 3)
												{
													position4.x = (float)int.Parse(this.GetParam(_params, 1));
													position4.z = (float)int.Parse(this.GetParam(_params, 2));
												}
												Vector3i regionPositionFromWorldPosition2 = DynamicMeshUnity.GetRegionPositionFromWorldPosition(position4);
												using (IEnumerator<DynamicMeshRegion> enumerator8 = DynamicMeshRegion.Regions.Values.GetEnumerator())
												{
													while (enumerator8.MoveNext())
													{
														DynamicMeshRegion dynamicMeshRegion4 = enumerator8.Current;
														if (dynamicMeshRegion4.WorldPosition.x == regionPositionFromWorldPosition2.x && dynamicMeshRegion4.WorldPosition.z == regionPositionFromWorldPosition2.z && dynamicMeshRegion4.RegionObject != null)
														{
															dynamicMeshRegion4.RegionObject.transform.position += new Vector3(0f, -3f, 0f);
														}
													}
													return;
												}
											}
											if (text == "previewclear")
											{
												ChunkPreviewManager instance = ChunkPreviewManager.Instance;
												if (instance == null)
												{
													return;
												}
												instance.ClearAll();
												return;
											}
											else
											{
												if (text == "up")
												{
													Vector3 position5 = GameManager.Instance.World.GetPrimaryPlayer().position;
													if (_params.Count == 3)
													{
														position5.x = (float)int.Parse(this.GetParam(_params, 1));
														position5.z = (float)int.Parse(this.GetParam(_params, 2));
													}
													DynamicMeshUnity.GetRegionPositionFromWorldPosition(position5);
													foreach (DynamicMeshRegion dynamicMeshRegion5 in DynamicMeshRegion.Regions.Values)
													{
														if (dynamicMeshRegion5.RegionObject != null)
														{
															dynamicMeshRegion5.RegionObject.transform.position += new Vector3(0f, 6f, 0f);
														}
													}
													Log.Out("Regions up");
													return;
												}
												if (text == "upp")
												{
													Vector3 position6 = GameManager.Instance.World.GetPrimaryPlayer().position;
													if (_params.Count == 3)
													{
														position6.x = (float)int.Parse(this.GetParam(_params, 1));
														position6.z = (float)int.Parse(this.GetParam(_params, 2));
													}
													DynamicMeshUnity.GetRegionPositionFromWorldPosition(position6);
													foreach (KeyValuePair<long, DynamicMeshItem> keyValuePair3 in DynamicMeshManager.Instance.ItemsDictionary)
													{
														if (keyValuePair3.Value.ChunkObject != null)
														{
															keyValuePair3.Value.ChunkObject.transform.position += new Vector3(0f, 10f, 0f);
															if (DynamicMeshManager.DoLog)
															{
																DynamicMeshManager.LogMsg(keyValuePair3.Value.ToDebugLocation() + " chunk new pos: " + keyValuePair3.Value.ChunkObject.transform.position.ToString());
															}
														}
													}
													Log.Out("Items up");
													return;
												}
												if (text == "downn")
												{
													Vector3 position7 = GameManager.Instance.World.GetPrimaryPlayer().position;
													if (_params.Count == 3)
													{
														position7.x = (float)int.Parse(this.GetParam(_params, 1));
														position7.z = (float)int.Parse(this.GetParam(_params, 2));
													}
													DynamicMeshUnity.GetRegionPositionFromWorldPosition(position7);
													using (IEnumerator<KeyValuePair<long, DynamicMeshItem>> enumerator = DynamicMeshManager.Instance.ItemsDictionary.GetEnumerator())
													{
														while (enumerator.MoveNext())
														{
															KeyValuePair<long, DynamicMeshItem> keyValuePair4 = enumerator.Current;
															if (keyValuePair4.Value.ChunkObject != null)
															{
																keyValuePair4.Value.ChunkObject.transform.position -= new Vector3(0f, 10f, 0f);
																if (DynamicMeshManager.DoLog)
																{
																	DynamicMeshManager.LogMsg(keyValuePair4.Value.ToDebugLocation() + " chunk new pos: " + keyValuePair4.Value.ChunkObject.transform.position.ToString());
																}
															}
														}
														return;
													}
												}
												if (text == "gui")
												{
													DynamicMeshManager.ShowGui = !DynamicMeshManager.ShowGui;
													return;
												}
												if (text == "white")
												{
													DynamicMeshManager.DebugStyle.normal.textColor = ((DynamicMeshManager.DebugStyle.normal.textColor == Color.magenta) ? Color.white : Color.magenta);
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void AddKit()
	{
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		primaryPlayer.inventory.DecItem(new ItemValue(ItemClass.GetItem("keystoneBlock", false).type, false), 1, false, null);
		primaryPlayer.inventory.DecItem(new ItemValue(ItemClass.GetItem("drinkJarBoiledWater", false).type, false), 1, false, null);
		primaryPlayer.inventory.DecItem(new ItemValue(ItemClass.GetItem("meleeToolTorch", false).type, false), 1, false, null);
		primaryPlayer.inventory.DecItem(new ItemValue(ItemClass.GetItem("foodCanChili", false).type, false), 1, false, null);
		primaryPlayer.inventory.DecItem(new ItemValue(ItemClass.GetItem("medicalFirstAidBandage", false).type, false), 1, false, null);
		primaryPlayer.inventory.DecItem(new ItemValue(ItemClass.GetItem("noteDuke01", false).type, false), 1, false, null);
		ItemValue itemValue = new ItemValue(ItemClass.GetItem("gunExplosivesT3RocketLauncher", false).type, 6, 6, true, null, 99f);
		if (primaryPlayer.inventory.GetItemCount(itemValue, false, -1, -1, true) == 0)
		{
			if (!primaryPlayer.inventory.AddItem(new ItemStack(itemValue, 1)))
			{
				primaryPlayer.bag.AddItem(new ItemStack(new ItemValue(ItemClass.GetItem("gunExplosivesT3RocketLauncher", false).type, 6, 6, true, new string[] { "" }, 99f), 1));
			}
			if (!primaryPlayer.inventory.AddItem(new ItemStack(new ItemValue(ItemClass.GetItem("gunRifleT3SniperRifle", false).type, 6, 6, true, new string[] { "modGunScopeLarge" }, 1f), 1)))
			{
				primaryPlayer.bag.AddItem(new ItemStack(new ItemValue(ItemClass.GetItem("gunRifleT3SniperRifle", false).type, 6, 6, true, new string[] { "modGunScopeLarge" }, 1f), 1));
			}
			if (!primaryPlayer.inventory.AddItem(new ItemStack(new ItemValue(ItemClass.GetItem("gunToolDiggerAdmin", false).type, 6, 6, true, null, 1f), 1)))
			{
				primaryPlayer.bag.AddItem(new ItemStack(new ItemValue(ItemClass.GetItem("gunToolDiggerAdmin", false).type, 6, 6, true, null, 1f), 1));
			}
			if (!primaryPlayer.inventory.AddItem(new ItemStack(new ItemValue(ItemClass.GetItem("concreteBlock", false).type, false), 5000)))
			{
				primaryPlayer.bag.AddItem(new ItemStack(new ItemValue(ItemClass.GetItem("concreteBlock", false).type, false), 5000));
			}
			if (!primaryPlayer.inventory.AddItem(new ItemStack(new ItemValue(ItemClass.GetItem("terrDirt", false).type, false), 5000)))
			{
				primaryPlayer.bag.AddItem(new ItemStack(new ItemValue(ItemClass.GetItem("terrDirt", false).type, false), 5000));
			}
			if (!primaryPlayer.inventory.AddItem(new ItemStack(new ItemValue(ItemClass.GetItem("terrDestroyedStone", false).type, false), 5000)))
			{
				primaryPlayer.bag.AddItem(new ItemStack(new ItemValue(ItemClass.GetItem("terrDestroyedStone", false).type, false), 5000));
			}
			primaryPlayer.bag.AddItem(new ItemStack(new ItemValue(ItemClass.GetItem("ammoRocketHE", false).type, false), 2000));
			primaryPlayer.bag.AddItem(new ItemStack(new ItemValue(ItemClass.GetItem("ammo762mmBulletBall", false).type, false), 2000));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetParam(List<string> _params, int index)
	{
		if (_params == null)
		{
			return null;
		}
		if (index >= _params.Count)
		{
			return null;
		}
		return _params[index];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetParamAsInt(List<string> _params, int index)
	{
		int num = -9999;
		if (_params == null)
		{
			return num;
		}
		if (index >= _params.Count)
		{
			return num;
		}
		int.TryParse(_params[index], out num);
		return num;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[]
		{
			DynamicMeshConsoleCmd.info,
			"zz"
		};
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return DynamicMeshConsoleCmd.info;
	}

	public static void DebugAll()
	{
		DynamicMeshThread.BuilderManager.SetNewLimit(Application.isEditor ? 8 : 16);
		DynamicMeshThread.ChunkDataQueue.MaxAllowedItems = 11111;
		Constants.cDigAndBuildDistance = 50f;
		Constants.cBuildIntervall = 0.2f;
		Constants.cCollectItemDistance = 50f;
		DynamicMeshConsoleCmd.AddKit();
		GamePrefs.Set(EnumGamePrefs.DebugMenuEnabled, true);
		GameManager.Instance.World.GetPrimaryPlayer().GodModeSpeedModifier = 15f;
		float num = 0.06f;
		float minValue = float.MinValue;
		float minValue2 = float.MinValue;
		SkyManager.SetFogDebug(num, minValue, minValue2);
		DynamicMeshManager.ShowGui = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string info = "Dynamic mesh";

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool GC_ENABLED = true;
}
