using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("NGUI/Examples/Equip Random Item")]
public class EquipRandomItem : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void OnClick()
	{
		if (this.equipment == null)
		{
			return;
		}
		List<InvBaseItem> items = InvDatabase.list[0].items;
		if (items.Count == 0)
		{
			return;
		}
		int num = 12;
		int num2 = Random.Range(0, items.Count);
		InvBaseItem invBaseItem = items[num2];
		InvGameItem invGameItem = new InvGameItem(num2, invBaseItem);
		invGameItem.quality = (InvGameItem.Quality)Random.Range(0, num);
		invGameItem.itemLevel = NGUITools.RandomRange(invBaseItem.minItemLevel, invBaseItem.maxItemLevel);
		this.equipment.Equip(invGameItem);
	}

	public InvEquipment equipment;
}
