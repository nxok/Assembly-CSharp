using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_StretchWarpSettings : XUiC_SignWarpSettings
{
	public override SignData.SignWarp CurrentWarp
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.currentWarp;
		}
	}

	public override void Init()
	{
		base.Init();
		this.cbxOffsetX = (XUiC_ComboBoxFloat)base.GetChildById("cbxOffsetX").GetChildById("value");
		this.cbxOffsetX.OnValueChangedGeneric += delegate(XUiController _)
		{
			this.OnValueChangedGeneric("Changed Stretch Offset X");
		};
		this.cbxOffsetY = (XUiC_ComboBoxFloat)base.GetChildById("cbxOffsetY").GetChildById("value");
		this.cbxOffsetY.OnValueChangedGeneric += delegate(XUiController _)
		{
			this.OnValueChangedGeneric("Changed Stretch Offset Y");
		};
		this.cbxRotation = (XUiC_ComboBoxFloat)base.GetChildById("cbxRotation").GetChildById("value");
		this.cbxRotation.OnValueChangedGeneric += delegate(XUiController _)
		{
			this.OnValueChangedGeneric("Changed Stretch Rotation");
		};
		this.cbxDistance = (XUiC_ComboBoxFloat)base.GetChildById("cbxDistance").GetChildById("value");
		this.cbxDistance.OnValueChangedGeneric += delegate(XUiController _)
		{
			this.OnValueChangedGeneric("Changed Stretch Distance");
		};
		this.cbxWidth = (XUiC_ComboBoxFloat)base.GetChildById("cbxWidth").GetChildById("value");
		this.cbxWidth.OnValueChangedGeneric += delegate(XUiController _)
		{
			this.OnValueChangedGeneric("Changed Stretch Width");
		};
		this.cbxExponent = (XUiC_ComboBoxFloat)base.GetChildById("cbxExponent").GetChildById("value");
		this.cbxExponent.OnValueChangedGeneric += delegate(XUiController _)
		{
			this.OnValueChangedGeneric("Changed Stretch Exponent");
		};
		SignData.StretchWarp stretchWarp = new SignData.StretchWarp();
		base.SetDefaultValue("cbxOffsetX", stretchWarp.offset.x);
		base.SetDefaultValue("cbxOffsetY", stretchWarp.offset.y);
		base.SetDefaultValue("cbxRotation", stretchWarp.rotation);
		base.SetDefaultValue("cbxDistance", stretchWarp.distance);
		base.SetDefaultValue("cbxWidth", stretchWarp.width);
		base.SetDefaultValue("cbxExponent", stretchWarp.exponent);
	}

	public override void SetWarp(SignData.SignWarp warp)
	{
		this.currentWarp = warp as SignData.StretchWarp;
		if (this.currentWarp == null)
		{
			this.cbxOffsetX.Value = 0.0;
			this.cbxOffsetY.Value = 0.0;
			this.cbxRotation.Value = 0.0;
			this.cbxDistance.Value = 0.0;
			this.cbxWidth.Value = 0.0;
			this.cbxExponent.Value = 0.0;
			return;
		}
		this.cbxOffsetX.Value = (double)this.currentWarp.offset.x;
		this.cbxOffsetY.Value = (double)this.currentWarp.offset.y;
		this.cbxRotation.Value = (double)this.currentWarp.rotation;
		this.cbxDistance.Value = (double)this.currentWarp.distance;
		this.cbxWidth.Value = (double)this.currentWarp.width;
		this.cbxExponent.Value = (double)this.currentWarp.exponent;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnValueChangedGeneric(string _changeDescription)
	{
		if (this.currentWarp == null)
		{
			return;
		}
		Action<string, bool> onPreLayerSettingsChanged = this.OnPreLayerSettingsChanged;
		if (onPreLayerSettingsChanged != null)
		{
			onPreLayerSettingsChanged(_changeDescription, false);
		}
		this.currentWarp.offset = new Vector2((float)this.cbxOffsetX.Value, (float)this.cbxOffsetY.Value);
		this.currentWarp.rotation = (float)this.cbxRotation.Value;
		this.currentWarp.distance = (float)this.cbxDistance.Value;
		this.currentWarp.width = (float)this.cbxWidth.Value;
		this.currentWarp.exponent = (float)this.cbxExponent.Value;
		Action onLayerSettingsChanged = this.OnLayerSettingsChanged;
		if (onLayerSettingsChanged == null)
		{
			return;
		}
		onLayerSettingsChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public SignData.StretchWarp currentWarp;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxOffsetX;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxOffsetY;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxRotation;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxWidth;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxExponent;
}
