using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SimpleButton : XUiController
{
	public event XUiEvent_OnPressEventHandler OnPressed;

	public new event XUiEvent_OnHoverEventHandler OnHovered;

	public string Text
	{
		get
		{
			XUiV_Label xuiV_Label = this.label;
			if (xuiV_Label == null)
			{
				return null;
			}
			return xuiV_Label.Text;
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
			return this.button.ToolTip;
		}
		set
		{
			if (this.button.ToolTip != value)
			{
				this.button.ToolTip = value;
			}
		}
	}

	public string DisabledToolTip
	{
		get
		{
			return this.button.DisabledToolTip;
		}
		set
		{
			if (this.button.DisabledToolTip != value)
			{
				this.button.DisabledToolTip = value;
			}
		}
	}

	public XUiV_Button Button
	{
		get
		{
			return this.button;
		}
	}

	[XuiXmlAttribute("button_enabled", false)]
	[XuiXmlBinding("enabled")]
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
			this.IsDirty = true;
		}
	}

	[XuiXmlBinding("selected")]
	public bool ButtonSelected
	{
		get
		{
			XUiV_Button xuiV_Button = this.Button;
			return xuiV_Button != null && xuiV_Button.Selected;
		}
	}

	[XuiXmlBinding("hovered")]
	public bool ButtonHovered
	{
		get
		{
			return this.isOver && this.isEnabled;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiV_Button xuiV_Button = base.ViewComponent as XUiV_Button;
		if (xuiV_Button != null)
		{
			this.button = xuiV_Button;
		}
		else
		{
			XUiController childById = base.GetChildById("clickable");
			XUiV_Button xuiV_Button2 = ((childById != null) ? childById.ViewComponent : null) as XUiV_Button;
			if (xuiV_Button2 == null)
			{
				throw new Exception(string.Concat(new string[]
				{
					"SimpleButton requires a Button view (itself or as a child named 'clickable'). WindowGroup: '",
					base.WindowGroup.Id,
					"', element: '",
					base.ViewComponent.ID,
					"'"
				}));
			}
			this.button = xuiV_Button2;
		}
		this.button.Controller.OnPress += this.Btn_OnPress;
		this.button.Controller.OnHover += this.Btn_OnHover;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Btn_OnHover(XUiController _sender, bool _isOver)
	{
		this.isOver = _isOver;
		this.IsDirty = true;
		XUiEvent_OnHoverEventHandler onHovered = this.OnHovered;
		if (onHovered == null)
		{
			return;
		}
		onHovered(this, _isOver);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Btn_OnPress(XUiController _sender, int _mouseButton)
	{
		if (!this.isEnabled)
		{
			return;
		}
		XUiEvent_OnPressEventHandler onPressed = this.OnPressed;
		if (onPressed != null)
		{
			onPressed(this, _mouseButton);
		}
		this.IsDirty = true;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty)
		{
			base.RefreshBindings();
			this.IsDirty = false;
		}
	}

	[XuiBindComponent("btnLabel", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Label label;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button button;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isEnabled = true;

	public string Tag;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOver;
}
