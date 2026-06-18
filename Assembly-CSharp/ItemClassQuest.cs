using System;
using UnityEngine.Scripting;

[Preserve]
public class ItemClassQuest : ItemClass
{
	public override bool CanDrop(ItemValue _iv = null)
	{
		return false;
	}

	public override bool CanStack()
	{
		return false;
	}

	public override bool KeepOnDeath()
	{
		return true;
	}

	public new static void Cleanup()
	{
		ItemClassQuest.questItemList = null;
	}

	public override bool CanPlaceInContainer()
	{
		return false;
	}

	public static ItemClassQuest GetItemQuestById(ushort _questTypeID)
	{
		if (ItemClassQuest.questItemList == null)
		{
			return null;
		}
		if (_questTypeID < 0 || (int)_questTypeID >= ItemClassQuest.questItemList.Length)
		{
			return null;
		}
		return ItemClassQuest.questItemList[(int)_questTypeID];
	}

	public static ItemClassQuest[] questItemList = new ItemClassQuest[100];
}
