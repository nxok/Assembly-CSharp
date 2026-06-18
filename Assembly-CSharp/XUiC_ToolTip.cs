using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ToolTip : XUiController
{
	public string ToolTip
	{
		get
		{
			return this.tooltip;
		}
		set
		{
			if (this.tooltip == value)
			{
				return;
			}
			this.tooltipOld = this.tooltip;
			if (value == null)
			{
				this.tooltip = "";
			}
			else if (value.Length > 0 && value[value.Length - 1] == '\n')
			{
				this.tooltip = value.Substring(0, value.Length - 1);
			}
			else
			{
				this.tooltip = value;
			}
			if (this.tooltip == "")
			{
				this.show = false;
				return;
			}
			base.ViewComponent.Position = this.xui.GetMouseXUiPosition() + this.PositionOffset;
			this.showDelay = Time.unscaledTime + this.ShowDelaySec;
		}
	}

	public override void Init()
	{
		base.Init();
		this.xui.ToolTipWindow = this;
		this.border = (XUiV_Sprite)base.GetChildById("sprBackgroundBorder").ViewComponent;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		this.xui.ToolTipWindow = null;
	}

	public override void Update(float _dt)
	{
		base.RefreshBindings();
		if (this.tooltip != "")
		{
			if (Time.unscaledTime > this.showDelay)
			{
				this.show = true;
			}
			Vector2i vector2i = this.xui.GetXUiScreenSize() / 2;
			if ((base.ViewComponent.Position + this.border.Size).x > vector2i.x)
			{
				base.ViewComponent.Position -= new Vector2i(this.border.Size.x, 0);
			}
			if ((base.ViewComponent.Position - this.border.Size).y < -vector2i.y)
			{
				base.ViewComponent.Position += new Vector2i(20, 20 + this.border.Size.y);
			}
		}
		base.Update(_dt);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (_bindingName == "screen_width")
		{
			_value = this.xui.GetXUiScreenSize().x.ToString();
			return true;
		}
		if (_bindingName == "text")
		{
			_value = ((this.tooltip.Length > 0) ? this.tooltip : this.tooltipOld);
			return true;
		}
		if (!(_bindingName == "show"))
		{
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
		_value = (this.show && GameManager.Instance.isAnyCursorWindowOpen(null)).ToString();
		return true;
	}

	public override bool ParseAttribute(string _name, string _value)
	{
		if (_name == "show_delay")
		{
			this.ShowDelaySec = (float)StringParsers.ParseSInt32(_value, 0, -1, NumberStyles.Integer);
			return true;
		}
		if (!(_name == "position_offset"))
		{
			return base.ParseAttribute(_name, _value);
		}
		this.PositionOffset = StringParsers.ParseVector2i(_value, ',');
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite border;

	[PublicizedFrom(EAccessModifier.Private)]
	public string tooltip = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string tooltipOld = "";

	public float ShowDelaySec = 0.3f;

	public Vector2i PositionOffset = new Vector2i(0, -36);

	[PublicizedFrom(EAccessModifier.Private)]
	public float showDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool show;
}
