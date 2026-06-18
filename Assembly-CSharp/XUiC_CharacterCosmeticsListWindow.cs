using System;
using System.Collections.Generic;
using UniLinq;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_CharacterCosmeticsListWindow : XUiController
{
	public override void Init()
	{
		base.Init();
		this.categoryList = base.GetChildByType<XUiC_CategoryList>();
		this.cosmeticGrid = base.GetChildByType<XUiC_CharacterCosmeticList>();
		this.cosmeticGrid.Owner = this;
		this.btnApply = base.GetChildById("btnApply").GetChildByType<XUiC_SimpleButton>();
		this.btnApply.OnPressed += this.BtnApply_OnPressed;
		base.GetChildById("btnApplySet").GetChildByType<XUiC_SimpleButton>().OnPressed += this.BtnApplySet_OnPressed;
		this.txtInput = (XUiC_TextInput)base.GetChildById("searchInput");
		if (this.txtInput != null)
		{
			this.txtInput.OnChangeHandler += this.HandleOnChangedHandler;
			this.txtInput.Text = "";
		}
		this.categoryList.CategoryChanged += this.CategoryList_CategoryChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplySelected()
	{
		ValueTuple<bool, EntitlementSetEnum> valueTuple = this.cosmeticGrid.SelectedEntry.IsUnlocked();
		bool item = valueTuple.Item1;
		EntitlementSetEnum item2 = valueTuple.Item2;
		if (!item && item2 != EntitlementSetEnum.None)
		{
			EntitlementManager.Instance.OpenStore(item2, delegate(EntitlementSetEnum _)
			{
				this.IsDirty = true;
			});
		}
		if (item)
		{
			this.xui.PlayerEquipment.Equipment.ApplyTempCosmeticSlot();
			EModelSDCS emodelSDCS = this.xui.playerUI.entityPlayer.emodel as EModelSDCS;
			if (emodelSDCS != null)
			{
				emodelSDCS.GenerateMeshes();
			}
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnApply_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.ApplySelected();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnApplySet_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.ApplySelected();
		if (!this.cosmeticGrid.SelectedEntry.IsUnlocked().Item1)
		{
			return;
		}
		ItemClassArmor itemClassArmor = this.xui.PlayerEquipment.Equipment.tempCosmeticSlot as ItemClassArmor;
		Equipment equipment = this.xui.PlayerEquipment.Equipment;
		if (itemClassArmor == null)
		{
			if (this.xui.PlayerEquipment.Equipment.tempCosmeticSlot == ItemClass.MissingItem)
			{
				this.xui.PlayerEquipment.Equipment.SetCosmeticSlot(0, -1);
				this.xui.PlayerEquipment.Equipment.SetCosmeticSlot(1, -1);
				this.xui.PlayerEquipment.Equipment.SetCosmeticSlot(2, -1);
				this.xui.PlayerEquipment.Equipment.SetCosmeticSlot(3, -1);
			}
			else
			{
				equipment.ClearCosmeticSlots();
			}
		}
		else
		{
			string armorGroup = itemClassArmor.ArmorGroup[0];
			List<ItemClass> list = ItemClass.list.Where(delegate(ItemClass item)
			{
				ItemClassArmor itemClassArmor2 = item as ItemClassArmor;
				return itemClassArmor2 != null && itemClassArmor2.ArmorGroup[0] == armorGroup && itemClassArmor2.IsCosmetic;
			}).ToList<ItemClass>();
			for (int i = 0; i < list.Count; i++)
			{
				equipment.SetCosmeticSlot(list[i] as ItemClassArmor);
			}
		}
		((XUiC_CharacterCosmeticWindowGroup)base.WindowGroup.Controller).ResetPreview();
		EModelSDCS emodelSDCS = this.xui.playerUI.entityPlayer.emodel as EModelSDCS;
		if (emodelSDCS != null)
		{
			emodelSDCS.GenerateMeshes();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnChangedHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		this.filterText = _text;
		this.IsDirty = true;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (this.categoryList != null)
		{
			this.categoryList.SetCategoryEntry(0, "Head", "ui_game_symbol_head_shot", Localization.Get("lblHeadgear", false, null));
			this.categoryList.SetCategoryEntry(1, "Chest", "ui_game_symbol_armor_iron", Localization.Get("lblChest", false, null));
			this.categoryList.SetCategoryEntry(2, "Hands", "ui_game_symbol_hand", Localization.Get("lblHands", false, null));
			this.categoryList.SetCategoryEntry(3, "Feet", "ui_game_symbol_splint", Localization.Get("lblFeet", false, null));
			for (int i = 4; i < this.categoryList.CategoryButtons.Count; i++)
			{
				this.categoryList.SetCategoryEmpty(i);
			}
		}
		if (!string.IsNullOrEmpty(XUiC_CharacterCosmeticsListWindow.defaultSelectedElement))
		{
			base.GetChildById(XUiC_CharacterCosmeticsListWindow.defaultSelectedElement).SelectCursorElement(true, false);
			XUiC_CharacterCosmeticsListWindow.defaultSelectedElement = "";
		}
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void SetCategory(EquipmentSlots equipSlot)
	{
		this.selectedSlot = equipSlot;
		this.categoryList.SetCategory(equipSlot.ToString());
	}

	public void SetEmptyCosmeticForCurrentCategory()
	{
		this.cosmeticGrid.SetEmptyCosmetic();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CategoryList_CategoryChanged(XUiC_CategoryEntry _categoryEntry)
	{
		this.selectedSlot = (EquipmentSlots)Enum.Parse(typeof(EquipmentSlots), _categoryEntry.CategoryName, true);
		if (this.xui.playerUI.entityPlayer.equipment.GetSlotItem((int)this.selectedSlot) == null)
		{
			this.btnApply.Tooltip = Localization.Get("ttCosmeticsDisabledSlot", false, null);
		}
		else
		{
			this.btnApply.Tooltip = null;
		}
		this.IsDirty = true;
		this.xui.PlayerEquipment.Equipment.ClearTempCosmeticSlot();
		((XUiC_CharacterCosmeticWindowGroup)base.WindowGroup.Controller).ResetPreview();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty)
		{
			List<ItemClass> list = ItemClass.list.Where(delegate(ItemClass item)
			{
				ItemClassArmor itemClassArmor = item as ItemClassArmor;
				return itemClassArmor != null && itemClassArmor.EquipSlot == this.selectedSlot && itemClassArmor.IsCosmetic && (this.filterText == "" || item.GetLocalizedItemName().ContainsCaseInsensitive(this.filterText)) && (item.SDCSData == null || (EntitlementManager.Instance.IsAvailableOnPlatform(item.SDCSData.PrefabName) && (EntitlementManager.Instance.IsEntitlementPurchasable(item.SDCSData.PrefabName) || EntitlementManager.Instance.HasEntitlement(item.SDCSData.PrefabName))));
			}).ToList<ItemClass>();
			this.cosmeticGrid.SetCosmeticList(list, this.selectedSlot);
			this.IsDirty = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		bool flag = false;
		EntitlementSetEnum entitlementSetEnum = EntitlementSetEnum.None;
		XUiC_CharacterCosmeticList xuiC_CharacterCosmeticList = this.cosmeticGrid;
		if (((xuiC_CharacterCosmeticList != null) ? xuiC_CharacterCosmeticList.SelectedEntry : null) != null)
		{
			ValueTuple<bool, EntitlementSetEnum> valueTuple = this.cosmeticGrid.SelectedEntry.IsUnlocked();
			flag = valueTuple.Item1;
			entitlementSetEnum = valueTuple.Item2;
		}
		if (bindingName == "cosmeticname")
		{
			value = "xuiCosmetics";
			if (this.cosmeticGrid != null && this.cosmeticGrid.SelectedEntry != null)
			{
				value = this.cosmeticGrid.SelectedEntry.Name;
			}
			return true;
		}
		if (bindingName == "applyTextKey")
		{
			value = "xuiCosmeticsApply";
			if (!flag && entitlementSetEnum != EntitlementSetEnum.None)
			{
				value = "xuiCosmeticsPurchase";
			}
			return true;
		}
		if (bindingName == "enableApply")
		{
			value = "false";
			if (flag || entitlementSetEnum != EntitlementSetEnum.None)
			{
				value = "true";
			}
			return true;
		}
		if (!(bindingName == "enableApplySet"))
		{
			return base.GetBindingValueInternal(ref value, bindingName);
		}
		value = "false";
		if (flag)
		{
			value = "true";
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CharacterCosmeticList cosmeticGrid;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CategoryList categoryList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnApply;

	public EquipmentSlots selectedSlot;

	public static string defaultSelectedElement;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public string filterText = "";
}
