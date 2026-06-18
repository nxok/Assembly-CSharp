using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_CollectorFuelGrid : XUiC_VariableHeightGrid, ITileEntityChangedListener
{
	public override XUiC_ItemStack.StackLocationTypes StackLocation
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			XUiC_ItemStack.StackLocationTypes stackLocationTypes = XUiC_ItemStack.StackLocationTypes.CollectorFuel;
			if (this.fuelBehavior == XUiC_CollectorFuelGrid.FuelBehavior.Catalyst)
			{
				stackLocationTypes = XUiC_ItemStack.StackLocationTypes.CollectorCatalysts;
			}
			return stackLocationTypes;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		XUiC_CollectorFuelGrid.FuelBehavior fuelBehavior = this.fuelBehavior;
		if (fuelBehavior == XUiC_CollectorFuelGrid.FuelBehavior.Fuel)
		{
			this.xui.CurrentCollectorFuelGrid = this;
			return;
		}
		if (fuelBehavior != XUiC_CollectorFuelGrid.FuelBehavior.Catalyst)
		{
			return;
		}
		this.xui.currentCollectorCatalystGrid = this;
	}

	public override void OnClose()
	{
		base.OnClose();
		XUiC_CollectorFuelGrid.FuelBehavior fuelBehavior = this.fuelBehavior;
		if (fuelBehavior == XUiC_CollectorFuelGrid.FuelBehavior.Fuel)
		{
			this.xui.CurrentCollectorFuelGrid = null;
			return;
		}
		if (fuelBehavior != XUiC_CollectorFuelGrid.FuelBehavior.Catalyst)
		{
			return;
		}
		this.xui.currentCollectorCatalystGrid = null;
	}

	public override bool ParseAttribute(string _name, string _value)
	{
		bool flag;
		if (_name == "fuelbehavior")
		{
			if (!Enum.TryParse<XUiC_CollectorFuelGrid.FuelBehavior>(_value, out this.fuelBehavior))
			{
				this.fuelBehavior = XUiC_CollectorFuelGrid.FuelBehavior.Fuel;
			}
			flag = true;
		}
		else
		{
			flag = base.ParseAttribute(_name, _value);
		}
		return flag;
	}

	public bool TryAddFuel(ItemClass newItemClass, ItemStack newItemStack)
	{
		string text = "";
		string[] array = new string[0];
		XUiC_CollectorFuelGrid.FuelBehavior fuelBehavior = this.fuelBehavior;
		if (fuelBehavior != XUiC_CollectorFuelGrid.FuelBehavior.Fuel)
		{
			if (fuelBehavior == XUiC_CollectorFuelGrid.FuelBehavior.Catalyst)
			{
				array = this.tileEntity.GetCatalystTypes();
			}
		}
		else
		{
			array = this.tileEntity.GetFuelTypes();
		}
		foreach (string text2 in array)
		{
			if (newItemClass.Name == text2)
			{
				text = text2;
				break;
			}
		}
		if (!string.IsNullOrEmpty(text))
		{
			int num = 0;
			fuelBehavior = this.fuelBehavior;
			if (fuelBehavior != XUiC_CollectorFuelGrid.FuelBehavior.Fuel)
			{
				if (fuelBehavior == XUiC_CollectorFuelGrid.FuelBehavior.Catalyst)
				{
					num = this.tileEntity.CatalystGridLength;
				}
			}
			else
			{
				num = this.tileEntity.FuelGridLength;
			}
			for (int j = 0; j < num; j++)
			{
				XUiC_ItemStack xuiC_ItemStack = this.itemControllers[j];
				if (xuiC_ItemStack.ItemStack.IsEmpty())
				{
					xuiC_ItemStack.ItemStack = newItemStack.Clone();
					newItemStack.count = 0;
					break;
				}
				int num2 = xuiC_ItemStack.ItemStack.StackTransferCount(newItemStack);
				xuiC_ItemStack.ItemStack.count += num2;
				newItemStack.count -= num2;
				if (newItemStack.count == 0)
				{
					break;
				}
			}
		}
		this.UpdateBackend(this.getUISlots());
		return newItemStack.count == 0;
	}

	public override void Init()
	{
		base.Init();
		XUiV_Grid xuiV_Grid = this.viewComponent as XUiV_Grid;
		XUiController childById = this.parent.Parent.GetChildById("slot_preview");
		XUiV_Grid xuiV_Grid2 = childById.ViewComponent as XUiV_Grid;
		base.InitVariableHeightGrids(new XUiV_Grid[] { xuiV_Grid, xuiV_Grid2 });
		XUiController[] childrenById = childById.GetChildrenById("slot", null);
		int num = childrenById.Length;
		this.previewSprites = new XUiV_Sprite[num];
		for (int i = 0; i < num; i++)
		{
			this.previewSprites[i] = childrenById[i].ViewComponent as XUiV_Sprite;
		}
	}

	public void SetTileEntity(TileEntityCollector te)
	{
		this.tileEntity = te;
		if (!this.tileEntity.listeners.Contains(this))
		{
			this.tileEntity.listeners.Add(this);
		}
		this.SetStacks(te.FuelSlots);
		this.blockCollector = te.blockValue.Block as BlockCollector;
		string[] array = new string[0];
		string[] array2 = new string[0];
		if (this.blockCollector != null)
		{
			if (this.fuelBehavior == XUiC_CollectorFuelGrid.FuelBehavior.Catalyst)
			{
				base.SetHeight(te.CatalystGridHeight);
				array = this.blockCollector.CatalystTypesSprites;
				array2 = this.blockCollector.CatalystTypesSpriteAtlases;
			}
			else
			{
				base.SetHeight(te.FuelGridHeight);
				array = this.blockCollector.FuelTypesSprites;
				array2 = this.blockCollector.FuelTypesSpriteAtlases;
			}
		}
		int num = array.Length;
		if (num > 0)
		{
			for (int i = 0; i < this.previewSprites.Length; i++)
			{
				int num2 = i % num;
				this.previewSprites[i].SpriteName = array[num2];
				this.previewSprites[i].UIAtlas = array2[num2];
			}
		}
	}

	public void OnTileEntityChanged(ITileEntity te)
	{
		TileEntityCollector tileEntityCollector = te as TileEntityCollector;
		if (tileEntityCollector != null && tileEntityCollector == this.tileEntity)
		{
			XUiC_CollectorFuelGrid.FuelBehavior fuelBehavior = this.fuelBehavior;
			if (fuelBehavior == XUiC_CollectorFuelGrid.FuelBehavior.Fuel)
			{
				this.SetStacks(tileEntityCollector.FuelSlots);
				base.SetHeight(tileEntityCollector.FuelGridHeight);
				return;
			}
			if (fuelBehavior != XUiC_CollectorFuelGrid.FuelBehavior.Catalyst)
			{
				return;
			}
			this.SetStacks(tileEntityCollector.CatalystSlots);
			base.SetHeight(tileEntityCollector.CatalystGridHeight);
		}
	}

	public override void HandleSlotChangedEvent(int slotNumber, ItemStack stack)
	{
		ItemStack[] uislots = this.getUISlots();
		XUiC_CollectorFuelGrid.FuelBehavior fuelBehavior = this.fuelBehavior;
		if (fuelBehavior != XUiC_CollectorFuelGrid.FuelBehavior.Fuel && fuelBehavior == XUiC_CollectorFuelGrid.FuelBehavior.Catalyst)
		{
			BlockCollector.CatalystConvert[] catalystConverts = this.blockCollector.CatalystConverts;
			for (int i = 0; i < catalystConverts.Length; i++)
			{
				ItemStack itemStack = catalystConverts[i].Convert(stack);
				if (itemStack != null)
				{
					uislots[slotNumber] = itemStack;
					break;
				}
			}
		}
		if (this.items != null)
		{
			this.items[slotNumber] = stack.Clone();
		}
		this.UpdateBackend(uislots);
		fuelBehavior = this.fuelBehavior;
		if (fuelBehavior != XUiC_CollectorFuelGrid.FuelBehavior.Fuel && fuelBehavior == XUiC_CollectorFuelGrid.FuelBehavior.Catalyst)
		{
			this.SetStacks(this.tileEntity.CatalystSlots);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateBackend(ItemStack[] stackList)
	{
		base.UpdateBackend(stackList);
		XUiC_CollectorFuelGrid.FuelBehavior fuelBehavior = this.fuelBehavior;
		if (fuelBehavior != XUiC_CollectorFuelGrid.FuelBehavior.Fuel)
		{
			if (fuelBehavior == XUiC_CollectorFuelGrid.FuelBehavior.Catalyst)
			{
				this.tileEntity.CatalystSlots = stackList;
			}
		}
		else
		{
			this.tileEntity.FuelSlots = stackList;
		}
		this.tileEntity.SetModified();
		this.windowGroup.Controller.SetAllChildrenDirty();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (_bindingName == "container_slots")
		{
			_value = this.collectorFuelGridLength.ToString();
			return true;
		}
		return base.GetBindingValueInternal(ref _value, _bindingName);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public new TileEntityCollector tileEntity;

	[PublicizedFrom(EAccessModifier.Protected)]
	public BlockCollector blockCollector;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CollectorFuelGrid.FuelBehavior fuelBehavior;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite[] previewSprites;

	[PublicizedFrom(EAccessModifier.Private)]
	public int collectorFuelGridLength;

	[PublicizedFrom(EAccessModifier.Private)]
	public enum FuelBehavior
	{
		Fuel,
		Catalyst
	}
}
