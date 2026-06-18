using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TabSelectorButton : XUiController
{
	public XUiC_TabSelectorTab Tab
	{
		get
		{
			return this.tab;
		}
		set
		{
			if (this.tab == value)
			{
				return;
			}
			if (this.tab != null)
			{
				this.tab.CustomAttributes.EntryModified -= this.tabCustomAttributesListener;
			}
			this.tab = value;
			if (this.tab != null)
			{
				this.tab.CustomAttributes.EntryModified += this.tabCustomAttributesListener;
			}
			base.RefreshBindings();
		}
	}

	[XuiXmlBinding("tab_selected")]
	public bool TabSelected
	{
		get
		{
			XUiC_TabSelectorTab xuiC_TabSelectorTab = this.Tab;
			return xuiC_TabSelectorTab != null && xuiC_TabSelectorTab.TabSelected;
		}
	}

	[XuiXmlBinding("tab_name")]
	public string TabName
	{
		get
		{
			XUiC_TabSelectorTab xuiC_TabSelectorTab = this.Tab;
			return ((xuiC_TabSelectorTab != null) ? xuiC_TabSelectorTab.TabKey : null) ?? "";
		}
	}

	[XuiXmlBinding("tab_name_localized")]
	public string TabNameLocalized
	{
		get
		{
			XUiC_TabSelectorTab xuiC_TabSelectorTab = this.Tab;
			return ((xuiC_TabSelectorTab != null) ? xuiC_TabSelectorTab.TabHeaderText : null) ?? "";
		}
	}

	[XuiXmlBinding("tab_sprite")]
	public string TabSprite
	{
		get
		{
			XUiC_TabSelectorTab xuiC_TabSelectorTab = this.Tab;
			return ((xuiC_TabSelectorTab != null) ? xuiC_TabSelectorTab.TabHeaderSprite : null) ?? "";
		}
	}

	[XuiXmlBinding("tab_highlight")]
	public bool TabHighlight
	{
		get
		{
			XUiC_TabSelectorTab xuiC_TabSelectorTab = this.Tab;
			return xuiC_TabSelectorTab != null && xuiC_TabSelectorTab.TabHighlight;
		}
	}

	[XuiXmlBinding("tab_custom_attributes")]
	public ObservableDictionary<string, object> TabCustomAttributes
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_TabSelectorTab xuiC_TabSelectorTab = this.Tab;
			return ((xuiC_TabSelectorTab != null) ? xuiC_TabSelectorTab.CustomAttributes : null) ?? this.CustomAttributes;
		}
	}

	public override void Init()
	{
		base.Init();
		this.OuterDimensionsTransform = base.ViewComponent.UiTransform.FindRecursive("border");
		if (this.simpleButton != null)
		{
			this.simpleButton.OnPressed += delegate(XUiController _, int _)
			{
				this.parentSelector.TabButtonClicked(this);
			};
			return;
		}
		this.interactionView = this.findClickableChild(this);
		if (this.interactionView == null)
		{
			Log.Error("[XUi] TabSelectorButton without SimpleButton or other view with 'on_press=true' in windowGroup '" + this.windowGroup.Id + "'");
			return;
		}
		this.interactionView.Controller.OnPress += delegate(XUiController _, int _)
		{
			this.parentSelector.TabButtonClicked(this);
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiView findClickableChild(XUiController _controller)
	{
		if (_controller.ViewComponent.EventOnPress)
		{
			return _controller.ViewComponent;
		}
		foreach (XUiController xuiController in _controller.Children)
		{
			XUiView xuiView = this.findClickableChild(xuiController);
			if (xuiView != null)
			{
				return xuiView;
			}
		}
		return null;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		base.handleDirtyUpdateDefault();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void tabCustomAttributesListener(object _sender, DictionaryChangedEventArgs<string, object> _e)
	{
		base.RefreshBindings();
	}

	public void UpdateSelectionState()
	{
		if (this.simpleButton != null)
		{
			this.simpleButton.Button.Selected = this.Tab.TabSelected;
		}
		base.RefreshBindings();
	}

	public void UpdateVisibilityState()
	{
		base.ViewComponent.IsVisible = this.Tab.TabVisible;
	}

	public void PlayClickSound()
	{
		if (this.simpleButton != null)
		{
			this.simpleButton.Button.PlayClickSound();
			return;
		}
		if (this.interactionView != null)
		{
			this.interactionView.PlayClickSound();
		}
	}

	[XuiBindParent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_TabSelector parentSelector;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TabSelectorTab tab;

	[XuiBindComponent(false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_SimpleButton simpleButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiView interactionView;

	public Transform OuterDimensionsTransform;
}
