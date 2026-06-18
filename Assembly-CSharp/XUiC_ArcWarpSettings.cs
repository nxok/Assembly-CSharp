using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ArcWarpSettings : XUiC_SignWarpSettings
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
		this.cbxRotation = (XUiC_ComboBoxFloat)base.GetChildById("cbxRotation").GetChildById("value");
		this.cbxRotation.OnValueChangedGeneric += delegate(XUiController _)
		{
			this.OnValueChangedGeneric("Changed Arc Rotation");
		};
		this.cbxRadius = (XUiC_ComboBoxFloat)base.GetChildById("cbxRadius").GetChildById("value");
		this.cbxRadius.OnValueChangedGeneric += delegate(XUiController _)
		{
			this.OnValueChangedGeneric("Changed Arc Radius");
		};
		this.cbxWidth = (XUiC_ComboBoxFloat)base.GetChildById("cbxWidth").GetChildById("value");
		this.cbxWidth.OnValueChangedGeneric += delegate(XUiController _)
		{
			this.OnValueChangedGeneric("Changed Arc Width");
		};
		SignData.ArcWarp arcWarp = new SignData.ArcWarp();
		base.SetDefaultValue("cbxRotation", arcWarp.rotation);
		base.SetDefaultValue("cbxRadius", arcWarp.radius);
		base.SetDefaultValue("cbxWidth", arcWarp.width);
	}

	public override void SetWarp(SignData.SignWarp warp)
	{
		this.currentWarp = warp as SignData.ArcWarp;
		if (this.currentWarp == null)
		{
			this.cbxRotation.Value = 0.0;
			this.cbxRadius.Value = 0.0;
			this.cbxWidth.Value = 0.0;
			return;
		}
		this.cbxRotation.Value = (double)this.currentWarp.rotation;
		this.cbxRadius.Value = (double)this.currentWarp.radius;
		this.cbxWidth.Value = (double)this.currentWarp.width;
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
		this.currentWarp.rotation = (float)this.cbxRotation.Value;
		this.currentWarp.radius = (float)this.cbxRadius.Value;
		this.currentWarp.width = (float)this.cbxWidth.Value;
		Action onLayerSettingsChanged = this.OnLayerSettingsChanged;
		if (onLayerSettingsChanged == null)
		{
			return;
		}
		onLayerSettingsChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public SignData.ArcWarp currentWarp;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxRotation;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxRadius;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxWidth;
}
