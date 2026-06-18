using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_Button : XUiController
{
	public override void Init()
	{
		base.Init();
		if (!base.ViewComponent.HasAnyEvent)
		{
			throw new Exception("[XUi] Button requires the view to listen to events. Hierarchy: " + base.GetXuiHierarchy());
		}
		this.CustomAttributes.EntryModified += this.OnCustomAttributeModified;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnCustomAttributeModified(object _sender, DictionaryChangedEventArgs<string, object> _e)
	{
		this.IsDirty = true;
	}

	[XuiBindEvent("OnInteraction", null)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void OnAnyEvent(XUiController _sender, EXUiControllerInteractionType _type)
	{
		this.IsDirty = true;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.HoldToActivate && this.isPressed)
		{
			this.HoldToActivateElapsed += Time.deltaTime;
		}
		base.handleDirtyUpdateDefault();
	}

	public override void Pressed(int _mouseButton)
	{
		if (this.HoldToActivate)
		{
			return;
		}
		base.Pressed(_mouseButton);
	}

	[XuiBindEvent("OnVisiblity", null)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void onVisibilityChanged(XUiController _sender, bool _visibleSelf, bool _visibleInScene)
	{
		if (_visibleInScene)
		{
			return;
		}
		this.isPressed = false;
		this.HoldToActivateElapsed = 0f;
	}

	[XuiBindEvent("OnMouseUpDown", null)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void onMouse(XUiController _sender, bool _mouseDown)
	{
		if (!this.HoldToActivate)
		{
			return;
		}
		this.isPressed = _mouseDown;
		if (_mouseDown)
		{
			return;
		}
		if (this.HoldToActivateState >= 1f)
		{
			base.ViewComponent.PlayClickSound();
			base.Pressed(-1);
		}
		this.HoldToActivateElapsed = 0f;
	}

	public bool HoldToActivate
	{
		get
		{
			return this.holdToActivateTime > 0f;
		}
	}

	[XuiXmlAttribute("hold_to_activate_time", false)]
	public float HoldToActivateTime
	{
		get
		{
			return this.holdToActivateTime;
		}
		set
		{
			if (Mathf.Approximately(this.holdToActivateTime, value))
			{
				return;
			}
			this.holdToActivateTime = value;
			this.IsDirty = true;
		}
	}

	public float HoldToActivateElapsed
	{
		get
		{
			return this.holdToActivateElapsed;
		}
		set
		{
			if (Mathf.Approximately(this.holdToActivateElapsed, value))
			{
				return;
			}
			this.holdToActivateElapsed = value;
			this.IsDirty = true;
		}
	}

	[XuiXmlBinding("hold_to_activate_state")]
	public float HoldToActivateState
	{
		get
		{
			if (!this.HoldToActivate)
			{
				return 0f;
			}
			return Mathf.Clamp01(this.holdToActivateElapsed / this.holdToActivateTime);
		}
	}

	[XuiXmlBinding("btn_enabled")]
	public bool BindingEnabled
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.viewComponent.Enabled;
		}
	}

	[XuiXmlBinding("btn_hovered")]
	public bool BindingHovered
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.viewComponent.IsHovered;
		}
	}

	[XuiXmlBinding("btn_attributes")]
	public ObservableDictionary<string, object> BindingCustomAttributes
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.CustomAttributes;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isPressed;

	[PublicizedFrom(EAccessModifier.Private)]
	public float holdToActivateTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float holdToActivateElapsed;
}
