using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ToggleButton : XUiController
{
	public event XUiEvent_ToggleButtonValueChanged OnValueChanged;

	[XuiXmlAttribute("enabled_font_color", false)]
	public Color EnabledLabelColor { get; set; }

	[XuiXmlAttribute("disabled_font_color", false)]
	public Color DisabledLabelColor { get; set; }

	public string Label
	{
		get
		{
			XUiV_Label xuiV_Label = this.label;
			return ((xuiV_Label != null) ? xuiV_Label.Text : null) ?? "";
		}
		set
		{
			if (this.label != null)
			{
				this.label.Text = value;
			}
		}
	}

	public string Tooltip
	{
		get
		{
			XUiV_Button xuiV_Button = this.button;
			return ((xuiV_Button != null) ? xuiV_Button.ToolTip : null) ?? "";
		}
		set
		{
			if (this.button != null)
			{
				this.button.ToolTip = value;
			}
		}
	}

	[XuiXmlAttribute("toggle_value", false)]
	[XuiXmlBinding("value")]
	public bool Value
	{
		get
		{
			return this.value;
		}
		set
		{
			if (value == this.value)
			{
				return;
			}
			this.value = value;
			this.IsDirty = true;
		}
	}

	public bool Enabled
	{
		get
		{
			return this.isEnabled;
		}
		set
		{
			if (value == this.isEnabled && (this.button == null || value == this.button.Enabled))
			{
				return;
			}
			this.isEnabled = value;
			if (this.button != null)
			{
				this.button.Enabled = value;
			}
			if (this.label != null)
			{
				this.label.Color = (value ? this.EnabledLabelColor : this.DisabledLabelColor);
			}
			this.IsDirty = true;
		}
	}

	public override void Init()
	{
		base.Init();
		this.button.Controller.OnPress += this.Btn_OnPress;
		if (this.label != null)
		{
			this.label.Color = this.EnabledLabelColor;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Btn_OnPress(XUiController _sender, int _mouseButton)
	{
		if (!this.isEnabled)
		{
			return;
		}
		this.value = !this.value;
		this.IsDirty = true;
		XUiEvent_ToggleButtonValueChanged onValueChanged = this.OnValueChanged;
		if (onValueChanged == null)
		{
			return;
		}
		onValueChanged(this, this.value);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		base.handleDirtyUpdateDefault();
	}

	[XuiBindComponent("btnLabel", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Label label;

	[XuiBindComponent("clickable", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Button button;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool value;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isEnabled = true;

	public string Tag;
}
