using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ItemPartStack : XUiC_BasePartStack
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
		ItemValue itemValue = this.xui.AssembleItem.CurrentItem.itemValue;
		ItemClass itemClass = itemValue.ItemClass;
		if (itemClass.HasAnyTags(itemClassModifier.DisallowedTags))
		{
			return false;
		}
		if (!itemClass.HasAnyTags(itemClassModifier.InstallableTags))
		{
			return false;
		}
		if (itemClassModifier.HasAnyTags(EntityDrone.StorageModifierTags))
		{
			return true;
		}
		if (itemClassModifier.ModifierTags.Test_AnySet(ItemClassModifier.CosmeticModTypes) && itemValue.CosmeticMods.Length != 0)
		{
			for (int i = 0; i < itemValue.CosmeticMods.Length; i++)
			{
				if (itemValue.CosmeticMods[i] == null || itemValue.CosmeticMods[i].IsEmpty())
				{
					return true;
				}
			}
			return false;
		}
		bool flag = itemClassModifier.InstallableTags.IsEmpty || itemClass.HasAnyTags(itemClassModifier.InstallableTags);
		int num = 0;
		ItemClass itemClass2 = base.ItemClass;
		for (int j = 0; j < itemValue.Modifications.Length; j++)
		{
			if (itemValue.Modifications[j].ItemClass != null)
			{
				ItemClassModifier itemClassModifier2 = itemValue.Modifications[j].ItemClass as ItemClassModifier;
				if (itemClassModifier2 != null && !itemValue.Modifications[j].Equals(this.itemValue) && itemClassModifier.ModifierTags.Test_AnySet(itemClassModifier2.ModifierTags))
				{
					num++;
				}
			}
		}
		return num < stack.itemValue.ItemClass.MaxModsAllowed && flag && (this.itemValue == null || this.itemValue.type == 0 || (this.itemValue.ItemClass as ItemClassModifier).Type == ItemClassModifier.ModifierTypes.Attachment);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool CanRemove()
	{
		bool flag = true;
		ItemClassModifier itemClassModifier = this.itemClass as ItemClassModifier;
		if (itemClassModifier != null)
		{
			flag = flag && itemClassModifier.Type == ItemClassModifier.ModifierTypes.Attachment;
			if (itemClassModifier.ModifierTags.Test_AnySet(EntityVehicle.StorageModifierTags))
			{
				XUiC_VehicleWindowGroup xuiC_VehicleWindowGroup = base.WindowGroup.Controller as XUiC_VehicleWindowGroup;
				if (xuiC_VehicleWindowGroup != null)
				{
					flag = flag && xuiC_VehicleWindowGroup.CurrentVehicleEntity.CanRemoveInventoryMod();
				}
			}
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SwapItem()
	{
		ItemClassModifier itemClassModifier = this.xui.DragAndDropWindow.CurrentStack.itemValue.ItemClass as ItemClassModifier;
		base.SwapItem();
		if (itemClassModifier != null && itemClassModifier.ModifierTags.Test_AnySet(ItemClassModifier.CosmeticModTypes))
		{
			this.xui.DragAndDropWindow.CurrentStack = ItemStack.Empty;
			this.xui.DragAndDropWindow.PickUpType = base.StackLocation;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue itemValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass expectedItemClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblStackMissing;
}
