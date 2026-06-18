using System;

public class XUiM_AssembleItem : XUiModel
{
	public ItemStack CurrentItem
	{
		get
		{
			return this.currentItem;
		}
		set
		{
			this.currentItem = value;
			if (value != null)
			{
				this.SetPartCount();
			}
		}
	}

	public XUiC_ItemStack CurrentItemStackController
	{
		get
		{
			return this.currentItemStackController;
		}
		set
		{
			if (this.currentItemStackController != null)
			{
				this.currentItemStackController.AssembleLock = false;
			}
			this.currentItemStackController = value;
			if (this.currentItemStackController != null)
			{
				this.currentItemStackController.AssembleLock = true;
			}
		}
	}

	public XUiC_EquipmentStack CurrentEquipmentStackController
	{
		get
		{
			return this.currentEquipmentStackController;
		}
		set
		{
			this.currentEquipmentStackController = value;
		}
	}

	public int PartCount
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetPartCount()
	{
		this.PartCount = 0;
		for (int i = 0; i < this.CurrentItem.itemValue.Modifications.Length; i++)
		{
			if (this.CurrentItem.itemValue.Modifications[i] == null)
			{
				this.CurrentItem.itemValue.Modifications[i] = ItemValue.None;
			}
			if (!this.CurrentItem.itemValue.Modifications[i].IsEmpty())
			{
				int partCount = this.PartCount;
				this.PartCount = partCount + 1;
			}
		}
	}

	public void RefreshAssembleItem()
	{
		this.PartCount = 0;
		bool flag = false;
		for (int i = 0; i < this.CurrentItem.itemValue.Modifications.Length; i++)
		{
			if (!this.CurrentItem.itemValue.Modifications[i].IsEmpty())
			{
				int partCount = this.PartCount;
				this.PartCount = partCount + 1;
			}
			else
			{
				flag = true;
			}
		}
		if (this.CurrentItemStackController != null)
		{
			this.CurrentItemStackController.ForceSetItemStack(this.CurrentItem);
			this.CurrentItemStackController.AssembleLock = true;
		}
		if (this.currentEquipmentStackController != null)
		{
			this.currentEquipmentStackController.ItemStack = this.CurrentItem;
		}
		if (flag)
		{
			QuestEventManager.Current.AssembledItem(this.CurrentItem);
		}
	}

