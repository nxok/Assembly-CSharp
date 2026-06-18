using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionShuffleContainers : ActionBaseContainersAction
	{
		public override bool CheckValidTileEntity(TileEntity te, out bool isEmpty)
		{
			isEmpty = false;
			TileEntityType tileEntityType = te.GetTileEntityType();
			if (tileEntityType != TileEntityType.Workstation)
			{
				ITileEntityLootable tileEntityLootable;
				if (tileEntityType == TileEntityType.Composite && te.TryGetSelfOrFeature(out tileEntityLootable))
				{
					isEmpty = tileEntityLootable.IsEmpty();
					return true;
				}
			}
			else if (this.includeOutputs)
			{
				TileEntityWorkstation tileEntityWorkstation = te as TileEntityWorkstation;
				if (tileEntityWorkstation != null)
				{
					isEmpty = tileEntityWorkstation.OutputEmpty();
					return true;
				}
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override bool HandleContainerAction(List<TileEntity> tileEntityList)
		{
			List<ItemStack> list = new List<ItemStack>();
			List<TileEntity> list2 = new List<TileEntity>();
			bool flag = false;
			for (int i = 0; i < tileEntityList.Count; i++)
			{
				TileEntityType tileEntityType = tileEntityList[i].GetTileEntityType();
				if (tileEntityType != TileEntityType.Workstation)
				{
					ITileEntityLootable tileEntityLootable;
					if (tileEntityType == TileEntityType.Composite && tileEntityList[i].TryGetSelfOrFeature(out tileEntityLootable))
					{
						list2.Add(tileEntityList[i]);
						list.AddRange(tileEntityLootable.items);
						if (!tileEntityLootable.IsEmpty())
						{
							flag = true;
						}
					}
				}
				else if (this.includeOutputs)
				{
					TileEntityWorkstation tileEntityWorkstation = tileEntityList[i] as TileEntityWorkstation;
					if (tileEntityWorkstation != null)
					{
						list2.Add(tileEntityWorkstation);
						list.AddRange(tileEntityWorkstation.Output);
						if (!tileEntityWorkstation.OutputEmpty())
						{
							flag = true;
						}
					}
				}
			}
			if (flag && this.changeName && base.Owner.Target != null)
			{
				PersistentPlayerData playerDataFromEntityID = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(base.Owner.Target.entityId);
				for (int j = 0; j < tileEntityList.Count; j++)
				{
					ITileEntitySignable tileEntitySignable;
					if (tileEntityList[j].TryGetSelfOrFeature(out tileEntitySignable))
					{
						tileEntitySignable.SetText(base.ModifiedName, true, (playerDataFromEntityID != null) ? playerDataFromEntityID.PrimaryId : null);
					}
				}
			}
			GameRandom random = GameEventManager.Current.Random;
			if (list2.Count > 0)
			{
				for (int k = 0; k < list.Count * 2; k++)
				{
					int num = random.RandomRange(list.Count);
					int num2 = random.RandomRange(list.Count);
					ItemStack itemStack = list[num];
					list[num] = list[num2];
					list[num2] = itemStack;
				}
				for (int l = 0; l < list2.Count; l++)
				{
					TileEntityType tileEntityType2 = list2[l].GetTileEntityType();
					if (tileEntityType2 != TileEntityType.Workstation)
					{
						ITileEntityLootable tileEntityLootable2;
						if (tileEntityType2 == TileEntityType.Composite && list2[l].TryGetSelfOrFeature(out tileEntityLootable2))
						{
							for (int m = 0; m < tileEntityLootable2.items.Length; m++)
							{
								tileEntityLootable2.items[m] = list[0];
								list.RemoveAt(0);
							}
						}
					}
					else if (this.includeOutputs)
					{
						TileEntityWorkstation tileEntityWorkstation2 = list2[l] as TileEntityWorkstation;
						if (tileEntityWorkstation2 != null)
						{
							ItemStack[] output = tileEntityWorkstation2.Output;
							for (int n = 0; n < output.Length; n++)
							{
								output[n] = list[0];
								list.RemoveAt(0);
							}
							tileEntityWorkstation2.Output = output;
						}
					}
					list2[l].SetModified();
				}
			}
			return flag;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseBool(ActionShuffleContainers.PropIncludeOutputs, ref this.includeOutputs);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionShuffleContainers
			{
				TargetingType = this.TargetingType,
				maxDistance = this.maxDistance,
				newName = this.newName,
				changeName = this.changeName,
				includeOutputs = this.includeOutputs,
				tileEntityList = this.tileEntityList
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool includeOutputs;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropIncludeOutputs = "include_outputs";
	}
}
