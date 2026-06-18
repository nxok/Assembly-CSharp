using System;
using System.Runtime.CompilerServices;
using UnityEngine.Scripting;

[Preserve]
public abstract class XUiC_OptionEntryAbs : XUiController
{
	public event Action<XUiC_OptionEntryAbs> ValueChanged;

	[PublicizedFrom(EAccessModifier.Protected)]
	public void invokeValueChanged()
	{
		Action<XUiC_OptionEntryAbs> valueChanged = this.ValueChanged;
		if (valueChanged == null)
		{
			return;
		}
		valueChanged(this);
	}

	[XuiXmlAttribute("prefname", false)]
	public string PrefName
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("apply_immediately", false)]
	public bool ApplyImmediately
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get;
		[PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[XuiXmlAttribute("apply_defaults", false)]
	public bool ApplyDefaults
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get;
		[PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[XuiXmlBinding("option_changed")]
	public abstract bool IsChanged { get; }

	[XuiXmlBinding("option_is_default")]
	public abstract bool IsDefault { get; }

	[XuiXmlBinding("option_hovered")]
	public bool OptionHovered
	{
		get
		{
			return this.optionHovered;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (this.optionHovered == value)
			{
				return;
			}
			this.optionHovered = value;
			this.IsDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void initCurrentValue();

	public abstract void DiscardCurrentChange();

	public abstract void ApplySelection();

	public abstract void ResetToDefault();

	public override void OnOpen()
	{
		base.OnOpen();
		this.initHovering();
		this.initCurrentValue();
	}

	public override void OnClose()
	{
		base.OnClose();
		this.DiscardCurrentChange();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initHovering()
	{
		if (this.hoveringInitialized)
		{
			return;
		}
		this.hoveringInitialized = true;
		this.<initHovering>g__ApplyEventToChildren|33_0(this);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty)
		{
			this.IsDirty = false;
			base.RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnHoveredElementChanged(XUiController _sender, bool _state)
	{
		this.OptionHovered = _state;
		this.parentOptionsDialog.HoveredOption = (_state ? this : null);
	}

	public static void DebugLog(string _s)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_OptionEntryAbs()
	{
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public void <initHovering>g__ApplyEventToChildren|33_0(XUiController _controller)
	{
		if (_controller.ViewComponent.HasAnyEvent)
		{
			_controller.ViewComponent.EventOnHover = true;
			_controller.OnHover += this.OnHoveredElementChanged;
		}
		foreach (XUiController xuiController in _controller.Children)
		{
			this.<initHovering>g__ApplyEventToChildren|33_0(xuiController);
		}
	}

	[XuiBindParent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_OptionsDialogBase parentOptionsDialog;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly XUiC_ComboBoxBase comboGeneric;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool optionHovered;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hoveringInitialized;
}
