using System;
using System.Collections.Generic;
using PrefabVolumes;
using UnityEngine;

public class SleeperEventData
{
	public void SetupData(Vector3 _position)
	{
		this.position = _position;
		PrefabInstance prefabFromWorldPos = GameManager.Instance.GetDynamicPrefabDecorator().GetPrefabFromWorldPos((int)this.position.x, (int)this.position.z);
		this.ShowQuestClearCount = prefabFromWorldPos.prefab.ShowQuestClearCount;
		if (prefabFromWorldPos != null)
		{
			for (int i = 0; i < prefabFromWorldPos.prefab.SleeperVolumeList.Count; i++)
			{
				PrefabSleeperVolume prefabSleeperVolume = prefabFromWorldPos.prefab.SleeperVolumeList[i];
				Vector3i startPos = prefabSleeperVolume.startPos;
				Vector3i size = prefabSleeperVolume.size;
				int num = GameManager.Instance.World.FindSleeperVolume(prefabFromWorldPos.boundingBoxPosition + startPos, prefabFromWorldPos.boundingBoxPosition + startPos + size);
				if (num != -1)
				{
					SleeperVolume sleeperVolume = GameManager.Instance.World.GetSleeperVolume(num);
					if (!sleeperVolume.isQuestExclude && !this.SleeperVolumes.Contains(sleeperVolume))
					{
						this.SleeperVolumes.Add(sleeperVolume);
					}
				}
			}
		}
	}

	public bool Update()
	{
		if (!this.hasRefreshed)
		{
			World world = GameManager.Instance.World;
			for (int i = 0; i < this.SleeperVolumes.Count; i++)
			{
				this.SleeperVolumes[i].DespawnAndReset(world);
			}
			this.hasRefreshed = true;
		}
		if (this.SleeperVolumes.Count <= 0)
		{
			return false;
		}
		bool flag = false;
		bool flag2 = false;
		for (int j = this.SleeperVolumes.Count - 1; j >= 0; j--)
		{
			SleeperVolume sleeperVolume = this.SleeperVolumes[j];
			if (sleeperVolume.wasCleared)
			{
				for (int k = 0; k < this.EntityList.Count; k++)
				{
					if ((GameManager.Instance.World.GetEntity(this.EntityList[k]) as EntityPlayer) is EntityPlayerLocal)
					{
						QuestEventManager.Current.SleeperVolumePositionRemoved(sleeperVolume.Center);
					}
					else
					{
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageQuestEvent>().Setup(NetPackageQuestEvent.QuestEventTypes.HideSleeperVolume, this.EntityList[k], sleeperVolume.Center), false, -1, -1, -1, null, 192, false);
					}
				}
				this.SleeperVolumes.RemoveAt(j);
				flag = true;
			}
			else
			{
				flag2 = true;
			}
		}
		if (flag)
		{
			if (this.SleeperVolumes.Count <= this.ShowQuestClearCount)
			{
				for (int l = 0; l < this.EntityList.Count; l++)
				{
					EntityPlayer entityPlayer = GameManager.Instance.World.GetEntity(this.EntityList[l]) as EntityPlayer;
					for (int m = 0; m < this.SleeperVolumes.Count; m++)
					{
						if (entityPlayer is EntityPlayerLocal)
						{
							QuestEventManager.Current.SleeperVolumePositionAdded(this.SleeperVolumes[m].Center);
						}
						else
						{
							SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageQuestEvent>().Setup(NetPackageQuestEvent.QuestEventTypes.ShowSleeperVolume, this.EntityList[l], this.SleeperVolumes[m].Center), false, this.EntityList[l], -1, -1, null, 192, false);
						}
					}
				}
			}
		}
		if (flag2)
		{
			return false;
		}
		bool flag3 = false;
		for (int n = 0; n < this.EntityList.Count; n++)
		{
			if ((GameManager.Instance.World.GetEntity(this.EntityList[n]) as EntityPlayer) is EntityPlayerLocal)
			{
				flag3 = true;
			}
			else
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageQuestEvent>().Setup(NetPackageQuestEvent.QuestEventTypes.ClearSleeper, this.EntityList[n], this.position), false, -1, -1, -1, null, 192, false);
			}
		}
		if (flag3)
		{
			QuestEventManager.Current.ClearedSleepers(this.position);
		}
		return true;
	}

	public List<SleeperVolume> SleeperVolumes = new List<SleeperVolume>();

	public List<int> EntityList = new List<int>();

	public bool hasRefreshed;

	public int ShowQuestClearCount = 1;

	public Vector3 position;
}
