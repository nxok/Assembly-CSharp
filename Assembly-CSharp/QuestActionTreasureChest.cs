using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class QuestActionTreasureChest : BaseQuestAction
{
	public override void SetupAction()
	{
	}

	public override void PerformAction(Quest ownerQuest)
	{
		World world = GameManager.Instance.World;
		EntityPlayer ownerPlayer = ownerQuest.OwnerJournal.OwnerPlayer;
		float num = ((this.Value == "" || this.Value == null) ? 50f : StringParsers.ParseFloat(this.Value, 0, -1, NumberStyles.Any));
		GameRandom gameRandom = world.GetGameRandom();
		Vector3 vector;
		vector..ctor(-1f + 2f * gameRandom.RandomFloat, 0f, -1f + 2f * gameRandom.RandomFloat);
		vector.Normalize();
		Vector3 vector2 = ownerPlayer.position + vector * num;
		int num2 = (int)vector2.x;
		int num3 = (int)vector2.z;
		int num4 = (int)(world.GetHeight(num2, num3) - 3);
		BlockValue blockValue = new BlockValue
		{
			type = 372
		};
		Vector3i vector3i = new Vector3i(num2, num4, num3);
		world.SetBlockRPC(vector3i, blockValue, sbyte.MaxValue);
		ownerQuest.DataVariables.Add("treasurecontainer", string.Format("{0},{1},{2}", num2, num4, num3));
	}

	public override BaseQuestAction Clone()
	{
		QuestActionTreasureChest questActionTreasureChest = new QuestActionTreasureChest();
		base.CopyValues(questActionTreasureChest);
		return questActionTreasureChest;
	}
}
