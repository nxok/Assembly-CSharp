using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionPOISetLightState : BaseAction
	{
		public override BaseAction.ActionCompleteStates OnPerformAction()
		{
			Vector3i poiposition = base.Owner.POIPosition;
			World world = GameManager.Instance.World;
			PrefabInstance poiinstance = base.Owner.POIInstance;
			if (poiinstance == null)
			{
				return BaseAction.ActionCompleteStates.InCompleteRefund;
			}
			Vector3i size = poiinstance.prefab.size;
			int num = World.toChunkXZ(poiposition.x - 1);
			int num2 = World.toChunkXZ(poiposition.x + size.x + 1);
			int num3 = World.toChunkXZ(poiposition.z - 1);
			int num4 = World.toChunkXZ(poiposition.z + size.z + 1);
			Rect rect;
			rect..ctor((float)poiposition.x, (float)poiposition.z, (float)size.x, (float)size.z);
			List<BlockChangeInfo> list = new List<BlockChangeInfo>();
			for (int i = num; i <= num2; i++)
			{
				for (int j = num3; j <= num4; j++)
				{
					Chunk chunk = world.GetChunkSync(i, j) as Chunk;
					if (chunk != null)
					{
						for (int k = 0; k < this.indexBlockNames.Length; k++)
						{
							List<Vector3i> list2 = chunk.IndexedBlocks[this.indexBlockNames[k]];
							if (list2 != null)
							{
								for (int l = 0; l < list2.Count; l++)
								{
									BlockValue blockValue = chunk.GetBlock(list2[l]);
									if (!blockValue.ischild)
									{
										Vector3i vector3i = chunk.ToWorldPos(list2[l]);
										if (rect.Contains(new Vector2((float)vector3i.x, (float)vector3i.z)))
										{
											BlockLight blockLight = blockValue.Block as BlockLight;
											if (blockLight != null && blockLight.OriginalLightState(blockValue))
											{
												blockValue = blockLight.SetLightState(world, vector3i, blockValue, this.enableLights);
												list.Add(new BlockChangeInfo(vector3i, blockValue));
											}
										}
									}
								}
							}
						}
					}
				}
			}
			if (list.Count > 0)
			{
				GameManager.Instance.StartCoroutine(this.UpdateBlocks(list));
			}
			return BaseAction.ActionCompleteStates.Complete;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public IEnumerator UpdateBlocks(List<BlockChangeInfo> blockChanges)
		{
			yield return new WaitForSeconds(1f);
			GameManager.Instance.World.SetBlocksRPC(blockChanges);
			yield break;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseBool(ActionPOISetLightState.PropEnabled, ref this.enableLights);
			string text = "";
			properties.ParseString(ActionPOISetLightState.PropIndexBlockName, ref text);
			this.indexBlockNames = text.Split(',', StringSplitOptions.None);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionPOISetLightState
			{
				enableLights = this.enableLights,
				indexBlockNames = this.indexBlockNames
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool enableLights;

		[PublicizedFrom(EAccessModifier.Protected)]
		public string[] indexBlockNames;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropEnabled = "enable_lights";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropIndexBlockName = "index_block_name";
	}
}
