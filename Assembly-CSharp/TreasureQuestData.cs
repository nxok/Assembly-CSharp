using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class TreasureQuestData : BaseQuestData
{
	public int BlocksPerReduction
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public Vector3i Position
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public Vector3 TreasureOffset
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public TreasureQuestData(int _questCode, int _entityID, int _blocksPerReduction, Vector3i _position, Vector3 _treasureOffset)
	{
		this.questCode = _questCode;
		this.entityList.Add(_entityID);
		this.Position = _position;
		this.TreasureOffset = _treasureOffset;
		this.BlocksPerReduction = _blocksPerReduction;
	}

	public void AddSharedQuester(int _entityID, int _blocksPerReduction)
	{
		if (_blocksPerReduction < this.BlocksPerReduction)
		{
			this.SendBlocksPerReductionUpdate(this.BlocksPerReduction);
		}
		base.AddSharedQuester(_entityID);
	}

	public void SendBlocksPerReductionUpdate(int _newBlocksPerReduction)
	{
		this.BlocksPerReduction = _newBlocksPerReduction;
		World world = GameManager.Instance.World;
		for (int i = 0; i < this.entityList.Count; i++)
		{
			EntityPlayer entityPlayer = world.GetEntity(this.entityList[i]) as EntityPlayer;
			if (entityPlayer is EntityPlayerLocal)
			{
				ObjectiveTreasureChest objectiveForQuest = entityPlayer.QuestJournal.GetObjectiveForQuest<ObjectiveTreasureChest>(this.questCode);
				if (objectiveForQuest != null)
				{
					objectiveForQuest.CurrentBlocksPerReduction = this.BlocksPerReduction;
				}
			}
			else
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageQuestTreasurePoint>().Setup(this.questCode, this.BlocksPerReduction), false, this.entityList[i], -1, -1, null, 192, false);
			}
		}
	}

	public void UpdatePosition(Vector3i _pos)
	{
		this.Position = _pos;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void RemoveFromDictionary()
	{
		QuestEventManager.Current.TreasureQuestDictionary.Remove(this.questCode);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnRemove(EntityPlayer player)
	{
	}
}
