using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionResetRegions : BaseAction
	{
		public override BaseAction.ActionCompleteStates OnPerformAction()
		{
			GameManager.Instance.StartCoroutine(this.HandleReset());
			return BaseAction.ActionCompleteStates.Complete;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public IEnumerator HandleReset()
		{
			yield return new WaitForSeconds(1f);
			World world = GameManager.Instance.World;
			ChunkCluster cc = world.ChunkCache;
			HashSetLong hashSetLong = new HashSetLong();
			HashSetLong regeneratedChunks = new HashSetLong();
			ChunkProviderGenerateWorld chunkProvider = world.ChunkCache.ChunkProvider as ChunkProviderGenerateWorld;
			if (this.ResetType == ActionResetRegions.ResetTypes.Full)
			{
				foreach (long num in chunkProvider.ResetAllChunks(ChunkProtectionLevel.None, EnumResetUnprotectedChunksGroupingMode.GroupedPOIs))
				{
					if (cc.ContainsChunkSync(num))
					{
						hashSetLong.Add(num);
					}
				}
				if (hashSetLong.Count > 0)
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Regenerating {0} synced chunks.", hashSetLong.Count));
					foreach (long chunkKey in hashSetLong)
					{
						if (!chunkProvider.GenerateSingleChunk(cc, chunkKey, true))
						{
							yield return new WaitForEndOfFrame();
							SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Region reset failed regenerating chunk at world XZ position: {0}, {1}", WorldChunkCache.extractX(chunkKey) << 4, WorldChunkCache.extractZ(chunkKey) << 4));
						}
						else
						{
							regeneratedChunks.Add(chunkKey);
						}
					}
					HashSetLong.Enumerator enumerator2 = default(HashSetLong.Enumerator);
					world.m_ChunkManager.ResendChunksToClients(regeneratedChunks);
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Regeneration complete.");
				}
			}
			yield break;
			yield break;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseEnum<ActionResetRegions.ResetTypes>(ActionResetRegions.PropResetType, ref this.ResetType);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionResetRegions
			{
				ResetType = this.ResetType
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public ActionResetRegions.ResetTypes ResetType;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropResetType = "reset_type";

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool isComplete;

		[PublicizedFrom(EAccessModifier.Protected)]
		public enum ResetTypes
		{
			None,
			Full
		}
	}
}
