using System;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DragAndDropWindow : XUiController
{
	public EntityPlayerLocal entityPlayer
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUi xui = this.xui;
			if (xui == null)
			{
				return null;
			}
			LocalPlayerUI playerUI = xui.playerUI;
			if (playerUI == null)
			{
				return null;
			}
			return playerUI.entityPlayer;
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void DropCurrentItem()
	{
		this.xui.PlayerInventory.DropItem(this.CurrentStack);
		this.CurrentStack = ItemStack.Empty;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void DropCurrentItem(int _count)
	{
		if (_count < this.CurrentStack.count)
		{
			ItemStack itemStack = this.CurrentStack.Clone();
			itemStack.count = _count;
			this.CurrentStack.count -= _count;
			this.xui.PlayerInventory.DropItem(itemStack);
			this.CurrentStack = this.itemStack;
			return;
		}
		this.DropCurrentItem();
	}

	public ItemStack CurrentStack
	{
		get
		{
			return this.itemStack;
		}
		set
		{
			this.itemStack = value;
			this.ItemStackControl.ItemStack = value;
			if (value != null && !value.IsEmpty())
			{
				this.ItemStackControl.PlayPickupSound(null);
			}
			if (this.entityPlayer != null)
			{
				this.entityPlayer.DragAndDropItem = value;
				base.RefreshBindings();
			}
		}
	}

	public override void Init()
	{
		base.Init();
		this.ItemStackControl = base.GetChildByType<XUiC_ItemStack>();
		this.ItemStackControl.IsDragAndDrop = true;
		this.ItemStackControl.ItemStack = ItemStack.Empty;
		this.xui.DragAndDropWindow = this;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		this.xui.DragAndDropWindow = null;
	}

	public override void Update(float _dt)
	{
		if (!this.InMenu)
		{
			this.PlaceItemBackInInventory();
		}
		if (this.itemStack != null && !this.itemStack.IsEmpty())
		{
			base.ViewComponent.Position = this.xui.GetMouseXUiPosition() + this.PositionOffset;
		}
		base.RefreshBindings();
		base.Update(_dt);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (this.entityPlayer != null)
		{
			ItemStack dragAndDropItem = this.entityPlayer.DragAndDropItem;
			if (dragAndDropItem != null && !dragAndDropItem.IsEmpty())
			{
				this.CurrentStack = this.entityPlayer.DragAndDropItem;
				this.PlaceItemBackInInventory();
			}
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		this.PlaceItemBackInInventory();
	}

	public void PlaceItemBackInInventory()
	{
		if (!this.CurrentStack.IsEmpty())
		{
			if (this.xui.PlayerInventory.AddItem(this.itemStack))
			{
				Manager.PlayXUiSound(this.placeSound, 0.75f);
				this.CurrentStack = ItemStack.Empty;
				return;
			}
			this.xui.PlayerInventory.DropItem(this.itemStack);
			this.CurrentStack = ItemStack.Empty;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (_bindingName == "hasItemStack")
		{
			_value = (this.itemStack != null && !this.itemStack.IsEmpty()).ToString();
			return true;
		}
		return base.GetBindingValueInternal(ref _value, _bindingName);
	}

	public override bool ParseAttribute(string _name, string _value)
	{
		if (_name == "position_offset")
		{
			this.PositionOffset = StringParsers.ParseVector2i(_value, ',');
			return true;
		}
		if (!(_name == "place_sound"))
		{
			return base.ParseAttribute(_name, _value);
		}
		this.xui.LoadData<AudioClip>(_value, delegate(AudioClip _o)
		{
			this.placeSound = _o;
		});
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemStack ItemStackControl;

	[PublicizedFrom(EAccessModifier.Private)]
	public AudioClip placeSound;

	public Vector2i PositionOffset = new Vector2i(0, 0);

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack itemStack = new ItemStack(new ItemValue(0, false), 0);

	public bool InMenu;

	public XUiC_ItemStack.StackLocationTypes PickUpType;
}
