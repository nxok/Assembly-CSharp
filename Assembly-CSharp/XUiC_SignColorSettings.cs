using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SignColorSettings : XUiC_SignLayerSettings
{
	public override void Init()
	{
		base.Init();
		this.colorPicker = base.GetChildByType<XUiC_AdvancedColorPicker>();
		XUiC_AdvancedColorPicker xuiC_AdvancedColorPicker = this.colorPicker;
		xuiC_AdvancedColorPicker.OnColorChanged = (Action<Color>)Delegate.Combine(xuiC_AdvancedColorPicker.OnColorChanged, new Action<Color>(this.OnColorChangedHandler));
		this.cbxMaskMode = base.GetChildById("cbxMaskMode").GetChildByType<XUiC_ComboBoxList<string>>();
		this.cbxMaskMode.OnValueChangedGeneric += delegate(XUiController _)
		{
			this.OnValueChangedGeneric("Changed Mask Mode");
			Action onMaskModeChanged = this.OnMaskModeChanged;
			if (onMaskModeChanged == null)
			{
				return;
			}
			onMaskModeChanged();
		};
	}

	public override void SetLayer(SignData.SignLayer layer)
	{
		this.currentLayer = layer;
		XUiC_AdvancedColorPicker xuiC_AdvancedColorPicker = this.colorPicker;
		SignData.SignLayer signLayer = this.currentLayer;
		xuiC_AdvancedColorPicker.SetColor((signLayer != null) ? signLayer.renderSettings.color : Color.clear, false);
		XUiC_ComboBoxList<string> xuiC_ComboBoxList = this.cbxMaskMode;
		SignData.SignLayer signLayer2 = this.currentLayer;
		xuiC_ComboBoxList.SelectedIndex = (int)((signLayer2 != null) ? signLayer2.renderSettings.mode : SignData.SignRenderSettings.Mode.ColorOnly);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnColorChangedHandler(Color color)
	{
		Action<string, bool> onPreLayerSettingsChanged = this.OnPreLayerSettingsChanged;
		if (onPreLayerSettingsChanged != null)
		{
			onPreLayerSettingsChanged("Changed Color", false);
		}
		this.currentLayer.renderSettings.color = color;
		Action onLayerSettingsChanged = this.OnLayerSettingsChanged;
		if (onLayerSettingsChanged == null)
		{
			return;
		}
		onLayerSettingsChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnValueChangedGeneric(string _changeDescription)
	{
		Action<string, bool> onPreLayerSettingsChanged = this.OnPreLayerSettingsChanged;
		if (onPreLayerSettingsChanged != null)
		{
			onPreLayerSettingsChanged(_changeDescription, false);
		}
		this.currentLayer.renderSettings.mode = (SignData.SignRenderSettings.Mode)this.cbxMaskMode.SelectedIndex;
		Action onLayerSettingsChanged = this.OnLayerSettingsChanged;
		if (onLayerSettingsChanged == null)
		{
			return;
		}
		onLayerSettingsChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_AdvancedColorPicker colorPicker;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> cbxMaskMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public SignData.SignLayer currentLayer;

	public Action OnMaskModeChanged;
}
