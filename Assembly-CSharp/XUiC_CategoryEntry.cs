using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_CategoryEntry : XUiController
{
	public XUiC_CategoryList CategoryList { get; set; }

	public string CategoryName
	{
		get
		{
			return this.categoryName;
		}
		set
		{
			this.categoryName = value;
			this.IsDirty = true;
		}
	}

	public string CategoryDisplayName
	{
		get
		{
			return this.categoryDisplayName;
		}
		set
		{
			this.categoryDisplayName = value;
			this.IsDirty = true;
		}
	}

	public string SpriteName
	{
		get
		{
			return this.spriteName;
		}
		set
		{
			this.spriteName = value;
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
			this.button.Selected = this.selected;
		}
	}

	public override void Init()
	{
		base.Init();
		this.button = (XUiV_Button)base.ViewComponent;
		base.OnPress += this.XUiC_CategoryEntry_OnPress;
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void XUiC_CategoryEntry_OnPress(XUiController _sender, int _mouseButton)
	{
		if (this.spriteName != string.Empty)
		{
			if (this.CategoryList.CurrentCategory == this && this.CategoryList.AllowUnselect)
			{
				this.CategoryList.CurrentCategory = null;
			}
			else
			{
				this.CategoryList.CurrentCategory = this;
			}
			this.CategoryList.HandleCategoryChanged();
		}
	}

	public void PlayButtonClickSound()
	{
		this.button.PlayClickSound();
	}

	public override void Update(float _dt)
	{
		if (this.IsDirty)
		{
			base.ViewComponent.IsNavigatable = !string.IsNullOrEmpty(this.SpriteName);
			base.RefreshBindings();
			this.IsDirty = false;
		}
		base.Update(_dt);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		if (bindingName == "categoryicon")
		{
			value = this.spriteName;
			return true;
		}
		if (!(bindingName == "categorydisplayname"))
		{
			return base.GetBindingValueInternal(ref value, bindingName);
		}
		value = this.categoryDisplayName;
		return true;
	}

	public override bool ParseAttribute(string _name, string _value)
	{
		if (_name == "categoryname")
		{
			if (!string.IsNullOrEmpty(_value))
			{
				this.CategoryName = _value;
			}
			return true;
		}
		if (_name == "spritename")
		{
			if (!string.IsNullOrEmpty(_value))
			{
				this.SpriteName = _value;
			}
			return true;
		}
		if (_name == "displayname")
		{
			if (!string.IsNullOrEmpty(_value))
			{
				this.CategoryDisplayName = _value;
			}
			return true;
		}
		if (!(_name == "displayname_key"))
		{
			return base.ParseAttribute(_name, _value);
		}
		if (!string.IsNullOrEmpty(_value))
		{
			this.CategoryDisplayName = Localization.Get(_value, false, null);
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string categoryName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string categoryDisplayName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string spriteName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool selected;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button button;
}
