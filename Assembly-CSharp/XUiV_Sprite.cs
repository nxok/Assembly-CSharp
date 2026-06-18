using System;
using UnityEngine;

public class XUiV_Sprite : XUiV_ImageBased
{
	public UISprite Sprite
	{
		get
		{
			return this.sprite;
		}
	}

	[XuiXmlAttribute("atlas", false)]
	public string UIAtlas
	{
		get
		{
			return this.uiAtlas;
		}
		set
		{
			if (this.uiAtlas == value)
			{
				return;
			}
			this.uiAtlas = value;
			this.uiAtlasChanged = true;
			base.SetDirty();
		}
	}

	[XuiXmlAttribute("sprite", false)]
	public string SpriteName
	{
		get
		{
			return this.spriteName;
		}
		set
		{
			if (this.spriteName == value)
			{
				return;
			}
			this.spriteName = value;
			base.SetDirty();
		}
	}

	[XuiXmlAttribute("color", false)]
	public Color Color
	{
		get
		{
			return this.color;
		}
		set
		{
			if (this.color == value)
			{
				return;
			}
			this.color = value;
			base.SetDirty();
		}
	}

	[XuiXmlAttribute("filldirection", false)]
	public UIBasicSprite.FillDirection FillDirection
	{
		get
		{
			return this.fillDirection;
		}
		set
		{
			if (this.fillDirection == value)
			{
				return;
			}
			this.fillDirection = value;
			base.SetDirty();
		}
	}

	[XuiXmlAttribute("fillspritepad", false)]
	public bool FillSpritePad
	{
		get
		{
			return this.fillSpritePad;
		}
		set
		{
			if (this.fillSpritePad == value)
			{
				return;
			}
			this.fillSpritePad = value;
			base.SetDirty();
		}
	}

	[XuiXmlAttribute("fillinvert", false)]
	public bool FillInvert
	{
		get
		{
			return this.fillInvert;
		}
		set
		{
			if (this.fillInvert == value)
			{
				return;
			}
			this.fillInvert = value;
			base.SetDirty();
		}
	}

	[XuiXmlAttribute("fill", false)]
	public float Fill
	{
		get
		{
			return this.fillAmount;
		}
		set
		{
			if (Mathf.Approximately(this.fillAmount, value))
			{
				return;
			}
			if ((double)Math.Abs((value - this.fillAmount) / value) < 0.005)
			{
				return;
			}
			this.fillAmount = Mathf.Clamp01(value);
			base.SetDirty();
		}
	}

	public XUiV_Sprite(XUi _xui, string _id)
		: base(_xui, _id)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void createComponents(GameObject _go)
	{
		_go.AddComponent<UISprite>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void captureComponents()
	{
		base.captureComponents();
		this.widget = (this.sprite = this.uiTransform.GetComponent<UISprite>());
	}

	public override void InitView()
	{
		base.InitView();
		this.updateData();
	}

	public override void SetDefaults(XUiController _parent)
	{
		base.SetDefaults(_parent);
		base.FillCenter = true;
		this.Type = 0;
		this.FillDirection = 0;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateData()
	{
		this.applyAtlasAndSprite(false);
		if (this.fillSpritePad)
		{
			XUiUtils.ApplyFillPaddedSprite(this.sprite, this.spriteName);
		}
		this.sprite.fixedAspect = this.keepSourceAspectRatio;
		this.sprite.color = base.opacityModColor(this.color);
		if (this.gradientStart != null)
		{
			this.sprite.gradientTop = this.gradientStart.Value;
			this.sprite.applyGradient = true;
		}
		if (this.gradientEnd != null)
		{
			this.sprite.gradientBottom = this.gradientEnd.Value;
			this.sprite.applyGradient = true;
		}
		if (this.sprite.centerType != (this.fillCenter ? 1 : 0))
		{
			this.sprite.centerType = (this.fillCenter ? 1 : 0);
		}
		this.sprite.fillDirection = this.fillDirection;
		this.sprite.invert = this.fillInvert;
		if (!Mathf.Approximately(this.sprite.fillAmount, this.fillAmount))
		{
			this.sprite.fillAmount = this.fillAmount;
		}
		this.sprite.type = this.type;
		this.sprite.flip = this.flip;
		base.updateData();
	}

	public void SetSpriteImmediately(string _spriteName)
	{
		this.spriteName = _spriteName;
		this.applyAtlasAndSprite(true);
	}

	public void SetColorImmediately(Color _color)
	{
		if (this.sprite != null)
		{
			this.sprite.color = _color;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool applyAtlasAndSprite(bool _force = false)
	{
		if (this.sprite == null)
		{
			return false;
		}
		if (!_force && this.sprite.spriteName != null && this.sprite.spriteName == this.spriteName && this.sprite.atlas != null && !this.uiAtlasChanged)
		{
			return false;
		}
		this.uiAtlasChanged = false;
		this.sprite.atlas = this.xui.GetAtlasByName(this.UIAtlas, this.spriteName);
		this.sprite.spriteName = this.spriteName;
		return true;
	}

	[XuiXmlAttribute("gradient_start", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeGradientStart(Color _value)
	{
		this.gradientStart = new Color?(_value);
	}

	[XuiXmlAttribute("gradient_end", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeGradientEnd(Color _value)
	{
		this.gradientEnd = new Color?(_value);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string uiAtlas = string.Empty;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool uiAtlasChanged;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string spriteName = string.Empty;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color color = Color.white;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color? gradientStart;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color? gradientEnd;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UIBasicSprite.FillDirection fillDirection;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool fillSpritePad;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool fillInvert;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float fillAmount;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UISprite sprite;
}
