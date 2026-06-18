using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_WorkstationMaterialInputGrid : XUiC_WorkstationInputGrid
{
	public override void Init()
	{
		base.Init();
		this.materialWindow = this.windowGroup.Controller.GetChildByType<XUiC_WorkstationMaterialInputWindow>();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		int num = 0;
		while (num < this.workstationData.TileEntity.Input.Length && num < this.itemControllers.Length)
		{
			float timerForSlot = this.workstationData.TileEntity.GetTimerForSlot(num);
			if (timerForSlot > 0f)
			{
				this.itemControllers[num].timer.IsVisible = true;
				this.itemControllers[num].timer.Text = string.Format("{0}:{1}", Mathf.Floor((timerForSlot + 0.95f) / 60f).ToCultureInvariantString("00"), Mathf.Floor((timerForSlot + 0.95f) % 60f).ToCultureInvariantString("00"));
			}
			else
			{
				this.itemControllers[num].timer.IsVisible = false;
			}
			num++;
		}
		this.workstationData.GetIsBurning();
	}

	public override ItemStack[] GetSlots()
	{
		return this.getUISlots();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override ItemStack[] getUISlots()
	{
		return this.items;
	}

	public override bool HasRequirement(Recipe recipe)
	{
		return this.materialWindow != null && this.materialWindow.HasRequirement(recipe);
	}

	public override void SetSlots(ItemStack[] stackList)
	{
		this.items = stackList;
		base.SetSlots(this.items);
		this.materialWindow.SetMaterialWeights(this.items);
	}

	public void SetWeight(ItemValue iv, int count)
	{
		ItemClass itemClass = iv.ItemClass;
		if (itemClass == null)
		{
			return;
		}
		string forgeCategory = itemClass.MadeOfMaterial.ForgeCategory;
		if (forgeCategory == null)
		{
			return;
		}
		for (int i = 3; i < this.items.Length; i++)
		{
			ItemClass itemClass2 = this.items[i].itemValue.ItemClass;
			if (itemClass2 == null)
			{
				if (this.materialWindow.MaterialNames[i - 3].EqualsCaseInsensitive(forgeCategory))
				{
					ItemStack itemStack = new ItemStack(iv, count);
					this.items[i] = itemStack;
					break;
				}
			}
			else if (itemClass2.MadeOfMaterial.ForgeCategory.EqualsCaseInsensitive(forgeCategory))
			{
				ItemStack itemStack2 = this.items[i].Clone();
				itemStack2.count += count;
				if (iv.ItemClass.Stacknumber.Value < itemStack2.count)
				{
					itemStack2.count = iv.ItemClass.Stacknumber.Value;
				}
				this.items[i] = itemStack2;
				break;
			}
		}
		this.materialWindow.SetMaterialWeights(this.items);
		this.UpdateBackend(this.items);
	}

	public int GetWeight(string materialName)
	{
		int num = 0;
		if (materialName == null)
		{
			return num;
		}
		for (int i = 3; i < this.items.Length; i++)
		{
			ItemClass itemClass = this.items[i].itemValue.ItemClass;
			if (itemClass != null && itemClass.MadeOfMaterial.ForgeCategory.EqualsCaseInsensitive(materialName))
			{
				num = this.items[i].count;
				break;
			}
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WorkstationMaterialInputWindow materialWindow;
}
