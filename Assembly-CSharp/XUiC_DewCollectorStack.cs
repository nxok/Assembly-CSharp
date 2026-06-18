using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DewCollectorStack : XUiC_RequiredItemStack
{
	public bool IsModded
	{
		get
		{
			return this.isModded;
		}
		set
		{
			this.isModded = value;
			this.currentFillColor = (this.isModded ? this.moddedFillColor : this.standardFillColor);
		}
	}

	public float FillAmount
	{
		get
		{
			return this.fillAmount;
		}
		set
		{
			if (this.fillAmount != value)
			{
				this.fillAmount = value;
			}
		}
	}

	public override void Init()
	{
		base.Init();
		XUiController childById = base.GetChildById("fillIcon");
		if (childById != null)
		{
			this.fillIcon = childById.ViewComponent as XUiV_Sprite;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsBlocked)
		{
			this.fillIcon.Color = this.blockedColor;
			return;
		}
		if (this.IsCurrentStack)
		{
			float num = Mathf.PingPong(Time.time, 0.5f);
			this.fillIcon.Color = Color.Lerp(Color.grey, this.currentFillColor, num * 4f);
		}
		else if (this.fillIcon.Color != this.disabledColor)
		{
			this.fillIcon.Color = this.disabledColor;
		}
		base.ViewComponent.IsNavigatable = !base.ItemStack.IsEmpty();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (_bindingName == "hasfill")
		{
			_value = (this.IsCurrentStack ? (this.FillAmount != -1f).ToString() : "false");
			return true;
		}
		if (_bindingName == "waterfill")
		{
			_value = (this.IsCurrentStack ? (this.FillAmount / this.MaxFill).ToString() : "0");
			return true;
		}
		if (_bindingName == "showitem")
		{
			_value = (!this.itemStack.IsEmpty()).ToString();
			return true;
		}
		if (_bindingName == "fillcolor")
		{
			if (this.IsBlocked)
			{
				_value = "255,0,0,255";
			}
			else if (this.isModded)
			{
				_value = this.moddedFillColorString;
			}
			else
			{
				_value = this.standardFillColorString;
			}
			return true;
		}
		if (!(_bindingName == "showempty"))
		{
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
		_value = this.itemStack.IsEmpty().ToString();
		return true;
	}

	public void SetStandardFillColor(string colorString)
	{
		this.standardFillColorString = colorString;
		this.standardFillColor = StringParsers.ParseColor32(this.standardFillColorString);
	}

	public void SetModdedFillColor(string colorString)
	{
		this.moddedFillColorString = colorString;
		this.moddedFillColor = StringParsers.ParseColor32(this.moddedFillColorString);
	}

	public override bool ParseAttribute(string name, string value)
	{
		bool flag = base.ParseAttribute(name, value);
		if (!flag)
		{
			if (!(name == "standard_fill_color"))
			{
				if (!(name == "modded_fill_color"))
				{
					return false;
				}
				this.moddedFillColorString = value;
				this.moddedFillColor = StringParsers.ParseColor32(this.moddedFillColorString);
			}
			else
			{
				this.standardFillColorString = value;
				this.standardFillColor = StringParsers.ParseColor32(this.standardFillColorString);
			}
			return true;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnHovered(bool _isOver)
	{
		if (this.itemStack.IsEmpty())
		{
			base.OnHovered(false);
			return;
		}
		base.OnHovered(_isOver);
	}

	public void SetSprite(string sprite)
	{
		this.fillIcon.SpriteName = sprite;
	}

	public bool IsCurrentStack;

	public bool IsBlocked;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isModded;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiV_Sprite fillIcon;

	[PublicizedFrom(EAccessModifier.Protected)]
	public TileEntityCollector tileEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public string standardFillColorString = "202,190,33,255";

	[PublicizedFrom(EAccessModifier.Private)]
	public Color standardFillColor = new Color32(202, 190, 33, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public string moddedFillColorString = "0,173,216,255";

	[PublicizedFrom(EAccessModifier.Private)]
	public Color moddedFillColor = new Color32(0, 173, 216, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public Color disabledColor = new Color32(64, 64, 64, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public Color blockedColor = new Color32(byte.MaxValue, 0, 0, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public Color currentFillColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public float fillAmount = -1f;

	public float MaxFill = 15f;
}
