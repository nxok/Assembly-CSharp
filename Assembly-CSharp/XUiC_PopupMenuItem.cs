using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PopupMenuItem : XUiController
{
	[XuiXmlBinding("menuicon")]
	public string Icon
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_PopupMenuItem.Entry entry = this.itemEntry;
			return ((entry != null) ? entry.IconName : null) ?? "";
		}
	}

	[XuiXmlBinding("menutext")]
	public string Text
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_PopupMenuItem.Entry entry = this.itemEntry;
			return ((entry != null) ? entry.Text : null) ?? "";
		}
	}

	[XuiXmlBinding("enabled")]
	public bool EntryEnabled
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_PopupMenuItem.Entry entry = this.itemEntry;
			return entry != null && entry.IsEnabled;
		}
	}

	[XuiXmlBinding("hovered")]
	public bool EntryHovered
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.isOver;
		}
	}

	[XuiXmlBinding("type")]
	public XUiC_PopupMenuItem.EEntryType EntryType
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_PopupMenuItem.Entry entry = this.itemEntry;
			if (entry == null)
			{
				return XUiC_PopupMenuItem.EEntryType.Button;
			}
			return entry.EntryType;
		}
	}

	public override void Init()
	{
		base.Init();
		this.label.Overflow = 2;
	}

	[XuiBindEvent("OnHoveredStateChanged", "slider")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void sliderHoveredChanged(XUiController _sender, bool _isOverMainArea, bool _isOverAnyPart)
	{
		this.OnHovered(this, _isOverAnyPart);
	}

	[XuiBindEvent("OnHover", null)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void OnHovered(XUiController _sender, bool _isOver)
	{
		this.isOver = _isOver;
		this.parentPopup.IsOver = _isOver;
		base.RefreshBindings();
	}

	[XuiBindEvent("OnPress", null)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void onPressed(XUiController _sender, int _mouseButton)
	{
		XUiC_PopupMenuItem.Entry entry = this.itemEntry;
		if (entry == null)
		{
			return;
		}
		if (entry.EntryType != XUiC_PopupMenuItem.EEntryType.Button)
		{
			return;
		}
		this.parentPopup.Close();
		if (entry.IsEnabled)
		{
			entry.HandleItemClicked();
		}
	}

	[XuiBindEvent("OnValueChanged", "slider")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void onValueChanged(XUiController _sender, double _oldValue, double _newValue)
	{
		if (this.itemEntry == null)
		{
			return;
		}
		if (this.itemEntry.EntryType != XUiC_PopupMenuItem.EEntryType.Slider)
		{
			return;
		}
		if (this.itemEntry.IsEnabled)
		{
			this.itemEntry.HandleValueChanged(_newValue);
		}
	}

	public int SetEntry(XUiC_PopupMenuItem.Entry _entry)
	{
		this.itemEntry = _entry;
		base.RefreshBindings();
		this.label.SetTextImmediately(this.label.Text);
		base.ViewComponent.IsVisible = _entry != null;
		if (_entry == null)
		{
			return 0;
		}
		if (_entry.EntryType == XUiC_PopupMenuItem.EEntryType.Slider)
		{
			this.slider.Value = _entry.Value;
			this.slider.Enabled = _entry.IsEnabled;
		}
		if (_entry.EntryType != XUiC_PopupMenuItem.EEntryType.Button)
		{
			return this.parentPopup.SliderMinWidth;
		}
		return (int)this.label.PrintedSize.x;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOver;

	[XuiBindParent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_PopupMenu parentPopup;

	[XuiBindComponent("lblText", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Label label;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxFloat slider;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PopupMenuItem.Entry itemEntry;

	public enum EEntryType
	{
		Button,
		Slider
	}

	public class Entry
	{
		public object Tag { get; set; }

		public event XUiC_PopupMenuItem.Entry.MenuItemClickedDelegate ItemClicked;

		public event XUiC_PopupMenuItem.Entry.MenuItemValueChangedDelegate ValueChanged;

		public void HandleItemClicked()
		{
			XUiC_PopupMenuItem.Entry.MenuItemClickedDelegate itemClicked = this.ItemClicked;
			if (itemClicked == null)
			{
				return;
			}
			itemClicked(this);
		}

		public void HandleValueChanged(double _newValue)
		{
			this.Value = _newValue;
			XUiC_PopupMenuItem.Entry.MenuItemValueChangedDelegate valueChanged = this.ValueChanged;
			if (valueChanged == null)
			{
				return;
			}
			valueChanged(this, this.Value);
		}

		public Entry(string _text, string _iconName, bool _isEnabled = true, object _tag = null, XUiC_PopupMenuItem.Entry.MenuItemClickedDelegate _handler = null)
		{
			this.Text = _text;
			this.IconName = _iconName;
			this.IsEnabled = _isEnabled;
			this.Tag = _tag;
			this.EntryType = XUiC_PopupMenuItem.EEntryType.Button;
			if (_handler != null)
			{
				this.ItemClicked += _handler;
			}
		}

		public Entry(string _text, string _iconName, double _initialValue, bool _isEnabled = true, object _tag = null, XUiC_PopupMenuItem.Entry.MenuItemValueChangedDelegate _handler = null)
		{
			this.Text = _text;
			this.IconName = _iconName;
			this.IsEnabled = _isEnabled;
			this.Tag = _tag;
			this.EntryType = XUiC_PopupMenuItem.EEntryType.Slider;
			this.Value = _initialValue;
			if (_handler != null)
			{
				this.ValueChanged += _handler;
			}
		}

		public readonly string Text;

		public readonly string IconName;

		public readonly bool IsEnabled;

		public double Value;

		public readonly XUiC_PopupMenuItem.EEntryType EntryType;

		public delegate void MenuItemClickedDelegate(XUiC_PopupMenuItem.Entry _entry);

		public delegate void MenuItemValueChangedDelegate(XUiC_PopupMenuItem.Entry _entry, double _newValue);
	}
}
