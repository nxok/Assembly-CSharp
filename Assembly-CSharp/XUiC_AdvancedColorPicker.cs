using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_AdvancedColorPicker : XUiController
{
	[XuiXmlAttribute("cursor_radius", false)]
	public float CursorRadius
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.cursorRadius;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (Mathf.Approximately(this.cursorRadius, value))
			{
				return;
			}
			this.cursorRadius = value;
		}
	}

	[XuiXmlAttribute("edit_alpha", false)]
	[XuiXmlBinding("show_alpha")]
	public bool EditAlpha
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.editAlpha;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (this.editAlpha == value)
			{
				return;
			}
			this.editAlpha = value;
			this.bindingsDirty = true;
		}
	}

	[XuiXmlBinding("has_copy")]
	public bool HasCopyBuffer
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return XUiC_AdvancedColorPicker.copyBuffer != null;
		}
	}

	[XuiXmlBinding("sampler_active")]
	public bool SamplerActive
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.samplerActive;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (this.samplerActive == value)
			{
				return;
			}
			this.samplerActive = value;
			this.bindingsDirty = true;
		}
	}

	public override void Init()
	{
		base.Init();
		this.svPicker.CreateMaterial("Game/UI/ColorPickerSV");
		this.hPicker.CreateMaterial("Game/UI/ColorPickerH");
		this.aPicker.CreateMaterial("Game/UI/ColorPickerA");
		XUiV_Texture xuiV_Texture = this.svPicker;
		xuiV_Texture.OnRenderTexture = (UIDrawCall.OnRenderCallback)Delegate.Combine(xuiV_Texture.OnRenderTexture, new UIDrawCall.OnRenderCallback(this.svPicker_OnRender));
		XUiV_Texture xuiV_Texture2 = this.hPicker;
		xuiV_Texture2.OnRenderTexture = (UIDrawCall.OnRenderCallback)Delegate.Combine(xuiV_Texture2.OnRenderTexture, new UIDrawCall.OnRenderCallback(this.hPicker_OnRender));
		XUiV_Texture xuiV_Texture3 = this.aPicker;
		xuiV_Texture3.OnRenderTexture = (UIDrawCall.OnRenderCallback)Delegate.Combine(xuiV_Texture3.OnRenderTexture, new UIDrawCall.OnRenderCallback(this.aPicker_OnRender));
		this.svPicker.Texture = Texture2D.whiteTexture;
		this.hPicker.Texture = Texture2D.whiteTexture;
		this.aPicker.Texture = Texture2D.whiteTexture;
		XUiC_AdvancedColorPicker.<Init>g__SetupPickerEvents|47_0(this.svPicker.Controller, new Action(this.svPicker_MouseUpdate));
		XUiC_AdvancedColorPicker.<Init>g__SetupPickerEvents|47_0(this.hPicker.Controller, new Action(this.hPicker_MouseUpdate));
		XUiC_AdvancedColorPicker.<Init>g__SetupPickerEvents|47_0(this.aPicker.Controller, new Action(this.aPicker_MouseUpdate));
		this.samplerColorSprite.Controller.OnHover += this.SamplerColor_OnHover;
		this.SetColor(Color.cyan, false);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.bindingsDirty = true;
		this.pickersDirty = true;
		this.hoverOverSampler = false;
		if (!this.EditAlpha)
		{
			this.alpha = 1f;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.bindingsDirty)
		{
			base.RefreshBindings();
			this.bindingsDirty = false;
		}
		if (this.pickersDirty)
		{
			this.updatePickerRects();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updatePickerRects()
	{
		this.svPickerRect = this.svPicker.GetXUiRect();
		this.hPickerRect = this.hPicker.GetXUiRect();
		this.aPickerRect = this.aPicker.GetXUiRect();
		this.pickersDirty = false;
	}

	public void SetColor(Color _color, bool _notifyListeners = false)
	{
		this.selectedColorSprite.Color = _color;
		this.updateHsva(_color);
		this.updateRgba(_color);
		this.updateHex(_color);
		if (_notifyListeners)
		{
			Action<Color> onColorChanged = this.OnColorChanged;
			if (onColorChanged == null)
			{
				return;
			}
			onColorChanged(_color);
		}
	}

	[XuiBindEvent("OnPressed", "btnCopy")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCopy_OnPressed(XUiController _sender, int _mouseButton)
	{
		XUiC_AdvancedColorPicker.copyBuffer = new Color?(this.getCurrentColor());
		base.RefreshBindings();
	}

	[XuiBindEvent("OnPressed", "btnPaste")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnPaste_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (XUiC_AdvancedColorPicker.copyBuffer != null)
		{
			this.SetColor(XUiC_AdvancedColorPicker.copyBuffer.Value, true);
		}
	}

	[XuiBindEvent("OnPress", "btnSampler")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSampler_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.samplerActive = true;
		GameManager.Instance.StartCoroutine(this.samplerCoroutine());
		this.bindingsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator samplerCoroutine()
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		this.samplerColorSprite.Color = this.selectedColorSprite.Color;
		while (this.samplerActive)
		{
			yield return new WaitForEndOfFrame();
			if (!this.hoverOverSampler && Input.mousePosition.x >= 0f && Input.mousePosition.y >= 0f && Input.mousePosition.x < (float)Screen.width && Input.mousePosition.y < (float)Screen.height)
			{
				this.samplerTexture.ReadPixels(new Rect(Input.mousePosition, Vector2.one), 0, 0);
				this.samplerTexture.Apply();
				this.samplerColor = this.samplerTexture.GetPixel(0, 0);
				this.samplerColorSprite.Color = this.samplerColor;
			}
		}
		yield break;
	}

	[XuiBindEvent("OnPress", "btnSamplerOverlay")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSamplerOverlay_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.SetColor(this.samplerColor, true);
		this.samplerActive = false;
		this.bindingsDirty = true;
	}

	[XuiBindEvent("OnRightPress", "btnSamplerOverlay")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSamplerOverlay_OnRightPressed(XUiController _sender, int _mouseButton)
	{
		this.samplerActive = false;
		this.bindingsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SamplerColor_OnHover(XUiController _sender, bool _isOver)
	{
		this.hoverOverSampler = _isOver;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void aPicker_MouseUpdate()
	{
		Vector2 relativeMousePos = this.getRelativeMousePos(this.aPickerRect);
		this.alpha = Mathf.Clamp01(relativeMousePos.x);
		this.OnHSVAChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void hPicker_MouseUpdate()
	{
		Vector2 relativeMousePos = this.getRelativeMousePos(this.hPickerRect);
		this.hue = Mathf.Clamp01(relativeMousePos.x);
		this.OnHSVAChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void svPicker_MouseUpdate()
	{
		Vector2 relativeMousePos = this.getRelativeMousePos(this.svPickerRect);
		this.saturation = Mathf.Clamp01(relativeMousePos.x);
		this.value = Mathf.Clamp01(relativeMousePos.y);
		this.OnHSVAChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 getRelativeMousePos(Rect _controllerXUiRect)
	{
		return (this.xui.GetMouseXUiPosition().AsVector2() - _controllerXUiRect.min) / _controllerXUiRect.size;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void aPicker_OnRender(Material _mat)
	{
		_mat.SetVector(XUiC_AdvancedColorPicker._Dimensions, this.aPicker.Size.AsVector2());
		Color currentColor = this.getCurrentColor();
		_mat.SetColor(XUiC_AdvancedColorPicker._Color, currentColor);
		_mat.SetFloat(XUiC_AdvancedColorPicker._CursorSize, this.CursorRadius);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void hPicker_OnRender(Material _mat)
	{
		_mat.SetVector(XUiC_AdvancedColorPicker._Dimensions, this.hPicker.Size.AsVector2());
		_mat.SetFloat(XUiC_AdvancedColorPicker._Hue, this.hue);
		_mat.SetFloat(XUiC_AdvancedColorPicker._CursorSize, this.CursorRadius);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void svPicker_OnRender(Material _mat)
	{
		_mat.SetVector(XUiC_AdvancedColorPicker._Dimensions, this.svPicker.Size.AsVector2());
		_mat.SetFloat(XUiC_AdvancedColorPicker._Hue, this.hue);
		_mat.SetFloat(XUiC_AdvancedColorPicker._Saturation, this.saturation);
		_mat.SetFloat(XUiC_AdvancedColorPicker._Value, this.value);
		_mat.SetFloat(XUiC_AdvancedColorPicker._CursorSize, this.CursorRadius);
	}

	[XuiBindEvent("OnChangeHandler", "txtHex")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void OnHexChanged(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (_text.Length != 6 && _text.Length != 8)
		{
			return;
		}
		Color color;
		if (!ColorUtility.TryParseHtmlString("#" + _text, ref color))
		{
			Log.Error("Failed to parse hex string `" + _text + "`.");
			return;
		}
		if (!_changeFromCode)
		{
			this.selectedColorSprite.Color = color;
			this.updateHsva(color);
			this.updateRgba(color);
			Action<Color> onColorChanged = this.OnColorChanged;
			if (onColorChanged == null)
			{
				return;
			}
			onColorChanged(color);
		}
	}

	[XuiBindEvent("OnChangeHandler", "txtColorR")]
	[XuiBindEvent("OnChangeHandler", "txtColorG")]
	[XuiBindEvent("OnChangeHandler", "txtColorB")]
	[XuiBindEvent("OnChangeHandler", "txtColorA")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void OnRGBAChanged(XUiController _sender, string _text, bool _changeFromCode)
	{
		int num;
		if (!int.TryParse(_text, out num))
		{
			return;
		}
		num = Mathf.Clamp(num, 0, 255);
		if (!_text.Equals(num.ToString()))
		{
			((XUiC_TextInput)_sender).Text = num.ToString();
		}
		byte b;
		byte b2;
		byte b3;
		byte b4;
		if (!_changeFromCode && byte.TryParse(this.txtColorR.Text, out b) && byte.TryParse(this.txtColorG.Text, out b2) && byte.TryParse(this.txtColorB.Text, out b3) && byte.TryParse(this.txtColorA.Text, out b4))
		{
			Color color = new Color32(b, b2, b3, b4);
			if (!this.EditAlpha)
			{
				color.a = 1f;
			}
			this.selectedColorSprite.Color = color;
			this.updateHsva(color);
			this.updateHex(color);
			Action<Color> onColorChanged = this.OnColorChanged;
			if (onColorChanged == null)
			{
				return;
			}
			onColorChanged(color);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnHSVAChanged()
	{
		Color currentColor = this.getCurrentColor();
		this.selectedColorSprite.Color = currentColor;
		this.updateRgba(currentColor);
		this.updateHex(currentColor);
		Action<Color> onColorChanged = this.OnColorChanged;
		if (onColorChanged == null)
		{
			return;
		}
		onColorChanged(currentColor);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Color getCurrentColor()
	{
		Color color = Color.HSVToRGB(this.hue, this.saturation, this.value);
		color.a = this.alpha;
		return color;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateHsva(Color _color)
	{
		Color.RGBToHSV(_color, ref this.hue, ref this.saturation, ref this.value);
		this.alpha = _color.a;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateRgba(Color _color)
	{
		Color32 color = _color;
		this.txtColorR.Text = color.r.ToString();
		this.txtColorG.Text = color.g.ToString();
		this.txtColorB.Text = color.b.ToString();
		this.txtColorA.Text = color.a.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateHex(Color _color)
	{
		this.txtHex.Text = (this.EditAlpha ? ColorUtility.ToHtmlStringRGBA(_color) : ColorUtility.ToHtmlStringRGB(_color));
	}

	[Conditional("DEBUG_LOG_PICKERS")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void debugLog(string _message)
	{
		Debug.Log("[AdvancedColorPicker] " + _message);
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static void <Init>g__SetupPickerEvents|47_0(XUiController _ctrl, Action _update)
	{
		_ctrl.OnPress += delegate(XUiController _, int _)
		{
			_update();
		};
		_ctrl.OnDrag += delegate(XUiController _, EDragType _, Vector2 _)
		{
			_update();
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _Dimensions = Shader.PropertyToID("_Dimensions");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _Saturation = Shader.PropertyToID("_Saturation");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _Value = Shader.PropertyToID("_Value");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _Hue = Shader.PropertyToID("_Hue");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _Color = Shader.PropertyToID("_Color");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _CursorSize = Shader.PropertyToID("_CursorSize");

	[PublicizedFrom(EAccessModifier.Private)]
	public static Color? copyBuffer;

	[XuiBindComponent("svPicker", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Texture svPicker;

	[XuiBindComponent("hPicker", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Texture hPicker;

	[XuiBindComponent("aPicker", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Texture aPicker;

	[PublicizedFrom(EAccessModifier.Private)]
	public Rect svPickerRect;

	[PublicizedFrom(EAccessModifier.Private)]
	public Rect hPickerRect;

	[PublicizedFrom(EAccessModifier.Private)]
	public Rect aPickerRect;

	[XuiBindComponent("selectedColor", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Sprite selectedColorSprite;

	[XuiBindComponent("samplerColor", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Sprite samplerColorSprite;

	[XuiBindComponent("txtColorR", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_TextInput txtColorR;

	[XuiBindComponent("txtColorG", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_TextInput txtColorG;

	[XuiBindComponent("txtColorB", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_TextInput txtColorB;

	[XuiBindComponent("txtColorA", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_TextInput txtColorA;

	[XuiBindComponent("txtHex", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_TextInput txtHex;

	[XuiBindComponent("btnCopy", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_SimpleButton btnCopy;

	[XuiBindComponent("btnPaste", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_SimpleButton btnPaste;

	[XuiBindComponent("btnSampler", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiController btnSampler;

	[XuiBindComponent("btnSamplerOverlay", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiController btnSamplerOverlay;

	public Action<Color> OnColorChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public float hue;

	[PublicizedFrom(EAccessModifier.Private)]
	public float saturation = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float value = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float alpha = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float cursorRadius = 10f;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool editAlpha = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool pickersDirty = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bindingsDirty = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool samplerActive;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color samplerColor = Color.clear;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Texture2D samplerTexture = new Texture2D(1, 1, 5, false);

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hoverOverSampler;
}
