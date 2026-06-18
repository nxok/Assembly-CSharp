using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SkewWarpSettings : XUiC_SignWarpSettings
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
		this.cbxSkewX = (XUiC_ComboBoxFloat)base.GetChildById("cbxSkewX").GetChildById("value");
		this.cbxSkewX.OnValueChangedGeneric += delegate(XUiController _)
		{
			this.OnValueChangedGeneric("Changed Skew X");
		};
		this.cbxSkewY = (XUiC_ComboBoxFloat)base.GetChildById("cbxSkewY").GetChildById("value");
		this.cbxSkewY.OnValueChangedGeneric += delegate(XUiController _)
		{
			this.OnValueChangedGeneric("Changed Skew Y");
		};
		this.cbxRotation = (XUiC_ComboBoxFloat)base.GetChildById("cbxRotation").GetChildById("value");
		this.cbxRotation.OnValueChangedGeneric += delegate(XUiController _)
		{
			this.OnValueChangedGeneric("Changed Skew Rotation");
		};
		SignData.SkewWarp skewWarp = new SignData.SkewWarp();
		base.SetDefaultValue("cbxSkewX", skewWarp.amount.x);
		base.SetDefaultValue("cbxSkewY", skewWarp.amount.y);
		base.SetDefaultValue("cbxRotation", skewWarp.rotation);
	}

	public override void SetWarp(SignData.SignWarp warp)
	{
		this.currentWarp = warp as SignData.SkewWarp;
		if (this.currentWarp == null)
		{
			this.cbxSkewX.Value = 0.0;
			this.cbxSkewY.Value = 0.0;
			this.cbxRotation.Value = 0.0;
			return;
		}
		this.cbxSkewX.Value = (double)this.currentWarp.amount.x;
		this.cbxSkewY.Value = (double)this.currentWarp.amount.y;
		this.cbxRotation.Value = (double)this.currentWarp.rotation;
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
		this.currentWarp.amount = new Vector2((float)this.cbxSkewX.Value, (float)this.cbxSkewY.Value);
		this.currentWarp.rotation = (float)this.cbxRotation.Value;
		Action onLayerSettingsChanged = this.OnLayerSettingsChanged;
		if (onLayerSettingsChanged == null)
		{
			return;
		}
		onLayerSettingsChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public SignData.SkewWarp currentWarp;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxSkewX;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxSkewY;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxRotation;
}
