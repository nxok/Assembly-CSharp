using System;
using System.Collections;
using Platform;
using UnityEngine;

public class XUiV_Label : XUiV_LabelBase
{
	[XuiXmlAttribute("overflow", false)]
	public UILabel.Overflow Overflow
	{
		get
		{
			return this.overflow;
		}
		set
		{
			this.overflow = value;
			base.SetDirty();
		}
	}

	[XuiXmlAttribute("overflow_height", false)]
	public int OverflowHeight
	{
		get
		{
			return this.overflowHeight;
		}
		set
		{
			this.overflowHeight = value;
			base.SetDirty();
		}
	}

	[XuiXmlAttribute("overflow_width", false)]
	public int OverflowWidth
	{
		get
		{
			return this.overflowWidth;
		}
		set
		{
			this.overflowWidth = value;
			base.SetDirty();
		}
	}

	[XuiXmlAttribute("overflow_ellipsis", false)]
	public bool OverflowEllipsis
	{
		get
		{
			return this.overflowEllipsis;
		}
		set
		{
			this.overflowEllipsis = value;
			base.SetDirty();
		}
	}

	[XuiXmlAttribute("parse_actions", false)]
	public bool ParseActions
	{
		get
		{
			return this.parseActions;
		}
		set
		{
			if (this.parseActions == value)
			{
				return;
			}
			this.parseActions = value;
			base.SetDirty();
		}
	}

	[XuiXmlAttribute("actions_default_format", false)]
	public string ActionsDefaultFormat
	{
		get
		{
			return this.actionsDefaultFormat;
		}
		set
		{
			if (this.actionsDefaultFormat == value)
			{
				return;
			}
			this.actionsDefaultFormat = value;
			base.SetDirty();
		}
	}

	[XuiXmlAttribute("force_input_style", false)]
	public XUiUtils.ForceLabelInputStyle ForceInputStyle
	{
		get
		{
			return this.forceInputStyle;
		}
		set
		{
			if (this.forceInputStyle == value)
			{
				return;
			}
			this.forceInputStyle = value;
			base.SetDirty();
		}
	}

	[XuiXmlAttribute("text", false)]
	public string Text
	{
		get
		{
			return this.text;
		}
		set
		{
			if (this.text == value)
			{
				return;
			}
			this.text = value ?? "";
			if (this.ellipsisAnimator == null)
			{
				base.SetDirty();
				this.bUpdateText = true;
				return;
			}
			this.ellipsisAnimator.SetBaseString(value, TextEllipsisAnimator.AnimationMode.All);
		}
	}

	[XuiXmlAttribute("max_line_count", false)]
	public int MaxLineCount
	{
		get
		{
			return this.maxLineCount;
		}
		set
		{
			if (value == this.maxLineCount)
			{
				return;
			}
			this.maxLineCount = value;
			base.SetDirty();
		}
	}

	[XuiXmlAttribute("use_ellipsis_animator", false)]
	public bool UseEllipsisAnimator
	{
		get
		{
			return this.useEllipsisAnimator;
		}
		set
		{
			if (this.useEllipsisAnimator == value)
			{
				return;
			}
			this.useEllipsisAnimator = value;
			base.SetDirty();
		}
	}

	public XUiV_Label(XUi _xui, string _id)
		: base(_xui, _id)
	{
	}

	public override void InitView()
	{
		base.InitView();
		if (this.useEllipsisAnimator)
		{
			if (this.supportBbCode)
			{
				this.ellipsisAnimator = new TextEllipsisAnimator(this.text, this, this.label);
			}
			else
			{
				Log.Warning("[XUi] Not enabling EllipsisAnimator on label, requires support_bb_code to be true. On " + base.Controller.GetParentWindow().ID + "." + base.ID);
			}
		}
		PlatformManager.NativePlatform.Input.OnLastInputStyleChanged += this.OnLastInputStyleChanged;
	}

	public override void SetDefaults(XUiController _parent)
	{
		base.SetDefaults(_parent);
		this.overflow = 0;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		IPlatform nativePlatform = PlatformManager.NativePlatform;
		if (((nativePlatform != null) ? nativePlatform.Input : null) != null)
		{
			PlatformManager.NativePlatform.Input.OnLastInputStyleChanged -= this.OnLastInputStyleChanged;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		TextEllipsisAnimator textEllipsisAnimator = this.ellipsisAnimator;
		if (textEllipsisAnimator == null)
		{
			return;
		}
		textEllipsisAnimator.GetNextAnimatedString(_dt);
	}

	public override bool anchorsKeepSize
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.label.overflowMethod != 2;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateData()
	{
		if (this.bUpdateText)
		{
			this.label.text = this.getFormattedText(this.text);
			this.bUpdateText = false;
		}
		this.label.overflowMethod = this.overflow;
		this.label.overflowWidth = this.overflowWidth;
		this.label.overflowHeight = this.overflowHeight;
		this.label.overflowEllipsis = this.overflowEllipsis;
		this.label.maxLineCount = this.maxLineCount;
		base.updateData();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnLastInputStyleChanged(PlayerInputManager.InputStyle _obj)
	{
		if (this.parseActions && this.currentTextHasActions)
		{
			this.ForceTextUpdate();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getFormattedText(string _text)
	{
		_text = base.getFormattedText(_text);
		if (this.parseActions)
		{
			this.currentTextHasActions = XUiUtils.ParseActionsMarkup(this.xui, _text, out _text, this.actionsDefaultFormat, this.forceInputStyle);
		}
		return _text;
	}

	public void SetTextImmediately(string _text)
	{
		this.text = _text ?? "";
		if (this.label == null)
		{
			return;
		}
		this.label.text = this.getFormattedText(this.text);
	}

	public void ForceTextUpdate()
	{
		this.bUpdateText = true;
		base.SetDirty();
	}

	[XuiXmlAttribute("text_key", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeTextKey(string _value)
	{
		if (!string.IsNullOrEmpty(_value))
		{
			this.Text = Localization.Get(_value, false, null);
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (this.anchoredLeftAndRight || this.anchoredTopAndBottom || (this.Overflow != null && this.Overflow != 2))
		{
			ThreadManager.StartCoroutine(this.markLabelChanged());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator markLabelChanged()
	{
		int num;
		for (int i = 0; i < 7; i = num + 1)
		{
			yield return XUiV_Label.markChangedDelay;
			this.label.MarkAsChanged();
			num = i;
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public UILabel.Overflow overflow;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int overflowHeight;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int overflowWidth;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool overflowEllipsis;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string text = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public int maxLineCount;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool parseActions;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string actionsDefaultFormat;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool currentTextHasActions;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiUtils.ForceLabelInputStyle forceInputStyle;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool bUpdateText;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool useEllipsisAnimator;

	[PublicizedFrom(EAccessModifier.Private)]
	public TextEllipsisAnimator ellipsisAnimator;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly WaitForSeconds markChangedDelay = new WaitForSeconds(0.01f);
}