	public bool AddPartToItem(ItemStack _partStack, out ItemStack _resultStack)
	{
		if (this.CurrentItem == null || this.CurrentItem.IsEmpty())
		{
			_resultStack = _partStack;
			return false;
		}
		ItemClassModifier itemClassModifier = _partStack.itemValue.ItemClass as ItemClassModifier;
		if (itemClassModifier != null)
		{
			if (this.CurrentItem.itemValue.ItemClass.HasAnyTags(itemClassModifier.DisallowedTags))
			{
				_resultStack = _partStack;
				return false;
			}
			if (itemClassModifier.ModifierTags.Test_AnySet(ItemClassModifier.CosmeticModTypes))
			{
				for (int i = 0; i < this.CurrentItem.itemValue.CosmeticMods.Length; i++)
				{
					if (this.CurrentItem.itemValue.CosmeticMods[i] != null && this.CurrentItem.itemValue.CosmeticMods[i].ItemClass != null)
					{
						ItemClassModifier itemClassModifier2 = this.CurrentItem.itemValue.CosmeticMods[i].ItemClass as ItemClassModifier;
						if (itemClassModifier2 != null && itemClassModifier.ModifierTags.Test_AnySet(itemClassModifier2.ModifierTags))
						{
							_resultStack = _partStack;
							return false;
						}
					}
				}
			}
			else
			{
				int num = 0;
				for (int j = 0; j < this.CurrentItem.itemValue.Modifications.Length; j++)
				{
					if (this.CurrentItem.itemValue.Modifications[j] != null && this.CurrentItem.itemValue.Modifications[j].ItemClass != null && !itemClassModifier.HasAnyTags(EntityDrone.StorageModifierTags))
					{
						ItemClassModifier itemClassModifier3 = this.CurrentItem.itemValue.Modifications[j].ItemClass as ItemClassModifier;
						if (itemClassModifier3 != null && itemClassModifier.ModifierTags.Test_AnySet(itemClassModifier3.ModifierTags))
						{
							num++;
						}
					}
				}
				if (num >= itemClassModifier.MaxModsAllowed)
				{
					_resultStack = _partStack;
					return false;
				}
			}
		}
		if (this.CurrentItem.itemValue.ItemClass.HasAnyTags(itemClassModifier.InstallableTags))
		{
			if (itemClassModifier.ModifierTags.Test_AnySet(ItemClassModifier.CosmeticModTypes))
			{
				if (this.CurrentItem.itemValue.CosmeticMods != null)
				{
					for (int k = 0; k < this.CurrentItem.itemValue.CosmeticMods.Length; k++)
					{
						if (this.CurrentItem.itemValue.CosmeticMods[k] == null || this.CurrentItem.itemValue.CosmeticMods[k].IsEmpty())
						{
							float num2 = 1f - this.CurrentItem.itemValue.PercentUsesLeft;
							this.CurrentItem.itemValue.CosmeticMods[k] = _partStack.itemValue.Clone();
							if (this.CurrentItemStackController != null)
							{
								XUiC_AssembleWindowGroup.GetWindowGroup(this.CurrentItemStackController.xui).ItemStack = this.CurrentItem;
							}
							if (this.currentEquipmentStackController != null)
							{
								XUiC_AssembleWindowGroup.GetWindowGroup(this.CurrentEquipmentStackController.xui).ItemStack = this.CurrentItem;
							}
							this.RefreshAssembleItem();
							if (this.CurrentItem.itemValue.MaxUseTimes > 0)
							{
								this.CurrentItem.itemValue.UseTimes = (float)((int)(num2 * (float)this.CurrentItem.itemValue.MaxUseTimes));
							}
							this.UpdateAssembleWindow();
							_resultStack = ItemStack.Empty;
							return true;
						}
					}
				}
			}
			else if (this.CurrentItem.itemValue.Modifications != null)
			{
				for (int l = 0; l < this.CurrentItem.itemValue.Modifications.Length; l++)
				{
					if (this.CurrentItem.itemValue.Modifications[l] == null || this.CurrentItem.itemValue.Modifications[l].IsEmpty())
					{
						float num3 = 1f - this.CurrentItem.itemValue.PercentUsesLeft;
						this.CurrentItem.itemValue.Modifications[l] = _partStack.itemValue.Clone();
						if (this.CurrentItemStackController != null)
						{
							XUiC_AssembleWindowGroup.GetWindowGroup(this.CurrentItemStackController.xui).ItemStack = this.CurrentItem;
						}
						if (this.currentEquipmentStackController != null)
						{
							XUiC_AssembleWindowGroup.GetWindowGroup(this.CurrentEquipmentStackController.xui).ItemStack = this.CurrentItem;
						}
						this.RefreshAssembleItem();
						if (this.CurrentItem.itemValue.MaxUseTimes > 0)
						{
							this.CurrentItem.itemValue.UseTimes = (float)((int)(num3 * (float)this.CurrentItem.itemValue.MaxUseTimes));
						}
						this.UpdateAssembleWindow();
						_resultStack = ItemStack.Empty;
						return true;
					}
				}
			}
		}
		_resultStack = _partStack;
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateAssembleWindow()
	{
		if (this.AssembleWindow != null)
		{
			this.AssembleWindow.ItemStack = this.CurrentItem;
			this.AssembleWindow.OnChanged();
		}
	}

	public XUiC_AssembleWindow AssembleWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack currentItem;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemStack currentItemStackController;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_EquipmentStack currentEquipmentStackController;
}
