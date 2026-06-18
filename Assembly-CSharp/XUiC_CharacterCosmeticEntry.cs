using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_CharacterCosmeticEntry : XUiController
{
	public ItemClass ItemClass
	{
		get
		{
			return this.itemClass;
		}
		set
		{
			this.itemClass = value;
			this.IsDirty = true;
		}
	}

	public XUiC_CharacterCosmeticEntry.EntryTypes EntryType
	{
		get
		{
			return this.entryType;
		}
		set
		{
			this.entryType = value;
			this.IsDirty = true;
		}
	}

	public int Index
	{
		get
		{
			return this.index;
		}
		set
		{
			this.index = value;
			this.IsDirty = true;
		}
	}

	public new bool Selected
	{
		get
		{
			return this.selected;
		}
		set
		{
			this.selected = value;
		}
	}

	public override void Init()
	{
		base.Init();
		this.IsDirty = true;
		XUiController childById = base.GetChildById("itemIcon");
		if (childById != null)
		{
			this.itemIconSprite = childById.ViewComponent as XUiV_Sprite;
		}
		XUiController childById2 = base.GetChildById("backgroundTexture");
		if (childById2 != null)
		{
			this.backgroundTexture = childById2.ViewComponent as XUiV_Texture;
			if (this.backgroundTexture != null)
			{
				this.backgroundTexture.CreateMaterial("Unlit/Transparent Colored Emissive TextureArray");
			}
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.xui.PlayerEquipment.Equipment.CosmeticUnlocked += this.Equipment_CosmeticUnlocked;
	}

	public override void OnClose()
	{
		base.OnClose();
		this.xui.PlayerEquipment.Equipment.CosmeticUnlocked -= this.Equipment_CosmeticUnlocked;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Equipment_CosmeticUnlocked(string itemName)
	{
		if (this.entryType == XUiC_CharacterCosmeticEntry.EntryTypes.Item)
		{
			if (this.ItemClass == null)
			{
				return;
			}
			if (this.ItemClass.GetItemName() == itemName)
			{
				if (this.Owner.SelectedEntry == this)
				{
					this.Owner.Owner.RefreshBindings();
				}
				this.IsDirty = true;
			}
		}
	}

	public Color32 SelectionBorderColor
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.selectionBorderColor;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			if (!this.selectionBorderColor.ColorEquals(value))
			{
				this.selectionBorderColor = value;
				this.IsDirty = true;
			}
		}
	}

	public bool IsDragAndDrop
	{
		get
		{
			return this.isDragAndDrop;
		}
		set
		{
			this.isDragAndDrop = value;
			if (!value)
			{
				return;
			}
			base.ViewComponent.EventOnPress = false;
			base.ViewComponent.EventOnHover = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateBorderColor()
	{
		if (this.IsDragAndDrop)
		{
			this.SelectionBorderColor = Color.clear;
			return;
		}
		if (this.Selected)
		{
			this.SelectionBorderColor = this.selectColor;
			return;
		}
		if (this.isHovered)
		{
			this.SelectionBorderColor = this.highlightColor;
			return;
		}
		this.SelectionBorderColor = this.backgroundColor;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnHovered(bool _isOver)
	{
		base.OnHovered(_isOver);
		if (this.EntryType == XUiC_CharacterCosmeticEntry.EntryTypes.Item && this.itemClass == null)
		{
			this.isHovered = false;
			return;
		}
		if (this.isHovered != _isOver)
		{
			this.isHovered = _isOver;
			base.RefreshBindings();
		}
	}

	public override void Update(float _dt)
	{
		if (this.IsDirty)
		{
			base.ViewComponent.IsNavigatable = (base.ViewComponent.IsSnappable = this.entryType != XUiC_CharacterCosmeticEntry.EntryTypes.Item || (this.entryType == XUiC_CharacterCosmeticEntry.EntryTypes.Item && this.itemClass != null));
			base.RefreshBindings();
			this.IsDirty = false;
		}
		this.updateBorderColor();
		base.Update(_dt);
	}

	[return: TupleElementNames(new string[] { "isUnlocked", "set" })]
	public ValueTuple<bool, EntitlementSetEnum> IsUnlocked()
	{
		if (this.entryType != XUiC_CharacterCosmeticEntry.EntryTypes.Item)
		{
			return new ValueTuple<bool, EntitlementSetEnum>(true, EntitlementSetEnum.None);
		}
		if (this.itemClass == null)
		{
			return new ValueTuple<bool, EntitlementSetEnum>(false, EntitlementSetEnum.None);
		}
		return this.xui.PlayerEquipment.Equipment.HasCosmeticUnlocked(this.itemClass);
	}

	public string Name
	{
		get
		{
			switch (this.EntryType)
			{
			case XUiC_CharacterCosmeticEntry.EntryTypes.Item:
				if (this.itemClass != null)
				{
					return this.itemClass.GetLocalizedItemName();
				}
				break;
			case XUiC_CharacterCosmeticEntry.EntryTypes.Empty:
				return Localization.Get("xuiEmpty", false, null);
			case XUiC_CharacterCosmeticEntry.EntryTypes.Hide:
				return Localization.Get("xuiHide", false, null);
			}
			return Localization.Get("xuiCosmetics", false, null);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		ValueTuple<bool, EntitlementSetEnum> valueTuple = this.IsUnlocked();
		bool item = valueTuple.Item1;
		EntitlementSetEnum item2 = valueTuple.Item2;
		if (bindingName == "itemicon")
		{
			value = "";
			switch (this.EntryType)
			{
			case XUiC_CharacterCosmeticEntry.EntryTypes.Item:
				if (this.itemClass != null)
				{
					value = this.itemClass.GetIconName();
				}
				break;
			case XUiC_CharacterCosmeticEntry.EntryTypes.Empty:
				value = "server_refresh";
				break;
			case XUiC_CharacterCosmeticEntry.EntryTypes.Hide:
				value = "ui_game_symbol_x";
				break;
			}
			return true;
		}
		if (bindingName == "iconatlas")
		{
			value = "ItemIconAtlas";
			if (this.EntryType != XUiC_CharacterCosmeticEntry.EntryTypes.Item)
			{
				value = "UIAtlas";
			}
			else
			{
				if (this.itemClass == null)
				{
					return true;
				}
				if (!item)
				{
					value = "ItemIconAtlasGreyscale";
				}
			}
			return true;
		}
		if (bindingName == "islocked")
		{
			value = "false";
			if (!item && item2 != EntitlementSetEnum.None)
			{
				value = "true";
			}
			return true;
		}
		if (bindingName == "selectionbordercolor")
		{
			value = this.selectionbordercolorFormatter.Format(this.SelectionBorderColor);
			return true;
		}
		if (bindingName == "backgroundcolor")
		{
			if (!item && this.itemClass != null)
			{
				value = this.backgroundcolorFormatter.Format(this.lockedBackgroundColor);
			}
			else
			{
				value = this.backgroundcolorFormatter.Format(this.backgroundColor);
			}
			return true;
		}
		if (!(bindingName == "tooltip"))
		{
			return base.GetBindingValueInternal(ref value, bindingName);
		}
		value = "";
		switch (this.EntryType)
		{
		case XUiC_CharacterCosmeticEntry.EntryTypes.Item:
			if (this.itemClass != null)
			{
				value = this.itemClass.GetLocalizedItemName();
			}
			break;
		case XUiC_CharacterCosmeticEntry.EntryTypes.Empty:
			value = Localization.Get("xuiEmpty", false, null);
			break;
		case XUiC_CharacterCosmeticEntry.EntryTypes.Hide:
			value = Localization.Get("xuiHide", false, null);
			break;
		}
		return true;
	}

	public override bool ParseAttribute(string _name, string _value)
	{
		if (_name == "select_color")
		{
			this.selectColor = StringParsers.ParseColor32(_value);
			return true;
		}
		if (_name == "press_color")
		{
			this.pressColor = StringParsers.ParseColor32(_value);
			return true;
		}
		if (_name == "background_color")
		{
			this.backgroundColor = StringParsers.ParseColor32(_value);
			return true;
		}
		if (_name == "locked_background_color")
		{
			this.backgroundColor = StringParsers.ParseColor32(_value);
			return true;
		}
		if (_name == "highlight_color")
		{
			this.highlightColor = StringParsers.ParseColor32(_value);
			return true;
		}
		if (!(_name == "holding_color"))
		{
			return base.ParseAttribute(_name, _value);
		}
		this.holdingColor = StringParsers.ParseColor32(_value);
		return true;
	}

	public XUiC_CharacterCosmeticList Owner;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass itemClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CharacterCosmeticEntry.EntryTypes entryType;

	[PublicizedFrom(EAccessModifier.Private)]
	public int index = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool selected;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isHovered;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiV_Sprite itemIconSprite;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiV_Texture backgroundTexture;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 selectionBorderColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 selectColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 pressColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 backgroundColor = new Color32(96, 96, 96, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 lockedBackgroundColor = new Color32(48, 48, 48, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 highlightColor = new Color32(222, 206, 163, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 holdingColor = new Color32(byte.MaxValue, 128, 0, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDragAndDrop;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor backgroundcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor selectionbordercolorFormatter = new CachedStringFormatterXuiRgbaColor();

	public enum EntryTypes
	{
		Item,
		Empty,
		Hide
	}
}
