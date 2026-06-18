using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_CollectedItem : XUiController
{
	public ItemStack ItemStack
	{
		get
		{
			return this.itemStack;
		}
		set
		{
			this.itemStack = value;
			this.itemClass = this.itemStack.itemValue.ItemClass;
			base.RefreshBindings();
		}
	}

	public override void Init()
	{
		base.Init();
		TweenColor tweenColor = base.ViewComponent.UiTransform.gameObject.AddComponent<TweenColor>();
		tweenColor.enabled = false;
		tweenColor.from = Color.white;
		tweenColor.to = new Color(1f, 1f, 1f, 0f);
		tweenColor.duration = 0.8f;
		base.ViewComponent.IsVisible = false;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
	}

	public void ShowItem()
	{
		TweenColor component = base.ViewComponent.UiTransform.gameObject.GetComponent<TweenColor>();
		component.from = Color.white;
		component.to = Color.white;
		component.duration = 0.1f;
		component.enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		if (bindingName == "itemicon")
		{
			value = ((this.itemClass != null) ? this.itemClass.GetIconName() : "");
			return true;
		}
		if (bindingName == "itemiconcolor")
		{
			Color32 color = Color.white;
			if (this.itemStack != null && this.itemStack.itemValue.type != 0)
			{
				color = this.itemClass.GetIconTint(this.itemStack.itemValue);
			}
			value = this.itemiconcolorFormatter.Format(color);
			return true;
		}
		if (bindingName == "itemcount")
		{
			value = "";
			if (this.ItemStack != null && this.itemStack.itemValue.type != 0)
			{
				value = ((this.ItemStack.count > 0) ? this.itemcountFormatter.Format(this.ItemStack.count) : "0");
			}
			return true;
		}
		if (bindingName == "itembackground")
		{
			value = "menu_empty";
			if (this.itemClass != null && this.itemStack.itemValue.type != 0)
			{
				value = "ui_game_popup";
			}
			return true;
		}
		if (!(bindingName == "itembackgroundcolor"))
		{
			return base.GetBindingValueInternal(ref value, bindingName);
		}
		value = "255, 255, 255, 0";
		if (this.itemClass != null && this.itemStack.itemValue.type != 0)
		{
			value = "255, 255, 255, 255";
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack itemStack = ItemStack.Empty;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass itemClass;

	public GameObject Item;

	public float TimeAdded;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor itemiconcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int> itemcountFormatter = new CachedStringFormatter<int>((int _i) => "+" + _i.ToString());
}
