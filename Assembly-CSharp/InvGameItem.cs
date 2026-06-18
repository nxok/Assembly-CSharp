using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InvGameItem
{
	public int baseItemID
	{
		get
		{
			return this.mBaseItemID;
		}
	}

	public InvBaseItem baseItem
	{
		get
		{
			if (this.mBaseItem == null)
			{
				this.mBaseItem = InvDatabase.FindByID(this.baseItemID);
			}
			return this.mBaseItem;
		}
	}

	public string name
	{
		get
		{
			if (this.baseItem == null)
			{
				return null;
			}
			return this.quality.ToString() + " " + this.baseItem.name;
		}
	}

	public float statMultiplier
	{
		get
		{
			float num = 0f;
			switch (this.quality)
			{
			case InvGameItem.Quality.Broken:
				num = 0f;
				break;
			case InvGameItem.Quality.Cursed:
				num = -1f;
				break;
			case InvGameItem.Quality.Damaged:
				num = 0.25f;
				break;
			case InvGameItem.Quality.Worn:
				num = 0.9f;
				break;
			case InvGameItem.Quality.Sturdy:
				num = 1f;
				break;
			case InvGameItem.Quality.Polished:
				num = 1.1f;
				break;
			case InvGameItem.Quality.Improved:
				num = 1.25f;
				break;
			case InvGameItem.Quality.Crafted:
				num = 1.5f;
				break;
			case InvGameItem.Quality.Superior:
				num = 1.75f;
				break;
			case InvGameItem.Quality.Enchanted:
				num = 2f;
				break;
			case InvGameItem.Quality.Epic:
				num = 2.5f;
				break;
			case InvGameItem.Quality.Legendary:
				num = 3f;
				break;
			}
			float num2 = (float)this.itemLevel / 50f;
			return num * Mathf.Lerp(num2, num2 * num2, 0.5f);
		}
	}

	public Color color
	{
		get
		{
			Color color = Color.white;
			switch (this.quality)
			{
			case InvGameItem.Quality.Broken:
				color..ctor(0.4f, 0.2f, 0.2f);
				break;
			case InvGameItem.Quality.Cursed:
				color = Color.red;
				break;
			case InvGameItem.Quality.Damaged:
				color..ctor(0.4f, 0.4f, 0.4f);
				break;
			case InvGameItem.Quality.Worn:
				color..ctor(0.7f, 0.7f, 0.7f);
				break;
			case InvGameItem.Quality.Sturdy:
				color..ctor(1f, 1f, 1f);
				break;
			case InvGameItem.Quality.Polished:
				color = NGUIMath.HexToColor(3774856959U);
				break;
			case InvGameItem.Quality.Improved:
				color = NGUIMath.HexToColor(2480359935U);
				break;
			case InvGameItem.Quality.Crafted:
				color = NGUIMath.HexToColor(1325334783U);
				break;
			case InvGameItem.Quality.Superior:
				color = NGUIMath.HexToColor(12255231U);
				break;
			case InvGameItem.Quality.Enchanted:
				color = NGUIMath.HexToColor(1937178111U);
				break;
			case InvGameItem.Quality.Epic:
				color = NGUIMath.HexToColor(2516647935U);
				break;
			case InvGameItem.Quality.Legendary:
				color = NGUIMath.HexToColor(4287627519U);
				break;
			}
			return color;
		}
	}

	public InvGameItem(int id)
	{
		this.mBaseItemID = id;
	}

	public InvGameItem(int id, InvBaseItem bi)
	{
		this.mBaseItemID = id;
		this.mBaseItem = bi;
	}

	public List<InvStat> CalculateStats()
	{
		List<InvStat> list = new List<InvStat>();
		if (this.baseItem != null)
		{
			float statMultiplier = this.statMultiplier;
			List<InvStat> stats = this.baseItem.stats;
			int i = 0;
			int count = stats.Count;
			while (i < count)
			{
				InvStat invStat = stats[i];
				int num = Mathf.RoundToInt(statMultiplier * (float)invStat.amount);
				if (num != 0)
				{
					bool flag = false;
					int j = 0;
					int count2 = list.Count;
					while (j < count2)
					{
						InvStat invStat2 = list[j];
						if (invStat2.id == invStat.id && invStat2.modifier == invStat.modifier)
						{
							invStat2.amount += num;
							flag = true;
							break;
						}
						j++;
					}
					if (!flag)
					{
						list.Add(new InvStat
						{
							id = invStat.id,
							amount = num,
							modifier = invStat.modifier
						});
					}
				}
				i++;
			}
			list.Sort(new Comparison<InvStat>(InvStat.CompareArmor));
		}
		return list;
	}

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public int mBaseItemID;

	public InvGameItem.Quality quality = InvGameItem.Quality.Sturdy;

	public int itemLevel = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public InvBaseItem mBaseItem;

	public enum Quality
	{
		Broken,
		Cursed,
		Damaged,
		Worn,
		Sturdy,
		Polished,
		Improved,
		Crafted,
		Superior,
		Enchanted,
		Epic,
		Legendary,
		_LastDoNotUse
	}
}
