using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TabSelectorTab : XUiController
{
	[XuiXmlAttribute("tab_key", false)]
	public string TabKey
	{
		get
		{
			return this.tabKey;
		}
		set
		{
			if (this.tabKey == value)
			{
				return;
			}
			this.tabKey = value;
			this.TabHeaderText = Localization.Get(this.tabKey, false, null);
			this.IsDirty = true;
		}
	}

	[XuiXmlAttribute("tab_caption", false)]
	public string TabHeaderText
	{
		get
		{
			return this.tabHeaderText;
		}
		set
		{
			if (this.tabHeaderText == value)
			{
				return;
			}
			this.tabHeaderText = value;
			if (this.TabButton != null)
			{
				this.TabButton.IsDirty = true;
			}
			this.IsDirty = true;
		}
	}

	[XuiXmlAttribute("tab_sprite", false)]
	public string TabHeaderSprite
	{
		get
		{
			return this.tabHeaderSprite;
		}
		set
		{
			if (this.tabHeaderSprite == value)
			{
				return;
			}
			this.tabHeaderSprite = value;
			if (this.TabButton != null)
			{
				this.TabButton.IsDirty = true;
			}
			this.IsDirty = true;
		}
	}

	[XuiXmlAttribute("tab_visible", false)]
	public bool TabVisible
	{
		get
		{
			return this.tabVisible;
		}
		set
		{
			if (value == this.tabVisible)
			{
				return;
			}
			this.tabVisible = value;
			XUiC_TabSelectorButton tabButton = this.TabButton;
			if (tabButton != null)
			{
				tabButton.UpdateVisibilityState();
			}
			XUiC_TabSelector xuiC_TabSelector = this.parentSelector;
			if (xuiC_TabSelector != null)
			{
				xuiC_TabSelector.TabVisibilityChanged(this, this.tabVisible);
			}
			this.IsDirty = true;
		}
	}

	[XuiXmlBinding("tab_selected")]
	public bool TabSelected
	{
		get
		{
			return this.tabSelected;
		}
		set
		{
			if (value == this.tabSelected)
			{
				return;
			}
			this.tabSelected = value;
			this.TabButton.UpdateSelectionState();
			base.ViewComponent.IsVisible = this.tabSelected;
			this.parentSelector.SelectedTab = this;
			this.IsDirty = true;
		}
	}

	[XuiXmlBinding("tab_highlight")]
	public bool TabHighlight
	{
		get
		{
			return this.tabHighlight;
		}
		set
		{
			if (value == this.tabHighlight)
			{
				return;
			}
			this.tabHighlight = value;
			this.TabButton.IsDirty = true;
			this.IsDirty = true;
		}
	}

	[XuiXmlAttribute("tab_caption_key", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeTabCaptionKey(string _value)
	{
		this.TabHeaderText = Localization.Get(_value, false, null);
	}

	public override void Init()
	{
		base.Init();
		this.parentSelector = base.GetParentByType<XUiC_TabSelector>();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		base.handleDirtyUpdateDefault();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TabSelector parentSelector;

	public XUiC_TabSelectorButton TabButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public string tabKey;

	[PublicizedFrom(EAccessModifier.Private)]
	public string tabHeaderText;

	[PublicizedFrom(EAccessModifier.Private)]
	public string tabHeaderSprite;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool tabVisible = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool tabSelected;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool tabHighlight;
}
