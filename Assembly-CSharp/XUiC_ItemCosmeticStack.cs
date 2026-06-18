using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ItemCosmeticStack : XUiC_BasePartStack
{
	public ItemValue ItemValue
	{
		get
		{
			return this.itemValue;
		}
		set
		{
			this.itemValue = value;
			base.ItemStack = new ItemStack(value, 1);
		}
	}

	public ItemClass ExpectedItemClass
	{
		get
		{
			return this.expectedItemClass;
		}
		set
		{
			this.expectedItemClass = value;
			this.SetEmptySpriteName();
		}
	}

	public override void Init()
	{
		base.Init();
		this.lblStackMissing = Localization.Get("lblPartStackMissing", false, null);
	}

	public override string GetAtlas()
	{
		if (base.ItemStack.IsEmpty())
		{
			return "ItemIconAtlasGreyscale";
		}
		return "ItemIconAtlas";
	}

	public override string GetPartName()
	{
		if (this.itemClass == null && this.expectedItemClass == null)
		{
			return "";
		}
		if (this.itemClass == null)
		{
			return string.Format(this.lblStackMissing, this.expectedItemClass.GetLocalizedItemName());
		}
		return this.itemClass.GetLocalizedItemName();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SetEmptySpriteName()
	{
		if (this.expectedItemClass != null && this.expectedItemClass.Id != 0)
		{
			this.emptySpriteName = this.expectedItemClass.GetIconName();
			return;
		}
		this.emptySpriteName = "";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool CanSwap(ItemStack stack)
	{
		ItemClassModifier itemClassModifier = stack.itemValue.ItemClass as ItemClassModifier;
		if (itemClassModifier == null)
		{
			return false;
		}
		if (this.xui.AssembleItem.CurrentItem.itemValue.ItemClass.HasAnyTags(itemClassModifier.DisallowedTags))
		{
			return false;
		}
		if (!this.xui.AssembleItem.CurrentItem.itemValue.ItemClass.HasAnyTags(itemClassModifier.InstallableTags))
		{
			return false;
		}
		if (itemClassModifier != null && !itemClassModifier.ModifierTags.Test_AnySet(ItemClassModifier.CosmeticModTypes) && this.xui.AssembleItem.CurrentItem.itemValue.Modifications.Length != 0)
		{
			bool flag = false;
			for (int i = 0; i < this.xui.AssembleItem.CurrentItem.itemValue.Modifications.Length; i++)
			{
				if (this.xui.AssembleItem.CurrentItem.itemValue.Modifications[i] == null || this.xui.AssembleItem.CurrentItem.itemValue.Modifications[i].IsEmpty())
				{
					flag = true;
				}
				else
				{
					ItemClassModifier itemClassModifier2 = this.xui.AssembleItem.CurrentItem.itemValue.Modifications[i].ItemClass as ItemClassModifier;
					if (itemClassModifier2 != null && itemClassModifier.ModifierTags.Test_AnySet(itemClassModifier2.ModifierTags))
					{
						flag = false;
						break;
					}
				}
			}
			return flag;
		}
		bool flag2 = itemClassModifier.InstallableTags.IsEmpty || this.xui.AssembleItem.CurrentItem.itemValue.ItemClass.HasAnyTags(itemClassModifier.InstallableTags);
		for (int j = 0; j < this.xui.AssembleItem.CurrentItem.itemValue.CosmeticMods.Length; j++)
		{
			if (this.xui.AssembleItem.CurrentItem.itemValue.CosmeticMods[j] != null && this.xui.AssembleItem.CurrentItem.itemValue.CosmeticMods[j].ItemClass != null && !this.xui.AssembleItem.CurrentItem.itemValue.CosmeticMods[j].ItemClass.ItemTags.IsEmpty)
			{
				ItemClassModifier itemClassModifier3 = this.xui.AssembleItem.CurrentItem.itemValue.CosmeticMods[j].ItemClass as ItemClassModifier;
				if (itemClassModifier3 != null && this.xui.AssembleItem.CurrentItem.itemValue.CosmeticMods[j] != this.itemValue && itemClassModifier.ModifierTags.Test_AnySet(itemClassModifier3.ModifierTags))
				{
					return false;
				}
			}
		}
		return flag2 && (this.itemValue == null || this.itemValue.type == 0 || (this.itemValue.ItemClass as ItemClassModifier).Type == ItemClassModifier.ModifierTypes.Attachment);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool CanRemove()
	{
		return this.itemClass is ItemClassModifier && (this.itemClass as ItemClassModifier).Type == ItemClassModifier.ModifierTypes.Attachment;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SwapItem()
	{
		ItemClassModifier itemClassModifier = this.xui.DragAndDropWindow.CurrentStack.itemValue.ItemClass as ItemClassModifier;
		base.SwapItem();
		if (itemClassModifier != null && !itemClassModifier.ModifierTags.Test_AnySet(ItemClassModifier.CosmeticModTypes) && itemClassModifier != null && !itemClassModifier.ModifierTags.Test_AnySet(ItemClassModifier.CosmeticModTypes) && this.xui.AssembleItem.CurrentItem.itemValue.Modifications.Length != 0)
		{
			bool flag = false;
			for (int i = 0; i < this.xui.AssembleItem.CurrentItem.itemValue.Modifications.Length; i++)
			{
				ItemClassModifier itemClassModifier2 = this.xui.AssembleItem.CurrentItem.itemValue.Modifications[i].ItemClass as ItemClassModifier;
				if (itemClassModifier2 != null && itemClassModifier.ModifierTags.Test_AnySet(itemClassModifier2.ModifierTags))
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				this.xui.DragAndDropWindow.CurrentStack = ItemStack.Empty;
				this.xui.DragAndDropWindow.PickUpType = base.StackLocation;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue itemValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass expectedItemClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblStackMissing;
}
