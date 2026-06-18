using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SignPolygonSettings : XUiC_SignLayerSettings
{
	public override void Init()
	{
		base.Init();
		this.cbxMode = base.GetChildById("cbxMode").GetChildByType<XUiC_ComboBoxList<string>>();
		this.cbxMode.OnValueChangedGeneric += this.OnModeChanged;
		this.cbxSides = (XUiC_ComboBoxInt)base.GetChildById("cbxSides").GetChildById("value");
		this.cbxSides.OnValueChangedGeneric += delegate(XUiController _)
		{
			this.OnValueChangedGeneric("Changed Polygon Sides");
		};
		this.cbxSmoothness = (XUiC_ComboBoxFloat)base.GetChildById("cbxSmoothness").GetChildById("value");
		this.cbxSmoothness.OnValueChangedGeneric += delegate(XUiController _)
		{
			this.OnValueChangedGeneric("Changed Polygon Smoothness");
		};
		this.cbxStarify = (XUiC_ComboBoxFloat)base.GetChildById("cbxStarify").GetChildById("value");
		this.cbxStarify.OnValueChangedGeneric += delegate(XUiController _)
		{
			this.OnValueChangedGeneric("Changed Polygon Starify");
		};
		this.cbxSoftness = (XUiC_ComboBoxFloat)base.GetChildById("cbxSoftness").GetChildById("value");
		this.cbxSoftness.OnValueChangedGeneric += delegate(XUiController _)
		{
			this.OnValueChangedGeneric("Changed Polygon Softness");
		};
		this.cbxDilate = (XUiC_ComboBoxFloat)base.GetChildById("cbxDilate").GetChildById("value");
		this.cbxDilate.OnValueChangedGeneric += delegate(XUiController _)
		{
			this.OnValueChangedGeneric("Changed Polygon Dilation");
		};
		this.cbxFrequencyRow = base.GetChildById("cbxFrequency");
		this.cbxFrequency = (XUiC_ComboBoxFloat)this.cbxFrequencyRow.GetChildById("value");
		this.cbxFrequency.OnValueChangedGeneric += delegate(XUiController _)
		{
			this.OnValueChangedGeneric("Changed Polygon Frequency");
		};
		this.defaultMaxSoftness = (float)this.cbxSoftness.Max;
		this.defaultMinDilate = (float)this.cbxDilate.Min;
		this.defaultMaxDilate = (float)this.cbxDilate.Max;
		SignData.PolygonSignLayer polygonSignLayer = new SignData.PolygonSignLayer();
		base.SetDefaultValue("cbxSides", polygonSignLayer.sides);
		base.SetDefaultValue("cbxSmoothness", polygonSignLayer.smoothness);
		base.SetDefaultValue("cbxStarify", polygonSignLayer.starify);
		base.SetDefaultValue("cbxSoftness", polygonSignLayer.softness);
		base.SetDefaultValue("cbxDilate", polygonSignLayer.dilate);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnModeChanged(XUiController _sender)
	{
		this.UpdateDefaultDilate();
		this.OnValueChangedGeneric("Changed Shape Mode");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateDefaultDilate()
	{
		if (this.currentLayer.softness != 0f)
		{
			return;
		}
		bool flag;
		switch (this.currentLayer.shapeMode)
		{
		case SignData.PolygonSignLayer.ShapeMode.Line:
			flag = this.currentLayer.dilate == 0.02f;
			goto IL_0074;
		case SignData.PolygonSignLayer.ShapeMode.Ripple:
			flag = this.currentLayer.dilate == 0.1f;
			goto IL_0074;
		}
		flag = this.currentLayer.dilate == 0f;
		IL_0074:
		if (!flag)
		{
			return;
		}
		switch ((byte)this.cbxMode.SelectedIndex)
		{
		case 2:
			this.cbxDilate.Value = 0.019999999552965164;
			return;
		case 3:
			this.cbxDilate.Value = 0.10000000149011612;
			return;
		}
		this.cbxDilate.Value = 0.0;
	}

	public override void SetLayer(SignData.SignLayer layer)
	{
		this.currentLayer = layer as SignData.PolygonSignLayer;
		if (this.currentLayer == null)
		{
			this.cbxMode.SelectedIndex = 0;
			this.cbxSides.Value = 3L;
			this.cbxSmoothness.Value = 0.0;
			this.cbxStarify.Value = 0.0;
			this.cbxSoftness.Value = 0.0;
			this.cbxDilate.Value = 0.0;
			this.cbxFrequency.Value = 5.0;
			this.cbxFrequencyRow.ViewComponent.IsVisible = false;
			return;
		}
		this.cbxSoftness.Max = (double)Mathf.Max(this.currentLayer.softness, this.defaultMaxSoftness);
		this.cbxDilate.Min = (double)Mathf.Min(this.currentLayer.dilate, this.defaultMinDilate);
		this.cbxDilate.Max = (double)Mathf.Max(this.currentLayer.dilate, this.defaultMaxDilate);
		this.cbxMode.SelectedIndex = (int)this.currentLayer.shapeMode;
		this.cbxSides.Value = (long)this.currentLayer.sides;
		this.cbxSmoothness.Value = (double)this.currentLayer.smoothness;
		this.cbxStarify.Value = (double)this.currentLayer.starify;
		this.cbxSoftness.Value = (double)this.currentLayer.softness;
		this.cbxDilate.Value = (double)this.currentLayer.dilate;
		this.cbxFrequency.Value = (double)this.currentLayer.frequency;
		this.cbxFrequencyRow.ViewComponent.IsVisible = this.currentLayer.shapeMode == SignData.PolygonSignLayer.ShapeMode.Ripple;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnValueChangedGeneric(string _changeDescription)
	{
		if (this.currentLayer == null)
		{
			return;
		}
		Action<string, bool> onPreLayerSettingsChanged = this.OnPreLayerSettingsChanged;
		if (onPreLayerSettingsChanged != null)
		{
			onPreLayerSettingsChanged(_changeDescription, false);
		}
		this.currentLayer.shapeMode = (SignData.PolygonSignLayer.ShapeMode)this.cbxMode.SelectedIndex;
		this.currentLayer.sides = (int)this.cbxSides.Value;
		this.currentLayer.smoothness = (float)this.cbxSmoothness.Value;
		this.currentLayer.starify = (float)this.cbxStarify.Value;
		this.currentLayer.softness = (float)this.cbxSoftness.Value;
		this.currentLayer.dilate = (float)this.cbxDilate.Value;
		this.currentLayer.frequency = (float)this.cbxFrequency.Value;
		this.cbxFrequencyRow.ViewComponent.IsVisible = this.currentLayer.shapeMode == SignData.PolygonSignLayer.ShapeMode.Ripple;
		Action onLayerSettingsChanged = this.OnLayerSettingsChanged;
		if (onLayerSettingsChanged == null)
		{
			return;
		}
		onLayerSettingsChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> cbxMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxInt cbxSides;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxSmoothness;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxStarify;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxSoftness;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxDilate;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController cbxFrequencyRow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxFrequency;

	[PublicizedFrom(EAccessModifier.Private)]
	public SignData.PolygonSignLayer currentLayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public float defaultMaxSoftness;

	[PublicizedFrom(EAccessModifier.Private)]
	public float defaultMinDilate;

	[PublicizedFrom(EAccessModifier.Private)]
	public float defaultMaxDilate;
}
