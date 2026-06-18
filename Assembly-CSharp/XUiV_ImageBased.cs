using System;
using UnityEngine;

public abstract class XUiV_ImageBased : XUiView_WidgetBased
{
	[XuiXmlAttribute("keepsourceaspectratio", false)]
	public bool KeepSourceAspectRatio
	{
		get
		{
			return this.keepSourceAspectRatio;
		}
		set
		{
			if (value == this.keepSourceAspectRatio)
			{
				return;
			}
			this.keepSourceAspectRatio = value;
			base.SetDirty();
		}
	}

	[XuiXmlAttribute("type", false)]
	public virtual UIBasicSprite.Type Type
	{
		get
		{
			return this.type;
		}
		set
		{
			if (this.type == value)
			{
				return;
			}
			this.type = value;
			base.SetDirty();
		}
	}

	[XuiXmlAttribute("fillcenter", false)]
	public bool FillCenter
	{
		get
		{
			return this.fillCenter;
		}
		set
		{
			if (this.fillCenter == value)
			{
				return;
			}
			this.fillCenter = value;
			base.SetDirty();
		}
	}

	[XuiXmlAttribute("flip", false)]
	public UIBasicSprite.Flip Flip
	{
		get
		{
			return this.flip;
		}
		set
		{
			if (this.flip == value)
			{
				return;
			}
			this.flip = value;
			base.SetDirty();
		}
	}

	[XuiXmlAttribute("globalopacitymod", false)]
	public float GlobalOpacityModifier
	{
		get
		{
			return this.globalOpacityModifier;
		}
		set
		{
			if (Mathf.Approximately(this.globalOpacityModifier, value))
			{
				return;
			}
			this.globalOpacityModifier = value;
			base.SetDirty();
		}
	}

	[XuiXmlAttribute("foregroundlayer", false)]
	public bool ForegroundLayer
	{
		get
		{
			return this.foregroundLayer;
		}
		set
		{
			if (this.foregroundLayer == value)
			{
				return;
			}
			this.foregroundLayer = value;
			base.SetDirty();
		}
	}

	public float GlobalOpacitySetting
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (!this.foregroundLayer)
			{
				return this.xui.BackgroundGlobalOpacity;
			}
			return this.xui.ForegroundGlobalOpacity;
		}
	}

	public override void Update(float _dt)
	{
		float globalOpacitySetting = this.GlobalOpacitySetting;
		if (!Mathf.Approximately(this.previousGlobalOpacity, globalOpacitySetting))
		{
			this.previousGlobalOpacity = globalOpacitySetting;
			base.SetDirty();
		}
		base.Update(_dt);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color opacityModColor(Color _origColor)
	{
		if (this.globalOpacityModifier == 0f)
		{
			return _origColor;
		}
		float globalOpacitySetting = this.GlobalOpacitySetting;
		if (globalOpacitySetting >= 0.999f)
		{
			return _origColor;
		}
		float num = Mathf.Clamp01(_origColor.a * this.globalOpacityModifier * globalOpacitySetting);
		return new Color(_origColor.r, _origColor.g, _origColor.b, num);
	}

	public XUiV_ImageBased(XUi _xui, string _id)
		: base(_xui, _id)
	{
	}

	public override void SetDefaults(XUiController _parent)
	{
		base.SetDefaults(_parent);
		this.FillCenter = true;
		this.Type = 0;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool keepSourceAspectRatio;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UIBasicSprite.Type type;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UIBasicSprite.Flip flip;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool fillCenter;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool foregroundLayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public float globalOpacityModifier = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float previousGlobalOpacity;
}
