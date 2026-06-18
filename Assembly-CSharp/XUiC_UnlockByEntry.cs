using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_UnlockByEntry : XUiController
{
	public Recipe Recipe { get; set; }

	public RecipeUnlockData UnlockData
	{
		get
		{
			return this.unlockData;
		}
		set
		{
			this.unlockData = value;
			this.IsDirty = true;
		}
	}

	public override void Init()
	{
		base.Init();
		this.IsDirty = false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		bool flag = this.UnlockData != null;
		if (bindingName == "name")
		{
			if (flag)
			{
				value = this.UnlockData.GetName();
			}
			else
			{
				value = "";
			}
			return true;
		}
		if (bindingName == "itemicon")
		{
			if (flag)
			{
				value = this.UnlockData.GetIcon();
			}
			else
			{
				value = "";
			}
			return true;
		}
		if (bindingName == "itemiconatlas")
		{
			if (flag)
			{
				value = this.UnlockData.GetIconAtlas();
			}
			else
			{
				value = "UIAtlas";
			}
			return true;
		}
		if (bindingName == "itemicontint")
		{
			Color32 color = Color.white;
			if (flag)
			{
				color = this.UnlockData.GetItemTint();
			}
			value = this.itemicontintcolorFormatter.Format(color);
			return true;
		}
		if (!(bindingName == "level"))
		{
			return base.GetBindingValueInternal(ref value, bindingName);
		}
		if (flag)
		{
			value = this.UnlockData.GetLevel(this.xui.playerUI.entityPlayer, this.Recipe.GetOutputItemClass().Name);
		}
		else
		{
			value = "";
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnCountChanged(XUiController _sender, OnCountChangedEventArgs _e)
	{
		this.IsDirty = true;
	}

	public override void Update(float _dt)
	{
		if (this.IsDirty)
		{
			base.RefreshBindings();
			base.ViewComponent.IsVisible = true;
			this.IsDirty = false;
		}
		base.Update(_dt);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public RecipeUnlockData unlockData;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor itemicontintcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt havecountFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt needcountFormatter = new CachedStringFormatterInt();
}
