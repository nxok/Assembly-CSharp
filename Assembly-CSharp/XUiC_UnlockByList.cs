using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_UnlockByList : XUiController
{
	public Recipe Recipe
	{
		get
		{
			return this.recipe;
		}
		set
		{
			this.recipe = value;
			this.IsDirty = true;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiController[] childrenByType = base.GetChildrenByType<XUiC_UnlockByEntry>(null);
		XUiController[] array = childrenByType;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != null)
			{
				this.unlockByEntries.Add(array[i]);
			}
		}
	}

	public override void Update(float _dt)
	{
		if (this.IsDirty)
		{
			if (this.recipe != null)
			{
				ItemClass forId = ItemClass.GetForId(this.recipe.itemValueType);
				RecipeUnlockData[] array;
				if (forId.IsBlock())
				{
					array = forId.GetBlock().UnlockedBy;
				}
				else
				{
					array = forId.UnlockedBy;
				}
				int num = 0;
				int count = this.unlockByEntries.Count;
				Progression progression = this.xui.playerUI.entityPlayer.Progression;
				if (array != null)
				{
					for (int i = 0; i < array.Length; i++)
					{
						XUiC_UnlockByEntry xuiC_UnlockByEntry = this.unlockByEntries[i] as XUiC_UnlockByEntry;
						if (xuiC_UnlockByEntry != null)
						{
							xuiC_UnlockByEntry.UnlockData = array[i];
							xuiC_UnlockByEntry.Recipe = this.recipe;
							num++;
						}
					}
				}
				for (int j = num; j < count; j++)
				{
					XUiC_UnlockByEntry xuiC_UnlockByEntry2 = this.unlockByEntries[j] as XUiC_UnlockByEntry;
					if (xuiC_UnlockByEntry2 != null)
					{
						xuiC_UnlockByEntry2.UnlockData = null;
					}
				}
			}
			else
			{
				int count2 = this.unlockByEntries.Count;
				for (int k = 0; k < count2; k++)
				{
					XUiC_UnlockByEntry xuiC_UnlockByEntry3 = this.unlockByEntries[k] as XUiC_UnlockByEntry;
					if (xuiC_UnlockByEntry3 != null)
					{
						xuiC_UnlockByEntry3.UnlockData = null;
					}
				}
			}
			this.IsDirty = false;
		}
		base.Update(_dt);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Recipe recipe;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiController> unlockByEntries = new List<XUiController>();
}
