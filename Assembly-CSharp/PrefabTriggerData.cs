using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class PrefabTriggerData
{
	public bool NeedsTriggerUpdate
	{
		get
		{
			return this.needsTriggerTimer != -1f;
		}
		set
		{
			if (this.Owner == null)
			{
				this.Owner = this.world.triggerManager;
			}
			if (value)
			{
				this.Owner.AddToUpdateList(this);
				this.needsTriggerTimer = 3f;
				return;
			}
			this.Owner.RemoveFromUpdateList(this);
			this.needsTriggerTimer = -1f;
		}
	}

	public PrefabTriggerData(PrefabInstance instance)
	{
		this.PrefabInstance = instance;
		this.world = GameManager.Instance.World;
		this.SetupData();
	}

	public void ResetData()
	{
		if (this.TriggeredLayers != null)
		{
			this.TriggeredLayers.Clear();
		}
		if (this.TriggeredByLayers != null)
		{
			this.TriggeredByLayers.Clear();
		}
		this.TriggeredByDictionary.Clear();
		this.TriggeredByVolumes.Clear();
		this.Triggers.Clear();
		this.SetupData();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void SetupData()
	{
		bool flag = GameManager.Instance.World.IsEditor();
		HashSetLong occupiedChunks = this.PrefabInstance.GetOccupiedChunks();
		Vector3i boundingBoxSize = this.PrefabInstance.boundingBoxSize;
		Vector3i boundingBoxPosition = this.PrefabInstance.boundingBoxPosition;
		foreach (long num in occupiedChunks)
		{
			Chunk chunkSync = this.world.ChunkCache.GetChunkSync(num);
			if (chunkSync != null)
			{
				foreach (BlockTrigger blockTrigger in chunkSync.GetBlockTriggers().list)
				{
					Vector3i vector3i = blockTrigger.ToWorldPos();
					if (boundingBoxPosition.x <= vector3i.x && boundingBoxPosition.y <= vector3i.y && boundingBoxPosition.z <= vector3i.z && boundingBoxPosition.x + boundingBoxSize.x > vector3i.x && boundingBoxPosition.y + boundingBoxSize.y > vector3i.y && boundingBoxPosition.z + boundingBoxSize.z > vector3i.z)
					{
						using (List<byte>.Enumerator enumerator3 = blockTrigger.TriggeredByIndices.GetEnumerator())
						{
							while (enumerator3.MoveNext())
							{
								int num2 = (int)enumerator3.Current;
								List<BlockTrigger> list;
								if (!this.TriggeredByDictionary.TryGetValue(num2, out list))
								{
									list = new List<BlockTrigger>();
									this.TriggeredByDictionary[num2] = list;
								}
								list.Add(blockTrigger);
								if (flag)
								{
									if (this.TriggeredByLayers == null)
									{
										this.TriggeredByLayers = new List<byte>();
									}
									if (!this.TriggeredByLayers.Contains((byte)num2))
									{
										this.TriggeredByLayers.Add((byte)num2);
									}
								}
							}
						}
						using (List<byte>.Enumerator enumerator3 = blockTrigger.TriggersIndices.GetEnumerator())
						{
							while (enumerator3.MoveNext())
							{
								int num3 = (int)enumerator3.Current;
								if (this.TriggeredLayers == null)
								{
									this.TriggeredLayers = new List<byte>();
								}
								if (!this.TriggeredLayers.Contains((byte)num3))
								{
									this.TriggeredLayers.Add((byte)num3);
								}
							}
						}
						this.Triggers.Add(blockTrigger);
						blockTrigger.TriggerDataOwner = this;
					}
				}
			}
		}
		List<SleeperVolume> sleeperVolumes = this.PrefabInstance.sleeperVolumes;
		for (int i = 0; i < sleeperVolumes.Count; i++)
		{
			SleeperVolume sleeperVolume = sleeperVolumes[i];
			for (int j = 0; j < sleeperVolume.TriggeredByIndices.Count; j++)
			{
				this.AddTriggeredBy(sleeperVolume);
				if (flag)
				{
					if (this.TriggeredByLayers == null)
					{
						this.TriggeredByLayers = new List<byte>();
					}
					byte b = sleeperVolume.TriggeredByIndices[j];
					if (!this.TriggeredByLayers.Contains(b))
					{
						this.TriggeredByLayers.Add(b);
					}
				}
			}
		}
		this.RefreshTriggers();
	}

	public void Update(float deltaTime)
	{
		if (this.needsTriggerTimer != -1f)
		{
			this.needsTriggerTimer -= deltaTime;
			if (this.needsTriggerTimer <= 0f)
			{
				this.HandleNeedTriggers();
				this.NeedsTriggerUpdate = false;
			}
		}
	}

	public void HandleNeedTriggers()
	{
		for (int i = 0; i < this.Triggers.Count; i++)
		{
			if (this.Triggers[i].NeedsTriggered == BlockTrigger.TriggeredStates.NeedsTriggered)
			{
				this.Trigger(null, this.Triggers[i]);
				this.Triggers[i].NeedsTriggered = BlockTrigger.TriggeredStates.HasTriggered;
			}
		}
	}

	public void RefreshTriggers()
	{
		if (!GameManager.Instance.IsEditMode())
		{
			for (int i = 0; i < this.Triggers.Count; i++)
			{
				this.Triggers[i].Refresh(FastTags<TagGroup.Global>.none);
			}
		}
	}

	public void RefreshTriggersForQuest(FastTags<TagGroup.Global> questTags)
	{
		if (!GameManager.Instance.IsEditMode())
		{
			for (int i = 0; i < this.Triggers.Count; i++)
			{
				this.Triggers[i].Refresh(questTags);
			}
		}
	}

	public void ResetTriggers()
	{
		if (!GameManager.Instance.IsEditMode())
		{
			for (int i = 0; i < this.Triggers.Count; i++)
			{
				this.Triggers[i].NeedsTriggered = BlockTrigger.TriggeredStates.NotTriggered;
			}
		}
	}

	public void AddPlayerInArea(int entityID)
	{
		if (!this.PlayersInArea.Contains(entityID))
		{
			this.PlayersInArea.Add(entityID);
		}
	}

	public void RemovePlayerInArea(int entityID)
	{
		if (this.PlayersInArea.Contains(entityID))
		{
			this.PlayersInArea.Remove(entityID);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void AddTriggeredBy(SleeperVolume triggeredVolume)
	{
		for (int i = 0; i < triggeredVolume.TriggeredByIndices.Count; i++)
		{
			byte b = triggeredVolume.TriggeredByIndices[i];
			if (!this.TriggeredByVolumes.ContainsKey((int)b))
			{
				this.TriggeredByVolumes.Add((int)b, new List<SleeperVolume>());
			}
			this.TriggeredByVolumes[(int)b].Add(triggeredVolume);
		}
	}

	public void Trigger(EntityPlayer player, byte index)
	{
		List<BlockChangeInfo> list = new List<BlockChangeInfo>();
		World world = GameManager.Instance.World;
		List<BlockTrigger> list2;
		if (this.TriggeredByDictionary.TryGetValue((int)index, out list2))
		{
			for (int i = 0; i < list2.Count; i++)
			{
				list2[i].OnTriggered(player, world, (int)index, list, null);
			}
		}
		List<SleeperVolume> list3;
		if (this.TriggeredByVolumes.TryGetValue((int)index, out list3))
		{
			foreach (SleeperVolume sleeperVolume in list3)
			{
				sleeperVolume.OnTriggered(player, world, (int)index);
			}
		}
		if (list.Count > 0)
		{
			this.UpdateBlocks(list);
		}
	}

	public void Trigger(EntityPlayer player, BlockTrigger trigger)
	{
		List<BlockChangeInfo> list = new List<BlockChangeInfo>();
		World world = GameManager.Instance.World;
		using (List<byte>.Enumerator enumerator = trigger.TriggersIndices.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				int num = (int)enumerator.Current;
				List<BlockTrigger> list2;
				if (this.TriggeredByDictionary.TryGetValue(num, out list2))
				{
					foreach (BlockTrigger blockTrigger in list2)
					{
						blockTrigger.OnTriggered(player, world, num, list, trigger);
					}
				}
				if (player != null && this.TriggeredByVolumes.ContainsKey(num))
				{
					foreach (SleeperVolume sleeperVolume in this.TriggeredByVolumes[num])
					{
						sleeperVolume.OnTriggered(player, world, num);
					}
				}
			}
		}
		if (list.Count > 0)
		{
			this.UpdateBlocks(list);
		}
	}

	public void Trigger(EntityPlayer player, TriggerVolume trigger)
	{
		List<BlockChangeInfo> list = new List<BlockChangeInfo>();
		World world = GameManager.Instance.World;
		for (int i = 0; i < trigger.TriggersIndices.Count; i++)
		{
			int num = (int)trigger.TriggersIndices[i];
			if (this.TriggeredByDictionary.ContainsKey(num))
			{
				for (int j = 0; j < this.TriggeredByDictionary[num].Count; j++)
				{
					this.TriggeredByDictionary[num][j].OnTriggered(player, world, num, list, null);
				}
			}
			if (this.TriggeredByVolumes.ContainsKey(num))
			{
				for (int k = 0; k < this.TriggeredByVolumes[num].Count; k++)
				{
					this.TriggeredByVolumes[num][k].OnTriggered(player, world, num);
				}
			}
		}
		if (list.Count > 0)
		{
			this.UpdateBlocks(list);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void UpdateBlocks(List<BlockChangeInfo> blockChanges)
	{
		if (GameManager.Instance.World != null && blockChanges != null)
		{
			GameManager.Instance.World.SetBlocksRPC(blockChanges);
		}
	}

	public void SetupTriggerTestNavObjects()
	{
		this.RemoveTriggerTestNavObjects();
		for (int i = 0; i < this.Triggers.Count; i++)
		{
			NavObject navObject = NavObjectManager.Instance.RegisterNavObject("editor_block_trigger", this.Triggers[i].ToWorldPos().ToVector3Center(), "", false, -1, null);
			navObject.name = this.Triggers[i].TriggerDisplay();
			navObject.OverrideColor = ((this.Triggers[i].TriggeredByIndices.Count > 0) ? Color.blue : Color.red);
		}
	}

	public void RemoveTriggerTestNavObjects()
	{
		for (int i = 0; i < this.Triggers.Count; i++)
		{
			NavObjectManager.Instance.UnRegisterNavObjectByPosition(this.Triggers[i].ToWorldPos().ToVector3Center(), "editor_block_trigger");
		}
	}

	public Dictionary<int, List<BlockTrigger>> TriggeredByDictionary = new Dictionary<int, List<BlockTrigger>>();

	public Dictionary<int, List<SleeperVolume>> TriggeredByVolumes = new Dictionary<int, List<SleeperVolume>>();

	public PrefabInstance PrefabInstance;

	[PublicizedFrom(EAccessModifier.Protected)]
	public World world;

	public List<int> PlayersInArea = new List<int>();

	public List<byte> TriggeredLayers;

	public List<byte> TriggeredByLayers;

	public List<BlockTrigger> Triggers = new List<BlockTrigger>();

	public TriggerManager Owner;

	[PublicizedFrom(EAccessModifier.Private)]
	public float needsTriggerTimer = -1f;
}
