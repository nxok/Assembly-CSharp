using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdChunkReset : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "chunkreset", "cr" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "resets the specified chunks";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Usage:\n  1. chunkreset <x1> <z1> <x2> <z2>\n  2. chunkreset [f]\n1. Rebuilds the chunks that contain the given coordinate range.\n2. Can only be executed by a player in the ingame console! Behaviour depends on whether the\n   player is currently within the bounds of a POI:\n   Within a POI: The POI is reset.\n   Not within a POI: The chunk the player is in and the eight chunks around that one are\n     rebuilt. Not deco! Does not reload POI data!\n   d - regen deco\n   f - fully regenerates chunks (may cause double entities!)\n   u, utimed, ue - Unload chunks or entities\n   nq - Enqueue a 3x3 group of chunks centred around the player to be reset when unsynced, unless they are otherwise protected from the chunk reset system.";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		GameManager.Instance.StartCoroutine(this.execute(_params, _senderInfo));
	}

	public IEnumerator execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		World world = GameManager.Instance.World;
		if (_params.Count >= 2)
		{
			int num;
			if (!int.TryParse(_params[0], out num))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("x1 is not a valid integer");
				yield break;
			}
			int num2;
			if (!int.TryParse(_params[1], out num2))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("z1 is not a valid integer");
				yield break;
			}
			int num3 = num;
			int num4 = num2;
			if (_params.Count >= 3 && !int.TryParse(_params[2], out num3))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("x2 is not a valid integer");
				yield break;
			}
			if (_params.Count >= 4 && !int.TryParse(_params[3], out num4))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("z2 is not a valid integer");
				yield break;
			}
			Vector2i vector2i = new Vector2i((num <= num3) ? num : num3, (num2 <= num4) ? num2 : num4);
			Vector2i vector2i2 = new Vector2i((num <= num3) ? num3 : num, (num2 <= num4) ? num4 : num2);
			if (vector2i2.x - vector2i.x > 16384 || vector2i2.y - vector2i.y > 16384)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("area too big");
				yield break;
			}
			vector2i = World.toChunkXZ(vector2i);
			vector2i2 = World.toChunkXZ(vector2i2);
			HashSetLong hashSetLong = new HashSetLong();
			for (int i = vector2i.x; i <= vector2i2.x; i++)
			{
				for (int j = vector2i.y; j <= vector2i2.y; j++)
				{
					hashSetLong.Add(WorldChunkCache.MakeChunkKey(i, j));
				}
			}
			ChunkCluster chunkCache = world.ChunkCache;
			ChunkProviderGenerateWorld chunkProviderGenerateWorld = world.ChunkCache.ChunkProvider as ChunkProviderGenerateWorld;
			if (chunkProviderGenerateWorld != null)
			{
				LockManager.Instance.ForceUnlockByChunk(hashSetLong);
				chunkProviderGenerateWorld.RemoveChunks(hashSetLong);
				foreach (long num5 in hashSetLong)
				{
					if (!chunkProviderGenerateWorld.GenerateSingleChunk(chunkCache, num5, true))
					{
						SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Failed regenerating chunk at position {0}/{1}", WorldChunkCache.extractX(num5) << 4, WorldChunkCache.extractZ(num5) << 4));
					}
				}
				GameManager.Instance.World.m_ChunkManager.ResendChunksToClients(hashSetLong);
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Reset chunks covering area {0}/{1} to {2}/{3} (chunk coordinates {4} to {5}).", new object[] { num, num2, num3, num4, vector2i, vector2i2 }));
				if (!(DynamicMeshManager.Instance != null))
				{
					goto IL_0968;
				}
				using (HashSetLong.Enumerator enumerator = hashSetLong.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						long num6 = enumerator.Current;
						DynamicMeshManager.Instance.AddChunk(num6, true, true, null);
					}
					yield break;
				}
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Can not reset chunks on this game");
		}
		else
		{
			if (_params.Count > 1 || (_senderInfo.RemoteClientInfo == null && (!_senderInfo.IsLocalGame || GameManager.IsDedicatedServer)))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Invalid arguments, please see command help.");
				yield break;
			}
			Vector3 vector;
			if (_senderInfo.RemoteClientInfo != null)
			{
				vector = world.Players.dict[_senderInfo.RemoteClientInfo.entityId].position;
			}
			else
			{
				vector = world.GetLocalPlayers()[0].position;
			}
			int num = World.toChunkXZ((int)vector.x) - 1;
			int num2 = World.toChunkXZ((int)vector.z) - 1;
			int num3 = num + 2;
			int num4 = num2 + 2;
			HashSetLong chunks = new HashSetLong();
			for (int k = num; k <= num3; k++)
			{
				for (int l = num2; l <= num4; l++)
				{
					chunks.Add(WorldChunkCache.MakeChunkKey(k, l));
				}
			}
			if (_params.Count == 1)
			{
				ChunkCluster chunkCache2 = world.ChunkCache;
				ChunkProviderGenerateWorld chunkProviderGenerateWorld2 = chunkCache2.ChunkProvider as ChunkProviderGenerateWorld;
				if (chunkProviderGenerateWorld2 == null)
				{
					yield break;
				}
				if (_params[0] == "nq")
				{
					foreach (long num7 in chunks)
					{
						chunkProviderGenerateWorld2.RequestChunkReset(num7);
					}
					yield break;
				}
				if (_params[0] == "d")
				{
					LockManager.Instance.ForceUnlockByChunk(chunks);
					foreach (long num8 in chunks)
					{
						Chunk chunkSync = chunkCache2.GetChunkSync(num8);
						if (chunkSync != null)
						{
							chunkSync.NeedsLightDecoration = true;
							chunkSync.NeedsLightCalculation = true;
						}
					}
					GameManager.Instance.World.m_ChunkManager.ResendChunksToClients(chunks);
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Generate deco around player");
				}
				else if (_params[0] == "f")
				{
					LockManager.Instance.ForceUnlockByChunk(chunks);
					foreach (long num9 in chunks)
					{
						if (!chunkProviderGenerateWorld2.GenerateSingleChunk(chunkCache2, num9, true))
						{
							SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Failed regenerating chunk at position {0}/{1}", WorldChunkCache.extractX(num9) << 4, WorldChunkCache.extractZ(num9) << 4));
						}
					}
					GameManager.Instance.World.m_ChunkManager.ResendChunksToClients(chunks);
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Generate chunks around player");
				}
				else
				{
					if (_params[0] == "r")
					{
						using (HashSetLong.Enumerator enumerator = chunks.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								long num10 = enumerator.Current;
								Chunk chunkSync2 = chunkCache2.GetChunkSync(num10);
								if (chunkSync2 != null)
								{
									chunkSync2.NeedsRegeneration = true;
								}
							}
							goto IL_0900;
						}
					}
					if (_params[0] == "u")
					{
						foreach (long num11 in chunks)
						{
							world.m_ChunkManager.RemoveChunk(num11);
						}
						SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Unload around player");
					}
					else if (_params[0] == "utimed")
					{
						if (this.unloadTimed != null)
						{
							GameManager.Instance.StopCoroutine(this.unloadTimed);
							this.unloadTimed = null;
						}
						else
						{
							this.unloadTimed = GameManager.Instance.StartCoroutine(this.UnloadTimed());
						}
						SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Unload timed at player {0}", new object[] { this.unloadTimed != null });
					}
					else if (_params[0] == "ue")
					{
						foreach (long num12 in chunks)
						{
							Chunk chunkSync3 = chunkCache2.GetChunkSync(num12);
							if (chunkSync3 != null)
							{
								for (int m = 0; m < chunkSync3.entityLists.Length; m++)
								{
									List<Entity> list = chunkSync3.entityLists[m];
									for (int n = list.Count - 1; n >= 0; n--)
									{
										Entity entity = list[n];
										if (!entity.bWillRespawn && (!(entity.AttachedMainEntity != null) || !entity.AttachedMainEntity.bWillRespawn))
										{
											world.unloadEntity(entity, EnumRemoveEntityReason.Unloaded);
											list.RemoveAt(n);
										}
									}
								}
							}
						}
						SingletonMonoBehaviour<SdtdConsole>.Instance.Output("UnloadEntities around player");
					}
				}
			}
			else
			{
				DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.GetDynamicPrefabDecorator();
				List<PrefabInstance> prefabsFromWorldPosInside;
				if (dynamicPrefabDecorator != null && (prefabsFromWorldPosInside = dynamicPrefabDecorator.GetPrefabsFromWorldPosInside(vector, FastTags<TagGroup.Global>.none)) != null)
				{
					yield return world.ResetPOIS(prefabsFromWorldPosInside, QuestEventManager.manualResetTag, -1, null, null);
				}
				else
				{
					LockManager.Instance.ForceUnlockByChunk(chunks);
					world.RebuildTerrain(chunks, Vector3i.zero, Vector3i.zero, false, true, true, false);
					GameManager.Instance.World.m_ChunkManager.ResendChunksToClients(chunks);
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Reset chunks around player");
				}
			}
			IL_0900:
			if (DynamicMeshManager.Instance != null)
			{
				foreach (long num13 in chunks)
				{
					DynamicMeshManager.Instance.AddChunk(num13, true, true, null);
				}
			}
			chunks = null;
		}
		IL_0968:
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator UnloadTimed()
	{
		int num4;
		for (int i = 0; i < 99999; i = num4)
		{
			World world = GameManager.Instance.World;
			if (world == null)
			{
				break;
			}
			EntityPlayerLocal primaryPlayer = world.GetPrimaryPlayer();
			if (!primaryPlayer)
			{
				break;
			}
			Vector3i blockPosition = primaryPlayer.GetBlockPosition();
			for (int j = -16; j <= 16; j += 16)
			{
				for (int k = -16; k <= 16; k += 16)
				{
					int num = World.toChunkXZ(blockPosition.x + k);
					int num2 = World.toChunkXZ(blockPosition.z + j);
					long num3 = WorldChunkCache.MakeChunkKey(num, num2);
					world.m_ChunkManager.RemoveChunk(num3);
				}
			}
			yield return new WaitForSeconds(1.5f);
			num4 = i + 1;
		}
		this.unloadTimed = null;
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Coroutine unloadTimed;
}
