using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ButtonSelectable : XUiC_Button
{
	public event XUiC_ButtonSelectable.ButtonSelectedDelegate OnButtonSelected;

	[XuiXmlBinding("btn_selected")]
	[XuiXmlAttribute("selected", false)]
	public bool IsSelected
	{
		get
		{
			return this.selected;
		}
		set
		{
			if (this.selected == value)
			{
				return;
			}
			this.selected = value;
			this.IsDirty = true;
			XUiC_ButtonSelectable.ButtonSelectedDelegate onButtonSelected = this.OnButtonSelected;
			if (onButtonSelected == null)
			{
				return;
			}
			onButtonSelected(this, this.selected);
		}
	}

	[XuiXmlAttribute("deselect_on_disable", false)]
	public bool DeselectOnDisable
	{
		get
		{
			return this.deselectOnDisable;
		}
		set
		{
			if (this.deselectOnDisable == value)
			{
				return;
			}
			this.deselectOnDisable = value;
			this.IsDirty = true;
		}
	}

	[XuiXmlAttribute("auto_toggle_selected", false)]
	public bool AutoToggleSelected { get; set; } = true;

	[XuiBindEvent("OnEnabled", null)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void Event_OnEnabled(XUiController _sender, bool _enabled)
	{
		if (!_enabled && this.deselectOnDisable)
		{
			this.IsSelected = false;
		}
	}

	public override void Pressed(int _mouseButton)
	{
		if (this.AutoToggleSelected)
		{
			this.IsSelected = !this.IsSelected;
		}
		base.Pressed(_mouseButton);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool selected;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool deselectOnDisable;

	public delegate void ButtonSelectedDelegate(XUiC_ButtonSelectable _sender, bool _isSelected);
}
